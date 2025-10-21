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

    public override ObservableCollection<OutNode> GetOutNodes()
        => OutNodes;
    
    public ObservableCollection<OutNode> OutNodes { get; set; } = [];

}
