using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RecipeApp.Models;
using RecipeApp.ViewModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RecipeApp.Services;

namespace RecipeApp.Controls.Pages;

public sealed partial class MealDBDetailsPage : NavigatorPage, INotifyPropertyChanged
{
    private MealDbRecipe _recipe;
    public MealDbRecipe Recipe 
    { 
        get => _recipe;
        private set
        {
            _recipe = value;
            OnPropertyChanged(nameof(Recipe));
        }
    }
    
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    

    public MealDBDetailsPage(Navigator navigator, MealDbRecipe recipe) : base(navigator)
    {
        _recipe = recipe;
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
            var savedRecipe = _recipe.ToSavedRecipe();

            await SavedRecipe.Add(savedRecipe);
            
            await Navigator.TryGoBack();
            Navigator.Navigate(new RecipeDetailsV2(Navigator, savedRecipe), $"Edit Recipe: {savedRecipe.Title}");
            
        }
    }
}
