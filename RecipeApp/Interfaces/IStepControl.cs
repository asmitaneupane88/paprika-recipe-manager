using Windows.Foundation;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Shapes;
using RecipeApp.Models.RecipeSteps;
using InNode = RecipeApp.Models.RecipeSteps.InNode;

namespace RecipeApp.Interfaces;

[ObservableObject]
public abstract partial class IStepControl : UserControl
{
    public event Action<IStepControl, Point>? CardMouseDown;
    public event Action<IStepControl>? CardMouseUp;
    public event Action<Ellipse, OutNode, IStepControl> OutNodeMouseDown;
    public event Action<Ellipse, OutNode, IStepControl> OutNodeMouseUp;
    public event Action<Ellipse, InNode, IStepControl> InNodeMouseDown;
    public event Action<Ellipse, InNode, IStepControl> InNodeMouseUp;

    [ObservableProperty] public partial IStep Step { get; set; }
    
    public abstract void UpdateActiveNodes(bool outNodeActive, bool inNodeActive);
    public abstract void ToggleSelection(bool isSelected);

    protected void Node_OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Ellipse { Tag: OutNode node } ellipse) 
            InvokeOutNodeMouseDown(ellipse, node);
        else if (sender is Ellipse { Tag: InNode node2 } ellipse2) 
            InvokeInNodeMouseDown(ellipse2, node2);
        
        e.Handled = true;
    }

    protected void Node_OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Ellipse { Tag: OutNode node } ellipse) 
            InvokeOutNodeMouseUp(ellipse, node);    
        else if (sender is Ellipse { Tag: InNode node2 } ellipse2) 
            InvokeInNodeMouseUp(ellipse2, node2);
        
        e.Handled = true;
    }
    
    protected void Card_OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        InvokeCardMouseDown(this, e.GetCurrentPoint(this).Position);
        
        e.Handled = true;
    }
    
    protected void Card_OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        InvokeCardMouseUp(this);
        
        e.Handled = true;
    }
    
    protected void InvokeCardMouseDown(IStepControl control, Point point) => CardMouseDown?.Invoke(control, point);
    protected void InvokeCardMouseUp(IStepControl control) => CardMouseUp?.Invoke(control);
    protected void InvokeOutNodeMouseDown(Ellipse ellipse, OutNode outNode) => OutNodeMouseDown?.Invoke(ellipse, outNode, this);
    protected void InvokeOutNodeMouseUp(Ellipse ellipse, OutNode outNode) => OutNodeMouseUp?.Invoke(ellipse, outNode, this);
    protected void InvokeInNodeMouseDown(Ellipse ellipse, InNode node) => InNodeMouseDown?.Invoke(ellipse, node, this);
    protected void InvokeInNodeMouseUp(Ellipse ellipse, InNode node) => InNodeMouseUp?.Invoke(ellipse, node, this);

    public virtual List<InNode> GetInNodes() => [];
    
    // Used Claude Sonnet 4.5 to make the code under this (FindEllipseWithTag and the two GetEllipseForOutNode methods)
    
    public Ellipse? GetEllipseForOutNode(OutNode outNode)
    {
        return FindEllipseWithTag(this, outNode);
    }

    public Ellipse? GetEllipseForInNode(InNode inNode)
    {
        return FindEllipseWithTag(this, inNode);
    }

    private static Ellipse? FindEllipseWithTag(DependencyObject parent, object tag)
    {
        if (parent is null) return null;

        var childCount = VisualTreeHelper.GetChildrenCount(parent);
        for (var i = 0; i < childCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            // Check if current child is an Ellipse with matching tag
            if (child is Ellipse ellipse && Equals(ellipse.Tag, tag))
                return ellipse;

            // Recursively search children
            var result = FindEllipseWithTag(child, tag);
            if (result is not null)
                return result;
        }

        return null;
    }
}
