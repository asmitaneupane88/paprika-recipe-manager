using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeApp.Models;

namespace RecipeApp.Services
{
    public static class PantryService
    {
        public static async Task Add(PantryIngredient item)
        {
            await PantryIngredient.Add(item);
        }

        public static async Task Remove(PantryIngredient item)
        {
            await PantryIngredient.Remove(item);
        }

        public static async Task<IReadOnlyCollection<PantryIngredient>> GetAll()
        {
            return await PantryIngredient.GetAll();

        }
    }
}    

