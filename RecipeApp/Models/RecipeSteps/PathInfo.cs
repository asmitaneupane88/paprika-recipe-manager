namespace RecipeApp.Models.RecipeSteps;

public record PathInfo(
    OutNode OutNode,
    bool IsValid,
    List<RecipeIngredient> MinIngredients,
    List<RecipeIngredient> MaxIngredients,
    double PrepTime = 0,
    double MinCookTime = 0,
    double MaxCookTime = 0,
    double CleanupTime = 0)
{
    public double MinTotalTime => PrepTime + MinCookTime + CleanupTime;
    public double MaxTotalTime => PrepTime + MaxCookTime + CleanupTime;
}
