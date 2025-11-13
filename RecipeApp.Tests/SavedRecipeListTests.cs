using System.Reflection;
using FluentAssertions;
using Microsoft.UI.Content;
using RecipeApp.Interfaces;
using RecipeApp.Models;
using RecipeApp.Models.RecipeSteps;
using Windows.UI.Input;

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
        //                      +-> A -> tH -----------------------------------------------------+-> merge3 -> Z -> finish
        //                      +-> B -> split2 -+                                   +-> merge2 -+
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

    private void BuildGraphWithMerge(){
        // start --> A --> Split +---> B ---+--> Merge --> D --> Finish
        //                       +---> C ---+ 

        // Arrange
        var start = new StartStep();
        var recipe = new SavedRecipe { Title = "Simple Graph with Merge and Split", RootStepNode = start };

        var A = new TextStep { Title = "A" };
        var split = new SplitStep();
        var B = new TextStep { Title = "B" };
        var C = new TextStep { Title = "C" };
        var merge = new MergeStep();
        var D = new TextStep { Title = "D"};
        var finish = new FinishStep();

        // Assign forward edges
        start.Paths = [new OutNode("A", A)];
        A.OutNodes = [new OutNode("split", split)];
        split.OutNodes = [new OutNode("B", B), new OutNode("C", C)];
        B.OutNodes = [new OutNode("merge", merge)];
        C.OutNodes = [new OutNode("merge", merge)];
        merge.NextStep = new OutNode("D", D);
        D.OutNodes = [new OutNode("Finish", finish)];
    }

    private void BuildSimpleGraph(){
        // start --> A --> B --> Finish

        // Arrange
        var start = new StartStep();
        var recipe = new SavedRecipe { Title = "Simple Graph", RootStepNode = start };

        var A = new TextStep { Title = "A" };
        var B = new TextStep { Title = "B" };
        var finish = new FinishStep();

        // Assign forward edges
        start.Paths = [new OutNode("A", A)];
        A.OutNodes = [new OutNode("B", B)];
        B.OutNodes = [new OutNode("Finish", finish)];
    }

    private void BuildGraphMergeAndTimer(){
        // start --> A --> Split +---> B ---+-->  Merge --> Timer --> D --> Finish
        //                       +---> C ---+

        // Arrange
        var start = new StartStep();
        var recipe = new SavedRecipe { Title = "Simple Graph with Merge and Split", RootStepNode = start };

        var A = new TextStep { Title = "A" };
        var split = new SplitStep();
        var B = new TextStep { Title = "B" };
        var C = new TextStep { Title = "C" };
        var merge = new MergeStep();
        var timer = new TimerStep();
        var D = new TextStep { Title = "D"};
        var finish = new FinishStep();

        // Assign forward edges
        start.Paths = [new OutNode("A", A)];
        A.OutNodes = [new OutNode("split", split)];
        split.OutNodes = [new OutNode("B", B), new OutNode("C", C)];
        B.OutNodes = [new OutNode("merge", merge)];
        C.OutNodes = [new OutNode("merge", merge)];
        merge.NextStep = new OutNode("timer", timer);
        timer.NextStep = new OutNode("D", D);
        D.OutNodes = [new OutNode("Finish", finish)];
    }

    [Test]
    public void GetPossiblePaths(){

        // start -> X -> split1 +
        //                      +-> A -> tH -----------------------------------------------------+-> merge3 -> Z -> finish
        //                      +-> B -> split2 -+                                   +-> merge2 -+
        //                                  +-> D  -----------------------------+
        //                                  +-> tE -----------------------------+
        //                                  +-> C  -> split3 +      +-> merge1 -+
        //                                                   +-> F -+
        //                                                   +-> G -+
        var start = new StartStep();
        var recipe = new SavedRecipe { Title = "Complex Nested Recipe", RootStepNode = start };

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
        start.Paths = [new OutNode("Step X", X)];
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
    
        var expected = new List<List<IStep>>
        {
            new List<IStep> { start, X, split1, A, tH, merge3, Z, finish },
            new List<IStep> { start, X, split1, B, split2, D, merge2, merge3, Z, finish },
            new List<IStep> { start, X, split1, B, split2, tE, merge2, merge3, Z, finish},
            new List<IStep> { start, X, split1, B, split2, C, split3, F, merge1, merge2, merge3, Z, finish},
            new List<IStep> { start, X, split1, B, split2, C, split3, G, merge1, merge2, merge3, Z, finish},

        };

        var result = SavedRecipe.GetPossiblePaths(start, null, null);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expected);

    }

    [Test]
    public void GetCommonParents(){
        // start -> X -> split1 +
        //                      +-> A -> tH -----------------------------------------------------+-> merge3 -> Z -> finish
        //                      +-> B -> split2 -+                                   +-> merge2 -+
        //                                  +-> D  -----------------------------+
        //                                  +-> tE -----------------------------+
        //                                  +-> C  -> split3 +      +-> merge1 -+
        //                                                   +-> F -+
        //                                                   +-> G -+
        var start = new StartStep();
        var recipe = new SavedRecipe { Title = "Complex Nested Recipe", RootStepNode = start };

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
        start.Paths = [new OutNode("Step X", X)];
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
    
        var expected = new List<IStep> { start, X, split1};
        var possiblePaths = SavedRecipe.GetPossiblePaths(start, null, null);
        var result = SavedRecipe.GetCommonParents(possiblePaths);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expected);

    }

    [Test]
    public void GetCommonDescendants(){
        // start -> X -> split1 +
        //                      +-> A -> tH -----------------------------------------------------+-> merge3 -> Z -> finish
        //                      +-> B -> split2 -+                                   +-> merge2 -+
        //                                  +-> D  -----------------------------+
        //                                  +-> tE -----------------------------+
        //                                  +-> C  -> split3 +      +-> merge1 -+
        //                                                   +-> F -+
        //                                                   +-> G -+
        var start = new StartStep();
        var recipe = new SavedRecipe { Title = "Complex Nested Recipe", RootStepNode = start };

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
        start.Paths = [new OutNode("Step X", X)];
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
    
        var expected = new List<IStep> { merge3, Z, finish};
        var possiblePaths = SavedRecipe.GetPossiblePaths(start, null, null);
        var result = SavedRecipe.GetCommonDescendants(possiblePaths);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expected);

    }

    [Test]
    public void GetUncommonElements(){
        // start -> X -> split1 +
        //                      +-> A -> tH -----------------------------------------------------+-> merge3 -> Z -> finish
        //                      +-> B -> split2 -+                                   +-> merge2 -+
        //                                  +-> D  -----------------------------+
        //                                  +-> tE -----------------------------+
        //                                  +-> C  -> split3 +      +-> merge1 -+
        //                                                   +-> F -+
        //                                                   +-> G -+
        var start = new StartStep();
        var recipe = new SavedRecipe { Title = "Complex Nested Recipe", RootStepNode = start };

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
        start.Paths = [new OutNode("Step X", X)];
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
    
        var expected = new List<List<IStep>>
        {
            new List<IStep> { A, tH},
            new List<IStep> { B, split2, D, merge2},
            new List<IStep> { B, split2, tE, merge2},
            new List<IStep> { B, split2, C, split3, F, merge1, merge2},
            new List<IStep> { B, split2, C, split3, G, merge1, merge2},
        };

        var possiblePaths = SavedRecipe.GetPossiblePaths(start, null, null);
        var commonParents = SavedRecipe.GetCommonParents(possiblePaths);
        var commonDescendants = SavedRecipe.GetCommonDescendants(possiblePaths);

        var result = SavedRecipe.GetUncommonElements(possiblePaths,commonParents,commonDescendants);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void HandlesComplexNestedStructure()
    {
        // start -> X -> split1 +
        //                      +-> A -> tH -----------------------------------------------------+-> merge3 -> Z -> finish
        //                      +-> B -> split2 -+                                   +-> merge2 -+
        //                                  +-> D  -----------------------------+
        //                                  +-> tE -----------------------------+
        //                                  +-> C  -> split3 +      +-> merge1 -+
        //                                                   +-> F -+
        //                                                   +-> G -+
        var start = new StartStep();

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
        start.Paths = [new OutNode("Step X", X)];
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

        // Act
        var recipe = new SavedRecipe { Title = "Complex Nested Recipe", RootStepNode = start };
        var result = recipe.NestedListRepresentation;

        var expected = new List<object> {
            start,
            X,
            split1,
            new List<object>{
                new List<object>{
                    B,
                    split2,
                    new List<object>{
                        new List<object>{C, split3, new List<object>{ new List<object>{F}, new List<object>{G} }, merge1},
                        new List<object>{D},
                        new List<object>{tE}
                    },
                    merge2
                },
                new List<object>{A, tH}
            },
            merge3,
            Z,
            finish
        };
        
        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expected);
    }

}