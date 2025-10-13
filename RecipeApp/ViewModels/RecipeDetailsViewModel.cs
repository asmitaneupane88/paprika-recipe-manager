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
        // If we have a category, make sure it exists in SavedCategory
        if (!string.IsNullOrWhiteSpace(_recipe.Category))
        {
            var categories = await SavedCategory.GetAll();
            var categoryExists = categories.Any(c => c.Name.Equals(_recipe.Category.Trim(), StringComparison.CurrentCultureIgnoreCase));
            
            if (!categoryExists)
            {
                await SavedCategory.Add(_recipe.Category.Trim(), null);
            }
        }

        var savedRecipe = new SavedRecipe
        {
            Title = _recipe.Title,
            Description = _recipe.Description ?? "",
            ImageUrl = _recipe.ImageUrl ?? "",
            Category = _recipe.Category?.Trim(),
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