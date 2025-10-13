namespace RecipeApp.Models;

/// <summary>
/// Handles the representation of a saved category along with loading and saving of the saved recipes.
/// </summary>
public partial class SavedCategory : IAutosavingClass<SavedCategory>
{
    [ObservableProperty] public required partial string Name { get; set; }
    [ObservableProperty] public partial int SortOrder { get; set; } = 0;

    /// <inheritdoc cref="IAutosavingClass{T}.Add(T)"/>
    public static async Task<SavedCategory> Add(string name, int? sortOrder)
    {
        var category = new SavedCategory
        {
            Name = name,
            SortOrder = sortOrder ?? 0
        };
        
        await Add(category);
        
        return category;
    }
}
