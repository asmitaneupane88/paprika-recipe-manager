using RecipeApp.Models.RecipeSteps;

namespace RecipeApp.Interfaces;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(StartStep), "StartStep")]
[JsonDerivedType(typeof(FinishStep), "FinishStep")]
[JsonDerivedType(typeof(TextStep), "TextStep")]
[JsonDerivedType(typeof(TimerStep), "TimerStep")]
[JsonDerivedType(typeof(MergeStep), "MergeStep")]
[JsonDerivedType(typeof(SplitStep), "SplitStep")]
public abstract partial class IStep : ObservableObject
{
    public abstract string GetTitle();
    public abstract string? GetDescription();

    /// <summary>
    /// When implemented, this should return a reference to the original list of items.
    /// </summary>
    /// <returns></returns>
    public abstract ObservableCollection<OutNode> GetOutNodes();

    
    [JsonIgnore] public string BindableTitle => GetTitle();
    [JsonIgnore] public string? BindableDescription => GetDescription();
    [JsonIgnore] public ObservableCollection<OutNode> BindableGetOutNodes => GetOutNodes();

    /// <summary>
    /// A dictionary of this step's properties.
    /// </summary>
    [JsonIgnore] public Dictionary<string, object> StepProperties => GetStepProperties();

    /// <summary>
    /// The X coordinate of where the step is on the edit canvas
    /// </summary>
    [ObservableProperty] public partial double X { get; set; } = 0;
    /// <summary>
    /// The Y coordinate of where the step is on the edit canvas
    /// </summary>
    [ObservableProperty] public partial double Y { get; set; } = 0;

    [ObservableProperty] public partial double MinutesToComplete { get; set; } = 0;
    [ObservableProperty] public partial ObservableCollection<RecipeIngredient> IngredientsToUse { get; set; } = [];

    public List<MergeStep> GetAllPossibleMerges(List<IStep>? visitedSteps = null)
    {
        visitedSteps ??= [];
        
        visitedSteps.Add(this);

        var mergeSteps = new List<MergeStep>();
        foreach (var outNode in GetOutNodes())
        {
            if (outNode.Next is { } next && !visitedSteps.Contains(next))
            {
                mergeSteps.AddRange(next.GetAllPossibleMerges(visitedSteps));
            }
        }
        
        if (this is MergeStep ms)
            mergeSteps.Add(ms);

        return mergeSteps;
    }
    
    /// <summary>
    /// Recursively searches through each out node of the current step to find the minimum and maximum time and ingredients.
    /// Additionally, this function verifies that a path is valid.
    /// </summary>
    /// <param name="visitedSteps">keep this as null, internal early to prevent an infinite loop</param>
    /// <returns></returns>
    public List<PathInfo> GetNestedPathInfo(List<IStep>? visitedSteps = null)
    {
        visitedSteps ??= [];
        
        if (visitedSteps.Contains(this)) return [ new PathInfo(null, true, [], []) ];
        
        visitedSteps.Add(this);
        
        switch (this)
        {
            case StartStep startStep:
            {
                List<PathInfo> returnInfo = [];

                foreach (var pathPair in startStep.Paths)
                    if (pathPair.Next is not null)
                        returnInfo.Add(pathPair.Next.GetNestedPathInfo(visitedSteps.ToList()).First() with { OutNode = pathPair, PrepTime = startStep.MinutesToComplete});
                    else
                        returnInfo.Add(new PathInfo(pathPair, false,[], []));
            
                return returnInfo;
            }
            case FinishStep:
                return [ new PathInfo(null!, true, IngredientsToUse?.ToObservableCollection()??[], IngredientsToUse?.ToObservableCollection()??[], 0, 0, 0, MinutesToComplete) ];
            default:
            {
                var outNodes = GetOutNodes();
                
                if (this is SplitStep)
                    outNodes = outNodes.Where(n => n.Next is not null).ToObservableCollection();
                
                var outPaths = outNodes
                    .SelectMany(n => n.Next?.GetNestedPathInfo(visitedSteps)?.Select(p => p with { OutNode = n }) ?? [])
                    .ToList();

                if (outPaths.Count == 0) return [ new PathInfo(null, false, [], []) ];

                if (this is SplitStep or MergeStep)
                {
                    // got to look for the highest min cook time here
                    var minPath = outPaths.OrderByDescending(p => p.MinCookTime).First();
                    var maxPath = outPaths.OrderByDescending(p => p.MaxCookTime).First();

                    var maxIngredients = IngredientsToUse.ToObservableCollection();
                    foreach (var path in outPaths)
                        maxIngredients = CombineIngredients(maxIngredients, path.MaxIngredients);
                    
                    var minIngredients = IngredientsToUse.ToObservableCollection();
                    foreach (var path in outPaths)
                        minIngredients = CombineIngredients(minIngredients, path.MinIngredients);

                    var newMinCookTime = minPath.MinCookTime + MinutesToComplete;
                    var newMaxCookTime = maxPath.MaxCookTime + MinutesToComplete;       
                
                    return [new PathInfo(null, outPaths.All(p => p.IsValid), minIngredients, maxIngredients, minPath.PrepTime, newMinCookTime, newMaxCookTime, minPath.CleanupTime)];
                }
                else
                {
                    var minPath = outPaths.OrderBy(p => p.MinCookTime).First();
                    var maxPath = outPaths.OrderByDescending(p => p.MaxCookTime).First();

                    var newMinIngredients = CombineIngredients(minPath.MinIngredients, IngredientsToUse);
                    var newMaxIngredients = CombineIngredients(maxPath.MaxIngredients, IngredientsToUse);

                    var newMinCookTime = minPath.MinCookTime + MinutesToComplete;
                    var newMaxCookTime = maxPath.MaxCookTime + MinutesToComplete;
                
                    return [new PathInfo(null, outPaths.All(p => p.IsValid), newMinIngredients, newMaxIngredients, minPath.PrepTime, newMinCookTime, newMaxCookTime, minPath.CleanupTime)];
                }
            }
        }
    }
    
    /// <summary>
    /// Packages the step's properties into a dictionary.
    /// </summary>
    /// <returns>
    /// A dictionary of this step's properties
    /// </returns>
    private Dictionary<string, object> GetStepProperties(){
        return new Dictionary<string, object>
        {
            ["Title"] = GetTitle(),
            ["Description"] = GetDescription(),
            ["MinutesToComplete"] = MinutesToComplete,
            ["Ingredients"] = IngredientsToUse,
            ["LocX"] = X,
            ["LocY"] = Y,
            ["StepType"] = GetType().Name,
            ["IngredientsToUse"] = IngredientsToUse,
        };
    }
    private ObservableCollection<RecipeIngredient> CombineIngredients(ObservableCollection<RecipeIngredient> x, ObservableCollection<RecipeIngredient> y)
    {
        foreach (var ingredient in y??[])
            if (x?.FirstOrDefault(i => (i.Name?.Equals(ingredient.Name, StringComparison.CurrentCultureIgnoreCase)??false) && i?.Unit == ingredient?.Unit) is { } existingIngredient)
            {
                var newIngredient = new RecipeIngredient();
                {
                    newIngredient.Name = existingIngredient.Name;
                    newIngredient.Quantity = existingIngredient.Quantity + ingredient.Quantity;
                    newIngredient.ModifierNote = existingIngredient.ModifierNote;
                    newIngredient.Unit = existingIngredient.Unit;
                }

                x?.Remove(existingIngredient);
                x?.Add(newIngredient);
            }
            else
                x?.Add(ingredient);

        return x??[];
    }
}

