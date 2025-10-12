namespace RecipeApp.Models;

public interface IRecipe
{
    string Title { get; }
    string? Category { get; }
    string? ImageUrl { get; }
    double Rating { get; }
    string? Description { get; }
}