using System.Drawing.Printing;
using System.Text;
using Windows.Graphics.Printing;
using Microsoft.UI.Xaml.Input;
using RecipeApp.Services;
using Uno.Extensions;

namespace RecipeApp.Controls.Pages;

public sealed partial class RecipePage : NavigatorPage
{
    private ObservableCollection<RecipeCard> FilteredRecipes { get; set => SetField(ref field, value); } = [];

    private string SearchText
    {
        get;
        set { SetField(ref field, value); UpdateShownRecipes(); }
    } = "";
    
    private bool CardsSelected { get; set => SetField(ref field, value); } = false;
    private Visibility ListVisibility { get; set => SetField(ref field, value); } = Visibility.Visible;
    private Visibility GridVisibility { get; set => SetField(ref field, value); } = Visibility.Collapsed;
    
    public RecipePage(Navigator? nav = null) : base(nav)
    {
        this.InitializeComponent();
        
        UpdateShownRecipes();
    }

    private async void UpdateShownRecipes()
    {
        var recipes = await SavedRecipe.GetAll();

        FilteredRecipes = recipes
            .Where(r => r.Title.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase))
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
        if (sender is Button { CommandParameter: RecipeCard rc })
        {
            // TODO use rc (recipe card) here for the details page.
        }
    }

    private void OnButtonAddClick(object sender, RoutedEventArgs e)
    {
        //TODO: go to add recipe page
    }

    private async void OnButtonRemoveClick(object sender, RoutedEventArgs e)
    {
        var recipesToRemove = GetSelectedRecipes();
        
        await SavedRecipe.Remove(recipesToRemove);
        
        UpdateShownRecipes();
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
    
    
}

