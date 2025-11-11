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
    /// Derives a list of edges from the graph. A.I. generated.
    /// </summary>
    public static List<Tuple<IStep, IStep>> DeriveGraphEdges(IStep root){
        var edges = new List<Tuple<IStep, IStep>>();
        if (root == null)
            return edges;
            
        var visitedNodes = new HashSet<IStep>();
        
        Traverse(root, visitedNodes, edges);
        return edges;
    }
    
    /// <summary>
    /// Recursively traverses the graph and builds a list of edges. A.I. generated.
    /// </summary>
    private static void Traverse(IStep currentStep, HashSet<IStep> visitedNodes, List<Tuple<IStep, IStep>> edges)
    {
        // Avoid infinite loops by checking if we've already visited this node
        if (visitedNodes.Contains(currentStep))
            return;

        visitedNodes.Add(currentStep);

        // Get all out nodes from the current step
        var outNodes = currentStep.GetOutNodes();

        // For each out node, create an edge if it has a next step
        foreach (var outNode in outNodes)
        {
            if (outNode.Next != null)
            {
                // Create an edge from current step to the next step
                edges.Add(new Tuple<IStep, IStep>(currentStep, outNode.Next));

                // Recursively traverse the next step
                Traverse(outNode.Next, visitedNodes, edges);
            }
        }
    }

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
        var nodeToParents = BuildNodeToParentsMapping(rootStep);

        // Second step: Filter out merge and split steps
        var filteredMapping = FilterMergeAndSplitSteps(nodeToParents);

        
        // TODO: Continue with nested list representation logic
        return rootStep;
    }

    
    /// <summary>
    /// Creates a mapping of each node to its parent node(s).
    /// Since nodes can have multiple parents (e.g., MergeStep), returns Dictionary&lt;IStep, HashSet&lt;IStep&gt;&gt;.
    /// </summary>
    /// <param name="rootStep">The root step to start building the parent mapping from.</param>
    /// <returns>A dictionary mapping each node to its set of parent nodes.</returns>
    private static Dictionary<IStep, HashSet<IStep>> BuildNodeToParentsMapping(IStep rootStep)
    {
        var nodeToParents = new Dictionary<IStep, HashSet<IStep>>();
        
        if (rootStep == null)
        {
            return nodeToParents;
        }
        
        // Traverse the graph and build the parent mapping
        var visitedNodes = new HashSet<IStep>();
        var queue = new Queue<IStep>();
        queue.Enqueue(rootStep);
        
        while (queue.Count > 0)
        {
            var currentStep = queue.Dequeue();
            if (!visitedNodes.Add(currentStep)) continue; // Skip if already visited
            
            // Initialize parent set for this node if it doesn't exist
            if (!nodeToParents.ContainsKey(currentStep))
            {
                nodeToParents[currentStep] = new HashSet<IStep>();
            }
            
            // Traverse all out nodes and build parent-child relationships
            foreach (var outNode in currentStep.GetOutNodes() ?? [])
            {
                if (outNode.Next is not null)
                {
                    // Add currentStep as a parent of outNode.Next
                    if (!nodeToParents.ContainsKey(outNode.Next))
                    {
                        nodeToParents[outNode.Next] = new HashSet<IStep>();
                    }
                    nodeToParents[outNode.Next].Add(currentStep);
                    
                    // Enqueue the next step for processing
                    queue.Enqueue(outNode.Next);
                }
            }
        }
        
        return nodeToParents;
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


