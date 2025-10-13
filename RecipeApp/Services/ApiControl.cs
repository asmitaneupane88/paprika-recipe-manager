using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RecipeApp.Models;

namespace RecipeApp.Services
{
    public class ApiControl : IRecipeService
    {
        private readonly HttpClient _httpClient = new();

        public async Task<IReadOnlyList<MealDbRecipe>> SearchAsync(string query, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(query))
                return Array.Empty<MealDbRecipe>();

            try
            {
                string url = $"https://www.themealdb.com/api/json/v1/1/search.php?s={Uri.EscapeDataString(query)}";
                string json =  await _httpClient.GetStringAsync(url, ct);
                
                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                var recipes = new List<MealDbRecipe>();

                foreach (var meal in apiResponse?.Meals ?? [])
                {
                    recipes.Add(meal);
                }
                return recipes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error: {ex.Message}");
                return GetDummyRecipes();
            }
        }
        private string GetIngredientsFromMeal(MealDbRecipe meal)
        {
            var ingredients = new List<string>();
            for (int i = 1; i <= 2; i++)
            {
                var ingredient = typeof(MealDbRecipe).GetProperty($"strIngredient{i}")?.GetValue(meal) as string;
                var measure = typeof(MealDbRecipe).GetProperty($"strMeasure{i}")?.GetValue(meal) as string;
                if (!string.IsNullOrEmpty(ingredient))
                {
                    ingredients.Add($"{measure?.Trim() ?? ""} {ingredient.Trim()}".Trim());
                }
            }
            return string.Join(", ", ingredients);
        }

        private List<MealDbRecipe> GetDummyRecipes() =>
        [
            new()
            {
                strMeal = "Dummy Pasta",
                strIngredient1 = "Pasta",
                strIngredient2 = "Sauce",
                strIngredient3 = "Cheese",
                strMeasure1 = "500g",
                strMeasure2 = "200ml",
                strMeasure3 = "100g",
                strInstructions = "Boil pasta, add sauce.",
                strMealThumb = "https://www.themealdb.com/images/media/meals/wxywrq1468235067.jpg"
            },

            new()
            {
                strMeal = "Dummy Salad",
                strIngredient1 = "Lettuce",
                strIngredient2 = "Tomato",
                strIngredient3 = "Dressing",
                strMeasure1 = "1 head",
                strMeasure2 = "2 medium",
                strMeasure3 = "2 tbsp",
                strInstructions = "Chop and toss.",
                strMealThumb = "https://www.themealdb.com/images/media/meals/abc123.jpg"
            }
        ];
    }
}

