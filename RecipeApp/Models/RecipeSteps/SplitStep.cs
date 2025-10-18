namespace RecipeApp.Models.RecipeSteps;

public class SplitStep : IStep
{
    public override string GetTitle()
    {
        throw new NotImplementedException();
    }

    public override string? GetDescription()
    {
        throw new NotImplementedException();
    }

    public override List<Node> GetOutNodes()
        => OutNodes;
    
    public List<Node> OutNodes { get; set; } = [];

}
