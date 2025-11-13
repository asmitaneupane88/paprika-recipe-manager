using RecipeApp.Models.RecipeSteps;
using System.Linq;
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


    /// <summary>
    /// Returns a list of all possible paths that could be taken in the graph.
    /// </summary>
    /// <param name="currentStep">The current step node being visited in the graph traversal.</param>
    /// <param name="possiblePaths">A list to collect all identified possible paths; may be null if starting fresh.</param>
    /// <param name="currentPath">The current path being constructed during traversal; may be null if starting fresh.</param>
    public static List<List<IStep>> GetPossiblePaths(IStep currentStep, List<List<IStep>>? possiblePaths, List<IStep>? currentPath){
         // A.I. translation of A.I. generated python code

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
    /// Finds the longest common prefix (common parents) among all paths.
    /// </summary>
    /// <param name="paths">List of paths to analyze.</param>
    /// <returns>List of steps that are common to the beginning of all paths.</returns>
    public static List<IStep> GetCommonParents(List<List<IStep>> paths)
    {
        // A.I. translation of A.I. generated python code
        
        if (paths == null || paths.Count == 0)
        {
            return [];
        }

        // Find the longest common prefix among all paths
        var commonPrefix = new List<IStep>(paths[0]);

        for (int i = 1; i < paths.Count; i++)
        {
            var tempPrefix = new List<IStep>();
            var path = paths[i];

            for (int j = 0; j < Math.Min(commonPrefix.Count, path.Count); j++)
            {
                if (ReferenceEquals(commonPrefix[j], path[j]))
                {
                    tempPrefix.Add(commonPrefix[j]);
                }
                else
                {
                    break;
                }
            }

            commonPrefix = tempPrefix;
        }

        return commonPrefix;
    }

    /// <summary>
    /// Finds the longest common suffix (common descendants) among all paths.
    /// </summary>
    /// <param name="paths">List of paths to analyze.</param>
    /// /// <returns>List of steps that are common to the end of all paths.</returns>
    public static List<IStep> GetCommonDescendants(List<List<IStep>> paths)
    {
        // A.I. translation of A.I. generated python code

        if (paths == null || paths.Count == 0)
        {
            return [];
        }

        // Find the longest common suffix among all paths
        var firstPath = paths[0];
        var commonSuffix = new List<IStep>(firstPath);
        commonSuffix.Reverse();

        for (int i = 1; i < paths.Count; i++)
        {
            var tempSuffix = new List<IStep>();
            var path = paths[i];
            var reversedPath = new List<IStep>(path);
            reversedPath.Reverse();

            for (int j = 0; j < Math.Min(commonSuffix.Count, reversedPath.Count); j++)
            {
                if (ReferenceEquals(commonSuffix[j], reversedPath[j]))
                {
                    tempSuffix.Add(commonSuffix[j]);
                }
                else
                {
                    break;
                }
            }

            commonSuffix = tempSuffix;
        }

        commonSuffix.Reverse();
        return commonSuffix;
    }

    /// <summary>
    /// Extracts the uncommon elements from each path (the portion between common parents and common descendants).
    /// </summary>
    /// <param name="paths">List of paths to analyze.</param>
    /// <param name="commonParents">Common prefix steps.</param>
    /// <param name="commonDescendants">Common suffix steps.</param>
    /// <returns>List of paths containing only the uncommon portions.</returns>
    public static List<List<IStep>> GetUncommonElements(
        List<List<IStep>> paths,
        List<IStep> commonParents,
        List<IStep> commonDescendants)
    {
        // A.I. translation of A.I. generated python code

        var uncommonElements = new List<List<IStep>>();

        foreach (var path in paths)
        {
            // Extract the portion of the path between common parents and common descendants
            var startIndex = commonParents.Count;
            var endIndex = path.Count - commonDescendants.Count;

            if (startIndex < endIndex)
            {
                // Python: path[start_index:end_index] - slice from startIndex to endIndex (exclusive)
                var uncommonPortion = new List<IStep>();
                for (int i = startIndex; i < endIndex; i++)
                {
                    uncommonPortion.Add(path[i]);
                }
                uncommonElements.Add(uncommonPortion);
            }
            else
            {
                // Add empty list when there's no uncommon portion
                uncommonElements.Add([]);
            }
        }

        return uncommonElements;
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


