# Classes

Note that this is not a complete list of classes.
Only the most important classes are documented here (view the API docs for the full list).

## Meal Planning Classes

### MealPlan

Represents a meal plan entry that associates a recipe with a specific date and meal type (breakfast, lunch, or dinner).
Each meal plan entry includes:

- Date of the meal
- Associated recipe
- Meal type (breakfast, lunch, or dinner)
- Unique identifier

See @RecipeApp.Models.MealPlan for details.


## SavedRecipe
Represents a saved recipe stored in a json file.
This class uses the [IAutoSavingClass](interfaces-and-abstract-classes.md#iautosavingclass) to handle saving and loading.

See @RecipeApp.Models.SavedRecipe for details.

## SavedCategory
Represents a saved category stored in a json file.
This class uses the [IAutoSavingClass](interfaces-and-abstract-classes.md#iautosavingclass) to handle saving and loading.

Furthermore, this class is used is assigned to a Recipe by setting the category name on the recipe to a valid category name.

See @RecipeApp.Models.SavedCategory for details.

## Navigator
Manages a history of visited pages, what pages are available, and what page is currently active.

See @RecipeApp.Models.Navigator for details.

## Step Classes:
These classes inherit from [IStep](interfaces-and-abstract-classes.md#istep).

### TimerStep
An IStep that represents a timer step which is used to show a timer to the user.

see @RecipeApp.Models.RecipeSteps.TimerStep for details.

### TextStep
A text/instruction step to show text to the user.

see @RecipeApp.Models.RecipeSteps.TextStep for details.

### MergeStep
This step is used to merge multiple steps into one flow/path.
This step will wait for all active steps that can lead to the merge to complete before continuing to the next step.

see @RecipeApp.Models.RecipeSteps.MergeStep for details.

### SplitStep
This step is used to split a path into multiple paths.
These steps will run in parallel and this step is used to help out with calculating the min/max time and ingredients (handled in IStep).

see @RecipeApp.Models.RecipeSteps.SplitStep for details.

### StartStep
can support multiple paths such as oven or microwave and this is the one that is saved to a recipe.

see @RecipeApp.Models.RecipeSteps.StartStep for details.

### FinishStep
The end of a recipe, all paths must lead to this step or getting the path info will return false for the `IsValid` property.

see @RecipeApp.Models.RecipeSteps.FinishStep for details.

