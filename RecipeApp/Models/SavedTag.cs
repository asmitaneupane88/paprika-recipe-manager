namespace RecipeApp.Models;

/// <summary>
/// Handles the representation of a saved tag along with loading and saving of the saved recipes.
/// </summary>
public partial class SavedTag : IAutosavingClass<SavedTag>
{
    [ObservableProperty] public required partial string Name { get; set; }
    [ObservableProperty] public partial int SortOrder { get; set; } = 0;

    /// <inheritdoc cref="IAutosavingClass{T}.Add(T)"/>
    public static async Task<SavedTag> Add(string name, int? sortOrder)
    {
        var category = new SavedTag
        {
            Name = name,
            SortOrder = sortOrder ?? 0
        };
        
        await Add(category);
        
        return category;
    }
}
