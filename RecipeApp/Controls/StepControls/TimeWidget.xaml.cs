using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using RecipeApp.Models.RecipeSteps;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace RecipeApp.Controls.StepControls;

public sealed partial class TimeWidget : IStepControl
{
    [ObservableProperty] public partial double OutNodeSize { get; set; } = 20;
    
    [ObservableProperty] public partial ObservableCollection<OutNode> Nodes { get; set; }
    
    [ObservableProperty] public partial InNode NodeIn { get; set; } = new(null, 10);
    
    public TimeWidget(TimerStep step)
    {
        this.InitializeComponent();
        
        Step = step;
        Nodes = step.GetOutNodes().ToObservableCollection();
    }

    public override void ToggleSelection(bool isSelected)
    {
        Card.BorderBrush = new SolidColorBrush(isSelected
            ? (Color)Application.Current.Resources["PrimaryColor"]
            : (Color)Application.Current.Resources["OutlineColor"]);
    }

    public override void UpdateActiveNodes(bool outNodeActive, bool inNodeActive)
    {
        OutNodeSize = outNodeActive ? 20 : 10;
        
        inNode.Width = (inNodeActive && NodeIn.Source is null) || (!inNodeActive && NodeIn.Source is not null) ? 20 : 10;
        inNode.Height = (inNodeActive && NodeIn.Source is null) || (!inNodeActive && NodeIn.Source is not null) ? 20 : 10;
    }
    
    public override List<InNode> GetInNodes() => [NodeIn];
}

