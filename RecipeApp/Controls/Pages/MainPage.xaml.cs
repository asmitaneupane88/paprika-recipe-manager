using RecipeApp.Models;
using System;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using RecipeApp.Services;

namespace RecipeApp.Controls.Pages;

public sealed partial class MainPage : NavigatorPage
{
	public MainPage(Navigator? nav = null) : base(nav)
    {
		this.InitializeComponent();
        this.Loaded += MainPage_Loaded;
	}
    

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        Navigator.Navigate(new ThirdPage(Navigator), "Third Page");
    }

    private async void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        
    }
}
