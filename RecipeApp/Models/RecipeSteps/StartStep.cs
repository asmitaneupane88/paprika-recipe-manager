namespace RecipeApp.Models.RecipeSteps;

public partial class StartStep : IStep
{
    public override string GetTitle() => "Start";

    public override string? GetDescription() 
    {
        var paths = this.GetNestedPathInfo();

        var sb = new StringBuilder();

        foreach (var path in paths)
        {
            var maxTimeString = ((path.MaxTotalTime - 0.001) > path.MinTotalTime)
                ? $"-{path.MaxTotalTime}" 
                : "";
            
            sb.AppendLine(path.OutNode.Title);
            sb.AppendLine($"    Cook time: {path.MinTotalTime}{maxTimeString} minutes");
            sb.AppendLine($"    Ingredients: ");
            foreach (var ingredient in path.MaxIngredients.OrderBy(i => i.Name))
            {
                var minIngredientString = (path.MinIngredients
                        .FirstOrDefault(i => (i.Name?.Equals(ingredient.Name, StringComparison.CurrentCultureIgnoreCase)??false))
                    is { } minIngredient && (minIngredient.Quantity + 0.001) < ingredient.Quantity)
                    ? $"{minIngredient.Quantity}-" 
                    : "";
                    
                sb.AppendLine($"        {ingredient.Name} ({minIngredientString}{ingredient.Quantity} {ingredient.Unit})");
            }
            sb.AppendLine();
        }
        
        return sb.ToString();
    }
    
    public override ObservableCollection<OutNode> GetOutNodes() 
        => Paths;

    public ObservableCollection<OutNode> Paths { get; set; } = [];
}

