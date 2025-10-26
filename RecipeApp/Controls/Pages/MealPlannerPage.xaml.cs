using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using RecipeApp.Models;
using Windows.UI;

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
                };

                var grid = new Grid
                {
                    Background = null, // Transparent background
                    AllowDrop = true
                };

                var textBlock = new TextBlock
                {
                    Text = "Drop recipe here",
                    Foreground = (SolidColorBrush)Application.Current.Resources["TextFillColorSecondaryBrush"],
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                grid.Children.Add(textBlock);
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