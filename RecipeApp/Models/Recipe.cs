using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace RecipeApp.Models;

public class Recipe
{
    public required string Title { get; set; }
    public string? Category { get; set; }
    public string? Author { get; set; }
    public required int PrepTimeMinutes { get; set; }
    public required int CookTimeMinutes { get; set; }
    public required int TotalTimeMinutes { get; set; }
    public required int Servings { get; set; }
    public required string Difficulty { get; set; }
    public required ObservableCollection<Ingredient> Ingredients { get; init; } = new();
    public required ObservableCollection<string> Directions { get; init; } = new();
    public double Rating { get; set; }
}

public class Ingredient
{
    public string? Amount { get; set; }
    public string? Unit { get; set; }
    public required string Name { get; set; }
    public string? Notes { get; set; }
}