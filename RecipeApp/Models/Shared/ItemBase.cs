using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RecipeApp.Models.Shared
{
    /// <summary>
    /// Base class for grocery and pantry items, providing shared properties.
    /// </summary>
    public partial class ItemBase : ObservableObject
    {
        [ObservableProperty] private string name = string.Empty;
        [ObservableProperty] private string category = string.Empty;
        [ObservableProperty] private double quantity = 0;
        [ObservableProperty] private string unit = string.Empty;
        [ObservableProperty] private string? imageUrl;
        [ObservableProperty] private DateTime? expirationDate;
    }
}

