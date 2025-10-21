using Windows.Foundation;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Shapes;
using RecipeApp.Controls.StepControls;
using RecipeApp.Models.RecipeSteps;
using Uno.Extensions.Specialized;

namespace RecipeApp.Controls.Pages;

public sealed partial class StepEditor : NavigatorPage
{
    private SavedRecipe _savedRecipe;

    private bool _pointerPressed;

    private (Ellipse, OutNode, StepConnectorLine, IStepControl)? _draggingNode;
    private (IStepControl, IStep, Point)? _draggingStep;
    private IStepControl? _selectedStep;

    private Dictionary<Ellipse, (StepConnectorLine, OutNode, InNode)> NodeLines = [];
    
    public StepEditor(SavedRecipe sr, Navigator? nav = null) : base(nav)
    {
        this.InitializeComponent();
        
        _savedRecipe = sr;
        
        // got to add everything at once and then update the connections between nodes
        
        AddStep(new StartStep { Paths=[ (new OutNode("Microwave", null), 5), (new OutNode("Oven", null), 10) ] });
        AddStep(new TextStep { MinutesToComplete = 1, Title = "Preheat oven to 425", OutNodes= [ new OutNode("Next", null) ] });
        AddStep(new TextStep { MinutesToComplete = 1, Title = "Put mini pizza in oven", OutNodes= [ new OutNode("Next", null) ] });
        AddStep(new TextStep { MinutesToComplete = 1, Title = "Put mini pizza in microwave", OutNodes= [ new OutNode("Next", null) ] });
        AddStep(new TimerStep { Title = "Set microwave to 1.5 minutes", MinutesToComplete = 1.5});
        AddStep(new TimerStep { Title = "Cook for 8 minutes", MinutesToComplete = 8});
        AddStep(new TextStep { MinutesToComplete = 1, Title = "Is the mini pizza cooked?", OutNodes= [ new OutNode("Yes", null), new OutNode("No", null) ] });
        AddStep(new TimerStep { Title = "Cook for another 2 minutes", MinutesToComplete = 2});
        AddStep(new SplitStep());
        AddStep(new TextStep { MinutesToComplete = 1, Title = "Turn off the oven", OutNodes= [ new OutNode("Next", null) ] });
        AddStep(new TextStep { MinutesToComplete = 1, Title = "Take the mini pizza out of the oven", OutNodes= [ new OutNode("Next", null) ] });
        AddStep(new TextStep { MinutesToComplete = 1, Title = "Take the mini pizza out of the microwave", OutNodes= [ new OutNode("Next", null) ] });
        AddStep(new TextStep { MinutesToComplete = 1, Title = "Cut the mini pizza into 4 slices", OutNodes= [ new OutNode("Next", null) ] });
        AddStep(new MergeStep { });
        AddStep(new MergeStep { });
        AddStep(new FinishStep { });
    }

