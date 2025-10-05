using RecipeApp.Models;

namespace RecipeApp.Tests;

public class RecipeTests
{
	[SetUp]
	public void Setup()
	{
	}

	[Test]
	public async Task Test1()
	{
        var currentRecipes = await Recipe.GetAll();
        var originalCount = currentRecipes.Count;

        Console.WriteLine(@"ORIGINAL RECIPES:");
        Console.WriteLine(string.Join(", ", currentRecipes.Select(r => r.Title)));
        
        var tempRecipe = await Recipe.Add("Title", "Description", "ImageUrl", "SourceUrl");
        
        currentRecipes = await Recipe.GetAll();
        var newCount = currentRecipes.Count;
        
        Console.WriteLine();
        Console.WriteLine(@"RECIPES WITH TEMP RECIPE ADDED:");
        Console.WriteLine(string.Join(", ", currentRecipes.Select(r => r.Title)));
        
        await Recipe.Remove(tempRecipe);

        currentRecipes = await Recipe.GetAll();
        var removeCount = currentRecipes.Count;
        
        Console.WriteLine();
        Console.WriteLine("RECIPES WITH TEMP RECIPE REMOVED:");
        Console.WriteLine(string.Join(", ", currentRecipes.Select(r => r.Title)));
        
        if (newCount != originalCount + 1 || removeCount != originalCount)
            Assert.Fail();
        else
            Assert.Pass();
    }
}
