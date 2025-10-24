namespace RecipeApp.Models.RecipeSteps;

public partial class OutNode(string title, IStep? next) : ObservableObject
{
    [ObservableProperty] public partial string Title { get; set; } = title;
    [ObservableProperty] public partial IStep? Next { get; set; } = next;
    
    public OutNode() : this("", null) { }
}

