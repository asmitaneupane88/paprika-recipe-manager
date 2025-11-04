using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RecipeApp.Models;
using RecipeApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RecipeApp.Controls.Pages;


public sealed partial class GroceryListPage : NavigatorPage
{
    [ObservableProperty]
    private ObservableCollection<GroceryItem> allGroceries = new();

    [ObservableProperty]
    private bool anySelected = false;
    public GroceryListPage(Navigator? nav) : base(nav)
    {
        this.InitializeComponent();

        _ = LoadGroceriesAsync();
    }
    
    private async Task LoadGroceriesAsync()
    {
        var groceries = await GroceryService.GetAll();
        AllGroceries.Clear();
        foreach (var item in groceries)
        {
            AllGroceries.Add(item);
        }
    }
    
    private async void ButtonAddGrocery_OnClick(object sender, RoutedEventArgs e)
    {
        var newItem = new GroceryItem { Name = "New Item", Quantity = 1 };
        AllGroceries.Add(newItem);
        await GroceryService.Add(newItem);
    }
    private void RefreshSelection()
    {
        AnySelected = AllGroceries.Any(g => g.IsPurchased);
    }
    
    private async void ButtonRemoveIngredient_OnClick(object sender, RoutedEventArgs e)
    {
        var toRemove = AllGroceries.Where(g => g.IsPurchased).ToList();
        await Task.WhenAll(toRemove.Select(GroceryService.Remove));
        foreach (var item in toRemove)
            AllGroceries.Remove(item);
        RefreshSelection();
    }
}
