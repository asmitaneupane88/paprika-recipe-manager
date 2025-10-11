using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RecipeApp.Models;
using RecipeApp.ViewModels;

namespace RecipeApp.Controls.Pages;

public sealed partial class RecipeDetails : NavigatorPage
{
    public Recipe Recipe { get; private set; }
    private RecipeDetailsViewModel ViewModel { get; }

    public RecipeDetails(Navigator navigator, Recipe recipe) : base(navigator)
    {
        Recipe = recipe;
        ViewModel = new RecipeDetailsViewModel(Recipe);
        this.InitializeComponent();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Save Recipe",
            Content = "Do you want to save this recipe to your local collection?",
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            await ViewModel.SaveRecipeAsync();
            
            // Show confirmation
            var infoBar = new InfoBar
            {
                Title = "Recipe Saved",
                Message = "The recipe has been saved to your collection.",
                Severity = InfoBarSeverity.Success,
                IsOpen = true
            };
        }
    }
}