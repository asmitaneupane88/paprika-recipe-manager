using RecipeApp.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RecipeApp.ViewModels;

public class RecipeDetailsViewModel : INotifyPropertyChanged
{
    private readonly Recipe _recipe;
    
    public RecipeDetailsViewModel(Recipe recipe)
    {
        _recipe = recipe;
    }

    public async Task<SavedRecipe> SaveRecipeAsync()
    {
        var savedRecipe = new SavedRecipe
        {
            Title = _recipe.Title,
            Description = _recipe.Description ?? "",
            ImageUrl = _recipe.ImageUrl ?? "",
            Category = _recipe.Category,
            Rating = 0
        };

        await SavedRecipe.Add(savedRecipe);
        return savedRecipe;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}