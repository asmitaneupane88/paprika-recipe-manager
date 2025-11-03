using FluentAssertions;
using RecipeApp.Models;
using RecipeApp.Models.RecipeSteps;
using System.Collections.ObjectModel;
using RecipeApp.Interfaces;
using RecipeApp.Enums;

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

    [Test]
    public void SavedRecipe_Ingredients_ExtractsFromGraph()
    // AI Generated
    {
        // Arrange
        var recipe = new SavedRecipe { Title = "Test Recipe" };
        var rootStep = new StartStep();
        var textStep1 = new TextStep 
        { 
            Title = "Step 1",
            IngredientsToUse =
            [   
                new RecipeIngredient { Name = "Flour", Quantity = 2, Unit = UnitType.CUP },
                new RecipeIngredient { Name = "Sugar", Quantity = 1, Unit = UnitType.CUP }
            ]
        };
        var textStep2 = new TextStep 
        { 
            Title = "Step 2",
            IngredientsToUse =
            [
                new RecipeIngredient { Name = "Eggs", Quantity = 3, Unit = UnitType.ITEM },
                new RecipeIngredient { Name = "Flour", Quantity = 1, Unit = UnitType.CUP } // Should combine with step 1
            ],
        };
        var finishStep = new FinishStep();
        
        rootStep.Paths = [new OutNode("Start", textStep1)];
        textStep1.OutNodes = [new OutNode("Next", textStep2)];
        textStep2.OutNodes = [new OutNode("Next", finishStep)];
        
        recipe.RootStepNode = rootStep;
        
        // Act
        var ingredients = recipe.Ingredients;
        
        // Assert
        ingredients.Should().NotBeNull();
        ingredients.Count.Should().BeGreaterThan(0);
        
        // Check that ingredients are accumulated (Flour should be combined: 2 + 1 = 3 cups)
        var flour = ingredients.FirstOrDefault(i => i.Name == "Flour");
        flour.Should().NotBeNull();
        flour!.Quantity.Should().Be(3); // Combined from both steps
        flour.Unit.Should().Be(UnitType.CUP);
        
        ingredients.Should().Contain(i => i.Name == "Sugar" && i.Quantity == 1);
        ingredients.Should().Contain(i => i.Name == "Eggs" && i.Quantity == 3);
    }

    [Test]
    public void SavedRecipe_Ingredients_ReturnsEmptyWhenNoRootStep()
    // AI Generated
    {
        // Arrange
        var recipe = new SavedRecipe { Title = "Test Recipe", RootStepNode = null };
        
        // Act
        var ingredients = recipe.Ingredients;
        
        // Assert
        ingredients.Should().NotBeNull();
        ingredients.Count.Should().Be(0);
    }

    [Test]
    public void SavedRecipe_GetNodeProperties_ReturnsAllNodes()
    // AI Generated
    {
        // Arrange
        var recipe = new SavedRecipe { Title = "Test Recipe" };
        var rootStep = new StartStep { X = 0, Y = 0 };
        var textStep = new TextStep { Title = "Test Step", X = 100, Y = 100, MinutesToComplete = 5 };
        var timerStep = new TimerStep { Title = "Timer", X = 200, Y = 200, MinutesToComplete = 10 };
        var finishStep = new FinishStep { X = 300, Y = 300 };
        
        rootStep.Paths = [new OutNode("Start", textStep)];
        textStep.OutNodes = [new OutNode("Next", timerStep)];
        timerStep.NextStep = new OutNode("Next", finishStep);
        
        recipe.RootStepNode = rootStep;
        
        // Act
        var nodeProperties = recipe.GetNodeProperties();
        
        // Assert
        nodeProperties.Should().NotBeNull();
        nodeProperties.Count.Should().Be(4); // StartStep, TextStep, TimerStep, FinishStep
        
        // Check that all nodes are present
        nodeProperties.Keys.Should().Contain(rootStep);
        nodeProperties.Keys.Should().Contain(textStep);
        nodeProperties.Keys.Should().Contain(timerStep);
        nodeProperties.Keys.Should().Contain(finishStep);
        
        // Check TextStep properties
        var textProps = nodeProperties[textStep];
        textProps.Should().ContainKey("Title");
        textProps["Title"].Should().Be("Test Step");
        textProps.Should().ContainKey("MinutesToComplete");
        textProps["MinutesToComplete"].Should().Be(5.0);
        textProps.Should().ContainKey("LocX");
        textProps["LocX"].Should().Be(100.0);
        textProps.Should().ContainKey("StepType");
        textProps["StepType"].Should().Be("TextStep");
    }

    [Test]
    public void SavedRecipe_GetNodeProperties_ReturnsEmptyWhenNoRootStep()
    // AI Generated
    {
        // Arrange
        var recipe = new SavedRecipe { Title = "Test Recipe", RootStepNode = null };
        
        // Act
        var nodeProperties = recipe.GetNodeProperties();
        
        // Assert
        nodeProperties.Should().NotBeNull();
        nodeProperties.Count.Should().Be(0);
    }

    [Test]
    public void Recipe_IRecipeIngredients_ConvertsIngredientToRecipeIngredient()
    // AI Generated
    {
        // Arrange
        var recipe = new Recipe
        {
            Title = "Test Recipe",
            PrepTimeMinutes = 10,
            CookTimeMinutes = 20,
            Servings = 4,
            Difficulty = "Easy",
            Ingredients = 
            [
                new Ingredient { Name = "Flour", Amount = "2", Unit = "CUP", Notes = "Sifted" },
                new Ingredient { Name = "Sugar", Amount = "1.5", Unit = "TBSP" },
                new Ingredient { Name = "Eggs", Amount = "3", Unit = "", Notes = "Large" },
                new Ingredient { Name = "Milk", Amount = "1", Unit = "cup" } // lowercase test
            ],
            Directions = []
        };
        
        // Act
        var recipeIngredients = ((IRecipe)recipe).Ingredients;
        
        // Assert
        recipeIngredients.Should().NotBeNull();
        recipeIngredients.Count.Should().Be(4);
        
        // Check conversions
        var flour = recipeIngredients.First(i => i.Name == "Flour");
        flour.Quantity.Should().Be(2);
        flour.Unit.Should().Be(UnitType.CUP);
        flour.ModifierNote.Should().Be("Sifted");
        
        var sugar = recipeIngredients.First(i => i.Name == "Sugar");
        sugar.Quantity.Should().Be(1.5);
        sugar.Unit.Should().Be(UnitType.TBSP);
        
        var eggs = recipeIngredients.First(i => i.Name == "Eggs");
        eggs.Quantity.Should().Be(3);
        eggs.Unit.Should().Be(UnitType.ITEM); // Empty unit defaults to ITEM
        eggs.ModifierNote.Should().Be("Large");
        
        var milk = recipeIngredients.First(i => i.Name == "Milk");
        milk.Unit.Should().Be(UnitType.CUP); // Case-insensitive conversion
    }

    [Test]
    public void Recipe_IRecipeIngredients_HandlesInvalidAmount()
    // AI Generated
    {
        // Arrange
        var recipe = new Recipe
        {
            Title = "Test Recipe",
            PrepTimeMinutes = 10,
            CookTimeMinutes = 20,
            Servings = 4,
            Difficulty = "Easy",
            Ingredients = 
            [
                new Ingredient { Name = "Salt", Amount = "pinch", Unit = "TSP" }, // Invalid number
                new Ingredient { Name = "Pepper", Amount = null, Unit = "TSP" } // Null amount
            ],
            Directions = []
        };
        
        // Act
        var recipeIngredients = ((IRecipe)recipe).Ingredients;
        
        // Assert
        recipeIngredients.Should().NotBeNull();
        recipeIngredients.Count.Should().Be(2);
        
        // Invalid amounts should default to 1
        recipeIngredients.First(i => i.Name == "Salt").Quantity.Should().Be(1);
        recipeIngredients.First(i => i.Name == "Pepper").Quantity.Should().Be(1);
    }
}
