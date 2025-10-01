using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using RecipeApp.Controls.Pages;

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
            Name = "Home",
            Page = nav => new MainPage(nav),
        },
        new()
        {
            Icon = Symbol.Help,
            Name = "Second",
            Page = nav => new SecondPage(nav),
        },
    ];
    
    /// <summary>
    /// Event that fires when the route changes.
    /// Parameters:
    /// 1. 
    /// 
    /// </summary>
    public event Action<NavigatorPage, Route?, string>? RouteChanged;
    [ObservableProperty] public partial NavigatorPage? CurrentPage { get; private set; }
    [ObservableProperty] public partial Route? CurrentRoute { get; private set; }
    [ObservableProperty] public partial string CurrentTitle { get; private set; }

    public int HistoryItems => History.Count;
    private Stack<(NavigatorPage? SavedPage, Route? PageRoute, string Name)> History = [];
    
    public Navigator()
    {
        Navigate(Routes[0]);
    }
    
    public bool GoBack()
    {
        if (!History.TryPop(out var page)) return false;

        CurrentPage = page.SavedPage;
        CurrentRoute = page.PageRoute;
            
        RouteChanged?.Invoke(CurrentPage!, CurrentRoute, CurrentTitle);
            
        return true;
    }
    
    
    public void Navigate(Route route)
    {
        // Keep only one root node at a time to prevent them from pilling
        if (History.TryPeek(out var lastPage) && Routes.Contains(lastPage.PageRoute)) 
            History.Pop(); 
        
        // check to see if it is a switch from root to root and skip if so.
        if (!Routes.Contains(route) || !Routes.Contains(CurrentRoute) || CurrentPage == null)
            History.Push((CurrentPage, CurrentRoute, CurrentTitle));
        
        CurrentPage = route.Page(this);
        CurrentRoute = route;
        CurrentTitle = route.Name;
        
        RouteChanged?.Invoke(CurrentPage, CurrentRoute, CurrentTitle);
    }

    public void Navigate(NavigatorPage page, string title)
    {
        History.Push((CurrentPage, CurrentRoute, CurrentTitle));
        
        CurrentPage = page;
        CurrentRoute = null;
        CurrentTitle = title;
        
        RouteChanged?.Invoke(CurrentPage, CurrentRoute, CurrentTitle);
    }
}
