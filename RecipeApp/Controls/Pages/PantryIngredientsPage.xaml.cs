using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Drawing.Printing;
using System.Text;
using Windows.Graphics.Printing;
using Microsoft.UI.Xaml.Input;
using RecipeApp.Services;
using Uno.Extensions;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RecipeApp.Controls.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class PantryIngredientsPage : NavigatorPage
{
    [ObservableProperty] private partial ObservableCollection<IngredientCard> AllIngredients { get; set; } = [];
    public PantryIngredientsPage(Navigator? nav) : base(nav)
    {
        this.InitializeComponent();
    }

    private async Task ShowIngredients ()
    {
        AllIngredients = (await RecipeIngredient.GetAll())
            .Select(i => new IngredientCard() { Ingredient = i, IsSelected = false})
            .ToObservableCollection();
    }
    
    private void OnRecipeCardChecked(object sender, RoutedEventArgs e)
    {
        RefreshSelected();
    }
    
    [ObservableProperty] private partial bool CardsSelected { get; set; } = false;
    
    private void RefreshSelected()
    {
        CardsSelected = GetSelectedIngredientCards()
            .Any(c => c.IsSelected);
    }
    
    private RecipeCard[] GetSelectedIngredientCards()
    {
        return AllIngredients
            .Where(c => c.IsSelected)
            .ToArray();
    }
}
