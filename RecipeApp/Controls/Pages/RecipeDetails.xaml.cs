using Microsoft.UI.Xaml.Controls;
using RecipeApp.Models;

namespace RecipeApp.Controls.Pages;

public sealed partial class RecipeDetails : NavigatorPage
{
    public Recipe Recipe { get; private set; }

    public RecipeDetails(Navigator? nav = null, Recipe? recipe = null) : base(nav)
    {
        this.InitializeComponent();
        if (recipe != null)
        {
            Recipe = recipe;
        }
    }
}