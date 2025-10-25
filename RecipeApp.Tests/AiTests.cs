using RecipeApp.Interfaces;
using RecipeApp.Models;
using RecipeApp.Services;

namespace RecipeApp.Tests;

public class AiTests
{
    private IRecipeService _recipeService = null!;
    
    [SetUp]
    public void Setup()
    {
        _recipeService = new ApiControl();
    }
    
    [Test, Explicit]
    public async Task ConvertMealDbToSavedRecipe()
    {
        var settings = await AiSettings.GetSettings();
        Console.WriteLine(@"Modify Ai settings (to keep the old setting, press enter):");
        Console.WriteLine(@$"ApiKey: {settings.ApiKey}");
        // var newApiKey = Console.ReadLine();
        // if (!string.IsNullOrWhiteSpace(newApiKey)) settings.ApiKey = newApiKey;
        Console.WriteLine($@"Endpoint: {settings.Endpoint}");
        // var newEndpoint = Console.ReadLine();
        // if (!string.IsNullOrWhiteSpace(newEndpoint)) settings.Endpoint = newEndpoint;
        Console.WriteLine();
        Console.WriteLine(@$"Last used model: {settings.LastUsedModel}");
        Console.WriteLine(@$"Models available:");
        foreach (var model in await AiHelper.GetModels())
        {
            Console.WriteLine(model);
        }
        Console.WriteLine();
        /*var newModel = Console.ReadLine();
        while (!await AiHelper.SetModel(string.IsNullOrWhiteSpace(newModel) ? settings.LastUsedModel : newModel))
        {
            Console.WriteLine("Unable to set model, refer to the models available above.");
            newModel = Console.ReadLine();
        }
        Console.WriteLine();*/
        // Console.WriteLine(@"Settings updated.");
        
        
        var recipes = await _recipeService.SearchAsync("Chicken");

        var result = await AiHelper.MealDbToSavedRecipe(recipes.First());
    }
}
