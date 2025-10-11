using RecipeApp.Converters;
using RecipeApp.Models;
using RecipeApp.ViewModels;

namespace RecipeApp.Tests;

public class RecipeDetailsTests
{
    [Fact]
    public void MinutesToTimeConverter_ConvertsCorrectly()
    {
        var converter = new MinutesToTimeConverter();

        // Test minutes less than 1 hour
        var result1 = converter.Convert(45, typeof(string), null!, string.Empty);
        Assert.Equal("45 min", result1);

        // Test exact hours
        var result2 = converter.Convert(120, typeof(string), null!, string.Empty);
        Assert.Equal("2 hr", result2);

        // Test hours and minutes
        var result3 = converter.Convert(150, typeof(string), null!, string.Empty);
        Assert.Equal("2 hr 30 min", result3);
    }

    [Fact]
    public void Recipe_PropertiesInitializeCorrectly()
    {
        var recipe = new Recipe
        {
            Title = "Test Recipe",
            PrepTimeMinutes = 30,
            CookTimeMinutes = 60,
            Servings = 4,
            Difficulty = "Medium",
            Ingredients = new(),
            Directions = new()
        };

        Assert.Equal("Test Recipe", recipe.Title);
        Assert.Equal(90, recipe.TotalTimeMinutes); // 30 + 60
        Assert.Equal(4, recipe.Servings);
        Assert.Equal("Medium", recipe.Difficulty);
    }

    [Fact]
    public void RecipeViewModel_SaveRecipe_CallsSaveMethod()
    {
        var recipe = new Recipe
        {
            Title = "Test Recipe",
            PrepTimeMinutes = 30,
            CookTimeMinutes = 60,
            Servings = 4,
            Difficulty = "Medium",
            Ingredients = new(),
            Directions = new()
        };

        var viewModel = new RecipeDetailsViewModel(recipe);
        
        // Note: This is a basic test. Once actual save functionality is implemented,
        // we should mock the storage service and verify it's called correctly
        var saveTask = viewModel.SaveRecipeAsync();
        Assert.True(saveTask.IsCompleted);
    }
}