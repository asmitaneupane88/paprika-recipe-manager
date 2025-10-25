using System.ClientModel;
using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using OpenAI;
using OpenAI.Chat;

namespace RecipeApp.Services;


/// <summary>
/// good documentation here for the NuGet package used:
/// https://microsoft.github.io/autogen-for-net/articles/Consume-LLM-server-from-LM-Studio.html
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

    public static async Task<List<string>> GetModels()
    {
        _client ??= await InitClient();

        var modelsResponse = await _client.GetOpenAIModelClient().GetModelsAsync();
        return modelsResponse?.Value?.Select(m => m.Id).ToList() ?? new List<string>();
    }

    public static async Task<bool> SetModel(string modelId)
    {
        _currentModel = modelId;
        var settings = await AiSettings.GetSettings();
        settings.LastUsedModel = modelId;
        return true;
    }

    public static async Task<SavedRecipe?> MealDbToSavedRecipe(MealDbRecipe mealDbRecipe)
    {
        _client ??= await InitClient();
        
        // this schema is a mix of claude sonnet 4.5 and me trying to make it work.
        
        ///TODO: get everything else added without breaking it again.
        var response = await _client.GetChatClient(_currentModel ?? "noModel")
            .CompleteChatAsync(
                new[] { new UserChatMessage($"Convert: {System.Text.Json.JsonSerializer.Serialize(mealDbRecipe)}") },
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
                                "$defs": 
                                {
                                    "SavedRecipe": {
                                      "type": "object",
                                      "properties": {
                                        "__id__": { "type": "string" },
                                        "Title": { "type": "string" },
                                        "Description": { "type": "string" },
                                        "ImageUrl": { "type": "string" },
                                        "SourceUrl": { "type": ["string", "null"] },
                                        "UserNote": { "type": ["string", "null"] },
                                        "Category": { "type": ["string", "null"] },
                                        "Rating": { "type": "integer", "minimum": 0, "maximum": 5 },
                                        "RootStepNode": {
                                          "$ref": "#/$defs/StartStep"
                                        }
                                      },
                                      "required": ["__id__", "Title", "Description"],
                                      "additionalProperties": false
                                    },
                                    "StartStep": {
                                      "type": "object",
                                      "properties": {
                                        "X": { "type": "number" },
                                        "Y": { "type": "number" },
                                        "MinutesToComplete": { "type": "number" },
                                        "IngredientsToUse": {
                                          "type": ["array", "null"],
                                          "items": { "$ref": "#/$defs/RecipeIngredient" }
                                        },
                                        "Paths": {
                                          "type": ["array", "null"],
                                          "items": { "$ref": "#/$defs/OutNode" }
                                        }
                                      },
                                      "required": ["X", "Y", "MinutesToComplete"],
                                      "additionalProperties": false
                                    },
                                    "RecipeIngredient": {
                                      "type": "object",
                                      "properties": {
                                        "Name": { "type": "string" },
                                        "ModifierNote": { "type": "string" },
                                        "Quantity": { "type": "number" },
                                        "Unit": { "type": "string", "description": "UnitType enum value" }
                                      },
                                      "required": ["Name", "ModifierNote", "Quantity", "Unit"],
                                      "additionalProperties": false
                                    },
                                    "OutNode": {
                                      "type": "object",
                                      "properties": {
                                        "Title": { "type": "string" },
                                        "Next": {
                                          "type": ["object", "null"],
                                          "description": "IStep reference - incomplete"
                                        }
                                      },
                                      "required": ["Title"],
                                      "additionalProperties": false
                                    }
                                }
                            }
                            """),
                        jsonSchemaIsStrict: true)
                });

        var jsonString = response.Value.Content[0].Text;
        jsonString = jsonString.Replace("\"__id__\":", "\"$id\":");
        
        var recipe = JsonSerializer.Deserialize<SavedRecipe>(jsonString);
        
        return recipe;
    }
}
