using Windows.UI.Text;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Input;

namespace RecipeApp.Controls;

[ObservableObject]
public sealed partial class UCClickToEditText : UserControl
{
    public static readonly DependencyProperty EditableTextProperty =
        DependencyProperty.Register(nameof(EditableText), typeof(string), typeof(UCClickToEditText), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty FontSizeProperty =
        DependencyProperty.Register(nameof(FontSize), typeof(double), typeof(UCClickToEditText), new PropertyMetadata(14.0));

    public static readonly DependencyProperty FontWeightProperty =
        DependencyProperty.Register(nameof(FontWeight), typeof(Windows.UI.Text.FontWeight), typeof(UCClickToEditText), new PropertyMetadata(FontWeights.Normal));
    
    public static readonly DependencyProperty AcceptsReturnProperty =
        DependencyProperty.Register(nameof(AcceptsReturn), typeof(bool), typeof(UCClickToEditText), new PropertyMetadata(true));
    
    [ObservableProperty] public partial Visibility EditImageHover { get; set; } = Visibility.Collapsed;
    
    public string EditableText
    {
        get => (string)GetValue(EditableTextProperty);
        set => SetValue(EditableTextProperty, value);
    }

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public FontWeight FontWeight
    {
        get => (FontWeight)GetValue(FontWeightProperty);
        set => SetValue(FontWeightProperty, value);
    }
    
    public bool AcceptsReturn
    {
        get => (bool)GetValue(AcceptsReturnProperty);
        set => SetValue(AcceptsReturnProperty, value);
    }
    
    public UCClickToEditText()
    {
        this.InitializeComponent();
    }
    
    private void OnTextClicked(object sender, PointerRoutedEventArgs e)
    {
        DisplayText.Visibility = Visibility.Collapsed;
        EditImageHover = Visibility.Collapsed;
        EditText.Visibility = Visibility.Visible;
        
        // got to do this so multiline text does not get truncated to a single line???
        // forces a binding update after it becomes visible.
        EditText.Text = EditableText;
        
        EditText.Focus(FocusState.Pointer);
    }

    private void OnEditLostFocus(object sender, RoutedEventArgs e)
    {
        EditText.Visibility = Visibility.Collapsed;
        DisplayText.Visibility = Visibility.Visible;
    }

    private void DisplayText_OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        EditImageHover = Visibility.Visible;
    }

    private void DisplayText_OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        EditImageHover = Visibility.Collapsed;
    }
}

