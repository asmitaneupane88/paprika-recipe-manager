using FluentAssertions;
using RecipeApp.Models;
using RecipeApp.Models.RecipeSteps;

namespace RecipeApp.Tests;

public class SavedRecipeListTests
{
	[SetUp]
	public void Setup()
	{
	}

    /// <summary>
    /// Builds the complex nested recipe. ASCII representation in the code comments.
    /// </summary>
    private SavedRecipe BuildComplexNestedRecipe(){
        // start -> X -> split1 +
        //                 +-> A -> tH -----------------------------------------------------+-> merge3 -> Z -> finish
        //                 +-> B -> split2 -+                                   +-> merge2 -+
        //                                  +-> D  -----------------------------+
        //                                  +-> tE -----------------------------+
        //                                  +-> C  -> split3 +      +-> merge1 -+
        //                                                   +-> F -+
        //                                                   +-> G -+
        var startStep = new StartStep();
        var recipe = new SavedRecipe { Title = "Complex Nested Recipe", RootStepNode = startStep };

        var split1 = new SplitStep();
        var split2 = new SplitStep();
        var split3 = new SplitStep();
        var merge1 = new MergeStep();
        var merge2 = new MergeStep();
        var merge3 = new MergeStep();
        var finish = new FinishStep();
        var X = new TextStep { Title = "Step X", MinutesToComplete = 5 };
        var A = new TextStep { Title = "Step A", MinutesToComplete = 5 };
        var B = new TextStep { Title = "Step B", MinutesToComplete = 10 };
        var C = new TextStep { Title = "Step C", MinutesToComplete = 15 };
        var D = new TextStep { Title = "Step D", MinutesToComplete = 20 };
        var F = new TextStep { Title = "Step F", MinutesToComplete = 30 };
        var G = new TextStep { Title = "Step G", MinutesToComplete = 35 };
        var tE = new TimerStep { Title = "Step E", MinutesToComplete = 25 };
        var tH = new TimerStep { Title = "Step H", MinutesToComplete = 40 };
        var Z = new TextStep { Title = "Step Z", MinutesToComplete = 45 };

        // connections line by line
        startStep.Paths = [new OutNode("Step X", X)];
        X.OutNodes = [new OutNode("Split1", split1)];
        
        split1.OutNodes = [new OutNode("A", A), new OutNode("B", B)];
        A.OutNodes = [new OutNode("tH", tH)];
        tH.NextStep = new OutNode("Merge3", merge3);
        merge3.NextStep = new OutNode("Step Z", Z);
        Z.OutNodes = [new OutNode("Finish", finish)];

        B.OutNodes = [new OutNode("Split2", split2)];
        split2.OutNodes = [new OutNode("D", D), new OutNode("tE", tE), new OutNode("C", C)];
        merge2.NextStep = new OutNode("Merge3", merge3);

        D.OutNodes = [new OutNode("Merge2", merge2)];
        
        tE.NextStep = new OutNode("Merge2", merge2);
        
        C.OutNodes = [new OutNode("Split3", split3)];
        split3.OutNodes = [new OutNode("F", F), new OutNode("G", G)];
        merge1.NextStep = new OutNode("Merge2", merge2);

        F.OutNodes = [new OutNode("Merge1", merge1)];

        G.OutNodes = [new OutNode("Merge1", merge1)];
        
        return recipe;
    }

    [Test]
    public void ReturnsEmptyListWhenNull()
    {
        // Arrange
        var recipe = new SavedRecipe { Title = "Test Recipe" };
        
        // Act
        var result = SavedRecipe.GetNestedListRepresentation(null!);
        
        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(new List<object>());
    }

    [Test]
    public void ReturnsEmptyListWhenNoPaths()
    {
        // Arrange
        var recipe = new SavedRecipe { Title = "Test Recipe" };
        var startStep = new StartStep { Paths = [] };
        
        // Act
        var result = SavedRecipe.GetNestedListRepresentation(startStep);
        
        // Assert
        result.Should().NotBeNull();
    }

    [Test]
    public void HandlesStartStepWithTextStep()
    {
        // Arrange
        var recipe = new SavedRecipe { Title = "Test Recipe" };
        var startStep = new StartStep();
        var textStep = new TextStep 
        { 
            Title = "Test Step",
            MinutesToComplete = 5
        };
        
        startStep.Paths = [new OutNode("Start", textStep)];
        
        // Act
        var result = SavedRecipe.GetNestedListRepresentation(startStep);
        
        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(new List<object> { new List<object> { textStep } });
    }

    [Test]
    public void HandlesStartStepWithSplitStep()
    {
        // Arrange
        var recipe = new SavedRecipe { Title = "Test Recipe" };
        var startStep = new StartStep();
        var splitStep = new SplitStep();
        var textStep1 = new TextStep { Title = "Step 1" };
        var textStep2 = new TextStep { Title = "Step 2" };
        
        splitStep.OutNodes = 
        [
            new OutNode("Path1", textStep1),
            new OutNode("Path2", textStep2)
        ];
        
        startStep.Paths = [new OutNode("Split", splitStep)];
        
        // Act
        var result = SavedRecipe.GetNestedListRepresentation(startStep);
        
        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(new List<object> { textStep1, textStep2 });
    }

    [Test]
    public void HandlesComplexNestedStructure()
    {
        // Arrange
        var X = new TextStep { Title = "Step X", MinutesToComplete = 5 };
        var Z = new TextStep { Title = "Step Z", MinutesToComplete = 45 };
        var A = new TextStep { Title = "Step A", MinutesToComplete = 5 };
        var B = new TextStep { Title = "Step B", MinutesToComplete = 10 };
        var C = new TextStep { Title = "Step C", MinutesToComplete = 15 };
        var D = new TextStep { Title = "Step D", MinutesToComplete = 20 };
        var F = new TextStep { Title = "Step F", MinutesToComplete = 30 };
        var G = new TextStep { Title = "Step G", MinutesToComplete = 35 };
        var tE = new TimerStep { Title = "Step E", MinutesToComplete = 25 };
        var tH = new TimerStep { Title = "Step H", MinutesToComplete = 40 };

        // Act
        var result = BuildComplexNestedRecipe().NestedListRepresentation;

        var expected = new List<object> {
            X,
            new HashSet<object>{
                new List<object> { A, tH },
                new List<object> {
                    B,
                    new HashSet<object>{
                        tE,
                        D,
                        new List<object>{
                            C,
                            new HashSet<object>{
                                F,
                                G,
                            }
                        },
                    }
                }
            },
            Z
        };
        
        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expected);
    }
}