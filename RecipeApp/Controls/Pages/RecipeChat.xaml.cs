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
using RecipeApp.Enums;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RecipeApp.Controls.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class RecipeChat : NavigatorPage
{
    [ObservableProperty] public partial SavedRecipe? CurrentRecipe { get; set; }
    
    [ObservableProperty] public partial ObservableCollection<AiMessage> Messages { get; set; } = [];
    
    public string UserInput { 
        get;
        set
        {
            SetProperty( ref field, value);
            RefreshVisibility();
        }
    } = string.Empty;
    
    [ObservableProperty] public partial bool SendButtonEnabled { get; set; } = true;
    
    [ObservableProperty] public partial Visibility IsResponding { get; set; } = Visibility.Collapsed;
    [ObservableProperty] public partial Visibility IsNotResponding { get; set; } = Visibility.Visible;

    
    private RecipeDetailsV2 DetailsPage { get; set; }
    
    public RecipeChat(Navigator? nav = null, SavedRecipe? currentRecipe = null) : base(nav)
    {
        InitializeComponent();
        
        if (currentRecipe?.AdvancedSteps ?? false) throw new Exception("Cannot handle advanced steps in the recipe chat page.");

        CurrentRecipe = currentRecipe?.DeepCopy() ?? new SavedRecipe { Title = "New Recipe" };
        
        DetailsPage = new RecipeDetailsV2(Navigator, CurrentRecipe, aiEditMode: true);

        SavedRecipeHolder.Child = DetailsPage;

        RefreshVisibility();
    }

    private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
    {
        
    }

    private void ButtonSave_OnClick(object sender, RoutedEventArgs e)
    {
        
    }

    private void ButtonSend_OnClick(object sender, RoutedEventArgs e)
    {
        IsResponding = Visibility.Visible;
        IsNotResponding = Visibility.Collapsed;
        SendButtonEnabled = false;
        
        Messages.Add(new AiMessage(Sender.User, UserInput));
    }
    
    private void RefreshVisibility()
    {
        SendButtonEnabled = !string.IsNullOrWhiteSpace(UserInput) && IsResponding != Visibility.Visible;
    }
}

