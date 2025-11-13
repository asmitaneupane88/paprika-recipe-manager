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
using RecipeApp.Services;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RecipeApp.Controls.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class RecipeChat : NavigatorPage
{
    [ObservableProperty] public partial SavedRecipe? OriginalRecipe { get; set; }

    
    [ObservableProperty] public partial SavedRecipe CurrentRecipe { get; set; }
    
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

        OriginalRecipe = currentRecipe;
        CurrentRecipe = currentRecipe?.DeepCopy() ?? new SavedRecipe { Title = "New Recipe" };
        
        DetailsPage = new RecipeDetailsV2(Navigator, CurrentRecipe, aiEditMode: true);

        SavedRecipeHolder.Child = DetailsPage;

        DataContext = this;
        
        RefreshVisibility();
        
        this.Unloaded += async (s, e) => await CleanupTags();
    }

    private async void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Cancel Recipe Creation",
            Content = "Are you sure you want to cancel? All progress will be lost.",
            PrimaryButtonText = "Yes",
            CloseButtonText = "No"
        };
        
        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            await CleanupTags();
            await Navigator.TryGoBack();
        }
    }

    private async void ButtonSave_OnClick(object sender, RoutedEventArgs e)
    {
        if (OriginalRecipe is not null)
            await SavedRecipe.Remove(OriginalRecipe);
        
        await SavedRecipe.Add(CurrentRecipe);
        
        await Navigator.TryGoBack();
        var newRecipePage = new RecipeDetailsV2(Navigator, CurrentRecipe);
        Navigator.Navigate(newRecipePage, $"Recipe Details: {CurrentRecipe.Title}");
        
        await CleanupTags();
    }

    private async void ButtonSend_OnClick(object sender, RoutedEventArgs e)
    {
        ProgressRing.Visibility = Visibility.Visible;
        SendButton.Visibility = Visibility.Collapsed;

        try
        {
            var task = AiHelper.RunPrompt(UserInput, Messages, CurrentRecipe);
            Messages.Add(new AiMessage(Sender.User, UserInput));
            UserInput = string.Empty;
            var result = await task;
            
            Messages.Add(new AiMessage(Sender.Assistant, result.Message));

            if (result.Recipe is not null)
                CurrentRecipe = result.Recipe;
            
            DetailsPage = new RecipeDetailsV2(Navigator, CurrentRecipe, aiEditMode: true);
            SavedRecipeHolder.Child = DetailsPage;

        }
        catch (Exception exception)
        {
            Messages.Add(new AiMessage(Sender.Error, $"{exception.Message}\n{exception.StackTrace}"));
        }

        ProgressRing.Visibility = Visibility.Collapsed;
        SendButton.Visibility = Visibility.Visible;
    }
    
    private void RefreshVisibility()
    {
        SendButtonEnabled = !string.IsNullOrWhiteSpace(UserInput);
    }

    private async Task CleanupTags()
    {
        var tags = await SavedTag.GetAll();
        var recipes = await SavedRecipe.GetAll();
        foreach (var tag in tags)
        {
            if (recipes.All(r => r.Tags.All(t => t != tag.Name)))
            {
                await SavedTag.Remove(tag);
            }
        }
    }
}
