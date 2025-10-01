using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using RecipeApp.Controls.Pages;
using RecipeApp.Models;

namespace RecipeApp.Controls;

/// <summary>
/// The main window of the application.
/// </summary>
public sealed partial class Shell : NavigatorPage
{
    public Shell()
    {
        this.InitializeComponent();
        
        Navigator.RouteChanged += NavigatorOnRouteChanged;

        //TODO: need to support nested roots, but need to focus on basics first
        foreach (var route in Navigator.Routes)
        {
            RoutesAvailable.Add(route);
        }
    }

    private void ButtonMenu_OnClick(object sender, RoutedEventArgs e)
    {
        Navbar.IsPaneOpen = !Navbar.IsPaneOpen;
    }

    public ObservableCollection<Route> RoutesAvailable { get; set; } = [];
    public Symbol ButtonSymbol { get; set => SetField(ref field, value); }
    public Visibility BackVisibility { get; set => SetField(ref field, value); } = Visibility.Collapsed;
    public Visibility NavbarVisibility { get; set => SetField(ref field, value); } = Visibility.Visible;
    
    private void NavigatorOnRouteChanged(Page page, Route? route, string title)
    {
        BackVisibility = Navigator.HistoryItems > 1 ? Visibility.Visible : Visibility.Collapsed;
        NavbarVisibility = Navigator.CurrentRoute is not null ? Visibility.Visible : Visibility.Collapsed;
    }

    private void NavigationView_OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer?.Tag is Route route)
        {
            Navigator.Navigate(route);
        }
    }

    private void ButtonBack_OnClick(object sender, RoutedEventArgs e)
    {
        Navigator.GoBack();
    }
}

