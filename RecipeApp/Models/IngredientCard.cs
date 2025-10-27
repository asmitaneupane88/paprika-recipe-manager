namespace RecipeApp.Models;

/// <summary>
/// For use by the <see cref="RecipeListPage"/> to display recipes and allow multi-selecting recipes.
/// </summary>
public partial class IngredientCard : ObservableObject
{
    [ObservableProperty] public required partial RecipeIngredient Ingredient { get; set; }
    [ObservableProperty] public required partial bool IsSelected { get; set; } = false;
}
