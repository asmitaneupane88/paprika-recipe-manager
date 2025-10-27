using System;
using System.Text.Json.Serialization;

namespace RecipeApp.Models;

public enum MealType
{
    Breakfast,
    Lunch,
    Dinner
}

public class MealPlan
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = Guid.NewGuid().ToString();

    [JsonPropertyName("date")]
    public required DateTime Date { get; init; }

    [JsonPropertyName("recipe")]
    public required IRecipe Recipe { get; init; }

    [JsonPropertyName("mealType")]
    public required MealType MealType { get; init; }
}