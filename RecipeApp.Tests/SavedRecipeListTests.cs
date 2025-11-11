using System.Reflection;
using FluentAssertions;
using RecipeApp.Interfaces;
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
    public void DeriveGraphEdgesReturnsCorrectEdges()
    {
        // Arrange
        var recipe = BuildComplexNestedRecipe();
        var edges = SavedRecipe.DeriveGraphEdges(recipe.RootStepNode!);
        var expectedEdges = new List<Tuple<IStep, IStep>>();

        // X -> split 1
        var X = new TextStep { Title = "Step X", MinutesToComplete = 5 };
        var split1 = new SplitStep();
        expectedEdges.Add(new Tuple<IStep, IStep>(X, split1));

        // split 1 -> A, B
        var A = new TextStep { Title = "Step A", MinutesToComplete = 5 };
        var B = new TextStep { Title = "Step B", MinutesToComplete = 10 };
        expectedEdges.Add(new Tuple<IStep, IStep>(split1, A));
        expectedEdges.Add(new Tuple<IStep, IStep>(split1, B));

        // Assert
        edges.Should().NotBeNull();

        // X -> split 1
        edges.Should().Contain(new Tuple<IStep, IStep>(X, split1));

        // split 1 -> A, B
        edges.Should().Contain(new Tuple<IStep, IStep>(split1, A));
        edges.Should().Contain(new Tuple<IStep, IStep>(split1, B));
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
    public void BuildNodeToParentsMapping_SimpleLinearChain_ShouldMapCorrectly()
    {
        // Arrange
        var root = new StartStep();
        var step1 = new TextStep { Title = "Step 1" };
        var step2 = new TextStep { Title = "Step 2" };
        var finish = new FinishStep();

        root.Paths = [new OutNode("Next", step1)];
        step1.OutNodes = [new OutNode("Next", step2)];
        step2.OutNodes = [new OutNode("Next", finish)];

        // Act
        var mapping = InvokeBuildNodeToParentsMapping(root);

        // Assert
        mapping.Should().NotBeNull();
        mapping.Count.Should().Be(4); // root, step1, step2, finish
        
        // Root should have no parents
        mapping[root].Should().BeEmpty();
        
        // Step1 should have root as parent
        mapping[step1].Should().ContainSingle().Which.Should().Be(root);
        
        // Step2 should have step1 as parent
        mapping[step2].Should().ContainSingle().Which.Should().Be(step1);
        
        // Finish should have step2 as parent
        mapping[finish].Should().ContainSingle().Which.Should().Be(step2);
    }

    [Test]
    public void BuildNodeToParentsMapping_MultipleParents_ShouldMapCorrectly()
    {
        // Arrange - Create a merge step with multiple parents
        var root = new StartStep();
        var step1 = new TextStep { Title = "Step 1" };
        var step2 = new TextStep { Title = "Step 2" };
        var merge = new MergeStep();
        var finish = new FinishStep();

        root.Paths = [new OutNode("Path1", step1), new OutNode("Path2", step2)];
        step1.OutNodes = [new OutNode("Next", merge)];
        step2.OutNodes = [new OutNode("Next", merge)];
        merge.NextStep = new OutNode("Next", finish);

        // Act
        var mapping = InvokeBuildNodeToParentsMapping(root);

        // Assert
        mapping.Should().NotBeNull();
        
        // Merge should have both step1 and step2 as parents
        mapping[merge].Should().HaveCount(2);
        mapping[merge].Should().Contain(step1);
        mapping[merge].Should().Contain(step2);
        
        // Step1 and step2 should both have root as parent
        mapping[step1].Should().ContainSingle().Which.Should().Be(root);
        mapping[step2].Should().ContainSingle().Which.Should().Be(root);
        
        // Finish should have merge as parent
        mapping[finish].Should().ContainSingle().Which.Should().Be(merge);
    }

    [Test]
    public void BuildNodeToParentsMapping_ComplexGraph_ShouldMapAllNodes()
    {
        // Arrange - Use the same complex graph from TestSteps
        var root = new StartStep();
        var s1 = new TextStep { MinutesToComplete = 5 };
        var s2 = new TextStep { MinutesToComplete = 3 };
        var merge0 = new MergeStep();
        var split2 = new SplitStep();
        var s5 = new TextStep { MinutesToComplete = 8 };

        root.Paths = [
            new OutNode("Oven", merge0),
            new OutNode("Microwave", split2),
        ];

        split2.OutNodes = [
            new OutNode("", merge0),
            new OutNode("", s5),
        ];

        // Act
        var mapping = InvokeBuildNodeToParentsMapping(root);

        // Assert
        mapping.Should().NotBeNull();
        
        // merge0 should have both root and split2 as parents
        mapping[merge0].Should().HaveCount(2);
        mapping[merge0].Should().Contain(root);
        mapping[merge0].Should().Contain(split2);
        
        // split2 should have root as parent
        mapping[split2].Should().ContainSingle().Which.Should().Be(root);
        
        // s5 should have split2 as parent
        mapping[s5].Should().ContainSingle().Which.Should().Be(split2);
        
        // Root should have no parents
        mapping[root].Should().BeEmpty();
    }

    [Test]
    public void BuildNodeToParentsMapping_EmptyGraph_ShouldReturnOnlyRoot()
    {
        // Arrange
        var root = new StartStep();
        root.Paths = []; // No connections

        // Act
        var mapping = InvokeBuildNodeToParentsMapping(root);

        // Assert
        mapping.Should().NotBeNull();
        mapping.Count.Should().Be(1);
        mapping[root].Should().BeEmpty();
    }

    [Test]
    public void BuildNodeToParentsMapping_SplitStep_ShouldMapCorrectly()
    {
        // Arrange
        var root = new StartStep();
        var split = new SplitStep();
        var step1 = new TextStep { Title = "Step 1" };
        var step2 = new TextStep { Title = "Step 2" };
        var merge = new MergeStep();

        root.Paths = [new OutNode("Next", split)];
        split.OutNodes = [
            new OutNode("Path1", step1),
            new OutNode("Path2", step2)
        ];
        step1.OutNodes = [new OutNode("Next", merge)];
        step2.OutNodes = [new OutNode("Next", merge)];

        // Act
        var mapping = InvokeBuildNodeToParentsMapping(root);

        // Assert
        mapping.Should().NotBeNull();
        
        // Split should have root as parent
        mapping[split].Should().ContainSingle().Which.Should().Be(root);
        
        // Step1 and step2 should both have split as parent
        mapping[step1].Should().ContainSingle().Which.Should().Be(split);
        mapping[step2].Should().ContainSingle().Which.Should().Be(split);
        
        // Merge should have both step1 and step2 as parents
        mapping[merge].Should().HaveCount(2);
        mapping[merge].Should().Contain(step1);
        mapping[merge].Should().Contain(step2);
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

    [Test]
    public void FilterMergeAndSplitSteps_ComplexGraph_ShouldFilterCorrectly()
    {
        // Arrange: Complex graph with multiple merges and splits
        var root = new StartStep();
        var step1 = new TextStep { Title = "Step1" };
        var step2 = new TextStep { Title = "Step2" };
        var split1 = new SplitStep();
        var step3 = new TextStep { Title = "Step3" };
        var step4 = new TextStep { Title = "Step4" };
        var merge1 = new MergeStep();
        var step5 = new TextStep { Title = "Step5" };

        root.Paths = [new OutNode("Path1", step1), new OutNode("Path2", step2)];
        split1.OutNodes = [new OutNode("Path1", step3), new OutNode("Path2", step4)];
        step1.OutNodes = [new OutNode("Next", split1)];
        step2.OutNodes = [new OutNode("Next", split1)];
        merge1.NextStep = new OutNode("Next", step5);
        step3.OutNodes = [new OutNode("Next", merge1)];
        step4.OutNodes = [new OutNode("Next", merge1)];

        var originalMapping = InvokeBuildNodeToParentsMapping(root);

        // Act
        var filtered = InvokeFilterMergeAndSplitSteps(originalMapping);

        // Assert
        filtered.Should().NotContainKey(split1);
        filtered.Should().NotContainKey(merge1);
        
        // Step3 and Step4 should have root as parent (via step1 and step2, but since step1/step2 connect to split, they should connect directly)
        // Actually, step1 and step2 both connect to split1, so step3 and step4 should have both step1 and step2 as parents
        filtered[step3].Should().HaveCount(2);
        filtered[step3].Should().Contain(step1);
        filtered[step3].Should().Contain(step2);
        
        filtered[step4].Should().HaveCount(2);
        filtered[step4].Should().Contain(step1);
        filtered[step4].Should().Contain(step2);
        
        // Step5 should have both step3 and step4 as parents
        filtered[step5].Should().HaveCount(2);
        filtered[step5].Should().Contain(step3);
        filtered[step5].Should().Contain(step4);
    }

    /// <summary>
    /// Helper method to invoke the private BuildNodeToParentsMapping method using reflection.
    /// </summary>
    private static Dictionary<IStep, HashSet<IStep>> InvokeBuildNodeToParentsMapping(IStep rootStep)
    {
        var method = typeof(SavedRecipe).GetMethod(
            "BuildNodeToParentsMapping",
            BindingFlags.NonPublic | BindingFlags.Static);

        method.Should().NotBeNull("BuildNodeToParentsMapping method should exist");

        var result = method!.Invoke(null, [rootStep]);
        return (Dictionary<IStep, HashSet<IStep>>)result!;
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