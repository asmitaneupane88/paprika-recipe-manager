# Classes
Note that this is not a complete list of classes.
Only the most important classes are documented here (view the API docs for the full list).

## SavedRecipe
Represents a saved recipe stored in a json file.
This class uses the [IAutoSavingClass](interfaces-and-abstract-classes.md#iautosavingclass) to handle saving and loading.

See @RecipeApp.Models.SavedRecipe for details.

## SavedRecipe
Represents a saved category stored in a json file.
This class uses the [IAutoSavingClass](interfaces-and-abstract-classes.md#iautosavingclass) to handle saving and loading.

Furthermore, this class is used is assigned to a Recipe by setting the category name on the recipe to a valid category name.

See @RecipeApp.Models.SavedCategory for details.

## Navigator
Manages a history of visited pages, what pages are available, and what page is currently active.

See @RecipeApp.Models.Navigator for details.


