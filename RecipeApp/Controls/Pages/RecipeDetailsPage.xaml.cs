using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RecipeApp.Models;
using RecipeApp.ViewModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RecipeApp.Controls.Pages;

public sealed partial class RecipeDetailsPage : NavigatorPage, INotifyPropertyChanged
{
    private Recipe _recipe;
    public Recipe Recipe 
    { 
        get => _recipe;
        private set
        {
            _recipe = value;
            OnPropertyChanged(nameof(Recipe));
            OnPropertyChanged(nameof(IsSavedRecipe));
            OnPropertyChanged(nameof(IsNewRecipe));
            OnPropertyChanged(nameof(CurrentRecipe));
        }
    }

    // This property is used for binding in XAML
    public IRecipe CurrentRecipe => _savedRecipe != null ? _savedRecipe : Recipe;
    
    private SavedRecipe? _savedRecipe;
    private RecipeDetailsViewModel ViewModel { get; }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Binding properties for visibility
    public bool IsSavedRecipe => _savedRecipe != null;
    public bool IsNewRecipe => !IsSavedRecipe;

    public RecipeDetailsPage(Navigator navigator, Recipe recipe) : base(navigator)
    {
        _recipe = recipe;
        ViewModel = new RecipeDetailsViewModel(Recipe);
        this.InitializeComponent();
    }

    public RecipeDetailsPage(Navigator navigator, SavedRecipe savedRecipe) : base(navigator) 
    {
        _savedRecipe = savedRecipe;
        _recipe = new Recipe
        {
            Title = savedRecipe.Title,
            Description = savedRecipe.Description ?? string.Empty,
            Category = savedRecipe.Category,
            ImageUrl = savedRecipe.ImageUrl,
            PrepTimeMinutes = 0, // TODO: These will be added in sprint 2
            CookTimeMinutes = 0,
            Servings = 1,
            Difficulty = "Unknown",
            Rating = savedRecipe.Rating,
            Ingredients = new(),  // Empty collections for now - will be populated in sprint 2
            Directions = new()
        };
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
            // Save and update our reference to the saved version
            _savedRecipe = await ViewModel.SaveRecipeAsync();
            
            // Show confirmation
            var infoBar = new InfoBar
            {
                Title = "Recipe Saved",
                Message = "The recipe has been saved to your collection.",
                Severity = InfoBarSeverity.Success,
                IsOpen = true,
                XamlRoot = this.XamlRoot
            };
        }
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (_savedRecipe != null)
        {
            Navigator.Navigate(new RecipeDetailsV2(Navigator, _savedRecipe), $"Edit {_savedRecipe.Title}");
        }
    }
}
