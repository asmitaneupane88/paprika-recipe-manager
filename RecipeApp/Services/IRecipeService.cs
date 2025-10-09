using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RecipeApp.Models;

namespace RecipeApp.Services
{
    public interface IRecipeService
    {
        /// <summary>
        /// Search for recipes using TheMealDB API.
        /// </summary>
        Task<IReadOnlyList<MealDbRecipe>> SearchAsync(string query, CancellationToken ct = default);
    }
}
