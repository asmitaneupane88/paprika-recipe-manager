namespace RecipeApp.Models.RecipeSteps;

public partial class FinishStep : IStep
{
    public override string GetTitle()
        => "Finish";

    public override string? GetDescription()
        => null;

    public override bool HasInNode() 
        => true;

    public override List<Node> GetOutNodes() => [];
}
