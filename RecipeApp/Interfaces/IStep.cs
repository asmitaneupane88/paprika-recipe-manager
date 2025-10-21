using RecipeApp.Models.RecipeSteps;

namespace RecipeApp.Interfaces;

public abstract partial class IStep : ObservableObject
{
    public abstract string GetTitle();
    public abstract string? GetDescription();

    public abstract ObservableCollection<OutNode> GetOutNodes();

    
    [JsonIgnore] public string BindableTitle => GetTitle();
    [JsonIgnore] public string? BindableDescription => GetDescription();
    [JsonIgnore] public ObservableCollection<OutNode> BindableGetOutNodes => GetOutNodes();

    [ObservableProperty] public partial double X { get; set; } = 0;
    [ObservableProperty] public partial double Y { get; set; } = 0;

    [ObservableProperty] public partial double MinutesToComplete { get; set; } = 0;
    [ObservableProperty] public partial List<RecipeIngredient> IngredientsToUse { get; set; } = [];
    

    public List<PathInfo> GetNestedPathInfo(List<IStep>? visitedSteps = null)
    {
        visitedSteps ??= [];
        
        visitedSteps.Add(this);
        
        if (this is StartStep startStep)
        {
            List<PathInfo> returnInfo = [];

            foreach (var pathPair in startStep.Paths)
                if (pathPair.Next is not null)
                    returnInfo.Add(pathPair.Next.GetNestedPathInfo(visitedSteps).First() with { OutNode = pathPair, PrepTime = startStep.MinutesToComplete});
                else
                    returnInfo.Add(new PathInfo(pathPair, false,[], []));
            
            return returnInfo;
        }
        else if (this is FinishStep)
        {
            return [ new PathInfo(null!, true, IngredientsToUse, IngredientsToUse, 0, 0, 0, MinutesToComplete) ];
        }
        else
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
    
    private List<RecipeIngredient> CombineIngredients(List<RecipeIngredient> x, List<RecipeIngredient> y)
    {
        foreach (var ingredient in y)
            if (x.FirstOrDefault(i => i.Name.Equals(ingredient.Name, StringComparison.CurrentCultureIgnoreCase)) is { } existingIngredient)
                existingIngredient.Quantity += ingredient.Quantity;
            else
                x.Add(ingredient);

        return x;
    }
}

