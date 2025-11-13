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
        // build
        var root = new StartStep();
        var s1 = new TextStep {MinutesToComplete = 5};
        var s2 = new TextStep {MinutesToComplete = 3};
        var s3 = new TextStep {MinutesToComplete = 0};
        var s4 = new TextStep {MinutesToComplete = 10};
        var s5 = new TextStep {MinutesToComplete = 8};
        var split1 = new SplitStep();
        var split2 = new SplitStep();
        var merge0 = new MergeStep();
        var merge1 = new MergeStep();
        var merge2 = new MergeStep();
        var end = new FinishStep {MinutesToComplete = 4};

        root.Paths =
        [
            new OutNode("Oven", merge0),
            new OutNode("Microwave", split2),
        ];

        split1.OutNodes =
        [
            new OutNode("", s1),
            new OutNode("", s2)
        ];
        
        // skip s1 to invalidate the path
        
        s2.OutNodes =
        [
            new OutNode("Next", merge1)
        ];
        
        merge1.NextStep = new OutNode("", s3);

        s3.OutNodes =
        [
            new OutNode("Skip", end),
            new OutNode("Next", s4)
        ];

        s4.OutNodes =
        [
            new OutNode("Next", merge2)
        ];

        s5.OutNodes =
        [
            new OutNode("", merge2)
        ];

        split2.OutNodes =
        [
            new OutNode("", merge0),
            new OutNode("", s5),
        ];
        
        merge0.NextStep = new OutNode("", split1);
        merge2.NextStep = new OutNode("", end);


        // test for fails
        root.GetNestedPathInfo().All(pi => pi.IsValid).Should().BeFalse();

        // rebuild a bit
        s1.OutNodes =
        [
            new OutNode("Next", merge1)
        ];

        // test for passes
        var info = root.GetNestedPathInfo();
        
        info.All(pi => pi.IsValid).Should().BeTrue();
        info[0].MaxCookTime.Should().Be(15); 
        info[0].MinCookTime.Should().Be(5);
        info[0].CleanupTime.Should().Be(4);
        
        info[1].MaxCookTime.Should().Be(15);
        info[1].MinCookTime.Should().Be(8);
    }
}
