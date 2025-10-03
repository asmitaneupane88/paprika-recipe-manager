using System.Collections.Generic;

namespace RecipeApp.Models
{
    public class Recipe
    {
        public string Name { get; set; } = string.Empty;
        public string Ingredients { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;

        public string strMeal { get; set; } = string.Empty;
        public string strMealThumb { get; set; } = string.Empty;
        public string strInstructions { get; set; } = string.Empty;
        public string strIngredient1 { get; set; } = string.Empty;
        public string strMeasure1 { get; set; } = string.Empty;
        public string strIngredient2 { get; set; } = string.Empty;
        public string strMeasure2 { get; set; } = string.Empty;
    }

    public class ApiResponse
    {
        public List<Recipe> Meals { get; set; } = new List<Recipe>();
    }
}
