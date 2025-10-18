namespace RecipeApp.Models.RecipeSteps;

public partial class StartStep : IStep
{
    public override string GetTitle() => "Start";

    public override string? GetDescription() => null;

    public override bool HasInNode() => false;

    public override List<Node> GetOutNodes() => [ new("Start", NextStep), ];

    public IStep? NextStep;

    public double? GetPrepTime() => MinutesToComplete;
    public double GetTotalTime() 
        => GetPrepTime() ?? 0 
        +  GetCookTime() 
        +  GetCleanupTime() ?? 0;
}
