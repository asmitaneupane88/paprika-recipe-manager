using RecipeApp.Models.RecipeSteps;

namespace RecipeApp.Models;

/// <summary>
/// Methods for converting a recipe graph into a nested list.
/// </summary>
public partial class SavedRecipe : IAutosavingClass<SavedRecipe>
{

    /// <summary>
    /// Returns a deep nested list representation of the recipe steps.
    /// </summary>
    [JsonIgnore] public object NestedListRepresentation => GetNestedListRepresentation(RootStepNode ?? new StartStep());


    /// <summary>
    /// Builds a deep nested list representation of the recipe steps. Parallel steps are represented as a HashSet of objects. Method assumes that the graph is acyclic.
    /// </summary>
    /// <returns>
    /// A deep nested list representation of the recipe steps.
    /// </returns>
    /// <example>
    ///     new List&lt;object&gt;{
    ///     A,
    ///     new List&lt;object&gt; {
    ///         B,
    ///         new HashSet&lt;object&gt;{
    ///             tE,
    ///             D,
    ///             new List&lt;object&gt;{
    ///                 C,
    ///                 new HashSet&lt;object&gt;{
    ///                     F,
    ///                     G,
    ///                 }
    ///             },
    ///         }
    ///     }
    /// };
    /// </example>
    public static object GetNestedListRepresentation(IStep rootStep)
    {
        // First step: Create a mapping of each node to its parent node(s)
        // var nodeToParents = BuildNodeToParentsMapping(rootStep);

        // Second, build a mapping of each node to it's branch depth.
        return rootStep;
    }


    
    /// <summary>
    /// Recursively creates a HashSet of forward graph edges.
    /// </summary>
    /// <param name="currentStep">The current step to build from.</param>
    /// <returns>A HashSet of tuples containing each (child, parent) pair.</returns>
    public static HashSet<Tuple<IStep, IStep>> BuildForwardEdges(IStep currentStep, HashSet<Tuple<IStep, IStep>>? ForwardEdges = null, HashSet<IStep>? visited = null)
    {
        // Initialize the ForwardEdges HashSet if it is null.
        ForwardEdges ??= new HashSet<Tuple<IStep, IStep>>();
        visited ??= new HashSet<IStep>();

        // Skip if already visited
        if (!visited.Add(currentStep))
        {
            return ForwardEdges;
        }

        // Guard against a null RootStepNode.
        var rootStepIsInvalid = currentStep is StartStep && (currentStep.GetOutNodes() == null || currentStep.GetOutNodes().Count == 0);

        // Fail early
        if (rootStepIsInvalid){
            return ForwardEdges;
        }

        // Break recursion
        if (currentStep is FinishStep){
            return ForwardEdges;
        }

        // Recursive step
        var currentOutNodes = currentStep.GetOutNodes();
        if (currentOutNodes == null)
        {
            return ForwardEdges;
        }

        foreach (var outNode in currentOutNodes)
        {
            if (outNode.Next is not null)
            {
                // Add edge: (parent: currentStep, child: outNode.Next)
                ForwardEdges.Add(new Tuple<IStep, IStep>(currentStep, outNode.Next));
                
                // Recursively process the next step (will skip if already visited)
                BuildForwardEdges(outNode.Next, ForwardEdges, visited);
            }
        }
        
        return ForwardEdges;
            
    }


