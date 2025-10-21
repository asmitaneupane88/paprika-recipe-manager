namespace RecipeApp.Models.RecipeSteps;

public partial class StartStep : IStep
{
    public override string GetTitle() => "Start";

    public override string? GetDescription() 
        => """
           This is the entry point for the recipe.
           """;
    
    public override List<OutNode> GetOutNodes() 
        => Paths;

    public List<OutNode> Paths { get; set; } = [];
}

