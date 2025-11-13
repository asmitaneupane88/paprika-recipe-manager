# Meal Planner Feature

The Meal Planner is a powerful feature that helps users plan their meals ahead of time by organizing recipes into a calendar-based schedule. This guide explains how to use the meal planner and its key features.

## Overview

The Meal Planner allows users to:

- Schedule recipes for specific dates and meal times (breakfast, lunch, dinner)
- View meals in a calendar layout
- Plan meals for multiple days in advance
- Organize recipes by meal type
- Easily modify or remove planned meals

## Components

### MealPlannerPage

The main interface for the meal planner is accessed through the `MealPlannerPage`. This page provides:

- A calendar view for selecting dates
- Options to add recipes to different meal slots
- The ability to view and edit existing meal plans

### Recipe Selection Dialog

When adding a meal to the planner:

1. Click on a meal slot (breakfast, lunch, or dinner)
2. The Recipe Selection Dialog appears
3. Choose from your saved recipes
4. Select the date and meal type
5. Confirm to add the meal to your plan

## Data Model

The meal planner uses two main data structures:

### MealPlan Class

- Stores individual meal plan entries
- Properties:
  - `Id`: Unique identifier
  - `Date`: When the meal is planned for
  - `Recipe`: The recipe to be prepared
  - `MealType`: When the meal will be served (breakfast, lunch, or dinner)

### Leftovers (IsLeftOver)

- Purpose: The `IsLeftOver` flag marks a planned meal as a leftover. Leftovers are visually highlighted in the planner so users can quickly identify meals that are intended to be reheated or reused.
- How to set: In the `Recipe Selection Dialog` each recipe row includes a small "Leftover" checkbox (bound to `IsLeftOver`). You can also toggle leftover state on existing planned meals in the planner UI.
- Behavior:
  - When a meal's `IsLeftOver` property is set to `true` the application will attempt to automatically add the same recipe to the next calendar day for the same meal type. This helps you plan to reuse leftovers without manually re-adding the recipe.
  - The auto-add logic avoids duplicates: before adding to the next day it checks whether the same recipe is already planned for that date/meal slot and will skip adding if a match exists.
  - Unchecking `IsLeftOver` will update the stored `MealPlan` entry to clear the leftover flag. It does not automatically remove the propagated next-day entry if one was previously created.
- Implementation notes:
  - See `MealPlan.cs` (`SetLeftOver`) for the exact persistence and auto-add behavior.
  - The planner uses a distinct brush for leftovers so they appear visually different in the grid (see `MealPlannerPage` for the `_leftoverBrush` color).

### MealType Enumeration

Defines the possible meal categories:

- `Breakfast`: Morning meal
- `Lunch`: Midday meal
- `Dinner`: Evening meal

## Usage Guide

### Adding a Meal

1. Navigate to the Meal Planner page
2. Select a date on the calendar
3. Click the "+" button for the desired meal slot (breakfast, lunch, or dinner)
4. Choose a recipe from your saved recipes
5. Click "Save" to add the meal to your plan

## Technical Implementation

The meal planner feature is implemented using:

- XAML/WinUI controls for the user interface
- JSON storage for persisting meal plan data
- The Navigator pattern for page management
- MVVM architecture for separation of concerns

### Key Files

- `MealPlannerPage.xaml/.cs`: Main UI and logic
- `RecipeSelectionDialog.xaml/.cs`: Recipe selection interface
- `MealPlan.cs`: Data model for meal plans
- `MealType.cs`: Enumeration for meal categories
