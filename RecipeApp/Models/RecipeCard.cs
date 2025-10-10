namespace RecipeApp.Models;

public partial class RecipeCard : ObservableObject
{
    [ObservableProperty] public required partial SavedRecipe SavedRecipe { get; set; }
    [ObservableProperty] public required partial bool IsSelected { get; set; } = false;
}
