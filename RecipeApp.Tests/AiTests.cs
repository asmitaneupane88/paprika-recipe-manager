using System.Text.Json;
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
        
        Console.WriteLine(JsonSerializer.Serialize(result));
    }
    
    [Test, Explicit]
    public async Task ConvertStringToSavedRecipe()
    {
        // just paste html in the text block below
        var result = await AiHelper.StringToSavedRecipe(
            """
            <!DOCTYPE html>
            <html>
            <head><meta charset=""UTF-8""><title>Converted PDF</title></head>
            <body><pre>ONE POT CREAMY PESTO CHICKEN PASTA INGREDIENTS: •1 lb. boneless, skinless chicken breast•2 tbsp butter•2 cloves garlic•&#189; lb. penne pasta•1.5 cups chicken broth•1 cup milk•3 oz (about 88.72 ml) cream cheese•1/3 cup basil pesto•&#188; cup grated Parmesan Cheese•Ground pepper to taste•1 pinch crushed red pepper•OPTIONAL ADD- INSo3 cups spinacho&#188; cup sun dried tomatoesDIRECTIONS: 1.Cut the chicken breast into 1-inch pieces. Add the butter to a deep skillet and melt overmedium heat. Add the chicken to the skillet and cook over medium heat until thechicken is slightly browned on the outside.2.While the chicken is cooking, mince the garlic. Add the garlic to the skillet with thechicken and continue to saut&#233; for one minute more.3.Add the uncooked pasta and chicken broth to the skillet with the chicken and garlic.Stir to dissolve any browned bits from the bottom of the skillet. Place a lid on theskillet, turn the heat up to medium-high, and bring the broth up to a boil.4.Once the broth comes to a full boil, give the pasta a quick stir, replace the lid, and turnthe heat down to medium-low. Let the pasta simmer over medium-low heat for about 8minutes, or until the pasta is tender and most of the broth has been absorbed. Stir thepasta briefly every two minutes as it simmers, replacing the lid quickly each time.5.Once the pasta is tender and most of the broth absorbed, add the milk, cream cheese(cut into chunks), and pesto. Stir and cook over medium heat until the cream cheesehas fully melted into the sauce. Finally, add the grated Parmesan and stir untilcombined.6.If using, add the fresh spinach and sliced sun-dried tomatoes. Stir until the spinach haswilted, then remove the pasta from the heat. Top the pasta with freshly cracked pepperand a pinch of crushed red pepper, then serve.
               BALANCING MEAL IDEAS:  1. Add optional add-ins of spinach or vegetable of choice  2. This is already a very balanced meal with protein (chicken), carbohydrate (pasta), and fat (dairy) sources!  NOTES: • The calculated cost of this meal is ~$8.00-$10.15  • The recipe yields about 4 servings  Reference: https://www.budgetbytes.com/one-pot-creamy-pesto-chicken-pasta/ 
            </pre></body>
            """);
        
        Console.WriteLine(JsonSerializer.Serialize(result));
    }
}
