namespace RecipeApp.Models.RecipeSteps;

public partial class MergeStep : IStep
{
    public override string GetTitle()
        => "Merge";
    public override string? GetDescription()
        => """
           A merge step will wait for all active steps that merge into it to complete.
           """;
    
    public override ObservableCollection<OutNode> GetOutNodes()
        => [ new("Next", NextStep) ];
    
    public IStep? NextStep;

}
