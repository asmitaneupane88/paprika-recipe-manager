using System.Collections.ObjectModel;
using RecipeApp.Models;
using RecipeApp.Models.RecipeSteps;

namespace RecipeApp.Services
{

    public static class RecipeConverter
    {
        public static SavedRecipe ToSavedRecipe(this MealDbRecipe mealDbRecipe)
        {
            var ingredients = new ObservableCollection<RecipeIngredient>();

            // Parse ingredients from MealDB format
            for (int i = 1; i <= 20; i++) // MealDB has up to 20 ingredients
            {
                var ingredient =
                    typeof(MealDbRecipe).GetProperty($"strIngredient{i}")?.GetValue(mealDbRecipe) as string;
                var measure = typeof(MealDbRecipe).GetProperty($"strMeasure{i}")?.GetValue(mealDbRecipe) as string;

                if (!string.IsNullOrWhiteSpace(ingredient))
                {
                    ingredients.Add(new RecipeIngredient
                    {
                        Name = ingredient.Trim(),
                        ModifierNote = measure?.Trim() ?? string.Empty,
                        Quantity = 1,
                        Unit = Enums.UnitType.Box
                    });
                }
            }

            // Split instructions into steps
            var instructionSteps = mealDbRecipe.strInstructions?
                .Split(new[] { '.', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim().TrimEnd('.'))
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList() ?? new List<string>();

            var startStep = new StartStep { MinutesToComplete = 0 };
            IStep currentStep = startStep;

            int stepCount = 1;
            foreach (var text in instructionSteps)
            {
                var nextStep = new TextStep
                {
                    Title = $"Step {stepCount++}",
                    Instructions = text,
                    MinutesToComplete = 1,
                    IngredientsToUse = ingredients
                };

                if (currentStep is StartStep start)
                    start.Paths = [new OutNode("Next", nextStep)];
                else if (currentStep is TextStep textStep)
                    textStep.OutNodes = [new OutNode("Next", nextStep)];

                currentStep = nextStep;
            }


            var savedRecipe = new SavedRecipe
            {
                Title = mealDbRecipe.strMeal ?? "Untitled Recipe",
                Description = mealDbRecipe.strInstructions ?? string.Empty,
                ImageUrl = mealDbRecipe.strMealThumb ?? string.Empty,
                Tags = mealDbRecipe.strTags?.Split(',').ToObservableCollection() ?? 
                       (mealDbRecipe.strCategory is { } category ? [ category ] : []),
                Rating = 0,
                UserNote = string.Empty,
                IsFromPdf = false,
                PdfPath = null,
                HtmlPath = null,
                RootStepNode = startStep
            };
            return savedRecipe;
        }
    }
}

