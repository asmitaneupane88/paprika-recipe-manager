using RecipeApp.Models.RecipeSteps;

namespace RecipeApp.Interfaces;

public abstract partial class IStep : ObservableObject
{
    public abstract string GetTitle();
    public abstract string? GetDescription();
    public abstract bool HasAnyInNode();
    public abstract bool HasMultipleInNode();

    public abstract List<Node> GetOutNodes();

    
    [JsonIgnore] public string BindableTitle => GetTitle();
    [JsonIgnore] public string? BindableDescription => GetDescription();
    [JsonIgnore] public bool BindableHasInNode => HasAnyInNode();
    [JsonIgnore] public bool BindableHasMultipleInNodes => HasMultipleInNode();

    [JsonIgnore] public List<Node> BindableGetOutNodes => GetOutNodes();

    [ObservableProperty] public partial int X { get; set; } = 0;
    [ObservableProperty] public partial int Y { get; set; } = 0;

    [ObservableProperty] public partial double MinutesToComplete { get; set; } = 0;
    [ObservableProperty] public partial List<RecipeIngredient> IngredientsToUse { get; set; } = [];
    
    public List<RecipeIngredient> GetNestedIngredients(List<IStep>? visited = null)
    {
        visited ??= [];

        visited.Add(this);
        
        var children = GetOutNodes()
            .SelectMany(node => node.Next)
            .Where(step => !visited.Contains(step));
        
        var ingredients = children
            .SelectMany(step => step.GetNestedIngredients())
            .ToList();
        
        ingredients = ingredients
            .Concat(IngredientsToUse.Where(i => !ingredients.Contains(i)))
            .ForEach(i => i.Quantity += IngredientsToUse.FirstOrDefault(x => x.Name == i.Name)?.Quantity ?? 0)
            .ToList();
        
        return ingredients;
    }

    public double? GetCleanupTime(List<IStep>? visited = null)
    {
        visited ??= [];

        visited.Add(this);
        
        var children = GetOutNodes()
            .SelectMany(node => node.Next)
            .Where(step => !visited.Contains(step));
        
        foreach (var node in children)
        {
            if (node is FinishStep)
                return node.MinutesToComplete;
            
            if (node.GetCleanupTime() is { } cleanupTime)
                return cleanupTime;
        }
        
        return null;
    }

    public double GetCookTime(List<IStep>? visited = null)
    {
        //TODO: needs a merge step and logic for dealing with different paths to get a range for the time
        throw new NotImplementedException(); 
        
        var minutes = this is not (StartStep or FinishStep) ? MinutesToComplete : 0;
        
        visited ??= [];

        visited.Add(this);
        
        var children = GetOutNodes()
            .SelectMany(node => node.Next)
            .Where(step => !visited.Contains(step));
        
        foreach (var node in children)
        {
            minutes += node.GetCookTime(visited);
        }
        
        return minutes;
    }

    public bool ArePathsValid(List<IStep>? visited = null)
    {
        visited ??= [];
        
        if (visited.Contains(this)) return true;

        visited.Add(this);

        var outNodes = GetOutNodes();
        
        if (this is not FinishStep && outNodes.Count == 0) return false;
        
        return outNodes
            .All(node => 
                node.Next.Count != 0 
                && node.Next.All(step => step.ArePathsValid(visited))
            );
    }
}


