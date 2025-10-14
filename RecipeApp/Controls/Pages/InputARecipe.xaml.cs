using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RecipeApp.Models;
using RecipeApp.Services;

namespace RecipeApp.Controls.Pages;

public sealed partial class InputARecipe : NavigatorPage
{
    public InputARecipe(Navigator? nav = null) : base(nav)
    {
        this.InitializeComponent();
    }
    
    private async void OnButtonAddClick(object sender, RoutedEventArgs e)
    {
        // Since SavedRecipe inherits from IAutosavingClass, changes are automatically saved
        // We just need to show a confirmation and navigate back
        var textbox = new TextBox();
        var dialog = new ContentDialog
        {
            Title = "Create Recipe",
            Content = textbox,
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            var newRecipe = new SavedRecipe()
            {
                Title = textbox.Text,
            };

            await SavedRecipe.Add(newRecipe);

            Navigator.Navigate(new EditRecipe(Navigator, newRecipe), title:$"Edit {newRecipe.Title}");
        }
    }
} 
