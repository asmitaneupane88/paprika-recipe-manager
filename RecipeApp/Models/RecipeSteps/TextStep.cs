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
    
    public override ObservableCollection<OutNode> GetOutNodes() => OutNodes;

    public string? Title { get;
        set
        {
            SetProperty(ref field, value);
            OnPropertyChanged(nameof(BindableTitle));
        }
    }
    [ObservableProperty] public partial string? Instructions { get; set; }
    
    public ObservableCollection<OutNode> OutNodes { get; set; } = [];
}
