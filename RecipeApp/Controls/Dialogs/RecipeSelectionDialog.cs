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
            MinWidth = 400
        };

        // Create a recipe item template
        recipeListView.ItemTemplate = (DataTemplate)XamlReader.Load(
            "<DataTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">" +
                "<Grid Padding=\"8\">" +
                    "<Grid.ColumnDefinitions>" +
                        "<ColumnDefinition Width=\"Auto\"/>" +
                        "<ColumnDefinition Width=\"*\"/>" +
                    "</Grid.ColumnDefinitions>" +
                    "<Image Width=\"60\" Height=\"60\" Stretch=\"UniformToFill\" Margin=\"0,0,12,0\">" +
                        "<Image.Source>" +
                            "<BitmapImage UriSource=\"{Binding ImageUrl}\"/>" +
                        "</Image.Source>" +
                        "<Image.Clip>" +
                            "<RectangleGeometry RadiusX=\"4\" RadiusY=\"4\" Rect=\"0,0,60,60\"/>" +
                        "</Image.Clip>" +
                    "</Image>" +
                    "<StackPanel Grid.Column=\"1\" VerticalAlignment=\"Center\">" +
                        "<TextBlock Text=\"{Binding Title}\" Style=\"{StaticResource BodyStrongTextBlockStyle}\" TextWrapping=\"Wrap\"/>" +
                        "<TextBlock Text=\"{Binding Category}\" Style=\"{StaticResource CaptionTextBlockStyle}\" Opacity=\"0.8\"/>" +
                    "</StackPanel>" +
                "</Grid>" +
            "</DataTemplate>");

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