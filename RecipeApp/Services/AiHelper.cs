using System.ClientModel;
using OpenAI;
using OpenAI.Chat;
using RecipeApp.Models.RecipeSteps;

namespace RecipeApp.Services;

/// <summary>
/// Used to make calls to an OpenAI API easier.
/// </summary>
public class AiHelper
{
    private static OpenAIClient? _client;
    private static string? _currentModel;

    private static async Task<OpenAIClient> InitClient()
    {
        var settings = await AiSettings.GetSettings();

        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri(settings.Endpoint),
        };

        _client = new OpenAIClient(new ApiKeyCredential(string.IsNullOrWhiteSpace(settings.ApiKey) ? "api-key" : settings.ApiKey), options);

        var modelsResponse = await _client.GetOpenAIModelClient().GetModelsAsync();
        if (modelsResponse?.Value != null)
        {
            _currentModel = settings.LastUsedModel ?? modelsResponse.Value.FirstOrDefault()?.Id;
        }

        return _client;
    }

    /// <summary>
    /// tries to get a list of models from the endpoint
    /// </summary>
    /// <returns></returns>
    public static async Task<List<string>> GetModels()
    {
        _client ??= await InitClient();

        var modelsResponse = await _client.GetOpenAIModelClient().GetModelsAsync();
        return modelsResponse?.Value?.Select(m => m.Id).ToList() ?? new List<string>();
    }

    /// <summary>
    /// sets the current model to use
    /// </summary>
    /// <param name="modelId"></param>
    /// <returns></returns>
    public static async Task<bool> SetModel(string modelId)
    {
        _currentModel = modelId;
        var settings = await AiSettings.GetSettings();
        settings.LastUsedModel = modelId;
        return true;
    }

    /// <summary>
    /// converts a mealDb recipe to json and passes it to the StringToSavedRecipe function.
    /// </summary>
    /// <param name="mealDbRecipe"></param>
    /// <returns></returns>
    public static async Task<SavedRecipe?> MealDbToSavedRecipe(MealDbRecipe mealDbRecipe)
    {
        return await StringToSavedRecipe(JsonSerializer.Serialize(mealDbRecipe));
    }

    /// <summary>
    /// calls the endpoint with a predefined JSON schema to convert the given string into something that almost looks like a recipe.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static async Task<SavedRecipe?> StringToSavedRecipe(string content)
    {
        _client ??= await InitClient();
        
        // this schema is a mix of claude sonnet 4.5 and me trying to make it work.
        // an original basic one was made by it which gave me a base to learn a bit of the syntax and finish this
        
        var response = await _client.GetChatClient(_currentModel ?? "noModel")
            .CompleteChatAsync(
                [
                    new SystemChatMessage("""
                                          You are a converter AI designed to convert any type of data into the standard format of a recipe.
                                          You should not make up too much information, only small changes that still reflect the original data.
                                          If you are missing too much information to make an accurate guess of what something is, leave the field blank instead.
                                          """),
                    new UserChatMessage($"Convert: {content}"),
                ],
                new ChatCompletionOptions
                {
                    ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                        jsonSchemaFormatName: "saved_recipe",
                        jsonSchema: BinaryData.FromString(
                            """
                            {
                              "$schema": "http://json-schema.org/draft/2020-12/schema#",
                              "properties": {
                                "recipe": {
                                  "$ref": "#/$defs/SavedRecipe"
                                }
                              },
                              "required": ["recipe"],
                              "additionalProperties": false,
                              "$defs": {
                                "SavedRecipe": {
                                  "type": "object",
                                  "properties": {
                                    "Title": { "type": "string", "description": "The title of the recipe. This should match the title of the original recipe as closely as possible." },
                                    "Description": { "type": "string", "description": "A description of the recipe such as what the dish is and a very brief overview of the ingredients and steps." },
                                    "ImageUrl": { "type": "string", "description": "url to the image of the recipe, leave this blank if there is no image, but try to find a url for the image within the given text or html." },
                                    "SourceUrl": { "type": ["string", "null"], "description": "Should be null/empty, this will be added by the system." },
                                    "UserNote": { "type": ["string", "null"], "description": "Should be null/empty, this will be added by the user." },
                                    "Category": { "type": ["string", "null"], "description": "This is probably going to be a guess based on the title and description, but for now use breakfast, lunch, dinner, dessert, or snack as the category. This will later be replaced by category tags." },
                                    "Rating": { "type": "integer", "minimum": 0, "maximum": 5, "description": "should be 0 stars unless it is specified already in the original recipe." },
                                    "Steps": {
                                      "type": "array",
                                      "items": { "$ref": "#/$defs/RecipeStep" },
                                      "description": "a list of steps which the user can use. The ingredients listed in each step are the ones used in that step and all used ingredients should reflect the total ingredients of the original recipe."
                                    }
                                  },
                                  "required": ["Title", "Description", "Steps"],
                                  "additionalProperties": false
                                },
                                "RecipeStep": {
                                  "type": "object",
                                  "description": "This represents an action by the user to prepare the dish. These actions can be broken up further from the original recipe so that it is easier to follow. Make sure to include a title for each step, a time for each step, and instruction steps should always have instructions attached to them",
                                  "properties": {
                                    "Type": { "type": "string", "enum": ["Instruction", "Timer"], "description": "Used for the UI, a Timer will show as a timer such as waiting for something to cook in a oven, while an Instruction will show as a text box with the instructions." },
                                    "Title": { "type": "string", "description": "This is the title of the step, it should be a few words." },
                                    "Instruction": { "type": ["string", "null"], "description": "Used only in instructions, this is the text of the instruction." },
                                    "MinutesToComplete": { "type": "number", "description": "This is used for both instructions and timers, the number of minutes it takes to complete the step which is used to calculate the total time to complete the recipe." },
                                    "Ingredients": {
                                      "type": ["array", "null"],
                                      "items": { "$ref": "#/$defs/RecipeIngredient" },
                                      "description": "ingredients used by this step, if any. Keep in mind that if the same ingredient is being used across steps, it should only be listed once so that the total ingredients reflects the original recipe."
                                    }
                                  },
                                  "required": ["Type", "Title", "Instruction", "MinutesToComplete"],
                                  "additionalProperties": false
                                },
                                "RecipeIngredient": {
                                  "type": "object",
                                  "properties": {
                                    "Name": { "type": "string", "description": "The common name of the ingredient, this is what the user will see in the UI and this is used to add up ingredients, so keep it as a standard/common name." },
                                    "ModifierNote": { "type": ["string", "null"], "description": "an optional note about how the ingredient should be. For example, if the ingredient is a tomato, you can add a note that it should be diced or chopped." },
                                    "Quantity": { "type": "number", "description": "How many of these are needed" },
                                    "Unit": { "type": "string", "enum": ["TSP", "TBSP", "CUP", "PINT", "QUART", "GALLON", "OZ", "LB", "KG", "ITEM"], "description": "This is the unit of measurement for the ingredient, if it is not specified, use ITEM which is for just how many such as use 2 ITEMs of onions, but the UI will hide this specific unit if it is ITEM." }
                                  },
                                  "required": ["Name", "Quantity", "Unit"],
                                  "additionalProperties": false
                                }
                              }
                            }
                            
                            """),
                        jsonSchemaIsStrict: true)
                });

        var jsonString = response.Value.Content[0].Text;
        
        var recipe = JsonSerializer.Deserialize<AiProcessedResponse>(jsonString, new JsonSerializerOptions
        {
            Converters = { new UnitTypeJsonConverter() }
        })?.recipe;
        
        if (recipe is null) return null;

        var savedRecipe = new SavedRecipe()
        {
            Title = recipe.Title,
            Description = recipe.Description,
            ImageUrl = recipe.ImageUrl,
            SourceUrl = recipe.SourceUrl,
            UserNote = recipe?.UserNote ?? string.Empty,
            Category = recipe?.Category ?? string.Empty,
            Rating = recipe?.Rating ?? 0,
        };

        var rootStep = new StartStep()
        {
            MinutesToComplete = 0
        };
        
        savedRecipe.RootStepNode = rootStep;
        
        IStep? currentStep = rootStep;
        foreach (var step in recipe?.Steps??[])
        {
            IStep newStep = step.Type switch
            {
                "Instruction" => new TextStep()
                {
                    MinutesToComplete = step.MinutesToComplete,
                    Title = step.Title,
                    Instructions = step.Instruction ?? string.Empty,
                    IngredientsToUse = step.Ingredients.ToObservableCollection()
                },
                "Timer" => new TimerStep()
                {
                    MinutesToComplete = step.MinutesToComplete,
                    Title = step.Title,
                    IngredientsToUse = step.Ingredients.ToObservableCollection()
                },
                _ => throw new Exception("Unknown step type")
            };

            switch (currentStep)
            {
                case TimerStep timeS:
                    timeS.NextStep = new OutNode("Next", newStep);
                    break;
                case TextStep textS:
                    textS.OutNodes = [new OutNode("Next", newStep)];
                    break;
                case StartStep startS:
                    startS.Paths = [new OutNode("Start", newStep)];
                    break;
            }
                    
            currentStep = newStep;
        }

        var finishStep = new FinishStep();
        
        switch (currentStep)
        {
            case TimerStep timeS:
                timeS.NextStep = new OutNode("Next", finishStep);
                break;
            case TextStep textS:
                textS.OutNodes = [new OutNode("Next", finishStep)];
                break;
            case StartStep startS:
                startS.Paths = [new OutNode("Start", finishStep)];
                break;
        }
        
        return savedRecipe;
    }

    private class AiProcessedResponse
    {
        public AiProcessedRecipe recipe { get; set; }
    }
    
    // used Claude Sonnet 4.5 for generating these classes from the schema 
    private class AiProcessedRecipe
    {
        public string Title { get; set; } = string.Empty;
    
        public string Description { get; set; } = string.Empty;
    
        public string ImageUrl { get; set; } = string.Empty;
    
        public string? SourceUrl { get; set; }
    
        public string? UserNote { get; set; }
    
        public string? Category { get; set; }
    
        public int Rating { get; set; }
    
        public List<AiProcessedStep> Steps { get; set; } = new();
    }
    private class AiProcessedStep
    {
        public string Type { get; set; } = string.Empty;
        
        public string Title { get; set; } = string.Empty;
        
        public string? Instruction { get; set; }
        
        public double MinutesToComplete { get; set; }
        
        public List<RecipeIngredient>? Ingredients { get; set; }
    }
}
