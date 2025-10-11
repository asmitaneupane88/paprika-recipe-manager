namespace RecipeApp.Models;

/// <summary>
/// Represents a route to an <see cref="NavigatorPage"/> including information
/// for the <see cref="Navigator"/> class to use.
/// </summary>
public class Route
{
    /// <summary>
    /// the symbol icon to show in the navigation bar next to the route name.
    /// </summary>
    public required Symbol Icon { get; init; }
    /// <summary>
    /// the name of the route to show as the page title and in the navigation bar next to the icon.
    /// </summary>
    public required string Name { get; init; }
    /// <summary>
    /// Should only be set for the topmost routes.
    /// Tells the <see cref="Navigator"/> that there is no parent route to go back to.
    /// </summary>
    public bool IsRoot { get; set; }

    /// <summary>
    /// Determines whether this route should be shown in the navigation view.
    /// Default is true.
    /// </summary>
    public bool ShowInNavigationView { get; init; } = true;
    /// <summary>
    /// A function that takes a <see cref="Navigator"/> and returns a <see cref="NavigatorPage"/> for the
    /// <see cref="Navigator"/> to use during page navigations.
    /// </summary>
    public required Func<Navigator, NavigatorPage> PageFactory { get; init; }

    // TODO: change this when subroutes are implemented, if subroutes need to be implemented at all.
    /// <summary>
    /// A list of subroutes which will show on the navigation bar when the route is selected.
    /// If this is empty, the navigation bar will show the routes in the same list as this route.
    /// </summary>
    /// <remarks>
    /// This is not implemented yet.
    /// </remarks>
    public List<Route> SubRoutes => [];  //{ get; init; } = [];
}
