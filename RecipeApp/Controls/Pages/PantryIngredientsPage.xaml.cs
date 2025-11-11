

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
        PantryIngredients = (await RecipeIngredient.GetAll())
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
        CardsSelected = GroupedPantry.SelectMany(g => g).Any(c => c.IsSelected);
    }
    
    private List<IngredientCard> GetSelectedIngredientCards()
    {
        return PantryIngredients
            .Where(c => c.IsSelected)
            .ToList();
    }

    private async void ButtonAddIngredient_OnClick(object sender, RoutedEventArgs e)
    {
        var newIngredient = new RecipeIngredient();
        
        PantryIngredients.Add(new IngredientCard() { Ingredient = newIngredient, IsSelected = false });
        await RecipeIngredient.Add(newIngredient);
    }

    private async void ButtonRemoveIngredient_OnClick(object sender, RoutedEventArgs e)
    {
        var selected = GetSelectedIngredientCards();
        
        await Task.WhenAll(selected.Select(c => RecipeIngredient.Remove(c.Ingredient)));
        selected.ForEach(i => PantryIngredients.Remove(i));
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
            
