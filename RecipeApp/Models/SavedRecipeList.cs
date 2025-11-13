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
    [JsonIgnore] public object NestedListRepresentation => GetFilteredNestedListRepresentation();


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
    /// Removes StartStep, MergeStep, SplitStep, and FinishStep nodes from a deep nested list of recipe steps.
    /// </summary>
    /// <returns>
    /// A cleaned deep nested list representation of the recipe steps.
    /// </returns>
    public static List<object> FilterNestedList(bool isStart, List<object>? nestedList)
    {
        // On start, create a new nestedList
        if (isStart && nestedList == null)
        {
            nestedList = new List<object>();
        }

        // Return early for empty lists
        if (!isStart && nestedList?.Count == 0)
        {
            return new List<object>();
        }

        // Return early if nestedList is null (shouldn't happen when isStart is true, but handle it)
        if (nestedList == null)
        {
            return new List<object>();
        }

        // Build a new result list
        var result = new List<object>();

        foreach (var item in nestedList)
        {
            // If the item is a List
            if (item is List<object> nestedItem)
            {
                // Recurse
                var cleanList = FilterNestedList(false, nestedItem);

                // Check if the list is empty
                if (cleanList.Count > 0)
                {
                    result.Add(cleanList);
                }
            }
            // Otherwise
            else
            {
                if (item is IStep step && isActualStep(step))
                {
                    result.Add(item);
                }

            }
        }

        return result;
    }


    /// <summary>
    /// Helper method to determine if a given step is actual step.
    /// </summary>
    /// <returns>
    /// Boolean indicating if recipeStep is an actual step.
    /// </returns>
    public static bool isActualStep(IStep recipeStep) {
        if (recipeStep is TextStep || recipeStep is TimerStep)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Recursively builds a deep nested list representation of the recipe steps, then filters the list and returns it. Method assumes that the graph is acyclic.
    /// </summary>
    /// <returns>
    /// A clean deep nested list representation of the recipe steps.
    /// </returns>
    public List<object> GetFilteredNestedListRepresentation()
    {
        if (RootStepNode is null)
        {
            return new List<object>();
        }
        
        var nestedList = GetNestedListRepresentation(RootStepNode, null, null);
        return FilterNestedList(true, nestedList);
    }

    /// <summary>
    /// Builds a deep nested list representation of the recipe steps. Method assumes that the graph is acyclic.
    /// </summary>
    /// <returns>
    /// A deep nested list representation of the recipe steps.
    /// </returns>
    public static List<object> GetNestedListRepresentation(IStep? currentStep, List<List<IStep>>? possiblePaths, List<IStep>? workingList)
    {
        workingList ??= [];
        possiblePaths ??= GetPossiblePaths(currentStep!, null, null);

        var refinedList = new List<object>();

        // recomputed after recursing
        var commonParents = GetCommonParents(possiblePaths);
        var commonDescendants = GetCommonDescendants(possiblePaths);
        var uncommonElements = GetUncommonElements(possiblePaths, commonParents, commonDescendants);

        // Start
        if (commonParents.Count > 0)
        {
            foreach (var item in commonParents)
            {
                refinedList.Add(item);
            }
        }

        // Middle of the paths
        if (uncommonElements.Count > 0)
        {
            // group by first uncommon element
            var uniqueNodes = new List<IStep>();
            foreach (var path in uncommonElements)
            {
                if (path.Count > 0 && !uniqueNodes.Contains(path[0]))
                {
                    uniqueNodes.Add(path[0]);
                }
            }

            var parallelPaths = new List<object>();
            foreach (var node in uniqueNodes)
            {
                var nodePaths = new List<List<IStep>>();

                // Create a new path for each unique 1st element
                foreach (var path in uncommonElements){
                    if (path.Count > 0 && ReferenceEquals(path[0], node)){
                        nodePaths.Add(path);
                    }
                }

                // only recurse if there are valid paths
                if (nodePaths.Count > 0)
                {
                    parallelPaths.Add(GetNestedListRepresentation(null, nodePaths, workingList));
                }
            }

            if (parallelPaths.Count > 0)
            {
                refinedList.Add(parallelPaths);
            }
        }

        // End of the list
        if (commonDescendants.Count > 0)
        {
            foreach (var item in commonDescendants)
            {
                if (!refinedList.Contains(item))
                {
                    refinedList.Add(item);
                }
            }
        }

        return refinedList;
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


