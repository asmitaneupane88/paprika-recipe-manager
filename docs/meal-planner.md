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