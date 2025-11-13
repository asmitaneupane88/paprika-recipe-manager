using System;
using System.Linq;
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
    /// <summary>
    /// When true, this meal was marked as a leftover. Leftover meals are visually distinct
    /// and will automatically be added to the next day when marked.
    /// </summary>
    public bool IsLeftOver { get; set; } = false;
    
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

    /// <summary>
    /// Adds a meal plan entry and optionally marks it as a leftover.
    /// </summary>
    public static async Task AddMealPlan(DateTime date, SavedRecipe recipe, MealType mealType, bool isLeftOver)
    {
        await Add(new MealPlan
        {
            Date = date,
            MealType = mealType,
            Recipe = recipe,
            IsLeftOver = isLeftOver
        });
    }

    /// <summary>
    /// Toggle or set the leftover state on an existing MealPlan. When setting to true,
    /// this will attempt to add the same recipe to the next day for the same meal type.
    /// </summary>
    public static async Task SetLeftOver(MealPlan plan, bool isLeftOver)
    {
        if (plan == null) return;
        var all = (await GetAll()).ToList();
        var existing = all.FirstOrDefault(p => p.Id == plan.Id);
        if (existing == null) return;

        existing.IsLeftOver = isLeftOver;
        // Persist the change
        await SaveAll();

        if (isLeftOver)
        {
            try
            {
                var nextDate = existing.Date.AddDays(1);
                // Avoid adding a duplicate for the same date/mealType/recipe
                var current = (await GetAll()).Where(p => p.Date.Date == nextDate.Date && p.MealType == existing.MealType && string.Equals(p.Recipe?.Title, existing.Recipe?.Title, StringComparison.OrdinalIgnoreCase)).ToList();
                if (!current.Any())
                {
                    await AddMealPlan(nextDate, existing.Recipe, existing.MealType, true);
                }
            }
            catch
            {
                // ignore failures when auto-adding
            }
        }
    }
}