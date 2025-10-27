using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using RecipeApp.Models;
using System;
using System.Collections.Generic;
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
        // Create the recipe selection dialog
        var recipeListView = new ListView
        {
            ItemsSource = _recipes,
            SelectionMode = ListViewSelectionMode.Single,
            MaxHeight = 400,
            MinWidth = 400,
            ItemTemplate = (DataTemplate)Application.Current.Resources["RecipeListItemTemplate"]
        };

        var dialog = new ContentDialog
        {
            Title = $"Select Recipe for {_mealType} on {_date:MMM d}",
            Content = recipeListView,
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = _xamlRoot
        };

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary ? recipeListView.SelectedItem as SavedRecipe : null;
    }
}