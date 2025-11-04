using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls;
using RecipeApp.Models;
using RecipeApp.Services;

namespace RecipeApp.Controls.Pages;


public sealed partial class PantryIngredientsPage : NavigatorPage
{
    [ObservableProperty] private partial ObservableCollection<IngredientCard> PantryIngredients { get; set; } = [];
    
    [ObservableProperty] private partial bool CardsSelected { get; set; } = false;
    public PantryIngredientsPage(Navigator? nav) : base(nav)
    {
        this.InitializeComponent();

        _ = ShowIngredients();
    }
    
    protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        _ = ShowIngredients();   // reload pantry every time the page opens
    }

    private async Task ShowIngredients()
    {
        PantryIngredients = (await PantryIngredient.GetAll())
            .Select(i => new IngredientCard
            {
                PIngredient = i,
                IsSelected = false,
                Ingredient = null
            })
            .ToObservableCollection();
    }
    
    private void OnRecipeCardChecked(object sender, RoutedEventArgs e)
    {
        RefreshSelected();
    }
    
   private void RefreshSelected()
    {
        CardsSelected = GetSelectedIngredientCards()
            .Any(c => c.IsSelected);
    }
    
    private List<IngredientCard> GetSelectedIngredientCards()
    {
        return PantryIngredients
            .Where(c => c.IsSelected)
            .ToList();
    }

    private async void ButtonAddIngredient_OnClick(object sender, RoutedEventArgs e)
    {
        var newIngredient = new PantryIngredient();
        
        PantryIngredients.Add(new IngredientCard
        {
            PIngredient = newIngredient,
            IsSelected = false,
            Ingredient = null
        });
        await PantryIngredient.Add(newIngredient);
    }

    private async void ButtonRemoveIngredient_OnClick(object sender, RoutedEventArgs e)
    {
        var selected = GetSelectedIngredientCards();
        
        await Task.WhenAll(selected.Select(c => PantryIngredient.Remove(c.PIngredient)));
        selected.ForEach(i => PantryIngredients.Remove(i));
        RefreshSelected();
    }
}
