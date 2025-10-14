using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RecipeApp.Models;
using RecipeApp.Services;
using System.Drawing.Printing;
using System.Text;
using Windows.Graphics.Printing;
using Microsoft.UI.Xaml.Input;
using RecipeApp.Services;
using Uno.Extensions;

namespace RecipeApp.Controls.Pages;

public sealed partial class InputARecipe : NavigatorPage
{
    public InputARecipe(Navigator? nav = null) : base(nav)
    {
        this.InitializeComponent();
        OnOpening();
    }
    
    private async void OnOpening()
    {
        //TODO: want this function to work like the click button but need to fix
        
        var dialog = new ContentDialog
        {
            Title = "Create New Recipe",
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.XamlRoot
        };
        var textbox = new TextBox()
        {
            PlaceholderText="Recipe Title",
        };
        textbox.TextChanged += (s, _) =>
        {
            dialog.IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(textbox.Text);
        };
        dialog.Content = textbox;
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
