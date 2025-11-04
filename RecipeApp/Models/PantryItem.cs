using System;
using RecipeApp.Models.Shared;

namespace RecipeApp.Models
{
    public class PantryItem : ItemBase
    {
        public DateTime AddedDate { get; set; } = DateTime.Now;
    }
}

