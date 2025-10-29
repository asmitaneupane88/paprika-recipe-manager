using Windows.Foundation;
using Windows.UI;
using Shapes = Microsoft.UI.Xaml.Shapes;

namespace RecipeApp.Controls.StepControls;

/// <summary>
/// For use in making the cool lines on the flow chart between two nodes.
/// This code is adapted from a simple example provided by Claude Sonnet 4.5 to work well in our setup.
/// </summary>
public class StepConnectorLine
{
    private readonly Shapes.Path _path;
    private readonly FrameworkElement _startControl;
    public FrameworkElement? _endControl { get; private set; }
    private Point? _endPoint;
    private readonly Canvas _stepCanvas;
    
    private readonly PathFigure _pathFigure = new();
    private readonly QuadraticBezierSegment _segment1 = new();
    private readonly QuadraticBezierSegment _segment2 = new();
    private readonly PathGeometry _pathGeometry = new();
    
    /// <summary>
    /// modified method from Claude Sonnet 4.5
    /// </summary>
    public StepConnectorLine(FrameworkElement startControl, Canvas stepCanvas)
    {
        _startControl = startControl;
        _stepCanvas = stepCanvas;

        _path = new Shapes.Path
        {
            Stroke = new SolidColorBrush((Color)Application.Current.Resources["SecondaryColor"]),
            StrokeThickness = 3,
            IsHitTestVisible = false
        };
        
        _pathFigure.Segments.Add(_segment1);
        _pathFigure.Segments.Add(_segment2);
        _pathGeometry.Figures.Add(_pathFigure);
        _path.Data = _pathGeometry;

        stepCanvas.Children.Add(_path);

        _startControl.LayoutUpdated += OnLayoutUpdated;
        _endPoint = GetControlCenter(_startControl, _stepCanvas);

        UpdateCurve();
    }

    /// <summary>
    /// new method
    /// </summary>
    public void SetEndLocation(Point? endPoint, FrameworkElement? endControl)
    {
        if (endPoint is null && endControl is null)
            throw new ArgumentException("Either endPoint or endControl must be provided.");
        
        if (_endControl != null)
        {
            _endControl.LayoutUpdated -= OnLayoutUpdated;
        }
        _endControl = endControl;
        if (_endControl != null)
        {
            _endControl.LayoutUpdated += OnLayoutUpdated;
        }
        _endPoint = endPoint;
        UpdateCurve();
    }

    /// <summary>
    /// original method from Claude Sonnet 4.5
    /// </summary>
    private void OnLayoutUpdated(object? sender, object e)
    {
        UpdateCurve();
    }

    private bool _inUpdate;
    /// <summary>
    /// modified method from Claude Sonnet 4.5
    /// it gave a good foundation, but it did not really understand what I was asking for, so this needed heavy modifications.
    /// </summary>
    private void UpdateCurve()
    {
        if (_inUpdate) return;
        _inUpdate = true;

        var startPoint = GetControlCenter(_startControl, _stepCanvas);
        var endPoint = _endPoint ?? GetControlCenter(_endControl!, _stepCanvas);

        var centerX = (startPoint.X + endPoint.X) / 2;
        var centerY = (startPoint.Y + endPoint.Y) / 2;

        _pathFigure.StartPoint = startPoint;
    
        _segment1.Point1 = new Point(centerX, startPoint.Y);
        _segment1.Point2 = new Point(centerX, centerY);
    
        _segment2.Point1 = new Point(centerX, endPoint.Y);
        _segment2.Point2 = endPoint;

        _inUpdate = false;
    }

    /// <summary>
    /// original method from Claude Sonnet 4.5
    /// </summary>
    private Point GetControlCenter(FrameworkElement control, Canvas container)
    {
        return control
            .TransformToVisual(container)
            .TransformPoint(new Point(control.ActualWidth / 2, control.ActualHeight / 2));
    }

    /// <summary>
    /// modified method from Claude Sonnet 4.5
    /// </summary>
    public void Dispose()
    {
        _startControl.LayoutUpdated -= OnLayoutUpdated;
        if (_endControl != null)
        {
            _endControl.LayoutUpdated -= OnLayoutUpdated;
        }
        _stepCanvas.Children.Remove(_path);
    }
}
