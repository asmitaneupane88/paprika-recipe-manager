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
    
    [ObservableProperty] public partial ObservableCollection<SavedRecipe> Recipes { get; set; } = [];
    
    public StepViewer(Navigator? nav) : base(nav)
    {
        this.InitializeComponent();
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
        
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            
        }
    }

    private void ButtonResetAll_OnClick(object sender, RoutedEventArgs e)
    {
        
    }
}

