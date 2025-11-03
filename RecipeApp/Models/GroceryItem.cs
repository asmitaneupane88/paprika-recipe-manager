using System;
using ReciepApp.Models.Shared;

namespace RecipeApp.Models
{
    public class GroceryItem : ItemBase
    {
        public bool IsPurchased {get; set;}
        public DateTime? AddedDate { get; set; }
    }
}

