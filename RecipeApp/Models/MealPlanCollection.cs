using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using RecipeApp.Services;

namespace RecipeApp.Models;

public class MealPlanCollection : IAutosavingClass<MealPlanCollection>
{
    [JsonPropertyName("plans")]
    public ObservableCollection<MealPlan> MealPlans { get; set; } = new();

    public MealPlanCollection() { }

    public MealPlan? GetMealPlan(DateTime date, MealType mealType)
    {
        return MealPlans.FirstOrDefault(mp => mp.Date.Date == date.Date && mp.MealType == mealType);
    }

    public static async Task AddMealPlan(DateTime date, IRecipe recipe, MealType mealType)
    {
        var allMealPlans = await GetAll();
        var collection = allMealPlans.FirstOrDefault() ?? new MealPlanCollection();
        
        // If no collection exists yet, add this new one
        if (!allMealPlans.Any())
        {
            await Add(collection);
        }
        
        // Remove any existing meal plan for this date and meal type
        var existingMealPlan = collection.GetMealPlan(date, mealType);
        if (existingMealPlan != null)
        {
            collection.MealPlans.Remove(existingMealPlan);
        }

        // Add the new meal plan
        collection.MealPlans.Add(new MealPlan 
        {
            Date = date,
            MealType = mealType,
            Recipe = recipe
        });

        // Save changes
        await SaveAll();
    }
}