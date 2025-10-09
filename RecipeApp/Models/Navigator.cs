namespace RecipeApp.Models;

/// <summary>
/// Handles navigation and where you are in the application. View each field and method for more info. 
/// </summary>
public partial class Navigator : ObservableObject
{
    /// <summary>
    /// This is what you edit to add pages.
    /// This is used to provide the navigation bar with info on when routes are visible and an idea on the path to a route.
    /// View the <see cref="Route"/> class for more info.
    /// </summary>
    public static readonly IReadOnlyList<Route> Routes = [
        new()
        {
            Icon = Symbol.Home,
            Name = "TEST Home",
            PageFactory = nav => new MainPage(nav),
        },
        new()
        {
            Icon = Symbol.List, //TODO: probably a better icon for this
            Name = "Saved Recipes",
            PageFactory = nav => new RecipePage(nav),
        },
        new()
        {
            Icon = Symbol.Help,
            Name = "Second",
            PageFactory = nav => new SecondPage(nav),
        },
        new()
        {
            Icon = Symbol.Scan,
            Name = "MealDB Recipes",
            PageFactory = nav => new SearchPage(nav),
        }
    ];
    
    /// <summary>
    /// Event that fires when the route changes.
    /// Parameters:
    /// 1. New current page.
    /// 2. New route or null.
    /// 3. Title of the new page.
    /// </summary>
    public event Action<NavigatorPage, Route?, string>? RouteChanged;
    /// <summary>
    /// Current page that should be displayed.
    /// </summary>
    [ObservableProperty] public partial NavigatorPage? CurrentPage { get; private set; }
    /// <summary>
    /// Current route of the current page.
    /// </summary>
    [ObservableProperty] public partial Route? CurrentRoute { get; private set; }
    /// <summary>
    /// Current title of the current page.
    /// </summary>
    [ObservableProperty] public partial string CurrentTitle { get; private set; }

    /// <summary>
    /// The count of the History stack. Should be one if it is on a root route.
    /// </summary>
    public int HistoryItems => History.Count;
    private Stack<(NavigatorPage? SavedPage, Route? PageRoute, string Name)> History = [];
    
    /// <summary>
    /// Constructor.
    /// Navigates to the first route automatically.
    /// </summary>
    public Navigator()
    {
        Navigate(Routes[0]);
    }
    
    /// <summary>
    /// Attempts to go back to the previous page.
    /// </summary>
    /// <returns>a <see cref="Boolean"/> indicating if the <see cref="Navigator"/> was able to go back</returns>
    public bool TryGoBack()
    {
        if (!History.TryPop(out var page)) return false;

        CurrentPage = page.SavedPage;
        CurrentRoute = page.PageRoute;
            
        RouteChanged?.Invoke(CurrentPage!, CurrentRoute, CurrentTitle);
            
        return true;
    }
    
    /// <summary>
    /// Navigates to the given route.
    /// This function returns void, but fires the <see cref="RouteChanged"/> event.
    /// </summary>
    /// <param name="route">the route to navigate to</param>
    public void Navigate(Route route)
    {
        // Keep only one root node at a time to prevent them from pilling
        if (History.TryPeek(out var lastPage) && Routes.Contains(lastPage.PageRoute)) 
            History.Pop(); 
        
        // check to see if it is a switch from root to root and skip if so.
        if (!Routes.Contains(route) || !Routes.Contains(CurrentRoute) || CurrentPage == null)
            History.Push((CurrentPage, CurrentRoute, CurrentTitle));
        
        CurrentPage = route.PageFactory(this);
        CurrentRoute = route;
        CurrentTitle = route.Name;
        
        RouteChanged?.Invoke(CurrentPage, CurrentRoute, CurrentTitle);
    }

    /// <summary>
    /// Navigates to the given page.
    /// This function returns void, but fires the <see cref="RouteChanged"/> event.
    /// </summary>
    /// <param name="page">The <see cref="NavigatorPage"/> to navigate to.</param>
    /// <param name="title">The title to update to on navigate.</param>
    public void Navigate(NavigatorPage page, string title)
    {
        History.Push((CurrentPage, CurrentRoute, CurrentTitle));
        
        CurrentPage = page;
        CurrentRoute = null;
        CurrentTitle = title;
        
        RouteChanged?.Invoke(CurrentPage, CurrentRoute, CurrentTitle);
    }
}
