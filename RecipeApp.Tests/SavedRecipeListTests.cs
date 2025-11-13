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

    [Test]
    public void BuildBranchDepths_SimpleGraph_ReturnsCorrectDepths(){
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

        var branchDepths = new Dictionary<IStep, int> {
            { start, 0 },
            { A, 0 },
            { B, 0 },
            { finish, 0 }
        };

        // Act
        var result = SavedRecipe.BuildBranchDepths(recipe.RootStepNode!);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(branchDepths);
    }

    [Test]
    public void BuildBranchDepths_SimpleGraphWithMergeAndSplit_ReturnsCorrectEdges(){
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

        var branchDepths = new Dictionary<IStep, int> {
            { start, 0 },
            { A, 0 },
            { split, 0 },
            { B, 1 },
            { C, 1},
            { merge, 0},
            { D, 0},
            { finish, 0}
        };

        // Act
        var result = SavedRecipe.BuildBranchDepths(recipe.RootStepNode!);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(branchDepths);
    }

    [Test]
    public void BuildForwardEdges_SimpleGraph_ReturnsCorrectEdges(){
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

        var forwardEdges = new HashSet<Tuple<IStep, IStep>>(){
            new Tuple<IStep, IStep>(start, A),
            new Tuple<IStep, IStep>(A, B),
            new Tuple<IStep, IStep>(B, finish)
        };
        

        // Act
        var result = SavedRecipe.BuildForwardEdges(recipe.RootStepNode!);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(forwardEdges);
    }

    [Test]
    public void BuildForwardEdges_SimpleGraphWithMergeAndSplit_ReturnsCorrectEdges(){
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

        var forwardEdges = new HashSet<Tuple<IStep, IStep>>(){
            new Tuple<IStep, IStep>(start, A),
            new Tuple<IStep, IStep>(A, split),
            new Tuple<IStep, IStep>(split, B),
            new Tuple<IStep, IStep>(split, C),
            new Tuple<IStep, IStep>(B, merge),
            new Tuple<IStep, IStep>(C, merge),
            new Tuple<IStep, IStep>(merge, D),
            new Tuple<IStep, IStep>(D, finish)
        };

        // Act
        var result = SavedRecipe.BuildForwardEdges(recipe.RootStepNode!);

        // Assert
        result.Should().NotBeNull();
        // Compare tuples - they should contain the same step object references
        // Since IStep doesn't override Equals, ComparingByValue will use reference equality
        result.Should().BeEquivalentTo(forwardEdges, options => options
            .ComparingByMembers<Tuple<IStep, IStep>>()
            .ComparingByValue<IStep>());
    }

    [Test]
    public void BuildForwardEdges_SimpleGraphWithMergeSplitTimer_ReturnsCorrectEdges(){
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

        var forwardEdges = new HashSet<Tuple<IStep, IStep>>(){
            new Tuple<IStep, IStep>(start, A),
            new Tuple<IStep, IStep>(A, split),
            new Tuple<IStep, IStep>(split, B),
            new Tuple<IStep, IStep>(split, C),
            new Tuple<IStep, IStep>(B, merge),
            new Tuple<IStep, IStep>(C, merge),
            new Tuple<IStep, IStep>(merge, timer),
            new Tuple<IStep, IStep>(timer, D),
            new Tuple<IStep, IStep>(D, finish)
        };

        // Act
        var result = SavedRecipe.BuildForwardEdges(recipe.RootStepNode!);

        // Assert
        result.Should().NotBeNull();
        // Compare tuples - they should contain the same step object references
        // Since IStep doesn't override Equals, ComparingByValue will use reference equality
        result.Should().BeEquivalentTo(forwardEdges, options => options
            .ComparingByMembers<Tuple<IStep, IStep>>()
            .ComparingByValue<IStep>());

    }

    [Test]
    public void FilterMergeAndSplitSteps_SimpleMerge_ShouldConnectParentsDirectlyToChild()
    {
        // Arrange: NodeA -> Merge1 -> NodeC, NodeB -> Merge1 -> NodeC
        var nodeA = new TextStep { Title = "NodeA" };
        var nodeB = new TextStep { Title = "NodeB" };
        var merge1 = new MergeStep();
        var nodeC = new TextStep { Title = "NodeC" };

        var originalMapping = new Dictionary<IStep, HashSet<IStep>>
        {
            [merge1] = new HashSet<IStep> { nodeA, nodeB },
            [nodeC] = new HashSet<IStep> { merge1 }
        };

        // Act
        var filtered = InvokeFilterMergeAndSplitSteps(originalMapping);

        // Assert
        filtered.Should().NotContainKey(merge1, "Merge step should be removed");
        filtered[nodeC].Should().HaveCount(2, "NodeC should have both NodeA and NodeB as parents");
        filtered[nodeC].Should().Contain(nodeA);
        filtered[nodeC].Should().Contain(nodeB);
    }

    [Test]
    public void FilterMergeAndSplitSteps_SimpleSplit_ShouldConnectParentDirectlyToChildren()
    {
        // Arrange: NodeA -> Split1 -> NodeB, NodeC
        var nodeA = new TextStep { Title = "NodeA" };
        var split1 = new SplitStep();
        var nodeB = new TextStep { Title = "NodeB" };
        var nodeC = new TextStep { Title = "NodeC" };

        var originalMapping = new Dictionary<IStep, HashSet<IStep>>
        {
            [split1] = new HashSet<IStep> { nodeA },
            [nodeB] = new HashSet<IStep> { split1 },
            [nodeC] = new HashSet<IStep> { split1 }
        };

        // Act
        var filtered = InvokeFilterMergeAndSplitSteps(originalMapping);

        // Assert
        filtered.Should().NotContainKey(split1, "Split step should be removed");
        filtered[nodeB].Should().ContainSingle().Which.Should().Be(nodeA);
        filtered[nodeC].Should().ContainSingle().Which.Should().Be(nodeA);
    }

    [Test]
    public void FilterMergeAndSplitSteps_MergeWithMultipleChildren_ShouldConnectCorrectly()
    {
        // Arrange: NodeA -> Merge1 -> NodeB, NodeC
        var nodeA = new TextStep { Title = "NodeA" };
        var merge1 = new MergeStep();
        var nodeB = new TextStep { Title = "NodeB" };
        var nodeC = new TextStep { Title = "NodeC" };

        merge1.NextStep = new OutNode("Next", nodeB);

        var originalMapping = new Dictionary<IStep, HashSet<IStep>>
        {
            [merge1] = new HashSet<IStep> { nodeA },
            [nodeB] = new HashSet<IStep> { merge1 }
        };

        // Act
        var filtered = InvokeFilterMergeAndSplitSteps(originalMapping);

        // Assert
        filtered.Should().NotContainKey(merge1);
        filtered[nodeB].Should().ContainSingle().Which.Should().Be(nodeA);
    }

    [Test]
    public void FilterMergeAndSplitSteps_NestedMergeSteps_ShouldFlattenCorrectly()
    {
        // Arrange: NodeA -> Merge1 -> Merge2 -> NodeC, NodeB -> Merge2 -> NodeC
        var nodeA = new TextStep { Title = "NodeA" };
        var nodeB = new TextStep { Title = "NodeB" };
        var merge1 = new MergeStep();
        var merge2 = new MergeStep();
        var nodeC = new TextStep { Title = "NodeC" };

        merge1.NextStep = new OutNode("Next", merge2);
        merge2.NextStep = new OutNode("Next", nodeC);

        var originalMapping = new Dictionary<IStep, HashSet<IStep>>
        {
            [merge1] = new HashSet<IStep> { nodeA },
            [merge2] = new HashSet<IStep> { merge1, nodeB },
            [nodeC] = new HashSet<IStep> { merge2 }
        };

        // Act
        var filtered = InvokeFilterMergeAndSplitSteps(originalMapping);

        // Assert
        filtered.Should().NotContainKey(merge1);
        filtered.Should().NotContainKey(merge2);
        filtered[nodeC].Should().HaveCount(2, "NodeC should have both NodeA and NodeB as parents");
        filtered[nodeC].Should().Contain(nodeA);
        filtered[nodeC].Should().Contain(nodeB);
    }

    [Test]
    public void FilterMergeAndSplitSteps_SplitThenMerge_ShouldConnectCorrectly()
    {
        // Arrange: NodeA -> Split1 -> NodeB, NodeC -> Merge1 -> NodeD
        var nodeA = new TextStep { Title = "NodeA" };
        var split1 = new SplitStep();
        var nodeB = new TextStep { Title = "NodeB" };
        var nodeC = new TextStep { Title = "NodeC" };
        var merge1 = new MergeStep();
        var nodeD = new TextStep { Title = "NodeD" };

        split1.OutNodes = [new OutNode("Path1", nodeB), new OutNode("Path2", nodeC)];
        merge1.NextStep = new OutNode("Next", nodeD);

        var originalMapping = new Dictionary<IStep, HashSet<IStep>>
        {
            [split1] = new HashSet<IStep> { nodeA },
            [nodeB] = new HashSet<IStep> { split1 },
            [nodeC] = new HashSet<IStep> { split1 },
            [merge1] = new HashSet<IStep> { nodeB, nodeC },
            [nodeD] = new HashSet<IStep> { merge1 }
        };

        // Act
        var filtered = InvokeFilterMergeAndSplitSteps(originalMapping);

        // Assert
        filtered.Should().NotContainKey(split1);
        filtered.Should().NotContainKey(merge1);
        filtered[nodeB].Should().ContainSingle().Which.Should().Be(nodeA);
        filtered[nodeC].Should().ContainSingle().Which.Should().Be(nodeA);
        filtered[nodeD].Should().HaveCount(2, "NodeD should have both NodeB and NodeC as parents");
        filtered[nodeD].Should().Contain(nodeB);
        filtered[nodeD].Should().Contain(nodeC);
    }

    [Test]
    public void FilterMergeAndSplitSteps_NoMergeOrSplitSteps_ShouldReturnSameMapping()
    {
        // Arrange: Simple chain without merge/split
        var nodeA = new TextStep { Title = "NodeA" };
        var nodeB = new TextStep { Title = "NodeB" };
        var nodeC = new TextStep { Title = "NodeC" };

        var originalMapping = new Dictionary<IStep, HashSet<IStep>>
        {
            [nodeB] = new HashSet<IStep> { nodeA },
            [nodeC] = new HashSet<IStep> { nodeB }
        };

        // Act
        var filtered = InvokeFilterMergeAndSplitSteps(originalMapping);

        // Assert
        filtered.Should().BeEquivalentTo(originalMapping);
    }

    [Test]
    public void FilterMergeAndSplitSteps_MergeWithNoParents_ShouldHandleGracefully()
    {
        // Arrange: Merge1 -> NodeA (but Merge1 has no parents)
        var merge1 = new MergeStep();
        var nodeA = new TextStep { Title = "NodeA" };

        merge1.NextStep = new OutNode("Next", nodeA);

        var originalMapping = new Dictionary<IStep, HashSet<IStep>>
        {
            [merge1] = new HashSet<IStep>(), // No parents
            [nodeA] = new HashSet<IStep> { merge1 }
        };

        // Act
        var filtered = InvokeFilterMergeAndSplitSteps(originalMapping);

        // Assert
        filtered.Should().NotContainKey(merge1);
        // NodeA should either not be in the mapping or have no parents
        if (filtered.ContainsKey(nodeA))
        {
            filtered[nodeA].Should().BeEmpty("NodeA should have no parents if merge had none");
        }
    }


    /// <summary>
    /// Helper method to invoke the private FilterMergeAndSplitSteps method using reflection.
    /// </summary>
    private static Dictionary<IStep, HashSet<IStep>> InvokeFilterMergeAndSplitSteps(Dictionary<IStep, HashSet<IStep>> nodeToParents)
    {
        var method = typeof(SavedRecipe).GetMethod(
            "FilterMergeAndSplitSteps",
            BindingFlags.NonPublic | BindingFlags.Static);

        method.Should().NotBeNull("FilterMergeAndSplitSteps method should exist");

        var result = method!.Invoke(null, [nodeToParents]);
        return (Dictionary<IStep, HashSet<IStep>>)result!;
    }
}