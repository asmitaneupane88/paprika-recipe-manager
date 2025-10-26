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
    /// The X coordinate of where the step is on the edit canvas
    /// </summary>
    [ObservableProperty] public partial double X { get; set; } = 0;
    /// <summary>
    /// The Y coordinate of where the step is on the edit canvas
    /// </summary>
    [ObservableProperty] public partial double Y { get; set; } = 0;

    [ObservableProperty] public partial double MinutesToComplete { get; set; } = 0;
    [ObservableProperty] public partial ObservableCollection<RecipeIngredient> IngredientsToUse { get; set; } = [];
    
    /// <summary>
    /// Recursively searches through each out node of the current step to find the minimum and maximum time and ingredients.
    /// Additionally, this function verifies that a path is valid.
    /// </summary>
    /// <param name="visitedSteps">keep this as null, internal early to prevent an infinite loop</param>
    /// <returns></returns>
    public List<PathInfo> GetNestedPathInfo(List<IStep>? visitedSteps = null)
    {
        visitedSteps ??= [];
        
        visitedSteps.Add(this);
        
        switch (this)
        {
            case StartStep startStep:
            {
                List<PathInfo> returnInfo = [];

                foreach (var pathPair in startStep.Paths)
                    if (pathPair.Next is not null)
                        returnInfo.Add(pathPair.Next.GetNestedPathInfo(visitedSteps).First() with { OutNode = pathPair, PrepTime = startStep.MinutesToComplete});
                    else
                        returnInfo.Add(new PathInfo(pathPair, false,[], []));
            
                return returnInfo;
            }
            case FinishStep:
                return [ new PathInfo(null!, true, IngredientsToUse, IngredientsToUse, 0, 0, 0, MinutesToComplete) ];
            default:
            {
                var outNodes = GetOutNodes();
                var outPaths = outNodes
                    .Select(n => (n.Next?.GetNestedPathInfo().First() ?? new PathInfo(n, false, [], [])) with { OutNode = n }) //TODO clean up this line
                    .ToList();
            
                if (outPaths.Count == 0) return [ new PathInfo(null, false, [], []) ];

                if (this is SplitStep)
                {
                    // got to look for the highest min cook time here
                    var minPath = outPaths.OrderByDescending(p => p.MinCookTime).First();
                    var maxPath = outPaths.OrderByDescending(p => p.MaxCookTime).First();
                
                    var newMinIngredients = CombineIngredients(minPath.MinIngredients, IngredientsToUse);
                    var newMaxIngredients = CombineIngredients(maxPath.MaxIngredients, IngredientsToUse);

                    var newMinCookTime = minPath.MinCookTime + MinutesToComplete;
                    var newMaxCookTime = maxPath.MaxCookTime + MinutesToComplete;       
                
                    return [new PathInfo(null, outPaths.All(p => p.IsValid), newMinIngredients, newMaxIngredients, minPath.PrepTime, newMinCookTime, newMaxCookTime, minPath.CleanupTime)];
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
    
    private ObservableCollection<RecipeIngredient> CombineIngredients(ObservableCollection<RecipeIngredient> x, ObservableCollection<RecipeIngredient> y)
    {
        foreach (var ingredient in y)
            if (x.FirstOrDefault(i => i.Name.Equals(ingredient.Name, StringComparison.CurrentCultureIgnoreCase) && i.Unit == ingredient.Unit) is { } existingIngredient)
                existingIngredient.Quantity += ingredient.Quantity;
            else
                x.Add(ingredient);

        return x;
    }
}

