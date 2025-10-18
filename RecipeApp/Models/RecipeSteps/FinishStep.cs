namespace RecipeApp.Models.RecipeSteps;

public partial class FinishStep : IStep
{
    public override string GetTitle()
        => "Finish";

    public override string? GetDescription()
        => """
           Ends the recipe. after making sure all steps are complete (i.e. this also acts as a merge step).
           """;

    public override bool HasAnyInNode() 
        => true;

    public override bool HasMultipleInNode()
        => true;

    public override List<Node> GetOutNodes() => [];
}
