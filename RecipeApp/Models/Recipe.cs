namespace RecipeApp.Models;

//TODO: need to get documentation done
/// <summary>
/// 
/// </summary>
public partial class Recipe : ObservableObject
{
    #region Instance Properties

    [ObservableProperty] public partial string Title { get; set; }
    [ObservableProperty] public partial string Description { get; set; }
    [ObservableProperty] public partial string ImageUrl { get; set; }
    [ObservableProperty] public partial string? SourceUrl { get; set; }
    [ObservableProperty] public partial string UserNote { get; set; }
    [ObservableProperty] public partial int Rating { get; set; }
    
    //TODO: implement in sprint 2
    // should be able to look at the steps and add it all up.
    public List<RecipeIngredient> Ingredients => [];
    public List<IRecipeStep> Steps => [];
    #endregion

    #region Instance Methods
    private Recipe() {}

    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="selfContained">Should be true unless the html will be used in generating a list of recipes</param>
    /// <returns>the html string representation of the recipe</returns>
    public string ConvertToHtml(bool selfContained = true)
    {
        throw new NotImplementedException();
    }

    #endregion
    
    #region Static Properties
    // private static bool IsLoaded { get; set; }
    // private static ulong IdCounter { get; set; }
    private static List<Recipe>? Recipes { get; set; }
    private static string? RecipesPath { get; set; }
    
    private static bool InSave;
    private static bool RequireResave;
    #endregion
    
    #region Static Methods
    /// <summary>
    /// Loading data if needed.
    /// </summary>
    /// <returns>a task containing a readonly collection of recipes</returns>
    public static async Task<IReadOnlyCollection<Recipe>> GetAll()
    {
        if (Recipes is null) await LoadRecipes();
        return Recipes!;
    }
    
    /// <summary>
    /// Loads Data if needed.
    /// Adds a recipe to the list of recipes.
    /// Saves the list of recipes.
    /// </summary>
    /// <param name="title"></param>
    /// <param name="description"></param>
    /// <param name="imageUrl"></param>
    /// <param name="sourceUrl"></param>
    /// <returns></returns>
    public static async Task<Recipe> Add(string title, string description, string imageUrl, string? sourceUrl = null)
    {
        if (Recipes is null) await LoadRecipes();

        var recipe = new Recipe()
        {
            Title = title,
            Description = description,
            ImageUrl = imageUrl,
            SourceUrl = sourceUrl,
        };
        
        Recipes!.Add(recipe);
        
        await SaveRecipes();
        
        return recipe;
    }

    /// <summary>
    /// Loads Data if needed.
    /// Removes a recipe from the list of recipes.
    /// Saves the list of recipes.
    /// </summary>
    /// <param name="recipeToRemove"></param>
    public static async Task Remove(Recipe recipeToRemove)
    {
        if (Recipes is null) await LoadRecipes();
        
        Recipes!.Remove(recipeToRemove);
        
        await SaveRecipes();
    }

    private static async Task LoadRecipes()
    {
        RecipesPath ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RecipeApp", "Recipes.json");

        if (File.Exists(RecipesPath))
        {
            var fileData = await File.ReadAllTextAsync(RecipesPath);

            Recipes = JsonSerializer.Deserialize<List<Recipe>>(fileData);
        }
        else
            Recipes = [];
    }
    
    private static async Task SaveRecipes(bool force = false)
    {
        RecipesPath ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RecipeApp", "Recipes.json");
        
        if (InSave)
        {
            RequireResave = true;
            return;
        }

        InSave = true;
        
        var json = JsonSerializer.Serialize(Recipes);
        await File.WriteAllTextAsync(RecipesPath, json);
        
        InSave = false;

        if (RequireResave)
        {
            await SaveRecipes(true);
            RequireResave = false;
        }    
    }
    #endregion
    

   
    
    
}
