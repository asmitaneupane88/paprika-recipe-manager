using RecipeApp.Models;

namespace RecipeApp.Services;

public class RecipeService
{
    private static readonly List<Recipe> _recipes = new();

    public static List<Recipe> GetRecipes()
    {
        if (_recipes.Count == 0)
        {
            // Add sample recipe from the screenshot
            var carrotCake = new Recipe
            {
                Title = "Crazy Good Carrot Cake",
                Category = "Cakes, Desserts",
                Author = "Grandma",
                PrepTimeMinutes = 20,
                CookTimeMinutes = 50,
                TotalTimeMinutes = 190, // 3 hr 10 min
                Servings = 12,
                Difficulty = "Medium",
                Rating = 5.0,
                Ingredients = new()
                {
                    new Ingredient { Name = "unsalted butter", Notes = "for the pan" },
                    new Ingredient { Amount = "12", Unit = "ounces", Name = "all-purpose flour", Notes = "approximately 2 1/2 cups, plus extra for pan" },
                    new Ingredient { Amount = "12", Unit = "ounces", Name = "grated carrots", Notes = "medium grate, approximately 6 medium" },
                    new Ingredient { Amount = "1/4", Unit = "teaspoon", Name = "ground allspice" },
                    new Ingredient { Amount = "1/4", Unit = "teaspoon", Name = "ground cinnamon" },
                    new Ingredient { Amount = "1/2", Unit = "teaspoon", Name = "salt" },
                    new Ingredient { Amount = "10", Unit = "ounces", Name = "sugar", Notes = "approximately 1 1/3 cups" },
                    new Ingredient { Amount = "2", Unit = "ounces", Name = "dark brown sugar", Notes = "approximately 1/4 cup firmly packed" },
                    new Ingredient { Amount = "3", Unit = "", Name = "large eggs" },
                    new Ingredient { Amount = "6", Unit = "ounces", Name = "plain yogurt" },
                    new Ingredient { Amount = "6", Unit = "ounces", Name = "vegetable oil" }
                },
                Directions = new()
                {
                    "Preheat oven to 350 degrees F.",
                    "Butter and flour a 9x3\" cake pan. Line the bottom with parchment paper. Set aside.",
                    "Put the grated carrots into a large mixing bowl and set aside.",
                    "Put the flour, baking powder, baking soda, spices, and salt into a food processor and process for 5 seconds. Add this mixture to the carrots and toss.",
                    "In the bowl of the food processor combine the sugar, brown sugar, eggs, and yogurt.",
                    "With the processor still running gradually pour in the vegetable oil. Add this mixture to the carrot mixture and stir only until combined. Pour into the cake pan and bake on the middle rack of the oven for 40 minutes. Reduce the heat to 325 degrees F and bake for another 20 minutes or until the cake reaches about 200 degrees F in the center.",
                    "Remove the pan from the oven and allow cake to cool completely. After 15 minutes, turn the cake out onto a rack and allow cake to cool completely. Frost with cream cheese frosting after cake has cooled completely."
                }
            };

            _recipes.Add(carrotCake);
        }

        return _recipes;
    }

    public static Recipe? GetRecipeByTitle(string title)
    {
        return GetRecipes().FirstOrDefault(r => r.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
    }
}