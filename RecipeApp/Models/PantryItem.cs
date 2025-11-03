using System;
using ReciepApp.Models.Shared;

namespace RecipeApp.Models
{
    public class PantryItem : ItemBase
    {
        public DateTime AddedDate { get; set; } = DateTime.Now;
    }
}

