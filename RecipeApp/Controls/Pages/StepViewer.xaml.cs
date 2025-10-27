using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using RecipeApp.Models.RecipeSteps;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RecipeApp.Controls.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class StepViewer : NavigatorPage
{
    [ObservableProperty] public partial ObservableCollection<ActiveStepInfo> Steps { get; set; } = [];
    
    
    public StepViewer(Navigator? nav) : base(nav)
    {
        this.InitializeComponent();
        
        _ = Initialize();
    }

    private async Task Initialize()
    {
        Steps = (await ActiveStepInfo.GetAll()).ToObservableCollection();
    }

    private async void ButtonAddRecipe_OnClick(object sender, RoutedEventArgs e)
    {
        var search = new SavedRecipeSearch();
        
        var dialog = new ContentDialog
        {
            Title = "Add Recipe",
            Content = search,
            PrimaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.XamlRoot,
        };
        
        search.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(search.SelectedRecipes))
            {
                dialog.PrimaryButtonText = search.SelectedRecipes.Count == 0 ? "Cancel" : $"Add {search.SelectedRecipes.Count} Recipes";
            }
        };
        
        await dialog.ShowAsync();

        foreach (var rc in search.SelectedRecipes)
        {
            if (rc.SavedRecipe.RootStepNode is null)
            {
                var dialog2 = new ContentDialog
                {
                    Title = $"Error - {rc.SavedRecipe.Title}",
                    Content = "This recipe does not have a root step node!\nPlease edit the recipe's steps to use it here.",
                    PrimaryButtonText = "Ok",
                    XamlRoot = this.XamlRoot,
                };
                
                await dialog2.ShowAsync();
                continue;
            } 
            if (rc.SavedRecipe.RootStepNode.GetNestedPathInfo() is { } pathInfo && pathInfo.Any(p => !p.IsValid))
            {
                var dialog2 = new ContentDialog
                {
                    Title = $"Error - {rc.SavedRecipe.Title}",
                    Content = $"This recipe does has invalid paths.\nPlease edit the recipe's steps to use it here. The following paths might not lead to the final step guaranteed:\n{string.Join("\n", pathInfo.Select(pi => $"    {pi.OutNode.Title}"))}",
                    PrimaryButtonText = "Ok",
                    XamlRoot = this.XamlRoot,
                };
                
                await dialog2.ShowAsync();
                continue;            
            }
            
            var activeStep = new ActiveStepInfo
            {
                RecipeTitle = rc.SavedRecipe.Title,
                RecipeImageUrl = rc.SavedRecipe.ImageUrl,
                RecipeId = Guid.NewGuid(), // used for when we need to split the steps
                CurrentStep = rc.SavedRecipe.RootStepNode,
                IngredientsUsed = []
            };
            
            Steps.Add(activeStep);
            await ActiveStepInfo.Add(activeStep);
        }
    }

    private async void ButtonResetAll_OnClick(object sender, RoutedEventArgs e)
    {
        await Task.WhenAll(Steps.Select(s => ActiveStepInfo.Remove(s)));
        Steps.Clear();
    }

    private async void ButtonOutNode_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: OutNode node } element)
        {
            ActiveStepInfo? step = null;
            var parent = VisualTreeHelper.GetParent(element);
            while (step is null)
            {
                if (parent is null) 
                    return;
                
                if (parent is FrameworkElement { Tag: ActiveStepInfo stepInfo })
                    step = stepInfo;
                
                parent = VisualTreeHelper.GetParent(parent);
            }

            step.AddIngredientsForStep(step.CurrentStep, step.CurrentStep.IngredientsToUse?.ToList()??[]);

            switch (node.Next)
            {
                case FinishStep finishStep:
                    //TODO we have the ingredients used, remove from pantry
                    
                    Steps.Remove(step);
                    await ActiveStepInfo.Remove(step);
                    break;
                case SplitStep splitStep:
                    foreach (var outNode in splitStep.OutNodes.Where(on => on.Next is not null))
                    {
                        var newActive = new ActiveStepInfo
                        {
                            RecipeTitle = step.RecipeTitle,
                            RecipeImageUrl = step.RecipeImageUrl,
                            RecipeId = step.RecipeId,
                            CurrentStep = outNode.Next!,
                            IngredientsUsed = step.IngredientsUsed,
                        };
                        
                        Steps.Add(newActive);
                        await ActiveStepInfo.Add(newActive);
                    }
                    
                    Steps.Remove(step);
                    await ActiveStepInfo.Remove(step);
                    break;
                case MergeStep mergeStep:
                    if (Steps.Any(i => i.CurrentStep == mergeStep))
                    {
                        Steps.Remove(step);
                        await ActiveStepInfo.Remove(step);
                    }
                    else
                    {
                        step.CurrentStep = node.Next;
                    }
                    break;
                case TextStep textStep:
                case TimerStep timerStep:
                    step.TimeLeft = node.Next.MinutesToComplete;
                    step.CurrentStep = node.Next;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            UpdateMergeSteps();
        }
    }

    private void UpdateMergeSteps()
    {
        var merges = Steps
            .Where(s => s.CurrentStep is MergeStep)
            .ToList();
        
        foreach (var merge in merges)
        {
            var waitingOnCount = Steps.Count(s => s.RecipeId == merge.RecipeId && s.CurrentStep.GetAllPossibleMerges().Contains(merge.CurrentStep));
            
            if (waitingOnCount == 0 && merge.CurrentStep is MergeStep { NextStep: { Next: { } next } } ms)
                merge.CurrentStep = next;
        }
    }
}

