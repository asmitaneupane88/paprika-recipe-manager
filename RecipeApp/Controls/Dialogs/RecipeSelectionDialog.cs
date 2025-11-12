using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Markup;
using RecipeApp.Models;
using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace RecipeApp.Controls.Dialogs;

public class RecipeSelectionDialog
{
    private readonly XamlRoot _xamlRoot;
    private readonly IEnumerable<SavedRecipe> _recipes;
    private readonly DateTime _date;
    private readonly MealType _mealType;

    public RecipeSelectionDialog(XamlRoot xamlRoot, IEnumerable<SavedRecipe> recipes, DateTime date, MealType mealType)
    {
        _xamlRoot = xamlRoot;
        _recipes = recipes;
        _date = date;
        _mealType = mealType;
    }

    private class RecipeSelectable : INotifyPropertyChanged
    {
        public SavedRecipe Recipe { get; }
        public RecipeSelectable(SavedRecipe r) => Recipe = r;
        public string? Title => Recipe.Title;
        // SavedRecipe doesn't have a Category property; show the first tag if available as a lightweight category.
        public string? Category => Recipe.Tags?.FirstOrDefault();
        public string? ImageUrl => Recipe.ImageUrl;
        public int Rating => Recipe.Rating;
        public int BindableMaxRating => Recipe.BindableMaxRating;

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }

        private bool _isLeftOver;
        /// <summary>
        /// Whether this recipe is marked as leftover for the slot being edited.
        /// Bound to a small control in the item template.
        /// </summary>
        public bool IsLeftOver
        {
            get => _isLeftOver;
            set
            {
                if (_isLeftOver == value) return;
                _isLeftOver = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLeftOver)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        // Command to generate grocery items from this recipe's ingredients
        public ICommand? GenerateGroceryListCommand { get; set; }
    }

    // Simple ICommand wrapper to allow async handlers from XAML-bound buttons
    private class AsyncCommand : ICommand
    {
        private readonly Func<object?, Task> _execute;
        public AsyncCommand(Func<object?, Task> execute) => _execute = execute;
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => true;
        public async void Execute(object? parameter) => await _execute(parameter);
    }

    private async Task GenerateGroceryListFor(RecipeSelectable rs)
    {
        try
        {
            var pathInfos = rs.Recipe?.RootStepNode?.GetNestedPathInfo();
            var ingredients = pathInfos?.SelectMany(pi => pi.MaxIngredients).Where(i => !string.IsNullOrWhiteSpace(i.Name)).ToList();
            if (ingredients == null || ingredients.Count == 0)
            {
                var noneDialog = new ContentDialog
                {
                    Title = "No ingredients found",
                    Content = "This recipe has no parsed ingredients to add to the grocery list.",
                    CloseButtonText = "OK",
                    XamlRoot = _xamlRoot
                };
                await noneDialog.ShowAsync();
                return;
            }

            int added = 0;
            foreach (var ingr in ingredients)
            {
                var grocery = new RecipeIngredient
                {
                    Name = ingr.Name,
                    Quantity = ingr.Quantity,
                    Unit = ingr.Unit,
                    ModifierNote = ingr.ModifierNote,
                    ScaleFactor = 1
                };
                await RecipeIngredient.Add(grocery);
                added++;
            }

            var okDialog = new ContentDialog
            {
                Title = "Grocery items added",
                Content = $"Added {added} item{(added == 1 ? string.Empty : "s")} to your grocery list.",
                CloseButtonText = "OK",
                XamlRoot = _xamlRoot
            };
            await okDialog.ShowAsync();
        }
        catch
        {
            var err = new ContentDialog
            {
                Title = "Error",
                Content = "An error occurred while generating the grocery list.",
                CloseButtonText = "OK",
                XamlRoot = _xamlRoot
            };
            await err.ShowAsync();
        }
    }

