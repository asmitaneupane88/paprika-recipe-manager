using RecipeApp.Models.RecipeSteps;

namespace RecipeApp.Models;

/// <summary>
/// Methods for converting a recipe graph into a nested list.
/// </summary>
public partial class SavedRecipe : IAutosavingClass<SavedRecipe>, IRecipe
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
    /// Recursively builds a deep nested list representation of the recipe steps. Parallel steps are represented as a HashSet of objects. Method assumes that the graph is acyclic.
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
        var result = new List<object>();

        // Fail early 
        if (rootStep == null || rootStep.GetOutNodes().Count == 0)
            return result;

        // Still working on this
        return result;
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


