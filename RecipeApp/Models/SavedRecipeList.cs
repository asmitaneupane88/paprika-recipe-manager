using RecipeApp.Models.RecipeSteps;
using UglyToad.PdfPig.Core;

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

    public static List<List<IStep>> GetPossiblePaths(IStep currentStep, List<List<IStep>>? possiblePaths, List<IStep>? currentPath){
        possiblePaths ??= [];
        currentPath ??= [];

        // Guard against a null RootStepNode.
        var rootStepIsInvalid = currentStep is StartStep && (currentStep.GetOutNodes() == null);

        // Fail early / break recursion
        if (rootStepIsInvalid){
            return [];
        }

        // If the node has no children, it's a leaf node.
        if (currentStep.GetOutNodes().Count == 0){
            currentPath.Add(currentStep);
            possiblePaths.Add(currentPath);
            return possiblePaths;
        }

        foreach (var node in currentStep.GetOutNodes())
        {
            var branchPath = new List<IStep>(currentPath) { currentStep };
            if (node.Next is not null)
            {
                GetPossiblePaths(node.Next, possiblePaths, branchPath);
            }
        }

        return possiblePaths;
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
        // First, get a HashSet of forward edges
        //var forwardEdges = BuildForwardEdges(rootStep);

        // Second, build a mapping of each node to it's branch depth.
        //var branchDepths = BuildBranchDepths(rootStep);

        return new List<IStep>();

        // {start, A, split1, B, D, ~, }
        // [start, A, split1, B, D, MERGE, C, split2, E, merge2, F, G, END]
        
    }

    
    
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


