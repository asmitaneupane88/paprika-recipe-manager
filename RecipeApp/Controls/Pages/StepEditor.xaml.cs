using Windows.Foundation;
using Microsoft.UI.Dispatching;
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
    private IStepControl? _selectedStepField;
    private IStepControl? _selectedStep
    {
        get => _selectedStepField;
        set
        {
            _selectedStepField = value;
            Bindings.Update();
        }
    }
    public TextStep? SelectedTextStep => _selectedStep?.Step as TextStep;
    public TimerStep? SelectedTimerStep => _selectedStep?.Step as TimerStep;
    public SplitStep? SelectedSplitStep => _selectedStep?.Step as SplitStep;
    public MergeStep? SelectedMergeStep => _selectedStep?.Step as MergeStep;
    public StartStep? SelectedStartStep => _selectedStep?.Step as StartStep;
    public FinishStep? SelectedFinishStep => _selectedStep?.Step as FinishStep;

    
    public Visibility IsNotNull(object? obj)
    {
        return obj is not null ? Visibility.Visible : Visibility.Collapsed;
    }
    
    public Visibility BothNull(object? obj1, object? obj2)
    {
        return obj1 is null && obj2 is null && _selectedStep is not null ? Visibility.Visible : Visibility.Collapsed;
    }

    private Dictionary<Ellipse, (StepConnectorLine, OutNode, InNode, IStepControl)> NodeLines = [];
    
    public StepEditor(SavedRecipe sr, Navigator? nav = null) : base(nav)
    {
        this.InitializeComponent();
        
        _savedRecipe = sr;
        
        LoadSteps();
    }

    // Used Claude Sonnet 4.5 to make the LoadSteps and the methods that it calls. Also modified it a bit to work with the current codebase

    private async void LoadSteps()
    {
        _savedRecipe.RootStepNode ??= new StartStep { Paths = [ new OutNode("Start", null) ] };

        var allSteps = new HashSet<IStep>();
        var queue = new Queue<IStep>();
        queue.Enqueue(_savedRecipe.RootStepNode);
    
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!allSteps.Add(current)) continue;
        
            foreach (var outNode in current.BindableGetOutNodes ?? [])
            {
                if (outNode.Next is not null)
                    queue.Enqueue(outNode.Next);
            }
        }
    
        if (!allSteps.Any(s => s is FinishStep))
        {
            var finishStep = new FinishStep();
            allSteps.Add(finishStep);
        }
    
        var stepToControlMap = new Dictionary<IStep, IStepControl>();
        foreach (var step in allSteps)
        {
            var incomingConnectionCount = step is MergeStep mergeStep ? 
                allSteps
                    .SelectMany(step1 => step1.BindableGetOutNodes ?? [])
                    .Count(outNode => outNode.Next == mergeStep) 
                : 0 ;
            
            
            var widget = AddStep(step, incomingConnectionCount);
            stepToControlMap[step] = widget;
        }
        
        DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, () =>
            {
                RebuildConnections(stepToControlMap);
                UpdateControlNodes(true, false);
            });
    }
    
    private void RebuildConnections(Dictionary<IStep, IStepControl> stepToControlMap)
    {
        foreach (var (step, control) in stepToControlMap)
        {
            var outNodes = step.BindableGetOutNodes ?? [];

            foreach (var outNode in outNodes)
            {
                if (outNode.Next is null) continue;
                if (!stepToControlMap.TryGetValue(outNode.Next, out var targetControl)) continue;

                var targetInNodes = targetControl.GetInNodes() ?? [];
                var targetInNode = targetInNodes.FirstOrDefault(n => n.Source is null);
            
                if (targetInNode is null) continue;

                var outEllipse = control.GetEllipseForOutNode(outNode);
                var inEllipse = targetControl.GetEllipseForInNode(targetInNode);

                if (outEllipse is null || inEllipse is null) continue;

                var line = new StepConnectorLine(outEllipse, StepCanvas);
                line.SetEndLocation(null, inEllipse);

                targetInNode.Source = outEllipse;
                NodeLines.Add(outEllipse, (line, outNode, targetInNode, control));
            }
        }
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

    private IStepControl AddStep(IStep step, int connectionCount = 0)
    {
        IStepControl widget = step switch
        {
            StartStep startStep => new StartWidget(startStep),
            FinishStep finishStep => new FinishWidget(finishStep),
            MergeStep mergeStep => new MergeWidget(mergeStep, connectionCount),
            SplitStep splitStep => new SplitWidget(splitStep),
            TimerStep timerStep => new TimeWidget(timerStep),
            TextStep textStep => new TextWidget(textStep),
            _ => throw new ArgumentOutOfRangeException(nameof(step), step, null)
        };

        widget.CardMouseDown += WidgetOnCardMouseDown;
        widget.CardMouseUp += WidgetOnCardMouseUp;
        widget.OutNodeMouseDown += WidgetOnOutNodeMouseDown;
        widget.OutNodeMouseUp += WidgetOnOutNodeMouseUp;
        widget.InNodeMouseDown += WidgetOnInNodeMouseDown;
        widget.InNodeMouseUp += WidgetOnInNodeMouseUp;
        
        widget.SetValue(Canvas.LeftProperty, step.X);
        widget.SetValue(Canvas.TopProperty, step.Y);
        
        StepCanvas.Children.Add(widget);
        _selectedStep = widget;
        UpdateSelect();
        
        return widget;
    }

    private void WidgetOnInNodeMouseUp(Ellipse arg1, InNode arg2, IStepControl arg3)
    {
        if (arg2.Source is null && _draggingNode is (var ellipse, var outNode, var connectorLine, var stepControl) && stepControl != arg3)
        {
            connectorLine.SetEndLocation(null, arg1);
            arg2.Source = ellipse;
            if (outNode != null)
            {
                outNode.Next = arg3.Step;
            }
            NodeLines.Add(ellipse, (connectorLine, outNode, arg2, stepControl));
        }

        PointerUp();
    }

    private void WidgetOnInNodeMouseDown(Ellipse arg1, InNode arg2, IStepControl arg3)
    {
        if (arg2.Source is { } source&& NodeLines.Remove(source, out var line))
        {
            _draggingNode = (source, line.Item2, line.Item1, line.Item4);
            arg2.Source = null;
            UpdateControlNodes(false, true, line.Item4);
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

    private void ButtonAddText_OnClick(object sender, RoutedEventArgs e)
    {
        AddStep(new TextStep { OutNodes = [ new OutNode("Next", null) ]});
    }

    private void ButtonAddTimer_OnClick(object sender, RoutedEventArgs e)
    {
        AddStep(new TimerStep());
    }

    private void ButtonAddSplit_OnClick(object sender, RoutedEventArgs e)
    {
        AddStep(new SplitStep());
    }

    private void ButtonAddMerge_OnClick(object sender, RoutedEventArgs e)
    {
        AddStep(new MergeStep());
    }

    private void ButtonRemoveStep_OnClick(object sender, RoutedEventArgs e)
    {
        foreach (var outNode in _selectedStep?.Step?.BindableGetOutNodes??[])
        {
            if (NodeLines.FirstOrDefault(pair => pair.Value.Item2 == outNode) is var lineToRemove)
            {
                if (lineToRemove.Value.Item3 != null)
                {
                    lineToRemove.Value.Item3.Source = null;
                }
                if (lineToRemove.Value.Item1 != null)
                {
                    lineToRemove.Value.Item1.Dispose();
                }
                if (lineToRemove.Key is not null)
                    NodeLines.Remove(lineToRemove.Key);
            }
        }
        
        foreach (var inNode in _selectedStep?.GetInNodes()??[])
        {
            if (inNode.Source is not null && NodeLines.Remove(inNode.Source, out var lineToRemove))
            {
                lineToRemove.Item3.Source = null;
                lineToRemove.Item1.Dispose();
            }
        }
        
        StepCanvas.Children.Remove(_selectedStep);
        _selectedStep = null;
        UpdateSelect();
    }

    private void ButtonAddOutNode_OnClick(object sender, RoutedEventArgs e)
    {
        if (_selectedStep?.Step is StartStep startStep)
            startStep.Paths.Add(new OutNode("", null));
        else if (_selectedStep?.Step is TextStep textStep)
            textStep.OutNodes.Add(new OutNode("", null));
    }

    private void ButtonRemoveOutNode_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: OutNode node })
        {
            if (_selectedStep?.Step is StartStep startStep)
                startStep.Paths.Remove(node);
            else if (_selectedStep?.Step is TextStep textStep)
                textStep.OutNodes.Remove(node);
            else return;

            if (NodeLines.FirstOrDefault(pair => pair.Value.Item2 == node) is { } lineToRemove)
            {
                lineToRemove.Value.Item3.Source = null;
                lineToRemove.Value.Item1.Dispose();
                NodeLines.Remove(lineToRemove.Key);
            }
        }
    }

    private void ButtonRemoveIngredient_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: RecipeIngredient ingredient })
            _selectedStep?.Step.IngredientsToUse.Remove(ingredient);
    }

    private void ButtonAddIngredient_OnClick(object sender, RoutedEventArgs e)
    {
        _selectedStep?.Step.IngredientsToUse.Add(new RecipeIngredient());
    }
}

