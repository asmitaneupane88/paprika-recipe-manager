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
using InNode = RecipeApp.Models.RecipeSteps.InNode;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace RecipeApp.Controls.StepControls;

public sealed partial class MergeWidget : IStepControl
{
    [ObservableProperty] public partial double InNodeSize { get; set; } = 20;
    
    [ObservableProperty] public partial ObservableCollection<InNode> Nodes { get; set; } = [];
    [ObservableProperty] public partial OutNode NodeOut { get; set; }
    
    public MergeWidget(MergeStep step, int connectionCount)
    {
        this.InitializeComponent();
        
        Step = step;
        NodeOut = step.NextStep;
        
        for (var i = 0; i <= connectionCount; i++)
            AddMergeOption();
    }

    private void AddMergeOption()
    {
        var newIn = new InNode(null, 10);
        Nodes.Add(newIn);
        newIn.PropertyChanged += NewInOnPropertyChanged;
    }

    private void NewInOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is InNode node && e.PropertyName == nameof(InNode.Source))
        {
            if (node.Source is null)
            {
                node.PropertyChanged -= NewInOnPropertyChanged;
                Nodes.Remove(node);
            }
            else
            {
                if (Nodes.All(n => n.Source is not null))
                    AddMergeOption();
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
        outNode.Width = outNodeActive ? 20 : 10;
        outNode.Height = outNodeActive ? 20 : 10;

        foreach (var node in Nodes)
        {
            node.Size = (inNodeActive && node.Source is null) || (!inNodeActive && node.Source is not null) ? 20 : 10;
        }
    }
    
    public override List<InNode> GetInNodes() => Nodes.ToList();

}

