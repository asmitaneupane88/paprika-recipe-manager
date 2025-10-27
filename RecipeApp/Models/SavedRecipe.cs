using RecipeApp.Models.RecipeSteps;

namespace RecipeApp.Models;

/// <summary>
/// Handles the representation of a saved recipe along with loading and saving of the saved recipes.
/// </summary>
public partial class SavedRecipe : IAutosavingClass<SavedRecipe>, IRecipe
{
    [JsonIgnore] public int BindableMaxRating => MaxRating;

    [ObservableProperty] public required partial string Title { get; set; }
    [ObservableProperty] public partial string Description { get; set; } = string.Empty;
    [ObservableProperty] public partial string ImageUrl { get; set; } = string.Empty;
    [ObservableProperty] public partial string? SourceUrl { get; set; }
    [ObservableProperty] public partial string UserNote { get; set; } = string.Empty;
    [ObservableProperty] public partial string? Category { get; set; }

    public bool IsFromPdf { get; set; }

    public int Rating
    {
        get;
        set => SetProperty(ref field, Math.Clamp(value, 0, MaxRating));
    }

    [ObservableProperty] public partial string? PdfPath { get; set; }
    [ObservableProperty] public partial string? HtmlPath { get; set; }
    [JsonIgnore] public bool HasPdf => !String.IsNullOrEmpty(PdfPath);
    [JsonIgnore] public bool HasHtml => !String.IsNullOrEmpty(HtmlPath);




    [ObservableProperty] public partial StartStep? RootStepNode { get; set; }

    public const string HtmlHeader =
        "<!DOCTYPE html><html><head><meta charset=\"UTF-8\"><title>Recipe</title></head><body>";

    public const string HtmlFooter = "</body></html>";

    public const int MaxRating = 5;

    public SavedRecipe()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="selfContained">Should be true unless the html will be used in generating a list of recipes
    /// (i.e. should a html header and footer be included).</param>
    /// <returns>the html string representation of the recipe</returns>
    public string ConvertToHtml(bool selfContained = true)
    {
        var sb = new StringBuilder();

        if (selfContained) sb.AppendLine(HtmlHeader);

        sb.AppendLine("<div style=\"display: flex; gap: 20px; margin-bottom: 20px;\">");
        sb.AppendLine(
            $"<img src=\"{ImageUrl}\" alt=\"{Title}\" style=\"width: 200px; height: 200px; object-fit: cover; border-radius: 8px; flex-shrink: 0;\">");
        sb.AppendLine("<div style=\"display: flex; flex-direction: column; justify-content: center;\">");
        sb.AppendLine($"<h1 style=\"margin: 0;\">{Title}</h1>");
        sb.AppendLine($"<h3 style=\"margin: 10px 0 0 0;\">{Rating}/{MaxRating} stars</h3>");
        sb.AppendLine("</div>");
        sb.AppendLine("</div>");

        sb.AppendLine($"<p>{Description}</p>");

        //TODO: ingredients and steps here when they are implemented.

        sb.AppendLine("<h2>Notes</h2>");
        sb.AppendLine($"<p>{UserNote}</p>");

        if (selfContained) sb.AppendLine(HtmlFooter);

        return sb.ToString();
    }

    /// <inheritdoc cref="IAutosavingClass{T}.Add(T)"/>
    public static async Task<SavedRecipe> Add(string title, string description, string imageUrl,
        string? sourceUrl = null)
    {
        var recipe = new SavedRecipe()
        {
            Title = title,
            Description = description,
            ImageUrl = imageUrl,
            SourceUrl = sourceUrl,
        };

        await Add(recipe);

        return recipe;
    }

    public static async Task Update(SavedRecipe recipe)
    {
        var all = await GetAll();
        var existing = all.FirstOrDefault(r => r.Title == recipe.Title);
        if (existing != null)
        {
            existing.PdfPath = recipe.PdfPath;
            existing.HtmlPath = recipe.HtmlPath;
            existing.Description = recipe.Description;
            existing.ImageUrl = recipe.ImageUrl;
            existing.Category = recipe.Category;
        }

        await SaveAll(all);
    }

    public static async Task SaveAll(IEnumerable<SavedRecipe> recipes)
    {
        var savePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RecipeApp", "SavedRecipes.json");

        var directory = Path.GetDirectoryName(savePath);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory!);

        var json = JsonSerializer.Serialize(recipes, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(savePath, json);
    }

}


