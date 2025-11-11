using System;
using System.Text.Json.Serialization;

namespace RecipeApp.Models;

public enum MealType
{
    Breakfast,
    Lunch,
    Dinner
}

public class MealPlan : IAutosavingClass<MealPlan>
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required DateTime Date { get; init; }
    public required SavedRecipe Recipe { get; init; }
    public required MealType MealType { get; init; }
    
    public static async Task AddMealPlan(DateTime date, SavedRecipe recipe, MealType mealType)
    {
        var allMealPlans = await GetAll();
        // Add a meal plan entry. We allow multiple entries per date+mealType so a meal slot can contain multiple recipes.
        await Add(new MealPlan
        {
            Date = date,
            MealType = mealType,
            Recipe = recipe
        });
    }
}