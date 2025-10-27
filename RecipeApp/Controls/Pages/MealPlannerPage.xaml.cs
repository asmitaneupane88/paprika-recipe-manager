using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using RecipeApp.Models;
using Windows.Foundation;
using System;

namespace RecipeApp.Controls.Pages;

public sealed partial class MealPlannerPage : NavigatorPage
{
    private Grid? _mealSlotsGrid;
    private DateTime _currentWeekStart;
    private readonly DateTime _minDate;
    private readonly DateTime _maxDate;
    
    public MealPlannerPage(Navigator? nav = null) : base(nav)
    {
        // Initialize date constraints (1 year past to 1 year future)
        var today = DateTime.Today;
        _minDate = today.AddYears(-1);
        _maxDate = today.AddYears(1);
        
        // Start with current week
        _currentWeekStart = GetStartOfWeek(today);
        
        this.InitializeComponent();
        UpdateDateDisplay();
        UpdateDayHeaders();
        InitializeMealSlots();
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

    private void UpdateDayHeaders()
    {
        var today = DateTime.Today;
        for (int i = 0; i < 7; i++)
        {
            var date = _currentWeekStart.AddDays(i);
            var dayHeader = this.FindName($"Day{i}Header") as TextBlock;
            if (dayHeader != null)
            {
                dayHeader.Text = $"{date:ddd}\n{date:MMM d}";
                
                // Clear any previous highlight
                var column = Grid.GetColumn(dayHeader);
                var columnDefinition = CalendarGrid.ColumnDefinitions[column];
                columnDefinition.Width = new GridLength(1, GridUnitType.Star);
                
                // Highlight today's column
                if (date.Date == today)
                {
                    // Get all elements in this column and highlight them
                    foreach (var child in CalendarGrid.Children)
                    {
                        if (child is FrameworkElement element && Grid.GetColumn(element) == i)
                        {
                            if (element is Border border)
                            {
                                border.Background = (SolidColorBrush)Resources["TodayColumnBrush"];
                            }
                        }
                    }
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
                var border = new Border
                {
                    BorderBrush = (SolidColorBrush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
                    BorderThickness = new Thickness(0, 0, 1, 1),
                    Padding = new Thickness(10),
                    Background = (_currentWeekStart.AddDays(col).Date == DateTime.Today) ? 
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