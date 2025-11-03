namespace RecipeApp.Models;

public partial class IngredientCard : ObservableObject
{
    [ObservableProperty] public required partial PantryIngredient PIngredient { get; set; }
    [ObservableProperty] public required partial RecipeIngredient Ingredient { get; set; }
    [ObservableProperty] public required partial bool IsSelected { get; set; } = false;
}
