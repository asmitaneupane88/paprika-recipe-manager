using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Markup;
using RecipeApp.Models;
using System;
using System.Collections.Generic;
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

    private class RecipeSelectable
    {
        public SavedRecipe Recipe { get; }
        public RecipeSelectable(SavedRecipe r) => Recipe = r;
        public string? Title => Recipe.Title;
    // SavedRecipe doesn't have a Category property; show the first tag if available as a lightweight category.
    public string? Category => Recipe.Tags?.FirstOrDefault();
        public string? ImageUrl => Recipe.ImageUrl;
        public int Rating => Recipe.Rating;
        public int BindableMaxRating => Recipe.BindableMaxRating;
        public bool IsSelected { get; set; }
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
    var recipesList = new System.Collections.ObjectModel.ObservableCollection<RecipeSelectable>(wrappers);

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
            PrimaryButtonText = "Add",
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