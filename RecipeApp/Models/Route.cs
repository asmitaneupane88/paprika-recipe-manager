using RecipeApp.Controls.Pages;

namespace RecipeApp.Models;

/// <summary>
/// TODO
/// </summary>
public class Route
{
    /// <summary>
    /// 
    /// </summary>
    public required Symbol Icon { get; init; }
    /// <summary>
    /// 
    /// </summary>
    public required string Name { get; init; }
    /// <summary>
    /// 
    /// </summary>
    public bool IsRoot { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public required Func<Navigator, NavigatorPage> Page { get; init; }
    /// <summary>
    /// 
    /// </summary>
    public List<Route> SubRoutes { get; init; } = [];
}
