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

public sealed partial class SplitWidget : IStepControl
{
    [ObservableProperty] public partial double OutNodeSize { get; set; } = 20;

    
    [ObservableProperty] public partial InNode NodeIn { get; set; } = new(null, 10);
    
    public SplitWidget(SplitStep step)
    {
        this.InitializeComponent();
        
        Step = step;
        
        if (Step.BindableGetOutNodes.Count == 0)
            AddSplitOption();
    }
    
    private void AddSplitOption()
    {
        var newOut = new OutNode("", null);
        Step.BindableGetOutNodes.Add(newOut);
        newOut.PropertyChanged += NewInOnPropertyChanged;
    }

    private void NewInOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is OutNode node && e.PropertyName == nameof(OutNode.Next))
        {
            if (node.Next is null)
            {
                node.PropertyChanged -= NewInOnPropertyChanged;
                Step.BindableGetOutNodes.Remove(node);
            }
            else
            {
                if (Step.BindableGetOutNodes.All(n => n.Next is not null))
                    AddSplitOption();
            }
        }
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