    /// <summary>
    /// Filters out merge steps and split steps from the parent mapping, connecting their parents directly to their children.
    /// For example, if NodeA and NodeB both connect to Merge1, and Merge1 connects to NodeC,
    /// the result will be NodeA and NodeB both connect directly to NodeC.
    /// </summary>
    /// <param name="nodeToParents">The original parent mapping dictionary.</param>
    /// <returns>A new dictionary with merge and split steps removed, and their parents connected directly to their children.</returns>
    private static Dictionary<IStep, HashSet<IStep>> FilterMergeAndSplitSteps(Dictionary<IStep, HashSet<IStep>> nodeToParents)
    {
        var filteredMapping = new Dictionary<IStep, HashSet<IStep>>();
        var mergeAndSplitSteps = new HashSet<IStep>();

        // First pass: identify all merge and split steps
        foreach (var node in nodeToParents.Keys)
        {
            if (node is MergeStep or SplitStep)
            {
                mergeAndSplitSteps.Add(node);
            }
        }

        // Second pass: build the filtered mapping
        foreach (var kvp in nodeToParents)
        {
            var node = kvp.Key;
            var parents = kvp.Value;

            // Skip merge and split steps themselves
            if (mergeAndSplitSteps.Contains(node))
            {
                continue;
            }

            // Initialize the filtered parent set for this node
            if (!filteredMapping.ContainsKey(node))
            {
                filteredMapping[node] = new HashSet<IStep>();
            }

            // Process each parent
            foreach (var parent in parents)
            {
                if (mergeAndSplitSteps.Contains(parent))
                {
                    // Parent is a merge/split step - need to find its parents and connect them
                    var mergeSplitParents = GetEffectiveParents(parent, nodeToParents, mergeAndSplitSteps);
                    foreach (var effectiveParent in mergeSplitParents)
                    {
                        filteredMapping[node].Add(effectiveParent);
                    }
                }
                else
                {
                    // Parent is a regular step - add it directly
                    filteredMapping[node].Add(parent);
                }
            }
        }

        // Third pass: handle children of merge/split steps that weren't in the original mapping
        // (e.g., if a merge step connects to a node that had no other parents)
        foreach (var mergeSplitStep in mergeAndSplitSteps)
        {
            var mergeSplitParents = GetEffectiveParents(mergeSplitStep, nodeToParents, mergeAndSplitSteps);
            
            // Get all children of this merge/split step
            foreach (var outNode in mergeSplitStep.GetOutNodes() ?? [])
            {
                if (outNode.Next is not null && !mergeAndSplitSteps.Contains(outNode.Next))
                {
                    // Ensure the child is in the filtered mapping
                    if (!filteredMapping.ContainsKey(outNode.Next))
                    {
                        filteredMapping[outNode.Next] = new HashSet<IStep>();
                    }
                    
                    // Connect the merge/split step's parents directly to its children
                    foreach (var effectiveParent in mergeSplitParents)
                    {
                        filteredMapping[outNode.Next].Add(effectiveParent);
                    }
                }
            }
        }

        return filteredMapping;
    }

    /// <summary>
    /// Recursively gets the effective parents of a node, bypassing merge and split steps.
    /// </summary>
    /// <param name="node">The node to get effective parents for.</param>
    /// <param name="nodeToParents">The original parent mapping.</param>
    /// <param name="mergeAndSplitSteps">Set of merge and split steps to bypass.</param>
    /// <returns>A set of effective parents (non-merge/split steps).</returns>
    private static HashSet<IStep> GetEffectiveParents(IStep node, Dictionary<IStep, HashSet<IStep>> nodeToParents, HashSet<IStep> mergeAndSplitSteps)
    {
        var effectiveParents = new HashSet<IStep>();
        
        if (!nodeToParents.ContainsKey(node))
        {
            return effectiveParents;
        }

        foreach (var parent in nodeToParents[node])
        {
            if (mergeAndSplitSteps.Contains(parent))
            {
                // Recursively get the effective parents of this merge/split step
                var recursiveParents = GetEffectiveParents(parent, nodeToParents, mergeAndSplitSteps);
                foreach (var recursiveParent in recursiveParents)
                {
                    effectiveParents.Add(recursiveParent);
                }
            }
            else
            {
                // Regular parent - add it directly
                effectiveParents.Add(parent);
            }
        }

        return effectiveParents;
    }
    

    // public static object RecurseGraph(IStep currentStep)
    // {
    //     if (currentStep is SplitStep)
    //     {
            
    //     }
    // }
    
    public Dictionary<IStep, Dictionary<string, object>> GetNodeProperties()
    {
        // store the result
        var result = new Dictionary<IStep, Dictionary<string, object>>();

        // if there are no steps
        if (RootStepNode == null)
        {
            return result;
        }

        // Using Breadth-first search to map nodes -> properties
        var allSteps = new HashSet<IStep>();
        var queue = new Queue<IStep>();
        queue.Enqueue(RootStepNode);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!allSteps.Add(current)) continue; // Skip if the node has already been visited

            // Create properties dictionary for this step
            var properties = current.StepProperties;

            // Add properties to the dictionary
            result[current] = properties;

            // Enqueue connected steps
            foreach (var outNode in current.GetOutNodes() ?? [])
            {
                var nextStep = outNode.Next;
                if (nextStep is not null)
                    queue.Enqueue(nextStep);
            }
        }
        return result;


    }

    [JsonIgnore] public ObservableCollection<RecipeIngredient> Ingredients
    {
        get
        {
            if (RootStepNode == null) return [];

        var pathInfo = RootStepNode.GetNestedPathInfo();
        if (pathInfo.Count == 0) return [];

        // Return MaxIngredients from the first path (contains all ingredients)
        return pathInfo[0].MaxIngredients ?? [];
        }

    }


}


