# Recipe Model Documentation

## Overview

The Recipe model is the core data structure in the Paprika Recipe Manager application, representing all aspects of a recipe including ingredients, instructions, timing, and metadata.

## Class Definition

### Recipe Class

```csharp
public class Recipe : INotifyPropertyChanged, IRecipe
```

The Recipe class implements:

- `INotifyPropertyChanged`: For UI updates
- `IRecipe`: Base recipe interface

## Properties

### Core Properties

| Property    | Type    | Description              | Required       |
| ----------- | ------- | ------------------------ | -------------- |
| Id          | string  | Unique identifier (GUID) | Auto-generated |
| Title       | string  | Recipe name              | Yes            |
| Description | string? | Recipe description/notes | No             |
| Category    | string? | Recipe category          | No             |
| Author      | string? | Recipe creator           | No             |

### Timing Properties

| Property         | Type | Description           | Required        |
| ---------------- | ---- | --------------------- | --------------- |
| PrepTimeMinutes  | int  | Preparation time      | Yes             |
| CookTimeMinutes  | int  | Cooking time          | Yes             |
| TotalTimeMinutes | int  | Total time (computed) | Auto-calculated |
| Servings         | int  | Number of portions    | Yes             |

### Recipe Content

| Property    | Type                             | Description             | Required |
| ----------- | -------------------------------- | ----------------------- | -------- |
| Ingredients | ObservableCollection<Ingredient> | List of ingredients     | Yes      |
| Directions  | ObservableCollection<string>     | Cooking steps           | Yes      |
| Difficulty  | string                           | Recipe difficulty level | Yes      |

### Metadata

| Property | Type    | Description       | Required |
| -------- | ------- | ----------------- | -------- |
| Rating   | int     | User rating (0-5) | No       |
| ImageUrl | string? | Recipe image URL  | No       |
| Source   | string? | Recipe source     | No       |
| MealDbId | string? | External API ID   | No       |

## Ingredient Class

```csharp
public class Ingredient : INotifyPropertyChanged
```

### Properties

| Property | Type    | Description      |
| -------- | ------- | ---------------- |
| Amount   | string? | Quantity         |
| Unit     | string? | Measurement unit |
| Name     | string  | Ingredient name  |
| Notes    | string? | Additional notes |

## Usage Examples

### Creating a New Recipe

```csharp
var recipe = new Recipe
{
    Title = "Spaghetti Carbonara",
    PrepTimeMinutes = 10,
    CookTimeMinutes = 20,
    Servings = 4,
    Difficulty = "Medium",
    Category = "Pasta",
    Ingredients = new ObservableCollection<Ingredient>
    {
        new Ingredient
        {
            Amount = "400",
            Unit = "g",
            Name = "Spaghetti"
        }
    },
    Directions = new ObservableCollection<string>
    {
        "Boil pasta according to package instructions",
        "Prepare sauce while pasta cooks"
    }
};
```

### Property Change Notifications

The Recipe class implements INotifyPropertyChanged to support two-way binding:

```csharp
private int _rating;
public int Rating
{
    get => _rating;
    set
    {
        if (_rating != value)
        {
            _rating = value;
            OnPropertyChanged();
        }
    }
}
```

