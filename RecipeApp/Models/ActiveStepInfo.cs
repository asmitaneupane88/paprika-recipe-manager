using RecipeApp.Models.RecipeSteps;

namespace RecipeApp.Models;

public partial class ActiveStepInfo : IAutosavingClass<ActiveStepInfo>
{
    [ObservableProperty] public partial string RecipeTitle { get; set; }
    [ObservableProperty] public partial string RecipeImageUrl { get; set; }
    [ObservableProperty] public partial Guid RecipeId { get; set; }
    
    [ObservableProperty] public partial double TimeLeft { get; set; }
        
    [ObservableProperty] public partial IStep CurrentStep { get; set; }
    
    [ObservableProperty] public partial Dictionary<IStep, RecipeIngredient> IngredientsUsed { get; set; }
}
