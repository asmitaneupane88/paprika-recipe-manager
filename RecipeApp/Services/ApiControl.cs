using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using RecipeApp.Models;

namespace RecipeApp.Services
{
    public class ApiControl
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public async Task<List<Recipe>> SearchRecipesAsync(string query)
        {
            try
            {
                string url = $"https://www.themealdb.com/api/json/v1/1/search.php?s={Uri.EscapeDataString(query)}";
                string json = await _httpClient.GetStringAsync(url);
                var response = JsonSerializer.Deserialize<ApiResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                var recipes = new List<Recipe>();
                foreach (var meal in response?.Meals ?? new List<Recipe>())
                {
                    var recipe = new Recipe
                    {
                        Name = meal.strMeal ?? "Unknown",
                        ImageUrl = meal.strMealThumb ?? string.Empty,
                        Instructions = meal.strInstructions ?? string.Empty,
                        Ingredients = GetIngredientsFromMeal(meal)
                    };
                    recipes.Add(recipe);
                }

                return recipes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error: {ex.Message}");
                return GetDummyRecipes();
            }
        }

        private string GetIngredientsFromMeal(Recipe meal)
        {
            var ingredients = new List<string>();
            for (int i = 1; i <= 2; i++)
            {
                var ingredient = typeof(Recipe).GetProperty($"strIngredient{i}")?.GetValue(meal) as string;
                var measure = typeof(Recipe).GetProperty($"strMeasure{i}")?.GetValue(meal) as string;
                if (!string.IsNullOrEmpty(ingredient))
                {
                    ingredients.Add($"{measure?.Trim() ?? ""} {ingredient.Trim()}");
                }
            }
            return string.Join(", ", ingredients);
        }

        private List<Recipe> GetDummyRecipes()
        {
            return new List<Recipe>
            {
                new Recipe
                {
                    Name = "Dummy Pasta",
                    Ingredients = "Pasta, Sauce, Cheese",
                    Instructions = "Boil pasta, add sauce.",
                    ImageUrl = "https://www.themealdb.com/images/media/meals/wxywrq1468235067.jpg"
                },
                new Recipe
                {
                    Name = "Dummy Salad",
                    Ingredients = "Lettuce, Tomato, Dressing",
                    Instructions = "Chop and toss.",
                    ImageUrl = "https://www.themealdb.com/images/media/meals/abc123.jpg"
                }
            };
        }
    }
}
