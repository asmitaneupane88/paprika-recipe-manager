namespace RecipeApp.Models.RecipeSteps;

public partial class StartStep : IStep
{
    public override string GetTitle() => "Start";

    public override string? GetDescription() 
        => """
           This is the entry point for the recipe.
           """;
    
    public override ObservableCollection<OutNode> GetOutNodes() 
        => Paths;

    public ObservableCollection<OutNode> Paths { get; set; } = [];
}

