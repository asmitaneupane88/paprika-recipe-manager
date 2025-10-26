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
                    Title = "Error",
                    Content = "This recipe does not have a root step node!\nPlease edit the recipe's steps to use it here.",
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
        }
    }

    private async void ButtonResetAll_OnClick(object sender, RoutedEventArgs e)
    {
        await Task.WhenAll(Steps.Select(s => ActiveStepInfo.Remove(s)));
        Steps.Clear();
    }
}