    private void StepCanvas_OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        
    }

    private void StepCanvas_OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        PointerUp();
    }

    private PointerRoutedEventArgs? _last;
    private void StepCanvas_OnPointerMoved(object? sender, PointerRoutedEventArgs? e)
    {
        if (!_pointerPressed) return;

        var mouseLocation = (e??_last)?.GetCurrentPoint(StepCanvas).Position;
        if (mouseLocation is null) return;
        if (e is not null) _last = e;
        
        if (_draggingStep?.Item1 is { } startWidget)
        {
            var newPos = new Point(mouseLocation.Value.X - _draggingStep.Value.Item3.X, mouseLocation.Value.Y - _draggingStep.Value.Item3.Y);
            startWidget.SetValue(Canvas.LeftProperty, newPos.X);
            startWidget.SetValue(Canvas.TopProperty, newPos.Y);
            startWidget.Step.X = newPos.X;
            startWidget.Step.Y = newPos.Y;
        }

        _draggingNode?.Item3.SetEndLocation(mouseLocation, null);
    }

    private void StepCanvas_OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        PointerUp();
    }

    private void UpdateSelect()
    {
        foreach (IStepControl control in StepCanvas.Where(c => c is IStepControl))
        {
            control.ToggleSelection(false);
        }
        
        if (_selectedStep is IStepControl selectedStartWidget)
        {
            selectedStartWidget.ToggleSelection(true);
        }
    }

    private void AddStep(IStep step)
    {
        IStepControl widget = step switch
        {
            StartStep startStep => new StartWidget(startStep),
            FinishStep finishStep => new FinishWidget(finishStep),
            MergeStep mergeStep => new MergeWidget(mergeStep),
            SplitStep splitStep => new SplitWidget(splitStep), //TODO
            TimerStep timerStep => new TimeWidget(timerStep), //TODO
            TextStep textStep => new TextWidget(textStep), //TODO
            _ => throw new ArgumentOutOfRangeException(nameof(step), step, null)
        };

        widget.CardMouseDown += WidgetOnCardMouseDown;
        widget.CardMouseUp += WidgetOnCardMouseUp;
        widget.OutNodeMouseDown += WidgetOnOutNodeMouseDown;
        widget.OutNodeMouseUp += WidgetOnOutNodeMouseUp;
        widget.InNodeMouseDown += WidgetOnInNodeMouseDown;
        widget.InNodeMouseUp += WidgetOnInNodeMouseUp;
            
        StepCanvas.Children.Add(widget);
        _selectedStep = widget;
        UpdateSelect();
        
    }

    private void WidgetOnInNodeMouseUp(Ellipse arg1, InNode arg2, IStepControl arg3)
    {
        if (arg2.Source is null && _draggingNode is { Item4: { } stepControl } && stepControl != arg3)
        {
            _draggingNode?.Item3.SetEndLocation(null, arg1);
            
            arg2.Source = _draggingNode?.Item1;
            _draggingNode?.Item2.Next = _draggingNode?.Item4?.Step;
            NodeLines.Add(_draggingNode?.Item1!, (_draggingNode?.Item3!, _draggingNode?.Item2!, arg2));
        }

        PointerUp();
    }

    private void WidgetOnInNodeMouseDown(Ellipse arg1, InNode arg2, IStepControl arg3)
    {
        if (arg2.Source is { } source&& NodeLines.Remove(source, out var line))
        {
            _draggingNode = (source, line.Item2, line.Item1, arg3);
            arg2.Source = null;
            UpdateControlNodes(false, true, arg3);
        }
        
        _pointerPressed = true;
    }

    private void WidgetOnOutNodeMouseUp(Ellipse arg1, OutNode arg2, IStepControl arg3)
    {
        PointerUp();
    }

    private void WidgetOnOutNodeMouseDown(Ellipse arg1, OutNode arg2, IStepControl arg3)
    {
        
        if (NodeLines.Remove(arg1, out var pair))
        {
            _draggingNode = (arg1, pair.Item2, pair.Item1, arg3);
            pair.Item3.Source = null;
        }
        else
        {
            _draggingNode = (arg1, arg2, new StepConnectorLine(arg1, StepCanvas), arg3);
        }
        
        _pointerPressed = true;
        
        UpdateControlNodes(false, true, arg3);
    }

    private void WidgetOnCardMouseUp(IStepControl stepControl)
    {
        PointerUp();
    }

    private void WidgetOnCardMouseDown(IStepControl stepControl, Point point)
    {
        // if (_draggingNode is not null) return;
        
        _draggingStep = (stepControl, stepControl.Step, point);
        _selectedStep = stepControl;
        _pointerPressed = true;
        UpdateSelect();
    }

    private void PointerUp()
    {
        _pointerPressed = false;
        _draggingStep = null;

        if (_draggingNode is { } tuple && tuple.Item3._endControl is null)
        {
            tuple.Item2.Next = null;
            NodeLines.Remove(_draggingNode?.Item1!);
            tuple.Item3.Dispose();
        }
        
        _draggingNode = null;
        
        UpdateControlNodes(true, false);
    }

    private void UpdateControlNodes(bool outNodeActive, bool inNodeActive, IStepControl? stepControl = null)
    {
        foreach (IStepControl control in StepCanvas.Where(c => c is IStepControl))
        {
            control.UpdateActiveNodes(outNodeActive, inNodeActive && control != stepControl);
        }
    }

    private void ScrollViewer_OnViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
    {
        StepCanvas_OnPointerMoved(null, null);
    }
}

