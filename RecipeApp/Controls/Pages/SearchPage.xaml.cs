using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RecipeApp.Models;
using RecipeApp.Services;

namespace RecipeApp.Controls.Pages
{
    public sealed partial class SearchPage : NavigatorPage
    {
        public SearchPage(Navigator? nav = null) : base(nav)
        {
            this.InitializeComponent();
        }
        private void Recipe_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is MealDbRecipe mealDbRecipe)
            {
                var recipe = mealDbRecipe.ToRecipe();
                Navigator.Navigate(new RecipeDetails(Navigator, recipe), recipe.Title);
            }
        }
    }
}
