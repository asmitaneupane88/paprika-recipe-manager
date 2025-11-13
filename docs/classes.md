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
This is the promary recipe model used throughout the app.
This class uses the [IAutoSavingClass](interfaces-and-abstract-classes.md#iautosavingclass) to handle saving and loading.

**Highlighted Methods:**

| Returns       | Name           | Parameters | Notes                                                                                                                                       |
|---------------|----------------|------------|---------------------------------------------------------------------------------------------------------------------------------------------|
| string        | ConvertToHtml  | bool       | The parameter selfContained is used to indicate if the html should contain a header/footer or be incomplete for combining multiple together |

Also has the methods from the IAutoSavingClass such as Add, Remove, GetAll.

**Highlighted Properties:**

| Type                         | Name              | Notes                                                                                    |
|------------------------------|-------------------|------------------------------------------------------------------------------------------|
| int                          | BindableMaxRating | Ignored in Json, used to bind to a max rating value for displaying properly.             |
| string                       | Title             | None                                                                                     |
| string                       | Description       | None                                                                                     |
| string                       | ImageUrl          | None                                                                                     |
| string?                      | SourceUrl         | None                                                                                     |
| string                       | UserNote          | None                                                                                     |
| ObservableCollection<string> | Tags              | None                                                                                     |
| bool                         | AdvancedSteps     | Are advanced steps enabled? This is needed as it is difficult to go back to being simple |
| bool                         | IsFromPdf         | None                                                                                     |
| int                          | Rating            | None                                                                                     |
| string?                      | PdfPath           | None                                                                                     |
| string?                      | HtmlPath          | None                                                                                     |

**Example Usage:**
TODO

See @RecipeApp.Models.SavedRecipe for details.

## SavedTag
Represents a saved tag stored in a json file.
This class uses the [IAutoSavingClass](interfaces-and-abstract-classes.md#iautosavingclass) to handle saving and loading.

Furthermore, this class is used is assigned to a Recipe by adding to the `SavedRecipe.Tags` property.

See @RecipeApp.Models.SavedCategory for details.

## Navigator
Manages a history of visited pages, what pages are available, and what page is currently active.

**Highlighted Methods:**

| Returns    | Name      | Parameters               | Notes                                                                                                 |
|------------|-----------|--------------------------|-------------------------------------------------------------------------------------------------------|
| Task<bool> | TryGoBack | None                     | Tries to pop an item off of the history stack and returns false if there are no items to go back to   |
| void       | Navigate  | Route                    | Navigates the current page to the given route                                                         |
| void       | Navigate  | NavigatorPage, string    | Navigates the current page to the given page, updating the title as well with the custom one provided |

**Highlighted Properties:**

| Type                  | Name      | Notes                                                             |
|-----------------------|-----------|-------------------------------------------------------------------|
| IReadOnlyList<Route>  | Routes    | Contains the route data for every page displayed in the left bar. |

**Example Usage:**
```csharp
// Navigate to a details page from a NavigatorPage

// somehow we have the data to pass, we leave it out to keep this simple:
var savedRecipe = null;

// create the page and pass our data
// note that the page must inherit from the NavigatorPage class
var detailsPage = new RecipeDetailsV2(Navigator, savedRecipe);

// navigate to the page
Navigator.Navigate(detailsPage, "Recipe Details");
```

See @RecipeApp.Models.Navigator for details.

## AiMessage
Simple class to contain the message and the sender of the message for the recipe chat page.

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

