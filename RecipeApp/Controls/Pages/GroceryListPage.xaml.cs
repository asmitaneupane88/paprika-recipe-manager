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
                    Category = CategoryHelper.AutoDetectCategory(ingredient.Name)
                };

                await PantryIngredient.Add(pantryItem);
            }

            await RecipeIngredient.Remove(ingredient);
            AllIngredients.Remove(card);
        }

        RefreshSelected();
    }

    private async void ButtonAddRecipeIngredients_OnClick(object sender, RoutedEventArgs e)
    {
        var search = new SavedRecipeSearch();
        
        var dialog = new ContentDialog
        {
            Title = "Add Recipe",
            Content = search,
            PrimaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.XamlRoot,
        };
        
        search.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(search.SelectedRecipes))
            {
                dialog.PrimaryButtonText = search.SelectedRecipes.Count == 0 ? "Cancel" : $"Add {search.SelectedRecipes.Count} Recipes";
            }
        };
        
        await dialog.ShowAsync();

        foreach (var rc in search.SelectedRecipes.Select(r => r.SavedRecipe).Where(r => r is not null))
        {
            var pathInfos = rc.RootStepNode!.GetNestedPathInfo();

            if (pathInfos.Count == 0)
            {
                var dialog2 = new ContentDialog
                {
                    Title = $"Error - {rc.Title}",
                    Content =
                        "This recipe does not have any paths and therefore does not have any ingredients to buy.",
                    PrimaryButtonText = "Ok",
                    XamlRoot = this.XamlRoot,
                };

                await dialog2.ShowAsync();
                return;
            }

            ObservableCollection<RecipeIngredient> ingredients = [];
            if (pathInfos.Count == 1)
                ingredients = pathInfos[0].MaxIngredients;
            else
            {
                // show popup to get a path:
                // "What do you want to buy for? 'Oven' with these ingredients, 'Stove' with these ingredients."
                // can use savedRecipe.RootStepNode.BindableDescription to show these ingredients

                // set ingredients
                var stack = new StackPanel();
                
                var dialog3 = new ContentDialog
                {
                    Title = $"Select a path to add ingredients for",
                    PrimaryButtonText = "Cancel",
                    XamlRoot = this.XamlRoot,
                };

                var textbox = new TextBlock()
                {
                    TextWrapping = TextWrapping.Wrap,
                    Text = rc.RootStepNode.BindableDescription
                };
                
                stack.Children.Add(textbox);
                
                foreach (var pathInfo in pathInfos)
                {
                    var button = new Button
                    {
                        Content = pathInfo.OutNode.Title,
                    };
                    
                    button.Click += (_, _) =>
                    {
                        ingredients = pathInfo.MaxIngredients;
                        dialog3.Hide();
                    };
                    
                    stack.Children.Add(button);
                }
                
                dialog3.Content = stack;
                
                await dialog3.ShowAsync();
            }

            // TODO maybe selecting which ingredients to buy or checking what is in the pantry already.

            List<Task> tasks = [];
            
            foreach (var ingredient in ingredients)
            {
                tasks.Add(RecipeIngredient.Add(ingredient));
            }

            await Task.WhenAll(tasks);
            await ShowIngredients();
        }
    }
}
