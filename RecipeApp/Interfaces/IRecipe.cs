namespace RecipeApp.Models;

public interface IRecipe
{
    string Title { get; }
    string? Category { get; }
    string? ImageUrl { get; }
    int Rating { get; }
    string? Description { get; }
}
