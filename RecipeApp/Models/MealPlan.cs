using System;
using System.Collections.ObjectModel;

namespace RecipeApp.Models;

public enum MealType
{
    Breakfast,
    Lunch,
    Dinner
}

public class MealPlan
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public required DateTime Date { get; init; }
    public required IRecipe Recipe { get; init; }
    public required MealType MealType { get; init; }
}

public class MealPlanCollection
{
    public ObservableCollection<MealPlan> MealPlans { get; } = new();

    public void AddMealPlan(DateTime date, IRecipe recipe, MealType mealType)
    {
        // Ensure we're comparing exact dates, not just days of the week
        var exactDate = date.Date; // Normalize to start of day
        var existingPlan = MealPlans.FirstOrDefault(mp => 
            mp.Date.Date.Equals(exactDate) && 
            mp.MealType == mealType);
        
        if (existingPlan != null)
        {
            MealPlans.Remove(existingPlan);
        }

        // Add the new meal plan
        MealPlans.Add(new MealPlan
        {
            Date = date.Date,
            Recipe = recipe,
            MealType = mealType
        });
    }

    public MealPlan? GetMealPlan(DateTime date, MealType mealType)
    {
        // Ensure we're comparing exact dates, not just days of the week
        var exactDate = date.Date; // Normalize to start of day
        return MealPlans.FirstOrDefault(mp => 
            mp.Date.Date.Equals(exactDate) && 
            mp.MealType == mealType);
    }
}