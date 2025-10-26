namespace RecipeApp.Interfaces
{
    public interface IRecipeService
    {
        /// <summary>
        /// Search for recipes using TheMealDB API.
        /// </summary>
        Task<IReadOnlyList<MealDbRecipe>> SearchAsync(string query, CancellationToken ct = default);
    }
}
