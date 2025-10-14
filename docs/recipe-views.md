# Recipe Views Documentation

## EditRecipe View

### Overview

The EditRecipe view provides a user interface for creating and modifying recipes in the Paprika Recipe Manager application. It implements a form-based interface with two-way data binding for real-time updates.

### XAML Structure

```xaml
<local:NavigatorPage
    x:Class="RecipeApp.Controls.Pages.EditRecipe"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:local="using:RecipeApp.Controls.Pages"
    xmlns:models="using:RecipeApp.Models">
```

### UI Components

#### Header Section

```xaml
<TextBlock Text="Edit Recipe"
          Style="{ThemeResource TitleLargeTextBlockStyle}"/>
```

#### Recipe Form Sections

1. **Information**

   - Title input
   - Description (multi-line)
   - Category selection
   - Rating control
   - Preparation time
   - Cooking time
   - Number of servings
   - Ingredient list
   - Add/remove controls
   - Amount and unit inputs

### Data Binding

Example of two-way binding implementation:

```xaml
<TextBox Text="{x:Bind Recipe.Title, Mode=TwoWay}"
         PlaceholderText="Enter recipe title"/>

<RatingControl Value="{x:Bind Recipe.Rating, Mode=TwoWay}"
               IsReadOnly="False"/>
```

### Code-Behind Features

```csharp
public sealed partial class EditRecipe : NavigatorPage
{
    public SavedRecipe Recipe { get; }

    public EditRecipe(Navigator navigator, SavedRecipe recipe)
    {
        this.InitializeComponent();
        Recipe = recipe;
    }
}
```

## RecipeDetails View

### Overview

The RecipeDetails view displays a comprehensive view of a recipe, including all ingredients, instructions, and metadata.

### XAML Structure

```xaml
<local:NavigatorPage
    x:Class="RecipeApp.Controls.Pages.RecipeDetails"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
```

### UI Components

1. **Header**

   - Recipe title
   - Category
   - Rating display
   - Image (if available)

2. **Recipe Information**

   - Preparation time
   - Cooking time
   - Total time
   - Servings
   - Difficulty level

3. **Ingredients List**

   - Quantities
   - Units
   - Ingredient names
   - Notes

4. **Cooking Instructions**
   - Numbered steps
   - Clear formatting
   - Easy to read layout

### View Features

1. **Navigation**

   - Edit button (if editable)
   - Back button
   - Share options

2. **Visual Elements**

   - Recipe image
   - Rating stars
   - Time indicators
   - Category tags

3. **Interactive Elements**
   - Ingredient checkboxes
   - Step completion tracking
   - Scaling controls

### Example Implementations

#### Ingredient List Display

```xaml
<ListView ItemsSource="{x:Bind Recipe.Ingredients}">
    <ListView.ItemTemplate>
        <DataTemplate x:DataType="models:Ingredient">
            <StackPanel Orientation="Horizontal">
                <TextBlock Text="{x:Bind Amount}"/>
                <TextBlock Text="{x:Bind Unit}"/>
                <TextBlock Text="{x:Bind Name}"/>
            </StackPanel>
        </DataTemplate>
    </ListView.ItemTemplate>
</ListView>
```

#### Time Display

```xaml
<StackPanel Orientation="Horizontal">
    <TextBlock Text="Prep Time:"/>
    <TextBlock Text="{x:Bind Recipe.PrepTimeMinutes}"/>
    <TextBlock Text="minutes"/>
</StackPanel>
```