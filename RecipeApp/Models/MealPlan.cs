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
        
        // Remove any existing meal plan for this date and meal type
        var existingMealPlan = allMealPlans.FirstOrDefault(mp => mp.Date.Date == date.Date && mp.MealType == mealType);
        if (existingMealPlan != null)
        {
            await Remove(existingMealPlan);
        }

        await Add(new MealPlan 
        {
            Date = date,
            MealType = mealType,
            Recipe = recipe
        });
    }
}
