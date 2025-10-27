using System.Drawing.Printing;
using System.Text;
using Windows.Graphics.Printing;
using Microsoft.UI.Xaml.Input;
using RecipeApp.Services;
using Uno.Extensions;

namespace RecipeApp.Controls.Pages;

public sealed partial class RecipeListPage : NavigatorPage
{
    private const int AllCategorySortOrder = -20252025;
    
    [ObservableProperty] private partial ObservableCollection<RecipeCard> AllRecipes { get; set; } = [];
    [ObservableProperty] private partial ObservableCollection<RecipeCard> FilteredRecipes { get; set; } = [];
    
    private string SearchText
    {
        get;
        set { SetProperty(ref field, value); _ = UpdateShownRecipes(); }
    } = "";
    
    private SavedCategory SelectedCategory { get; set { SetProperty(ref field, value); _ = UpdateShownRecipes(); } }
    [ObservableProperty] private partial ObservableCollection<SavedCategory> Categories { get; set; } = [];
    [ObservableProperty] private partial bool CardsSelected { get; set; } = false;
    [ObservableProperty] private partial Visibility ListVisibility { get; set; } = Visibility.Visible;
    [ObservableProperty] private partial Visibility GridVisibility { get; set; } = Visibility.Collapsed;
    
    public RecipeListPage(Navigator? nav = null) : base(nav)
    {
        this.InitializeComponent();
        
        InitializeRecipes()
            .ContinueWith(_ => UpdateShownCategories()
                .ContinueWith(_ => UpdateShownRecipes()));
        
    }

    private async Task InitializeRecipes()
    {
        var recipes = await SavedRecipe.GetAll();
        
        AllRecipes = recipes
            .Select(r => new RecipeCard { SavedRecipe = r, IsSelected = false })
            .ToObservableCollection();
    }

    private async Task UpdateShownCategories()
    {
        Categories = (await SavedCategory.GetAll()).ToObservableCollection();
        
        var allCategory = new SavedCategory
        {
            Name = "All Categories",
            SortOrder = AllCategorySortOrder
        };
        
        Categories.Insert(0, allCategory);
        SelectedCategory = allCategory;
    }
    
    private async Task UpdateShownRecipes()
    {
        FilteredRecipes = AllRecipes
            .Where(r => r.SavedRecipe.Title.Contains(SearchText.Trim(), StringComparison.CurrentCultureIgnoreCase))
            .Where(r => SelectedCategory.SortOrder == AllCategorySortOrder
                        || (r.SavedRecipe.Category is not null 
                            && r.SavedRecipe.Category.Trim()
                                .Equals(SelectedCategory.Name.Trim(), StringComparison.CurrentCultureIgnoreCase)))
            .ToObservableCollection();
    }
    
    private void RefreshSelected()
    {
        CardsSelected = GetSelectedRecipeCards()
            .Any(c => c.IsSelected);
    }

    private RecipeCard[] GetSelectedRecipeCards()
    {
        return FilteredRecipes
            .Where(c => c.IsSelected)
            .ToArray();
    }

    private SavedRecipe[] GetSelectedRecipes()
    {
        return GetSelectedRecipeCards()
            .Select(c => c.SavedRecipe)
            .ToArray();
    }
    
    private void OnRecipeCardChecked(object sender, RoutedEventArgs e)
    {
        RefreshSelected();
    }

    private void OnRecipeCardClicked(object sender, RoutedEventArgs e)
    {
        if (sender is ListView { SelectedItem: RecipeCard rc } lv)
        {
            lv.SelectedItem = null;
            var details = new RecipeDetailsPage(Navigator, savedRecipe: rc.SavedRecipe);
            Navigator.Navigate(details, $"Recipe: {rc.SavedRecipe.Title}");
        }
    }

    private async void OnButtonAddClick(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Create New Recipe",
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            IsPrimaryButtonEnabled = false,
            XamlRoot = this.XamlRoot
        };
        
        var textbox = new TextBox
        {
            PlaceholderText="Recipe Title",
        };
        
        textbox.TextChanged += (s, _) =>
        {
            dialog.IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(textbox.Text);
        };
        
        dialog.Content = textbox;
        
        var result = await dialog.ShowAsync();
        
        if (result == ContentDialogResult.Primary)
        {
            var newRecipe = new SavedRecipe()
            {
                Title = textbox.Text,
            };

            await SavedRecipe.Add(newRecipe);

            Navigator.Navigate(new EditRecipe(Navigator, newRecipe), title:$"Edit {newRecipe.Title}");
        }
    }

    private async void OnButtonRemoveClick(object sender, RoutedEventArgs e)
    {
        var recipesToRemove = GetSelectedRecipeCards();
        
        await SavedRecipe.Remove(recipesToRemove.Select(r => r.SavedRecipe).ToArray());
        recipesToRemove.ForEach(r => AllRecipes.Remove(r));
        
        await UpdateShownRecipes();
    }

    private void OnButtonGroceryListClick(object sender, RoutedEventArgs e)
    {
        //TODO: create list from selected and open it
    }

    private async void OnButtonPrintClick(object sender, RoutedEventArgs e)
    {
        var recipesToPrint = GetSelectedRecipes();

        var sb = new StringBuilder();

        sb.AppendLine(SavedRecipe.HtmlHeader);
        
        foreach (var recipe in recipesToPrint)
        {
            sb.AppendLine(recipe.ConvertToHtml());
            sb.AppendLine("<hr/>");
        }

        sb.AppendLine(SavedRecipe.HtmlFooter);

        var html = sb.ToString();
        
        //TODO: look into moving from save file to print maybe?
        // I could not find a cross platform library to use and do not want to do each platform and have to test each one.

        await FileHelper.SaveHtmlAsPdf(html);
    }

    /// <inheritdoc />
    public override async Task Restore()
    {
        await UpdateShownRecipes();
        await base.Restore();
    }
}

