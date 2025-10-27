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
using RecipeApp.Controls.Dialogs;

namespace RecipeApp.Controls.Pages;

public sealed partial class MealPlannerPage : NavigatorPage
{
    private Grid? _mealSlotsGrid;
    private DateTime _currentWeekStart;
    private readonly DateTime _minDate;
    private readonly DateTime _maxDate;

        public MealPlannerPage(Navigator navigator) : base(navigator)
        {
            this.InitializeComponent();
            _currentWeekStart = GetStartOfWeek(DateTime.Today);
            UpdateDateDisplay();
            LoadAndInitializeMealPlans();
        }    private DateTime GetStartOfWeek(DateTime date)
    {
        // Assuming Monday is the start of the week
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff).Date;
    }

    private async void LoadAndInitializeMealPlans()
    {
        try
        {
            // Initialize meal slots first
            _mealSlotsGrid = this.FindName("MealSlotsGrid") as Grid;
            if (_mealSlotsGrid == null) return;

            // Clear existing content
            _mealSlotsGrid.Children.Clear();

            var allMealPlans = await MealPlan.GetAll();

            // Create meal slots for each day and meal time
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 7; col++)
                {
                    var date = _currentWeekStart.AddDays(col);
                    var isToday = date.Date == DateTime.Today;
                    var mealType = (MealType)row;  // Breakfast = 0, Lunch = 1, Dinner = 2

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
                        Background = null,
                        AllowDrop = true
                    };

                    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    // Get meal plan for this slot
                    var mealPlan = allMealPlans.FirstOrDefault(mp => mp.Date.Date == date.Date && mp.MealType == mealType);

                    // Add meal content
                    var contentPanel = new StackPanel
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    var mealText = new TextBlock
                    {
                        TextWrapping = TextWrapping.Wrap,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    if (mealPlan != null)
                    {
                        mealText.Text = mealPlan.Recipe.Title;
                        mealText.FontWeight = Microsoft.UI.Text.FontWeights.SemiBold;
                    }
                    else
                    {
                        mealText.Text = "No meal planned";
                        mealText.Foreground = (SolidColorBrush)Application.Current.Resources["TextFillColorTertiaryBrush"];
                        mealText.Opacity = 0.6;
                    }

                    contentPanel.Children.Add(mealText);
                    Grid.SetRow(contentPanel, 0);
                    grid.Children.Add(contentPanel);

                    // Add the plus button
                    var addButton = new Button
                    {
                        Content = "\uE710",
                        FontFamily = new FontFamily("Segoe MDL2 Assets"),
                        Style = (Style)Application.Current.Resources["AccentButtonStyle"],
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        Margin = new Thickness(0, 5, 0, 0),
                        Padding = new Thickness(8, 4, 8, 4),
                        Tag = new Point(row, col)
                    };
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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading meal plans: {ex.Message}");
        }
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
        // Previous week button is enabled if we're not at the earliest allowed date
        PreviousWeekButton.IsEnabled = _currentWeekStart.AddDays(-7) >= DateTime.Today.AddYears(-1);
        // Next week button is always enabled for future planning
        NextWeekButton.IsEnabled = true;
    }

    private void PreviousWeekButton_Click(object sender, RoutedEventArgs e)
    {
        var newDate = _currentWeekStart.AddDays(-7);
        // Don't go back more than one year
        if (newDate >= DateTime.Today.AddYears(-1))
        {
            _currentWeekStart = newDate;
            UpdateDateDisplay();
            UpdateDayHeaders();
            LoadAndInitializeMealPlans(); // Reload meal plans for the new week
        }
    }

    private void NextWeekButton_Click(object sender, RoutedEventArgs e)
    {
        _currentWeekStart = _currentWeekStart.AddDays(7);
        UpdateDateDisplay();
        UpdateDayHeaders();
        LoadAndInitializeMealPlans(); // Reload meal plans for the new week
    }

    private void TodayButton_Click(object sender, RoutedEventArgs e)
    {
        _currentWeekStart = GetStartOfWeek(DateTime.Today);
        UpdateDateDisplay();
        UpdateDayHeaders();
        LoadAndInitializeMealPlans(); // Reload meal plans for the current week
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

            var dialog = new RecipeSelectionDialog(this.XamlRoot, savedRecipes, date, mealType);
            var savedRecipe = await dialog.ShowAsync();
            
            if (savedRecipe != null)
            {
                await MealPlan.AddMealPlan(date, savedRecipe, mealType);
                
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

                    // Create a stack panel to hold the recipe info and meal type
                    var stackPanel = new StackPanel
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    var recipeInfo = new TextBlock
                    {
                        Text = savedRecipe.Title,
                        TextWrapping = TextWrapping.Wrap,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
                    };
                    stackPanel.Children.Add(recipeInfo);

                    Grid.SetRow(stackPanel, 0);
                    cellGrid.Children.Add(stackPanel);
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
