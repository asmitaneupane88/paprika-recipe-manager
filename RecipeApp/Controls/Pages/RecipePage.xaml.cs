using System.Drawing.Printing;
using System.Text;
using Windows.Graphics.Printing;
using Microsoft.UI.Xaml.Input;
using RecipeApp.Services;
using Uno.Extensions;

namespace RecipeApp.Controls.Pages;

public sealed partial class RecipePage : NavigatorPage
{
    private const int AllCategorySortOrder = -20252025;
    
    private ObservableCollection<RecipeCard> FilteredRecipes { get; set => SetField(ref field, value); } = [];
    
    private string SearchText
    {
        get;
        set { SetField(ref field, value); _ = UpdateShownRecipes(); }
    } = "";
    
    private SavedCategory SelectedCategory { get; set { SetField(ref field, value); _ = UpdateShownRecipes(); } }
    private ObservableCollection<SavedCategory> Categories { get; set => SetField(ref field, value); } = [];
    
    private bool CardsSelected { get; set => SetField(ref field, value); } = false;
    private Visibility ListVisibility { get; set => SetField(ref field, value); } = Visibility.Visible;
    private Visibility GridVisibility { get; set => SetField(ref field, value); } = Visibility.Collapsed;
    
    public RecipePage(Navigator? nav = null) : base(nav)
    {
        this.InitializeComponent();
        
        UpdateShownCategories()
            .ContinueWith(_ => UpdateShownRecipes());
        
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
        var recipes = await SavedRecipe.GetAll();

        FilteredRecipes = recipes
            .Where(r => r.Title.Contains(SearchText.Trim(), StringComparison.CurrentCultureIgnoreCase))
            .Where(r => SelectedCategory.SortOrder == AllCategorySortOrder
                        || (r.Category is not null 
                        && r.Category.Trim()
                            .Equals(SelectedCategory.Name.Trim(), StringComparison.CurrentCultureIgnoreCase)))
            .Select(r => new RecipeCard { SavedRecipe = r, IsSelected = false })
            .ToObservableCollection();
        
        RefreshSelected();
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
            var details = new RecipeDetails(Navigator, savedRecipe: rc.SavedRecipe);
            Navigator.Navigate(details, $"Recipe: {rc.SavedRecipe.Title}");
        }
    }

    private async void OnButtonAddClick(object sender, RoutedEventArgs e)
    {
        // Since SavedRecipe inherits from IAutosavingClass, changes are automatically saved
        // We just need to show a confirmation and navigate back
        
        var dialog = new ContentDialog
        {
            Title = "Create New Recipe",
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.XamlRoot
        };
        var textbox = new TextBox()
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
        var recipesToRemove = GetSelectedRecipes();
        
        await SavedRecipe.Remove(recipesToRemove);
        
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

