namespace RecipeApp.Models.RecipeSteps;

public partial class StartStep : IStep
{
    public override string GetTitle() => "Start";

    public override string? GetDescription() 
        => """
           This is the entry point for the recipe.
           """;
    
    public override List<OutNode> GetOutNodes() 
        => Paths.Select(p => p.outNode).ToList();

    public List<(OutNode outNode, double prepTime)> Paths { get; set; } = [];
}

