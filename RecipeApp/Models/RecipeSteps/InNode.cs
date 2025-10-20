using Microsoft.UI.Xaml.Shapes;

namespace RecipeApp.Models.RecipeSteps;

public partial class InNode(Ellipse? source, double size) : ObservableObject
{
    [ObservableProperty] public partial Ellipse? Source { get; set; } = source;
    [ObservableProperty] public partial double Size { get; set; } = size;
}
