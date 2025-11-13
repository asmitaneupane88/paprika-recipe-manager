# Grocery -> Pantry Auto-Move (Purchased items)
## Overview
This feature automatically transfers Grocery List items into the Pantry when the user marks them as **Purchased**. The goal is to keep pantry inventory up to date with minimal manual effort.

This feature supports:
* Removing items from the Grocery List
* Creating (or updating) the corresponding PantryItem
* Persisting change immediately
* Update the UI to reflect the new pantry state

## System behavior
### 1. User interaction
* User check one or more items in the Grocery List UI
* User press **Purchased** button
### 2. App logic
GroceryItem -> PantryIngredient mapping occurs
If the pantry already contains an item with the same name:
- Increase the quantity

Else:
Add a new PantryIngredient entry
### Persistence
- `RecipeIngredient.Remove()` removes the grocery item
- `PantryIngredient.Add()` inserts or updates the pantry item
- UI automatically refreshes through `ObservableCollection<T>`
## Implementation Details

### Shared Ingredient Model Mapping

Fields currently supported:

| GroceryItem field | PantryIngredient field |
|-------------------|------------------------|
| Name              | Name                   |
| Quantity          | Quantity               |
| Unit              | Unit                   |
| ModifierNote      | ModifierNote           |
| ScaleFactor       | ScaleFactor            |

All fields persist automatically through the `IAutosavingClass<T>` system.

### Sequence Flow Diagram
```scss
[User checks item]
      ↓
[Click "Purchased"]
      ↓
[GroceryListPage.xaml.cs]
  • GetSelectedItems()
  • For each item:
        - Map fields
        - PantryIngredient.Add()
        - RecipeIngredient.Remove()
        - Remove from UI lists
      ↓
[PantryIngredientsPage.xaml]
  • Auto-refresh triggered via ObservableCollection

```
## Testing checklist
### Functional Tests
* Mark single grocery item as purchased → moves to pantry
* Mark multiple items → all move correctly
* Items removed from grocery list
* Pantry displays updated items
* Category grouping still works
### UI Tests
* Purchased button disables when no items selected
* Pantry refreshes without page reload
