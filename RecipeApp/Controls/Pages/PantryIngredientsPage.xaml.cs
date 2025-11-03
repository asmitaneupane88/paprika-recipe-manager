

namespace RecipeApp.Controls.Pages;


public sealed partial class PantryIngredientsPage : NavigatorPage
{
    [ObservableProperty] private partial ObservableCollection<IngredientCard> PantryIngredients { get; set; } = [];
    public PantryIngredientsPage(Navigator? nav) : base(nav)
    {
        this.InitializeComponent();

        _ = ShowIngredients();
    }

    private async Task ShowIngredients()
    {
        PantryIngredients = (await PantryIngredient.GetAll())
            .Select(i => new IngredientCard
            {
                PIngredient = i,
                IsSelected = false,
                Ingredient = null
            })
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
        return PantryIngredients
            .Where(c => c.IsSelected)
            .ToList();
    }

    private async void ButtonAddIngredient_OnClick(object sender, RoutedEventArgs e)
    {
        var newIngredient = new PantryIngredient();
        
        PantryIngredients.Add(new IngredientCard
        {
            PIngredient = newIngredient,
            IsSelected = false,
            Ingredient = null
        });
        await PantryIngredient.Add(newIngredient);
    }

    private async void ButtonRemoveIngredient_OnClick(object sender, RoutedEventArgs e)
    {
        var selected = GetSelectedIngredientCards();
        
        await Task.WhenAll(selected.Select(c => PantryIngredient.Remove(c.PIngredient)));
        selected.ForEach(i => PantryIngredients.Remove(i));
        RefreshSelected();
    }
}
