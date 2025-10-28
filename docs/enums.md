# Enums
Note that this is not a complete list of enums.
Only the most important enums are documented here (view the API docs for the full list).

## Meal Type
An enum that defines the different types of meals in the meal planner. Used to categorize meals throughout the day.

```csharp
public enum MealType
{
    Breakfast,  // Morning meal
    Lunch,      // Midday meal
    Dinner      // Evening meal
}
```

See @RecipeApp.Models.MealType for implementation details.

## Unit Type
An enum that represents the unit type for use by an ingredient. A list of the values are below:
```csharp
public enum UnitType
{
    TSP,
    TBSP,
    CUP,
    PINT,
    QUART,
    GALLON,
    OZ,
    LB,
    KG,
}
```

See @RecipeApp.Models.UnitType for implementation details.
