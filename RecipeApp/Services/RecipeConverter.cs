using System.Collections.ObjectModel;

namespace RecipeApp.Services;

public static class RecipeConverter
{
    public static Recipe ToRecipe(this MealDbRecipe mealDbRecipe)
    {
        var ingredients = new ObservableCollection<Ingredient>();
        var directions = new ObservableCollection<string>();

        // Parse ingredients from MealDB format
        for (int i = 1; i <= 20; i++) // MealDB has up to 20 ingredients
        {
            var ingredient = typeof(MealDbRecipe).GetProperty($"strIngredient{i}")?.GetValue(mealDbRecipe) as string;
            var measure = typeof(MealDbRecipe).GetProperty($"strMeasure{i}")?.GetValue(mealDbRecipe) as string;
            
            if (!string.IsNullOrWhiteSpace(ingredient))
            {
                ingredients.Add(new Ingredient
                {
                    Name = ingredient.Trim(),
                    Amount = measure?.Trim(),
                });
            }
        }

        // Split instructions into steps
        var instructionSteps = mealDbRecipe.Instructions
            .Split(new[] { '.', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();

        foreach (var step in instructionSteps)
        {
            directions.Add(step);
        }

        return new Recipe
        {
            Title = mealDbRecipe.Name,
            Category = mealDbRecipe.Category,
            PrepTimeMinutes = 15, // Default values since MealDB doesn't provide these
            CookTimeMinutes = 30,
            Servings = 4,
            Difficulty = "Medium",
            ImageUrl = mealDbRecipe.ImageUrl,
            Ingredients = ingredients,
            Directions = directions,
            Source = "MealDB",
            MealDbId = mealDbRecipe.Id
        };
    }
}