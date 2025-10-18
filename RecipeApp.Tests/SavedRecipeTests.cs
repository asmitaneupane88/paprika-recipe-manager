using FluentAssertions;
using RecipeApp.Models;
using RecipeApp.Models.RecipeSteps;

namespace RecipeApp.Tests;

public class SavedRecipeTests
{
	[SetUp]
	public void Setup()
	{
	}

	[Test]
	public async Task Test1()
	{
        var currentRecipes = await SavedRecipe.GetAll();
        var originalCount = currentRecipes.Count;

        Console.WriteLine(@"ORIGINAL RECIPES:");
        Console.WriteLine(string.Join(", ", currentRecipes.Select(r => r.Title)));
        
        var tempRecipe = await SavedRecipe.Add("Title", "Description", "ImageUrl", "SourceUrl");
        
        currentRecipes = await SavedRecipe.GetAll();
        var newCount = currentRecipes.Count;
        
        Console.WriteLine();
        Console.WriteLine(@"RECIPES WITH TEMP RECIPE ADDED:");
        Console.WriteLine(string.Join(", ", currentRecipes.Select(r => r.Title)));
        
        await SavedRecipe.Remove(tempRecipe);

        currentRecipes = await SavedRecipe.GetAll();
        var removeCount = currentRecipes.Count;
        
        Console.WriteLine();
        Console.WriteLine(@"RECIPES WITH TEMP RECIPE REMOVED:");
        Console.WriteLine(string.Join(", ", currentRecipes.Select(r => r.Title)));
        
        if (newCount != originalCount + 1 || removeCount != originalCount)
            Assert.Fail();
        else
            Assert.Pass();
    }

    [Test]
    public async Task Test2()
    {
        if ((await SavedRecipe.GetAll()).Count == 0)
        {
            await SavedRecipe.Add("Chicken Something", "yummy chicken", "https://www.cookingclassy.com/wp-content/uploads/2022/07/grilled-chicken-breast-4.jpg", "https://www.google.com/");
            await SavedRecipe.Add("Steak", "cooked steak", "https://wallpapercave.com/wp/wp2115243.jpg", "https://www.google.com/");
            await SavedRecipe.Add("Cookies", "A delicious cookie with no image or source", "");
        }
        
        Assert.Pass();
    }

    [Test]
    public void TestSteps()
    {
        var root = new StartStep();
        
        Console.WriteLine(@"Testing with only root");
        root.MinutesToComplete = 5;
        root.ArePathsValid().Should().BeFalse();
        root.GetTotalTime().Should().Be(5);

        Console.WriteLine(@"Testing with root and final step");
        var final = new FinishStep { MinutesToComplete = 10 };
        root.NextStep = final;
        root.ArePathsValid().Should().BeTrue();
        root.GetTotalTime().Should().Be(15);
        root.GetPrepTime().Should().Be(5);
        root.GetCookTime().Should().Be(0);
        root.GetCleanupTime().Should().Be(10);
        
        Console.WriteLine(@"Testing with wrong intermediate step A");
        var next = new TextStep
        {
            MinutesToComplete = 3,
            OutNodes = [
                new("Next", final),
                new("Back", [])
            ]
        };
        root.NextStep = next;
        
        root.ArePathsValid().Should().BeFalse();
        
        Console.WriteLine(@"Testing with wrong intermediate step B");
        next.OutNodes = [];
        
        root.ArePathsValid().Should().BeFalse();

        Console.WriteLine(@"Testing with correct intermediate step");
        next.OutNodes = [
            new("Next", final),
            new("Back", [ next ])
        ];
        
        root.ArePathsValid().Should().BeTrue();
        root.GetTotalTime().Should().Be(18);
        root.GetPrepTime().Should().Be(5);
        root.GetCookTime().Should().Be(3);
        root.GetCleanupTime().Should().Be(10);
    }
}