    /// <summary>
    /// Shows the dialog and returns the list of selected recipes (or null if cancelled).
    /// </summary>
    public async Task<System.Collections.Generic.List<SavedRecipe>?> ShowAsync()
    {
        var rootGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
            },
            MinWidth = 400,
            Height = 500 // Fixed height
        };

        // Create search box
        var searchBox = new AutoSuggestBox
        {
            PlaceholderText = "Search recipes...",
            QueryIcon = new SymbolIcon(Symbol.Find),
            Margin = new Thickness(0, 0, 0, 8)
        };

    // Wrap recipes in a selectable wrapper so we can bind a CheckBox to IsSelected
    var wrappers = _recipes.Select(r => new RecipeSelectable(r)).ToList();

    // Pre-select any recipes already added to the meal plan for this date+mealType
    var plansForSlot = new List<MealPlan>();
    try
    {
        var allMealPlans = await MealPlan.GetAll();
        plansForSlot = allMealPlans.Where(mp => mp.Date.Date == _date.Date && mp.MealType == _mealType).ToList();
        foreach (var w in wrappers)
        {
            if (plansForSlot.Any(p => string.Equals(p.Recipe?.Title, w.Recipe?.Title, StringComparison.OrdinalIgnoreCase)))
            {
                w.IsSelected = true;
            }

            if (plansForSlot.Any(p => string.Equals(p.Recipe?.Title, w.Recipe?.Title, StringComparison.OrdinalIgnoreCase) && p.IsLeftOver))
            {
                w.IsLeftOver = true;
            }
        }
    }
    catch
    {
        // ignore any errors loading meal plans; default to none selected
    }
    var recipesList = new System.Collections.ObjectModel.ObservableCollection<RecipeSelectable>(wrappers);

    // When a wrapper's IsSelected or IsLeftOver is changed by the user, react accordingly.
    foreach (var w in wrappers)
    {
        w.PropertyChanged += async (s, e) =>
        {
            var rs = (RecipeSelectable)s!;
            if (e.PropertyName == nameof(RecipeSelectable.IsSelected))
            {
                if (!rs.IsSelected)
                {
                    try
                    {
                        var toRemove = plansForSlot.Where(p => string.Equals(p.Recipe?.Title, rs.Recipe?.Title, StringComparison.OrdinalIgnoreCase)).ToArray();
                        if (toRemove.Length > 0)
                        {
                            await MealPlan.Remove(toRemove);
                            // remove from local cache so repeated unchecks don't try again
                            plansForSlot = plansForSlot.Except(toRemove).ToList();
                        }
                    }
                    catch
                    {
                        // ignore removal errors for now
                    }
                }
            }
            else if (e.PropertyName == nameof(RecipeSelectable.IsLeftOver))
            {
                try
                {
                    // Find existing meal plans for this slot that match the recipe
                    var matching = plansForSlot.Where(p => string.Equals(p.Recipe?.Title, rs.Recipe?.Title, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (matching.Any())
                    {
                        // Update each existing plan's IsLeftOver flag
                        foreach (var mp in matching)
                        {
                            await MealPlan.SetLeftOver(mp, rs.IsLeftOver);
                        }
                    }
                    else if (rs.IsLeftOver && rs.IsSelected)
                    {
                        // No existing plan in this slot, but user marked leftover and recipe is selected -> create one and mark leftover
                        await MealPlan.AddMealPlan(_date, rs.Recipe, _mealType, true);
                        // refresh local cache of plans for slot
                        var all = await MealPlan.GetAll();
                        plansForSlot = all.Where(mp => mp.Date.Date == _date.Date && mp.MealType == _mealType).ToList();
                    }
                }
                catch
                {
                    // ignore errors
                }
            }
        };
    }

    // Attach per-item commands (generate grocery list) so item template buttons can call into code.
    foreach (var w in wrappers)
    {
        w.GenerateGroceryListCommand = new AsyncCommand(async _ => await GenerateGroceryListFor(w));
    }

        // Create a ScrollViewer to ensure proper scrolling behavior
        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };

        // Create the recipe list view
        // Create a DataTemplate for checklist items (image, title, category, rating, checkbox)
        var itemTemplateXaml = @"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' xmlns:controls='using:Microsoft.UI.Xaml.Controls'>
            <Grid Padding='8'>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width='60'/>
                    <ColumnDefinition Width='*'/>
                    <ColumnDefinition Width='Auto'/>
                </Grid.ColumnDefinitions>
                <Border CornerRadius='4' Width='60' Height='60' Margin='0,0,12,0'>
                    <Border.Background>
                        <ImageBrush ImageSource='{Binding ImageUrl}' Stretch='UniformToFill'/>
                    </Border.Background>
                </Border>
                <StackPanel Grid.Column='1' VerticalAlignment='Center'>
                    <TextBlock Text='{Binding Title}' FontWeight='SemiBold' TextWrapping='Wrap'/>
                    <TextBlock Text='{Binding Category}' Foreground='{ThemeResource TextFillColorSecondaryBrush}' Style='{StaticResource CaptionTextBlockStyle}'/>
                    <controls:RatingControl Value='{Binding Rating}' MaxRating='{Binding BindableMaxRating}' IsReadOnly='True' Margin='0,4,0,0'/>
                                    <CheckBox Content='🍽  Mark as Leftover' IsChecked='{Binding IsLeftOver, Mode=TwoWay}' IsEnabled='{Binding IsSelected}' Margin='0,6,0,0' ToolTipService.ToolTip='Mark as leftover'/>
                                    <Button Content='Generate grocery list' Command='{Binding GenerateGroceryListCommand}' Margin='0,8,0,0' HorizontalAlignment='Left' Padding='8,4' />
                </StackPanel>
                <CheckBox Grid.Column='2' VerticalAlignment='Center' IsChecked='{Binding IsSelected, Mode=TwoWay}' Margin='8,0,0,0'/>
            </Grid>
        </DataTemplate>";

        var recipeListView = new ListView
        {
            ItemsSource = recipesList,
            // Use explicit CheckBox bindings for selection; disable built-in ListView selection UI
            SelectionMode = ListViewSelectionMode.None,
            ItemTemplate = (DataTemplate)XamlReader.Load(itemTemplateXaml),
            MinHeight = 400,
            MaxHeight = 400
        };

        scrollViewer.Content = recipeListView;

        // Set up search functionality
        // wire up search to filter wrappers by title
        var allWrappers = wrappers;
        searchBox.TextChanged += (s, e) =>
        {
            var searchText = searchBox.Text;
            if (string.IsNullOrWhiteSpace(searchText))
            {
                recipeListView.ItemsSource = recipesList;
            }
            else
            {
                var filtered = allWrappers.Where(w => w.Title?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true).ToList();
                recipeListView.ItemsSource = new System.Collections.ObjectModel.ObservableCollection<RecipeSelectable>(filtered);
            }
        };

        // Add controls to grid
        Grid.SetRow(searchBox, 0);
        Grid.SetRow(scrollViewer, 1);
        rootGrid.Children.Add(searchBox);
        rootGrid.Children.Add(scrollViewer);

        var dialog = new ContentDialog
        {
            Title = $"Select Recipe for {_mealType} on {_date:MMM d}",
            Content = rootGrid,
            PrimaryButtonText = "Update",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = _xamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary) return null;

        // Collect selected wrappers from the current items source (which may be filtered)
        var currentItems = recipeListView.ItemsSource as System.Collections.IEnumerable;
        var selected = new System.Collections.Generic.List<SavedRecipe>();
        if (currentItems != null)
        {
            foreach (var item in currentItems)
            {
                if (item is RecipeSelectable rs && rs.IsSelected)
                    selected.Add(rs.Recipe);
            }
        }

        // No fallback to ListView.SelectedItems because SelectionMode is None; selection is driven by the CheckBox only.

        return selected.Count > 0 ? selected : null;
    }
}