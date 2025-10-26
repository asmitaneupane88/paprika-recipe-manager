using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using RecipeApp.Models;
using Windows.Foundation;

namespace RecipeApp.Controls.Pages;

public sealed partial class MealPlannerPage : NavigatorPage
{
    private Grid? _mealSlotsGrid;

    public MealPlannerPage(Navigator? nav = null) : base(nav)
    {
        this.InitializeComponent();
        InitializeMealSlots();
    }

    private void InitializeMealSlots()
    {
        _mealSlotsGrid = this.FindName("MealSlotsGrid") as Grid;
        if (_mealSlotsGrid == null) return;

        // Create meal slots for each day and meal time
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 7; col++)
            {
                var border = new Border
                {
                    BorderBrush = (SolidColorBrush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
                    BorderThickness = new Thickness(0, 0, 1, 1),
                    Padding = new Thickness(10)
                };

                var grid = new Grid
                {
                    Background = null, // Transparent background
                    AllowDrop = true
                };

                // Add row definitions for content layout
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                // Add the placeholder text
                var textBlock = new TextBlock
                {
                    Text = "No meal planned",
                    Foreground = (SolidColorBrush)Application.Current.Resources["TextFillColorSecondaryBrush"],
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetRow(textBlock, 0);
                grid.Children.Add(textBlock);

                // Add the plus button
                var addButton = new Button
                {
                    Content = "\uE710", // Plus symbol
                    FontFamily = new FontFamily("Segoe MDL2 Assets"),
                    Style = (Style)Application.Current.Resources["AccentButtonStyle"],
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(0, 5, 0, 0),
                    Padding = new Thickness(8, 4, 8, 4)
                };
                
                // Store the row and column in the button's Tag for use in the click handler
                addButton.Tag = new Point(row, col);
                addButton.Click += AddButton_Click;
                Grid.SetRow(addButton, 1);
                grid.Children.Add(addButton);

                border.Child = grid;

                Grid.SetRow(border, row);
                Grid.SetColumn(border, col);
                _mealSlotsGrid.Children.Add(border);

                // Add drop handlers
                grid.DragOver += Grid_DragOver;
                grid.Drop += Grid_Drop;
            }
        }
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Point position)
        {
            var row = (int)position.X;
            var col = (int)position.Y;
            
            // Get the meal type based on row
            string mealType = row switch
            {
                0 => "Breakfast",
                1 => "Lunch",
                2 => "Dinner",
                _ => "Meal"
            };

            // Get the day based on column
            string day = col switch
            {
                0 => "Monday",
                1 => "Tuesday",
                2 => "Wednesday",
                3 => "Thursday",
                4 => "Friday",
                5 => "Saturday",
                6 => "Sunday",
                _ => "Day"
            };

            // TODO: Show recipe selection dialog
            // For now, just show a message that we'll implement the feature
            var dialog = new ContentDialog
            {
                Title = $"Add {mealType} for {day}",
                Content = "Recipe selection will be implemented here",
                PrimaryButtonText = "OK",
                DefaultButton = ContentDialogButton.Primary
            };

            // Show the dialog
            _ = dialog.ShowAsync();
        }
    }

    private void Grid_DragOver(object sender, Microsoft.UI.Xaml.DragEventArgs e)
    {
        e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
    }

    private void Grid_Drop(object sender, Microsoft.UI.Xaml.DragEventArgs e)
    {
        // TODO: Handle recipe drops
        if (sender is Grid grid)
        {
            // Update the cell with the dropped recipe
        }
    }
}