using Microsoft.UI.Xaml.Input;

namespace RecipeApp.Controls.Pages;

public sealed partial class RecipePage : NavigatorPage
{
    private ObservableCollection<RecipeCard> FilteredRecipes { get; set => SetField(ref field, value); } = [];

    private string SearchText
    {
        get;
        set { SetField(ref field, value); UpdateSearch(); }
    } = "";
    
    private bool CardsSelected { get; set => SetField(ref field, value); } = false;
    private Visibility ListVisibility { get; set => SetField(ref field, value); } = Visibility.Visible;
    private Visibility GridVisibility { get; set => SetField(ref field, value); } = Visibility.Collapsed;
    
    public RecipePage(Navigator? nav = null) : base(nav)
    {
        this.InitializeComponent();
        
        UpdateSearch();
    }

    private async void UpdateSearch()
    {
        var recipes = await Recipe.GetAll();

        FilteredRecipes = recipes
            .Where(r => r.Title.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase))
            .Select(r => new RecipeCard { Recipe = r, IsSelected = false })
            .ToObservableCollection();
    }
    
    private void OnRecipeCardChecked(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox checkBox && checkBox.DataContext is RecipeCard card)
        {
            CardsSelected = FilteredRecipes.Any(c => c.IsSelected);
        }
    }

    private void OnRecipeCardClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.CommandParameter is RecipeCard card)
        {
        }
    }

    private void UIElement_OnKeyUp(object sender, KeyRoutedEventArgs e)
    {
        UpdateSearch();
    }
}

