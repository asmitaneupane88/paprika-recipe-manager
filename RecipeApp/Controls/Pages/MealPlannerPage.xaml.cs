using Microsoft.UI;
using Windows.UI;
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
    // Palette of brushes to color each saved recipe in the meal planner.
    // We pick a set of distinct, fairly muted colors so multiple items are easy to distinguish.
    private readonly SolidColorBrush[] _recipeBrushes = new[]
    {
        new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0x4C, 0xAF, 0x50)), // green
        new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0x21, 0x96, 0xF3)), // blue
        new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0xFF, 0xA7, 0x26)), // orange
        new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0x9C, 0x27, 0xB0)), // purple
        new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0xE5, 0x39, 0x35)), // red
        new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0x00, 0x96, 0x88)), // teal
        new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0xFF, 0xC1, 0x07)), // amber
        new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0x8E, 0x24, 0xAA)), // deep purple
    };

    // Alternate brushes to make items easier to visually separate (even / odd)
    private readonly SolidColorBrush _alternateBrushEven = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0xE8, 0xF0, 0xFF)); // very light blue
    private readonly SolidColorBrush _alternateBrushOdd = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0xFF, 0xF6, 0xE0)); // very light amber

    private SolidColorBrush GetBrushForRecipe(RecipeApp.Models.SavedRecipe recipe)
    {
    if (recipe == null) return new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));
        var title = recipe.Title ?? string.Empty;
        // Simple deterministic mapping: sum of chars mod palette size
        int sum = 0;
        foreach (var c in title)
        {
            sum = (sum + c) % _recipeBrushes.Length;
        }
        return _recipeBrushes[sum];
    }

    private SolidColorBrush GetForegroundForBackground(Windows.UI.Color bg)
    {
        // Calculate relative luminance to pick white/black foreground for contrast
        double r = bg.R / 255.0;
        double g = bg.G / 255.0;
        double b = bg.B / 255.0;
        double luminance = 0.2126 * r + 0.7152 * g + 0.0722 * b;
    return luminance > 0.6 ? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 0, 0)) : new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255));
    }

        public MealPlannerPage(Navigator navigator) : base(navigator)
        {
            this.InitializeComponent();
            _currentWeekStart = GetStartOfWeek(DateTime.Today);
            UpdateDateDisplay();
            UpdateDayHeaders();
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

                    // Get all meal plans for this slot (allowing multiple recipes per meal type)
                    var mealPlansForSlot = allMealPlans.Where(mp => mp.Date.Date == date.Date && mp.MealType == mealType).ToList();

                    // Add meal content. Wrap the stack in a ScrollViewer so a slot with many
                    // planned recipes becomes scrollable instead of growing the cell height.
                    var contentStack = new StackPanel
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Top,
                        Orientation = Orientation.Vertical
                    };

                    var contentScroll = new ScrollViewer
                    {
                        Content = contentStack,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                        MaxHeight = 160 // limit height so scrollbars appear when many items
                    };

                    if (mealPlansForSlot.Any())
                    {
                        // Show each planned recipe on its own line and alternate chip background
                        // colors (even / odd) to make multiple items easier to distinguish.
                        for (int i = 0; i < mealPlansForSlot.Count; i++)
                        {
                            var mp = mealPlansForSlot[i];

                            // Alternate between the two defined brushes
                            var chipBackground = (i % 2 == 0) ? _alternateBrushEven : _alternateBrushOdd;

                            var chip = new Border
                            {
                                Background = chipBackground,
                                CornerRadius = new CornerRadius(6),
                                Padding = new Thickness(4, 4, 4, 4),
                                Margin = new Thickness(0, 2, 0, 2),
                                HorizontalAlignment = HorizontalAlignment.Stretch
                            };

                            // Use an internal grid so we can show the recipe title and a small remove button
                            var chipGrid = new Grid();
                            chipGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                            chipGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                            var mealText = new TextBlock
                            {
                                Text = mp.Recipe.Title,
                                TextWrapping = TextWrapping.Wrap,
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                VerticalAlignment = VerticalAlignment.Center,
                                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                                Foreground = GetForegroundForBackground(((SolidColorBrush)chipBackground).Color),
                                Margin = new Thickness(4,0,8,0)
                            };

                            Grid.SetColumn(mealText, 0);
                            chipGrid.Children.Add(mealText);

                            var removeButton = new Button
                            {
                                Content = "\uE711", // delete glyph
                                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                                Style = (Style)Application.Current.Resources["AccentButtonStyle"],
                                Padding = new Thickness(6, 2, 6, 2),
                                HorizontalAlignment = HorizontalAlignment.Right,
                                VerticalAlignment = VerticalAlignment.Center,
                                Tag = mp
                            };

                            // Async click handler removes the specific MealPlan and refreshes the UI
                            removeButton.Click += async (s, e) =>
                            {
                                try
                                {
                                    if (s is Button btn && btn.Tag is MealPlan toRemove)
                                    {
                                        await MealPlan.Remove(toRemove);
                                        // Refresh the grid so the removed recipe disappears
                                        LoadAndInitializeMealPlans();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Error removing meal plan: {ex.Message}");
                                }
                            };

                            Grid.SetColumn(removeButton, 1);
                            chipGrid.Children.Add(removeButton);

                            chip.Child = chipGrid;
                            contentStack.Children.Add(chip);
                        }
                    }
                    else
                    {
                        var mealText = new TextBlock
                        {
                            Text = "No meal planned",
                            TextWrapping = TextWrapping.Wrap,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Foreground = (SolidColorBrush)Application.Current.Resources["TextFillColorTertiaryBrush"],
                            Opacity = 0.6
                        };
                        contentStack.Children.Add(mealText);
                    }

                    Grid.SetRow(contentScroll, 0);
                    grid.Children.Add(contentScroll);

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
            var selectedRecipes = await dialog.ShowAsync();

            if (selectedRecipes != null && selectedRecipes.Count > 0)
            {
                foreach (var sr in selectedRecipes)
                {
                    await MealPlan.AddMealPlan(date, sr, mealType);
                }

                // Refresh the grid so the newly-added recipes appear
                LoadAndInitializeMealPlans();
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