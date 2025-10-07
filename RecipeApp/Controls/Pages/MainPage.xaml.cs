using RecipeApp.Models;

namespace RecipeApp.Controls.Pages;

public sealed partial class MainPage : NavigatorPage
{
	public MainPage(Navigator? nav = null) : base(nav)
    {
		this.InitializeComponent();
	}
    

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        Navigator.Navigate(new ThirdPage(Navigator), "Third Page");
    }
    
    private void ButtonTwo_OnClick(object sender2, RoutedEventArgs f)
    {
        Navigator.Navigate(new InputARecipe(Navigator), "Enter a Recipe Manually");
    }
}
