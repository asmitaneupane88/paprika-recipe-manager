using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using RecipeApp.Models.Shared;
using RecipeApp.Services;
using RecipeApp.Enums;
using RecipeApp.Interfaces;

namespace RecipeApp.Models
{
    /// <summary>
    /// Represents an item in the Grocery list.
    /// Automatically moves to Pantry when marked as purchased.
    /// </summary>
    public partial class GroceryItem : IAutosavingClass<GroceryItem>
    {
        [ObservableProperty] private string name = string.Empty;
        [ObservableProperty] private double quantity = 0;
        [ObservableProperty] private bool isPurchased;

        public DateTime? AddedDate { get; set; }
        
        partial void OnIsPurchasedChanged(bool oldValue, bool newValue)
        {
            if (newValue)
            {
                _ = AutoMoveToPantryAsync();
            }
        }

       private async Task AutoMoveToPantryAsync()
        {
            try
            {
                var pantryItem = new PantryIngredient
                {
                    Name         = this.Name,
                    Quantity     = 1,
                    Unit         = UnitType.ITEM,
                    ModifierNote = string.Empty,
                    ScaleFactor  = 1.0
                };

                await PantryService.Add(pantryItem);
                await GroceryService.Remove(this);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AutoMoveToPantry] Error: {ex.Message}");
            }
        }
    }
}

