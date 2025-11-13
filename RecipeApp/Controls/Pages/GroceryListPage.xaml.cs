using RecipeApp.Models;
using RecipeApp.Services;

namespace RecipeApp.Controls.Pages;


public sealed partial class GroceryListPage : NavigatorPage
{
    [ObservableProperty] private partial ObservableCollection<IngredientCard> AllIngredients { get; set; } = [];
    public GroceryListPage(Navigator? nav) : base(nav)
    {
        this.InitializeComponent();

        _ = ShowIngredients();
    }

    private async Task ShowIngredients()
    {
        AllIngredients = (await RecipeIngredient.GetAll())
            .Select(i => new IngredientCard
            {
                Ingredient = i,
                IsSelected = false,
                PIngredient = null
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
        return AllIngredients
            .Where(c => c.IsSelected)
            .ToList();
    }

    private async void ButtonAddIngredient_OnClick(object sender, RoutedEventArgs e)
    {
        var newIngredient = new RecipeIngredient();
        
        AllIngredients.Add(new IngredientCard
        {
            Ingredient = newIngredient,
            IsSelected = false,
            PIngredient = null
        });
        await RecipeIngredient.Add(newIngredient);
    }

    private async void ButtonRemoveIngredient_OnClick(object sender, RoutedEventArgs e)
    {
        var selected = GetSelectedIngredientCards();
        
        await Task.WhenAll(selected.Select(c => RecipeIngredient.Remove(c.Ingredient)));
        selected.ForEach(i => AllIngredients.Remove(i));
        RefreshSelected();
    }
    
    private async void ButtonPurchased_OnClick(object sender, RoutedEventArgs e)
    {
        var selected = GetSelectedIngredientCards();

        if (selected.Count == 0)
            return;
        
        var pantryItems = await PantryIngredient.GetAll();

        foreach (var card in selected.ToList())
        {
            var ingredient = card.Ingredient;
            
            if (ingredient == null)
                continue;
            
            var existing = pantryItems.FirstOrDefault(p =>
                string.Equals(p.Name?.Trim(), ingredient.Name?.Trim(),
                    StringComparison.OrdinalIgnoreCase));
            
            if (existing != null)
            {
                existing.Quantity += ingredient.Quantity;
                await PantryIngredient.Remove(existing);
                await PantryIngredient.Add(existing);
            }
            else
            {

                var pantryItem = new PantryIngredient
                {
                    Name = ingredient.Name ?? "New Item",
                    Quantity = ingredient.Quantity,
                    Unit = ingredient.Unit,
                    ModifierNote = ingredient.ModifierNote,
                    Category = "Uncategorized"
                };

                await PantryIngredient.Add(pantryItem);
            }

            await RecipeIngredient.Remove(ingredient);
            AllIngredients.Remove(card);
        }

        RefreshSelected();
    }
}
