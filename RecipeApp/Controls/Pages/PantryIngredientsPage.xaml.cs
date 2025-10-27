

namespace RecipeApp.Controls.Pages;


public sealed partial class PantryIngredientsPage : NavigatorPage
{
    [ObservableProperty] private partial ObservableCollection<IngredientCard> AllIngredients { get; set; } = [];
    public PantryIngredientsPage(Navigator? nav) : base(nav)
    {
        this.InitializeComponent();

        _ = ShowIngredients();
    }

    private async Task ShowIngredients()
    {
        AllIngredients = (await RecipeIngredient.GetAll())
            .Select(i => new IngredientCard() { Ingredient = i, IsSelected = false})
            .ToObservableCollection();
    }
    
    private void OnRecipeCardChecked(object sender, RoutedEventArgs e)
    {
        RefreshSelected();
    }
    
    [ObservableProperty] private partial bool CardsSelected { get; set; } = false;
    
    private void RefreshSelected()
    {
        CardsSelected = GetSelectedIngredientCards()
            .Any(c => c.IsSelected);
    }
    
    private List<IngredientCard> GetSelectedIngredientCards()
    {
        return AllIngredients
            .Where(c => c.IsSelected)
            .ToList();
    }

    private async void ButtonAddIngredient_OnClick(object sender, RoutedEventArgs e)
    {
        var newIngredient = new RecipeIngredient();
        
        AllIngredients.Add(new IngredientCard() { Ingredient = newIngredient, IsSelected = false });
        await RecipeIngredient.Add(newIngredient);
    }

    private async void ButtonRemoveIngredient_OnClick(object sender, RoutedEventArgs e)
    {
        var selected = GetSelectedIngredientCards();
        
        await Task.WhenAll(selected.Select(c => RecipeIngredient.Remove(c.Ingredient)));
        selected.ForEach(i => AllIngredients.Remove(i));
    }
}
