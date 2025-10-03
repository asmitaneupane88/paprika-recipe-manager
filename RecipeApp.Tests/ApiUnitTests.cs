using System.Diagnostics;
using RecipeApp.Services;

namespace RecipeApp.Tests;

public class ApiUnitTests
{
	[SetUp]
	public void Setup()
	{
	}
    

    [Test]
    public async Task ApiTest()
    {
        var failTest = false;
        
        try
        {
            var api = new ApiControl();
            var recipes = await api.SearchRecipesAsync("Chicken");
            Console.WriteLine($"Found {recipes.Count} recipes:");
            foreach (var recipe in recipes)
            {
                Console.WriteLine($"- {recipe.Name} (Ingredients: {recipe.Ingredients})");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Test Error: {ex.Message}");
            failTest = true;
        }
        
        Assert.That(failTest, Is.False);
    }
}
