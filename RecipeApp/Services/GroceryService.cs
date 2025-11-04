using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RecipeApp.Models;

namespace RecipeApp.Services
{
    public static class GroceryService
    {
        public static async Task Add(GroceryItem item)
        {
            await GroceryItem.Add(item);
        }

        public static async Task Remove(GroceryItem item)
        {
            await GroceryItem.Remove(item);
        }

        public static async Task<IList<GroceryItem>> GetAll()
        {
            var items = await GroceryItem.GetAll();
            return items.ToList();
        }
    }
}
