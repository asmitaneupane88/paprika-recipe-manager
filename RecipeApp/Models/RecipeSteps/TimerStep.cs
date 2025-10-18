namespace RecipeApp.Models.RecipeSteps;

public partial class TimerStep : IStep
{
    public override string GetTitle()
        => Title 
        ?? "Timer";

    public override string? GetDescription()
        => Title is not null 
            ? null 
            : "A step that can be used to set a timer.";

    public override bool HasInNode() => false;

    public override List<Node> GetOutNodes()
        => BackNodeEnabled
            ? [ new("Next", NextStep), new("Last", LastStep) ]
            : [ new("Next", NextStep) ];
    
    [ObservableProperty] public partial string? Title { get; set; }
    [ObservableProperty] public partial bool BackNodeEnabled { get; set; }
    
    public IStep? NextStep;
    public IStep? LastStep;
}
