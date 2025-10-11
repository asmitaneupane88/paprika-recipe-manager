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

    public Task SaveRecipeAsync()
    {
        // TODO: Implement saving to local storage
        // This would involve:
        // 1. Converting MealDB recipe to local format if needed
        // 2. Saving to local storage (SQLite, JSON file, etc.)
        // 3. Updating the saved recipes collection
        return Task.CompletedTask;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}