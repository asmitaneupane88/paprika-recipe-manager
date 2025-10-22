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
    
    public override ObservableCollection<OutNode> GetOutNodes()
        => [ NextStep ];
    
    public string? Title { get;
        set
        {
            SetProperty(ref field, value);
            OnPropertyChanged(nameof(BindableTitle));
        }
    }
    
    public OutNode NextStep = new OutNode("",null);
}
