using RecipeApp.Models;
using RecipeApp.Services;

namespace RecipeApp.Controls.Pages;


public sealed partial class GroceryListPage : NavigatorPage
{
    [ObservableProperty] private partial ObservableCollection<IngredientCard> AllIngredients { get; set; } 
    [ObservableProperty] private partial ObservableCollection<IngredientCard> DairyIngredients { get; set; }
    [ObservableProperty] private partial ObservableCollection<IngredientCard> ProduceIngredients { get; set; }
    [ObservableProperty] private partial ObservableCollection<IngredientCard> MeatAndSeafoodIngredients { get; set; }
    [ObservableProperty] private partial ObservableCollection<IngredientCard> BakingIngredients { get; set; }
    [ObservableProperty] private partial ObservableCollection<IngredientCard> OtherIngredients { get; set; }
    
    public ObservableCollection<string> CategoryOptions { get; } =
    [
        "All Categories", "Vegetables", "Fruits", "Dairy", "Meat", "Seafood",
        "Baking", "Beverages", "Snacks", "Others", "Uncategorized"
    ];
    
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
        
        DairyIngredients = (await RecipeIngredient.GetAll())
            .Where(i => i.Category == "Dairy")
            .Select(i => new IngredientCard
            {
                Ingredient = i,
                IsSelected = false,
                PIngredient = null
            })
            .ForEach(i => AllIngredients.Add(i))
            .ToObservableCollection();
        
        ProduceIngredients = (await RecipeIngredient.GetAll())
            .Where(i => i.Category == "Fruits" || i.Category == "Vegetables")
            .Select(i => new IngredientCard
            {
                Ingredient = i,
                IsSelected = false,
                PIngredient = null
            })
            .ForEach(i => AllIngredients.Add(i))
            .ToObservableCollection();
        
        MeatAndSeafoodIngredients = (await RecipeIngredient.GetAll())
            .Where(i => i.Category == "Meat" || i.Category == "Seafood")
            .Select(i => new IngredientCard
            {
                Ingredient = i,
                IsSelected = false,
                PIngredient = null
            })
            .ForEach(i => AllIngredients.Add(i))
            .ToObservableCollection();
        
        BakingIngredients = (await RecipeIngredient.GetAll())
            .Where(i => i.Category == "Baking")
            .Select(i => new IngredientCard
            {
                Ingredient = i,
                IsSelected = false,
                PIngredient = null
            })
            .ForEach(i => AllIngredients.Add(i))
            .ToObservableCollection();
        
        OtherIngredients = (await RecipeIngredient.GetAll())
            .Where(i => i.Category == "Beverages" || i.Category == "Snacks" || i.Category == "Others" || i.Category == "Uncategorized")
            .Select(i => new IngredientCard
            {
                Ingredient = i,
                IsSelected = false,
                PIngredient = null
            })
            .ForEach(i => AllIngredients.Add(i))
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

        foreach (var card in selected.ToList())
        {
            var ingredient = card.Ingredient;

            var pantryItem = new PantryIngredient
            {
                Name = ingredient.Name ?? "New Item",
                Quantity = ingredient.Quantity,
                Unit = ingredient.Unit,
                ModifierNote = ingredient.ModifierNote,
                Category = ingredient.Category
            };

            await PantryIngredient.Add(pantryItem);
            await RecipeIngredient.Remove(ingredient);
            AllIngredients.Remove(card);
        }

        RefreshSelected();
    }
}
