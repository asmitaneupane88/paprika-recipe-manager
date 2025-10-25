namespace RecipeApp.Controls.Pages;

/// <summary>
/// A basic extension of the Page class to provide a unified way to access and pass along data such as the Navigator field.
/// </summary>
[ObservableObject]
public abstract partial class NavigatorPage : Page
{
    /// <summary>
    /// Access to the Navigator which can be passed to the constructor of other NavigatorPages.
    /// </summary>
    /// <remarks>
    /// Using the <code>{ get; set => SetField(ref field, value); }</code> is the same a using
    /// the "ObservableObject" and the "ObservableProperty" attribute, but applies it to a window instead of a class.
    /// </remarks>
    [ObservableProperty] protected partial Navigator Navigator { get; set; }
    
    /// <summary>
    /// For use by the Navigator.cs class and the Route.cs class.
    /// </summary>
    /// <param name="navigator"></param>
    public NavigatorPage(Navigator? navigator = null)
    {
        this.Navigator = navigator ?? new Navigator();
    }

    /// <summary>
    /// For use by the <see cref="Navigator"/> when returning to this page, which could require refreshing data.
    /// </summary>
    public virtual async Task Restore()
    {
        
    }
}
