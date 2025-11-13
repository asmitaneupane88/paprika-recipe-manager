using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml;
using RecipeApp.Models;
using RecipeApp.Services;

namespace RecipeApp.Controls.Pages;


public sealed partial class PantryIngredientsPage : NavigatorPage
{
    [ObservableProperty]
    private partial ObservableCollection<IngredientCard> PantryIngredients { get; set; } = new();
    
    [ObservableProperty]
    private partial bool CardsSelected { get; set; }

    public ObservableCollection<Grouping<string, IngredientCard>> GroupedPantry { get; set; } = new();
    
    public ObservableCollection<string> CategoryOptions { get; } =
    [
        "All Categories", "Vegetables", "Fruits", "Dairy", "Meat", "Seafood",
        "Baking", "Beverages", "Snacks", "Others", "Uncategorized"
    ];
    
    private string AutoDetectCategory(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Uncategorized";

        string lower = name.ToLower();

        if (lower.Contains("milk") || lower.Contains("cheese") || lower.Contains("yogurt"))
            return "Dairy";

        if (lower.Contains("chicken") || lower.Contains("beef") || lower.Contains("pork"))
            return "Meat";

        if (lower.Contains("apple") || lower.Contains("banana") || lower.Contains("orange"))
            return "Fruits";

        if (lower.Contains("broccoli") || lower.Contains("onion") || lower.Contains("carrot"))
            return "Vegetables";

        if (lower.Contains("shrimp") || lower.Contains("fish"))
            return "Seafood";

        if (lower.Contains("flour") || lower.Contains("sugar"))
            return "Baking";

        return "Uncategorized";
    }

    
    public PantryIngredientsPage(Navigator? nav) : base(nav)
    {
        this.InitializeComponent();
        
        PantryIngredients.CollectionChanged += (s, e) =>
        {
            if (e.NewItems != null)
            {
                foreach (IngredientCard card in e.NewItems)
                {
                    // When user types, auto-set category
                    card.PIngredient.PropertyChanged += (sender, args) =>
                    {
                        if (args.PropertyName == nameof(PantryIngredient.Name))
                        {
                            var ing = (PantryIngredient)sender;
                            ing.Category = AutoDetectCategory(ing.Name);
                        }
                    };
                }
            }
        };

        _ = ShowIngredients();
    }
    
    private async Task ShowIngredients()
    {
        var allItems = (await PantryIngredient.GetAll())
            .Select(i => new IngredientCard
            {
                Ingredient = null,
                PIngredient = i,
                IsSelected = false
                
            })
            .OrderBy(c => c.PIngredient.Category)
            .ThenBy(c => c.PIngredient.Name)
            .ToList();

        var grouped = allItems
            .GroupBy(c => string.IsNullOrWhiteSpace(c.PIngredient.Category)
                ? "Uncategorized"
                : c.PIngredient.Category)
            .Select(g => new Grouping<string, IngredientCard>(g.Key, g))
            .ToList();

        GroupedPantry.Clear();
        foreach (var group in grouped)
            GroupedPantry.Add(group);
    }
    
    private void OnRecipeCardChecked(object sender, RoutedEventArgs e)
    {
        RefreshSelected();
    }
    
    private void RefreshSelected()
    {
        CardsSelected = GroupedPantry.SelectMany(g => g).Any(c => c.IsSelected);
    }
    
    private List<IngredientCard> GetSelectedIngredientCards()
    {
        return GroupedPantry.SelectMany(g => g).Where(c => c.IsSelected).ToList();
    }

    private async void ButtonAddIngredient_OnClick(object sender, RoutedEventArgs e)
    {
        var newIngredient = new PantryIngredient
        {
            Name = string.Empty,
            Category = "Uncategorized"
        };
        
        await PantryIngredient.Add(newIngredient);

        var card = new IngredientCard
        {
            Ingredient = null!,
            PIngredient = newIngredient,
            IsSelected = false
        };
        var group = GroupedPantry.FirstOrDefault(g => g.Key == newIngredient.Category);
        if (group == null)
        {
            group = new Grouping<string, IngredientCard>(newIngredient.Category, Enumerable.Empty<IngredientCard>());
            GroupedPantry.Add(group);
        }

        group.Add(card);
        RefreshSelected();
    }

    private async void ButtonRemoveIngredient_OnClick(object sender, RoutedEventArgs e)
    {
        var selected = GetSelectedIngredientCards();
        if (selected.Count == 0) return;
        
        foreach (var card in selected)
        {
            await PantryIngredient.Remove(card.PIngredient);
            var group = GroupedPantry.FirstOrDefault(g => g.Contains(card));
            group?.Remove(card);
        }
        foreach (var empty in GroupedPantry.Where(g => g.Count == 0).ToList())
            GroupedPantry.Remove(empty);

        RefreshSelected();
    }
      
    private async void ButtonRestock_OnClick(object sender, RoutedEventArgs e)
    {
        var selected = GetSelectedIngredientCards();
        if (selected.Count == 0) return;

        foreach (var card in selected.ToList())
        { 
            var pantry = card.PIngredient;

            var groceryItem = new RecipeIngredient
            {
                Name = pantry.Name,
                Quantity = pantry.Quantity,
                Unit = pantry.Unit,
                ModifierNote = pantry.ModifierNote,
                ScaleFactor = pantry.ScaleFactor
                };

         
            await RecipeIngredient.Add(groceryItem);
            await PantryIngredient.Remove(pantry);
            var group = GroupedPantry.FirstOrDefault(g => g.Contains(card));
            group?.Remove(card);
        }
        foreach (var empty in GroupedPantry.Where(g => g.Count == 0).ToList())
            GroupedPantry.Remove(empty);

        RefreshSelected();
    }
    private async void CategoryFilterBox_SelectionChanged(object sender, Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs e)
    {
        var selectedCategory = CategoryFilterBox.SelectedItem as string;
        if (string.IsNullOrEmpty(selectedCategory) || selectedCategory == "All Categories")
        {
            await ShowIngredients();
        }
        else
        {
            var allItems = (await PantryIngredient.GetAll())
                .Select(i => new IngredientCard
                {
                    PIngredient = i,
                    IsSelected = false,
                    Ingredient = null
                })
                .Where(c => c.PIngredient.Category == selectedCategory)
                .OrderBy(c => c.PIngredient.Name)
                .ToList();

            var grouped = allItems
                .GroupBy(c => c.PIngredient.Category)
                .Select(g => new Grouping<string, IngredientCard>(g.Key, g))
                .ToList();

            GroupedPantry.Clear();
            foreach (var group in grouped)
                GroupedPantry.Add(group);
        }
    }

    // ✅ Grouping helper
    public class Grouping<TKey, TItem> : ObservableCollection<TItem>
    {
        public TKey Key { get; }

        public Grouping(TKey key, IEnumerable<TItem> items) : base(items)
        {
            Key = key;
        }
    }
}
            
