using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RecipeApp.Models;
using RecipeApp.Services;

namespace RecipeApp.Controls.Pages
{
    public sealed partial class MealDbSearchPage : NavigatorPage
    {
        public MealDbSearchPage(Navigator? nav = null) : base(nav)
        {
            this.InitializeComponent();
        }

        private void Recipe_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is MealDbRecipe mealDbRecipe)
            {
                var recipe = mealDbRecipe.ToRecipe();
                Navigator.Navigate(new RecipeDetailsPage(Navigator, recipe), recipe.Title);
            }
        }
    }
}
