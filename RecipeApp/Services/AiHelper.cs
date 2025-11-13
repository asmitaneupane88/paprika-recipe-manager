using System.ClientModel;
using System.ComponentModel.DataAnnotations;
using NJsonSchema;
using OpenAI;
using OpenAI.Chat;
using RecipeApp.Enums;
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
        
        var schema = JsonSchema
            .FromType<AiProcessedRecipe>()
            .ToJson();
        
        var response = await _client.GetChatClient(_currentModel ?? "noModel")
            .CompleteChatAsync(
                [
                    new SystemChatMessage($"""
                                          You are a converter AI designed to convert any type of data into the standard format of a recipe.
                                          Your goal is to extract as much information as possible from a given string and convert it into a recipe format.
                                          You should also maintain how the wording of the recipe, focusing on separating out the data.
                                          The following is the list of the available recipe tags for the property "Tags". You should try to match these as much as possible, but you can also add new tags if needed.
                                          {string.Join(", ", (await SavedTag.GetAll()).Select(x => x.Name))}
                                          """),
                    new UserChatMessage($"Convert the following to the provided schema: {content}"),
                ],
                new ChatCompletionOptions
                {
                    ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                        jsonSchemaFormatName: "saved_recipe",
                        jsonSchema: BinaryData.FromString(schema),
                        jsonSchemaIsStrict: true)
                });

        var jsonString = response.Value.Content[0].Text;
        
        var recipe = JsonSerializer.Deserialize<AiProcessedRecipe>(
            jsonString,
            new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase), new UnitTypeJsonConverter() },
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            }
        );

        return await ParseResponseRecipe(recipe);
    }
    
    public static async Task<AiResponse> RunPrompt(string prompt, IEnumerable<AiMessage> messages, SavedRecipe recipe)
    {
        _client ??= await InitClient();

        
        var chatMessages = messages
            .Select<AiMessage, ChatMessage?>(m => m.Sender switch
            {
                Sender.Assistant => new AssistantChatMessage(m.Message),
                Sender.User => new UserChatMessage(m.Message),
                _ => null
            })
            .Where(m => m is not null)
            .Prepend(
                new SystemChatMessage(
                    $"""
                            You are an assistant designed to help users modify and create new recipes.
                            You should only change the parts of a recipe when requested and you should try to keep it realistic. 
                            
                            The following is the list of the available recipe tags for the property "Tags". You should try to match these as much as possible, but you can also add new tags if needed:
                            {string.Join(", ", (await SavedTag.GetAll()).Select(x => x.Name))} 
                            
                            The following is a list of the available ingredients in the user's pantry. You can use other ingredients, but the user could specify that they only want to use ingredients from the pantry:
                            {string.Join(", ", (await PantryIngredient.GetAll()).Select(x => x.Name))} 
                            
                            The following is the current recipe that the user is working on. Your response will replace the recipe, so anything unchanged should be copied over:
                            {JsonSerializer.Serialize(recipe, new JsonSerializerOptions
                            {
                                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase), new UnitTypeJsonConverter() },
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                WriteIndented = true
                            })}
                            """))
            .Append(new UserChatMessage(prompt));
        
        var schema = JsonSchema
            .FromType<AiProcessedResponse>()
            .ToJson();
        
        var chatCompletionResponse = await _client.GetChatClient(_currentModel ?? "noModel")
            .CompleteChatAsync(
                chatMessages,
                new ChatCompletionOptions
                {
                    ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                        jsonSchemaFormatName: "ai_response",
                        jsonSchema: BinaryData.FromString(schema),
                        jsonSchemaIsStrict: true)
                });

        var jsonString = chatCompletionResponse.Value.Content[0].Text;
        
        var rawProcessedResponse = JsonSerializer.Deserialize<AiProcessedResponse>(
            jsonString,
            new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase), new UnitTypeJsonConverter() },
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            }
        );

        var finalResponse = new AiResponse
        {
            Message = rawProcessedResponse?.Message??"",
            Recipe = await ParseResponseRecipe(rawProcessedResponse?.recipe)
        };

        return finalResponse;
    }
        
    public class AiResponse
    {
        public string Message { get; set; } = string.Empty;
        public SavedRecipe? Recipe { get; set; }
    }

    private class AiProcessedResponse
    {
        [Required]
        [Description("The response to the user. This should be limited in size and mostly be used for questions or comments about the recipe or to the user.")]
        public string Message { get; set; } = string.Empty;
        
        [Required]
        [Description("The recipe that was processed. This is visible to the user to modify before the next message is sent.")]
        public AiProcessedRecipe recipe { get; set; }
    }
    
    private class AiProcessedRecipe
    {
        
        [Required]
        [MinLength(1)]
        public string Title { get; set; } = string.Empty;
    
        [Required]
        public string Description { get; set; } = string.Empty;
    
        [Url]
        [Description("URL to the recipe's image")]
        public string ImageUrl { get; set; } = string.Empty;
        
        [Description("Category Tags of the recipe. These should try to match the tags used in the app, but can be new tags as well.")]
        public string[] Tags { get; set; } = [];
        
        [Range(0, int.MaxValue)]
        public int Rating { get; set; } = 0;
    
        [Description("The steps to make a recipe, these should be seperated out into seperate steps that are displayed one at a time.")]
        public List<AiProcessedStep> Steps { get; set; } = [];
    }
    
    private class AiProcessedStep
    {
        [Required]
        [Description("The type of step. This should be either 'Instruction' or 'Timer'. The Timer step is used for when something needs to cook for a certain amount of time (example is having something in the oven for a certain amount of time), and the Instruction step is used for everything else.")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public StepType Type { get; set; }
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        [Description("The instruction to follow for this step. This should be left blank if the step is a timer.")]
        public string? Instruction { get; set; }
        
        [Range(0, double.MaxValue)]
        [Description("The time in minutes to complete this step. This should have a time for both step types and is added up for the total recipe time. For timer steps, this is used for the timer UI.")]
        public double MinutesToComplete { get; set; }
        
        
        [Description("The ingredients to use for this step only. These ingredients are added up to get total ingredients for the recipe. Keep in mind that timer steps cannot have instructions attached and that the titles should match the original instructions, just seperated out.")]
        public List<RecipeIngredient>? Ingredients { get; set; }
    }
    
    private enum StepType
    {
        Instruction,
        Timer
    }

    private static async Task<SavedRecipe?> ParseResponseRecipe(AiProcessedRecipe? recipe)
    {
        if (recipe is null) return null;

        // 1-1 properties and basic creation of the saved recipe
        
        var savedRecipe = new SavedRecipe()
        {
            Title = recipe.Title,
            Description = recipe.Description,
            ImageUrl = recipe.ImageUrl,
            Rating = recipe?.Rating ?? 0,
        };
        
        // map tags, adding ot tag list if new
        
        var allTags = await SavedTag.GetAll();
        savedRecipe.Tags = [];
        
        List<Task> tasks = [];
        
        foreach (var tag in recipe?.Tags ?? [])
        {
            savedRecipe.Tags.Add(tag);

            if (allTags.All(x => x.Name != tag))
            {
                tasks.Add(SavedTag.Add(new SavedTag { Name = tag }));
            }
        }
        
        // map steps to the graph structure

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
                StepType.Instruction => new TextStep()
                {
                    MinutesToComplete = step.MinutesToComplete,
                    Title = step.Title,
                    Instructions = step.Instruction ?? string.Empty,
                    IngredientsToUse = step.Ingredients?.ToObservableCollection() ?? [],
                },
                StepType.Timer => new TimerStep()
                {
                    MinutesToComplete = step.MinutesToComplete,
                    Title = step.Title,
                    IngredientsToUse = step.Ingredients?.ToObservableCollection() ?? [],
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

        await Task.WhenAll(tasks);
        
        return savedRecipe;
    }
}
