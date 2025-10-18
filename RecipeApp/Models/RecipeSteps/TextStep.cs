namespace RecipeApp.Models.RecipeSteps;

public partial class TextStep : IStep
{
    public override string GetTitle() 
        => Title 
        ?? "Text Step";

    public override string? GetDescription()
        => Instructions
        ?? """
           A step that can be an instruction, note, or question.
           This step supports multiple outputs to other steps.
           """;

    public override bool HasInNode() => true;

    public override List<Node> GetOutNodes() => OutNodes;

    [ObservableProperty] public partial string? Title { get; set; }
    [ObservableProperty] public partial string? Instructions { get; set; }
    
    public List<Node> OutNodes { get; set; } = [];
}
