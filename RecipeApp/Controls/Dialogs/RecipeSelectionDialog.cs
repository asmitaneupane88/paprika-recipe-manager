using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Markup;
using RecipeApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecipeApp.Controls.Dialogs;

public class RecipeSelectionDialog
{
    private readonly XamlRoot _xamlRoot;
    private readonly IEnumerable<SavedRecipe> _recipes;
    private readonly DateTime _date;
    private readonly MealType _mealType;

    public RecipeSelectionDialog(XamlRoot xamlRoot, IEnumerable<SavedRecipe> recipes, DateTime date, MealType mealType)
    {
        _xamlRoot = xamlRoot;
        _recipes = recipes;
        _date = date;
        _mealType = mealType;
    }

    public async Task<SavedRecipe?> ShowAsync()
    {
        var rootGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
            },
            MinWidth = 400,
            Height = 500 // Fixed height
        };

        // Create search box
        var searchBox = new AutoSuggestBox
        {
            PlaceholderText = "Search recipes...",
            QueryIcon = new SymbolIcon(Symbol.Find),
            Margin = new Thickness(0, 0, 0, 8)
        };

        // Create observable collection for recipes
        var recipesList = new System.Collections.ObjectModel.ObservableCollection<SavedRecipe>(_recipes);

        // Create a ScrollViewer to ensure proper scrolling behavior
        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };

        // Create the recipe list view
        var recipeListView = new ListView
        {
            ItemsSource = recipesList,
            SelectionMode = ListViewSelectionMode.Single,
            ItemTemplate = (DataTemplate)Application.Current.Resources["RecipeListItemTemplate"],
            MinHeight = 400, // Ensure minimum height
            MaxHeight = 400  // Match parent grid's available space
        };

        scrollViewer.Content = recipeListView;

        // Set up search functionality
        searchBox.TextChanged += (s, e) =>
        {
            var searchText = searchBox.Text;
            recipeListView.ItemsSource = string.IsNullOrWhiteSpace(searchText)
                ? _recipes // Show all recipes when search is empty
                : _recipes.Where(recipe => 
                    recipe.Title?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true);
        };

        // Add controls to grid
        Grid.SetRow(searchBox, 0);
        Grid.SetRow(scrollViewer, 1);
        rootGrid.Children.Add(searchBox);
        rootGrid.Children.Add(scrollViewer);

        var dialog = new ContentDialog
        {
            Title = $"Select Recipe for {_mealType} on {_date:MMM d}",
            Content = rootGrid,
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = _xamlRoot
        };

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary ? recipeListView.SelectedItem as SavedRecipe : null;
    }
}