using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;
using RecipeApp.Models;
using RecipeApp.Services;

namespace RecipeApp.Controls.Pages;

public sealed partial class MainPage : NavigatorPage
{
    public ObservableCollection<Recipe> Recipes { get; } = new();

	public MainPage(Navigator? nav = null) : base(nav)
    {
		this.InitializeComponent();
        foreach (var recipe in RecipeService.GetRecipes())
        {
            Recipes.Add(recipe);
        }
	}
    
    private void Recipe_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is Recipe recipe)
        {
            Navigator.Navigate(new RecipeDetails(Navigator, recipe), recipe.Title);
        }
    }
}
