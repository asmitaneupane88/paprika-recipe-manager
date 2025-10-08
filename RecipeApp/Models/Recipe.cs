using System.Text;

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
    /// <summary>
    /// DO NOT USE, only for use by the jsonSerializer.
    /// </summary>
    public Recipe() {}

    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="selfContained">Should be true unless the html will be used in generating a list of recipes
    /// (i.e. should a html header and footer be included).</param>
    /// <returns>the html string representation of the recipe</returns>
    public string ConvertToHtml(bool selfContained = true)
    {
        var sb = new StringBuilder();
        
        if (selfContained) sb.AppendLine(HtmlHeader);
        
        sb.AppendLine($"<img src=\"{ImageUrl}\" alt=\"{Title}\">");
        sb.AppendLine($"<h1>{Title}</h1>");
        sb.AppendLine($"<h3>{Rating}/{MaxRating} stars<h3>"); //TODO: render as stars later.
        
        sb.AppendLine($"<p>{Description}</p>");
        
        //TODO: ingredients and steps here
        
        sb.AppendLine("<h2>Notes</h2>");
        sb.AppendLine($"<p>{UserNote}</p>");
        
        if (selfContained) sb.AppendLine(HtmlFooter);
        
        return sb.ToString();
    }

    #endregion
    
    #region Static Properties
    private static List<Recipe>? Recipes { get; set; }
    private static string? RecipesPath { get; set; }
    
    private static bool InSave;
    private static bool RequireResave;
    
    public const string HtmlHeader = "<!DOCTYPE html><html><head><meta charset=\"UTF-8\"><title>Recipe</title></head><body>";
    public const string HtmlFooter = "</body></html>";
    
    public const int MaxRating = 5;
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
    /// <param name="recipesToRemove"></param>
    public static async Task Remove(params Recipe[] recipesToRemove)
    {
        if (Recipes is null) await LoadRecipes();

        foreach (var recipe in recipesToRemove)
        {
            Recipes!.Remove(recipe);
        }
        
        await SaveRecipes();
    }

    private static async Task LoadRecipes()
    {
        RecipesPath ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RecipeApp", "Recipes.json");

        if (File.Exists(RecipesPath) && await File.ReadAllTextAsync(RecipesPath) is { Length: > 0 } fileData)
        {
            for (var i = 0; i < 3 && Recipes is null; i++) 
            {
                try
                {
                    Recipes = JsonSerializer.Deserialize<List<Recipe>>(fileData);
                }
                catch (Exception e)
                {
                    //TODO: ignored
                }
            }

            Recipes ??= [];
        }
        else
            Recipes = [];
    }
    
    private static async Task SaveRecipes(bool force = false)
    {
        RecipesPath ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RecipeApp", "Recipes.json");
        
        if (InSave && !force)
        {
            RequireResave = true;
            return;
        }

        InSave = true;

        Directory.CreateDirectory(Path.GetDirectoryName(RecipesPath)); 
        
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
