using System.ComponentModel;
using System.Runtime.CompilerServices;
using RecipeApp.Models;

namespace RecipeApp.Controls.Pages;

/// <summary>
/// A basic extension of the Page class to provide a unified way to access and pass along data such as the Navigator field.
/// </summary>
public abstract class NavigatorPage : Page, INotifyPropertyChanged
{
    /// <summary>
    /// Access to the Navigator which can be passed to the constructor of other NavigatorPages.
    /// </summary>
    /// <remarks>
    /// Using the <code>{ get; set => SetField(ref field, value); }</code> is the same a using
    /// the "ObservableObject" and the "ObservableProperty" attribute, but applies it to a window instead of a class.
    /// </remarks>
    protected Navigator Navigator { get; set => SetField(ref field, value); }
    
    /// <summary>
    /// For use only by the designer.
    /// </summary>
    // public NavigatorPage()
    // {
    //     Navigator = new Navigator();
    // }
    
    /// <summary>
    /// For use by the Navigator.cs class and the Route.cs class.
    /// </summary>
    /// <param name="navigator"></param>
    public NavigatorPage(Navigator? navigator = null)
    {
        this.Navigator = navigator ?? new Navigator();
    }
    
    #region Rider Generated INotifyPropertyChanged Implementation
    /// <summary>
    /// Envent that fires on property changed only if the property has the SetField stuff setup.
    /// View the documentation of <see cref="Navigator"/> to see details.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// internal function to be used to set values for fields.
    /// </summary>
    /// <example>
    /// <code>protected Navigator Navigator { get; set => SetField(ref field, value); }</code>
    /// <code>public YourClass YourPropertyName { get; set => SetField(ref field, value); }</code>
    /// </example>
    /// <param name="field">a reference to the field to update</param>
    /// <param name="value">the value to update to</param>
    /// <param name="propertyName">do not include this, this is given to the function automatically by an attribute</param>
    /// <typeparam name="T">Type of the field</typeparam>
    /// <returns>a boolean indicating if the field was updated, returns false when both are equal</returns>
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
    #endregion
    
}
