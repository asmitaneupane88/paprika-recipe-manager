# AiHelper
This service/static class is used for interacting with AI models through the OpenAI API.\
Currently, this service is used only to convert string or the json of a MealDB recipe to a SavedRecipe.

This service relies on the @RecipeApp.Models.AiSettings model which saves to %APPDATA%\RecipeApp\aisettings.json on Windows.\
These settings include the endpoint, api key, and the model to use. Not having these settings will result in an error at the time of writting this.

**The most used functions are:**
- StringToSavedRecipe(string content)
- MealDbToSavedRecipe(MealDbRecipe recipe)

see @RecipeApp.Services.AiHelper for more info.
