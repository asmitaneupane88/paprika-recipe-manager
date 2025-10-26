using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using RecipeApp.Models;
using RecipeApp.Interfaces;
using RecipeApp.Services;
using Windows.Foundation;
using System;
using System.Linq;
using Microsoft.UI.Xaml.Markup;

namespace RecipeApp.Controls.Pages;

public sealed partial class MealPlannerPage : NavigatorPage
{
    private Grid? _mealSlotsGrid;
    private DateTime _currentWeekStart;
    private readonly DateTime _minDate;
    private readonly DateTime _maxDate;
    private readonly MealPlanCollection _mealPlans = new();

    public MealPlannerPage(Navigator? nav = null) : base(nav)
    {
        // Use current system date
        var today = DateTime.Today;
        
        // Initialize date constraints (1 year past to 1 year future from today)
        _minDate = today.AddYears(-1);
        _maxDate = today.AddYears(1);
        
        // Start with current week
        _currentWeekStart = GetStartOfWeek(today);
        
        this.InitializeComponent();
        InitializeMealSlots(); // Initialize slots first
        UpdateDateDisplay();
        UpdateDayHeaders(); // This will ensure day headers are properly highlighted
    }

    private DateTime GetStartOfWeek(DateTime date)
    {
        // Assuming Monday is the start of the week
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff).Date;
    }

    private void UpdateDateDisplay()
    {
        // Update the date range display
        var weekEnd = _currentWeekStart.AddDays(6);
        
        // Format: "October 23 - 29, 2023" or "October 30 - November 5, 2023"
        string dateRange;
        if (_currentWeekStart.Month == weekEnd.Month)
        {
            dateRange = $"{_currentWeekStart:MMMM} {_currentWeekStart.Day} - {weekEnd.Day}, {_currentWeekStart.Year}";
        }
        else
        {
            dateRange = $"{_currentWeekStart:MMMM} {_currentWeekStart.Day} - {weekEnd:MMMM} {weekEnd.Day}, {weekEnd.Year}";
        }
        
        DateRangeText.Text = dateRange;
        
        // Update navigation button states
        PreviousWeekButton.IsEnabled = _currentWeekStart > _minDate;
        NextWeekButton.IsEnabled = _currentWeekStart < _maxDate;
    }

    private void PreviousWeekButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentWeekStart > _minDate)
        {
            _currentWeekStart = _currentWeekStart.AddDays(-7);
            UpdateDateDisplay();
            UpdateDayHeaders();
        }
    }

    private void NextWeekButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentWeekStart < _maxDate)
        {
            _currentWeekStart = _currentWeekStart.AddDays(7);
            UpdateDateDisplay();
            UpdateDayHeaders();
        }
    }

    private void TodayButton_Click(object sender, RoutedEventArgs e)
    {
        _currentWeekStart = GetStartOfWeek(DateTime.Today);
        UpdateDateDisplay();
        UpdateDayHeaders();
    }

    private void ClearAllHighlights()
    {
        if (_mealSlotsGrid != null)
        {
            foreach (var child in _mealSlotsGrid.Children)
            {
                if (child is Border border)
                {
                    border.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
                }
            }
        }

        for (int i = 0; i < 7; i++)
        {
            if (this.FindName($"Day{i}Header") is TextBlock header)
            {
                header.FontWeight = Microsoft.UI.Text.FontWeights.Normal;
            }
        }
    }

    private void UpdateDayHeaders()
    {
        var today = DateTime.Today;
        ClearAllHighlights();

        for (int i = 0; i < 7; i++)
        {
            var date = _currentWeekStart.AddDays(i);
            var dayHeader = this.FindName($"Day{i}Header") as TextBlock;
            if (dayHeader != null)
            {
                dayHeader.Text = $"{date:ddd}\n{date:MMM d}";
                
                // Highlight today's date
                if (date.Date == today)
                {
                    dayHeader.FontWeight = Microsoft.UI.Text.FontWeights.SemiBold;
                    
                    // Highlight the meal slots column
                    if (_mealSlotsGrid != null)
                    {
                        for (int row = 0; row < 3; row++)
                        {
                            var cell = _mealSlotsGrid.Children[i + (row * 7)] as Border;
                            if (cell != null)
                            {
                                cell.Background = (SolidColorBrush)Resources["TodayColumnBrush"];
                            }
                        }
                    }
                }
                else
                {
                    dayHeader.FontWeight = Microsoft.UI.Text.FontWeights.Normal;
                }
            }
        }
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
                // Check if this slot is for today
                var slotDate = _currentWeekStart.AddDays(col);
                var isToday = slotDate.Date == DateTime.Today;

                var border = new Border
                {
                    BorderBrush = (SolidColorBrush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
                    BorderThickness = new Thickness(0, 0, 1, 1),
                    Padding = new Thickness(10),
                    Background = isToday ? 
                        (SolidColorBrush)Resources["TodayColumnBrush"] : 
                        new SolidColorBrush(Microsoft.UI.Colors.Transparent)
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
                var date = _currentWeekStart.AddDays(col);
                var mealType = row switch
                {
                    0 => MealType.Breakfast,
                    1 => MealType.Lunch,
                    2 => MealType.Dinner,
                    _ => MealType.Breakfast // Default to breakfast, though this case should never occur
                };

                var mealPlan = _mealPlans.GetMealPlan(date, mealType);
                // Remove any existing content from the grid
                var existingContent = grid.Children.Where(c => Grid.GetRow(c) == 0).ToList();
                foreach (var content in existingContent)
                {
                    grid.Children.Remove(content);
                }

                if (mealPlan != null)
                {
                    // Add the recipe information
                    var recipeInfo = new TextBlock
                    {
                        Text = $"{mealPlan.Recipe.Title}",
                        TextWrapping = TextWrapping.Wrap,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetRow(recipeInfo, 0);
                    grid.Children.Add(recipeInfo);
                }
                else
                {
                    var textBlock = new TextBlock
                    {
                        Text = "No meal planned",
                        Foreground = (SolidColorBrush)Application.Current.Resources["TextFillColorTertiaryBrush"],
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Opacity = 0.6
                    };
                    Grid.SetRow(textBlock, 0);
                    grid.Children.Add(textBlock);
                }

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

    private async void AddButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Point position)
        {
            var row = (int)position.X;
            var col = (int)position.Y;
            
            // Get the meal type based on row
            var mealType = row switch
            {
                0 => MealType.Breakfast,
                1 => MealType.Lunch,
                2 => MealType.Dinner,
                _ => MealType.Breakfast // Default to breakfast, though this case should never occur
            };

            // Get the exact date based on column
            var date = _currentWeekStart.AddDays(col);

            var savedRecipes = await SavedRecipe.GetAll();
            if (!savedRecipes.Any())
            {
                var noRecipesDialog = new ContentDialog
                {
                    Title = "No Saved Recipes",
                    Content = "You don't have any saved recipes yet. Save some recipes first to add them to your meal plan.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await noRecipesDialog.ShowAsync();
                return;
            }

            // Create the recipe selection dialog
            var recipeListView = new ListView
            {
                ItemsSource = savedRecipes,
                SelectionMode = ListViewSelectionMode.Single,
                MaxHeight = 400,
                MinWidth = 400
            };

            // Create a recipe item template
            recipeListView.ItemTemplate = (DataTemplate)XamlReader.Load(
                "<DataTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">" +
                    "<Grid Padding=\"8\">" +
                        "<Grid.RowDefinitions>" +
                            "<RowDefinition Height=\"Auto\"/>" +
                            "<RowDefinition Height=\"Auto\"/>" +
                        "</Grid.RowDefinitions>" +
                        "<TextBlock Text=\"{Binding Title}\" Style=\"{StaticResource BodyStrongTextBlockStyle}\" TextWrapping=\"Wrap\"/>" +
                        "<TextBlock Text=\"{Binding Category}\" Grid.Row=\"1\" Style=\"{StaticResource CaptionTextBlockStyle}\" Opacity=\"0.8\"/>" +
                    "</Grid>" +
                "</DataTemplate>");

            var dialog = new ContentDialog
            {
                Title = $"Select Recipe for {mealType.ToString()} on {date:MMM d}",
                Content = recipeListView,
                PrimaryButtonText = "Add",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            
            if (result == ContentDialogResult.Primary && recipeListView.SelectedItem is SavedRecipe savedRecipe)
            {
                // Add the meal plan
                _mealPlans.AddMealPlan(date, savedRecipe, mealType);
                
                // Update just this specific cell
                var border = _mealSlotsGrid?.Children
                    .Cast<Border>()
                    .FirstOrDefault(b => Grid.GetRow(b) == row && Grid.GetColumn(b) == col);

                if (border?.Child is Grid cellGrid)
                {
                    // Clear existing content in the first row (keeping the add button in the second row)
                    var itemsToRemove = cellGrid.Children.Where(c => Grid.GetRow(c) == 0).ToList();
                    foreach (var item in itemsToRemove)
                    {
                        cellGrid.Children.Remove(item);
                    }

                    // Add the recipe information
                    var recipeInfo = new TextBlock
                    {
                        Text = $"{savedRecipe.Title}",
                        TextWrapping = TextWrapping.Wrap,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetRow(recipeInfo, 0);
                    cellGrid.Children.Add(recipeInfo);
                }
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