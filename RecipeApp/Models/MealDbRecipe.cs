﻿using System.Collections.Generic;

namespace RecipeApp.Models
{
    public class MealDbRecipe
    {
        public string? Id { get; set; }
        public string? idMeal { get; set; }
        public string? strMeal { get; set; }
        public string? strCategory { get; set; }
        public string? strArea { get; set; }
        public string? strInstructions { get; set; }
        public string? strMealThumb { get; set; }
        public string? strTags { get; set; }
        public string? strYoutube { get; set; }
        
        // MealDB has up to 20 ingredients and measures
        public string? strIngredient1 { get; set; }
        public string? strIngredient2 { get; set; }
        public string? strIngredient3 { get; set; }
        public string? strIngredient4 { get; set; }
        public string? strIngredient5 { get; set; }
        public string? strIngredient6 { get; set; }
        public string? strIngredient7 { get; set; }
        public string? strIngredient8 { get; set; }
        public string? strIngredient9 { get; set; }
        public string? strIngredient10 { get; set; }
        public string? strIngredient11 { get; set; }
        public string? strIngredient12 { get; set; }
        public string? strIngredient13 { get; set; }
        public string? strIngredient14 { get; set; }
        public string? strIngredient15 { get; set; }
        public string? strIngredient16 { get; set; }
        public string? strIngredient17 { get; set; }
        public string? strIngredient18 { get; set; }
        public string? strIngredient19 { get; set; }
        public string? strIngredient20 { get; set; }

        public string? strMeasure1 { get; set; }
        public string? strMeasure2 { get; set; }
        public string? strMeasure3 { get; set; }
        public string? strMeasure4 { get; set; }
        public string? strMeasure5 { get; set; }
        public string? strMeasure6 { get; set; }
        public string? strMeasure7 { get; set; }
        public string? strMeasure8 { get; set; }
        public string? strMeasure9 { get; set; }
        public string? strMeasure10 { get; set; }
        public string? strMeasure11 { get; set; }
        public string? strMeasure12 { get; set; }
        public string? strMeasure13 { get; set; }
        public string? strMeasure14 { get; set; }
        public string? strMeasure15 { get; set; }
        public string? strMeasure16 { get; set; }
        public string? strMeasure17 { get; set; }
        public string? strMeasure18 { get; set; }
        public string? strMeasure19 { get; set; }
        public string? strMeasure20 { get; set; }

        // For backward compatibility with existing code
        public string Name 
        { 
            get => strMeal ?? string.Empty;
            set => strMeal = value;
        }
        
        public string Category
        {
            get => strCategory ?? string.Empty;
            set => strCategory = value;
        }
        
        public string Instructions
        {
            get => strInstructions ?? string.Empty;
            set => strInstructions = value;
        }
        
        public string ImageUrl
        {
            get => strMealThumb ?? string.Empty;
            set => strMealThumb = value;
        }
        
        public string IngredientsPreview
        {
            get
            {
                var ingredients = new List<string>();
                if (!string.IsNullOrWhiteSpace(strIngredient1)) ingredients.Add($"{strMeasure1} {strIngredient1}");
                if (!string.IsNullOrWhiteSpace(strIngredient2)) ingredients.Add($"{strMeasure2} {strIngredient2}");
                if (!string.IsNullOrWhiteSpace(strIngredient3)) ingredients.Add($"{strMeasure3} {strIngredient3}");
                return string.Join(", ", ingredients);
            }
        }
    }
    public class ApiResponse
    {
        public List<MealDbRecipe> Meals { get; set; } = [];
    }
}
