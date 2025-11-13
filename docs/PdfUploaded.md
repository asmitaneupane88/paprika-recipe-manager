# PDF Upload & AI Conversion (Saved Recipes Feature)
## Overview
Implements **PDF Upload, text extraction, and AI-based recipe conversion.**

This feature allows users to upload recipe PDFs, convert them into readable text (HTML), and automatically generate structured recipes via locally running **LM Studio AI model.**

## Architecture
**Flow:**
1. User clicks to **Upload** button on the `Saved Recipe` page.
2. `RecipeListPage.xaml.cs` opens a file picker for `.pdf` files.
3. The selected PDF is saved in `Documents\RecipeAppFiles`.
4. `FileHelper.ConvertPdfToTextAsync()` converts the file into HTML.
5. The text is sent to `AiHelpper.StringToSavedRecipe()` to create a structured recipe.
6. The new recipe is stored via `SavedRecipe.Add(aiRecipe`.
7. User receives dialogs summarizing upload, conversion, and AI generation results.

## File involved
| File | Role |
|------|------|
| `Controls/Pages/RecipeListPage.xaml.cs` | Handles UI actions and PDF upload workflow |
| `Services/FileHelper.cs` | Converts PDF → HTML text |
| `Services/AiHelper.cs` | Connects to LM Studio and parses AI responses |
| `Models/SavedRecipe.cs` | Represents stored recipes in local database |
| `Docs/AiSettings.json` | Configures LM Studio API endpoint and model |
| `Converters/BoolToVisibilityConverter.cs` | (Planned) Controls PDF icon visibility on UI |

## Example Code
```csharp
// // RecipeListPage.xaml.cs
        // Step 1 – Save PDF
        var localFolderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "RecipeAppFiles");
        Directory.CreateDirectory(localFolderPath);

        var savedFilePath = Path.Combine(localFolderPath, file.Name);
        await file.CopyAsync(await StorageFolder.GetFolderFromPathAsync(localFolderPath),
            file.Name, NameCollisionOption.GenerateUniqueName);

        // Step 2 – Convert PDF → HTML
        var htmlText = await FileHelper.ConvertPdfToTextAsync(savedFilePath);
        var htmlPath = Path.Combine(localFolderPath,
            $"{Path.GetFileNameWithoutExtension(savedFilePath)}.html");
        await File.WriteAllTextAsync(htmlPath, htmlText);

        // Step 3 – AI Conversion (LM Studio)
        try
        {
            var aiRecipe = await AiHelper.StringToSavedRecipe(htmlText);
            if (aiRecipe != null)
                await SavedRecipe.Add(aiRecipe);

            await new ContentDialog
            {
                Title = "AI Recipe Created",
                Content = aiRecipe != null
                    ? $"Created new recipe from PDF:\n{aiRecipe.Title}"
                    : "AI returned no recipe",
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            }.ShowAsync();
        }
        catch (Exception aiEx)
        {
            await new ContentDialog
            {
                Title = "AI Conversion Failed",
                Content = $"Could not generate recipe automatically.\n{aiEx.Message}",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            }.ShowAsync();
        }
  ```
## LM Studio Configuration

| Setting                | Description                                                                     |
|------------------------|---------------------------------------------------------------------------------|
| Config file            | `%LOCALAPPDATA%\O=RecipeApp\com.companyname.RecipeApp\Settings\AiSettings.json` |
| End point              | `http://localhost:1234/v1`                                                      |
| Model                  | `phi-3-mini-4k-instruct`                                                        |
| API Key                | Empty (local model only)                                                        |

## Example:
```json
{
  "Endpoint": "http://localhost:1234/v1",
  "ApiKey": "",
  "LastUsedModel": "phi-3-mini-4k-instruct"
}
```
To verify connectivity:
`curl http://localhost:1234/v1/models`

Expected response:
```json
{
  "data": [
    { "id": "phi-3-mini-4k-instruct", "object": "model" },
    { "id": "text-embedding-nomic-embed-text-v1.5", "object": "model" }
  ],
  "object": "list"
}
```
## Output Example
```plaintext
Documents/
└── RecipeAppFiles/
    ├── chicken_pasta.pdf
    └── chicken_pasta.html
```
If AI succeeds, a new recipe appears in Saved Recipes:
```json
{
  "Title": "Creamy Chicken Pasta",
  "Description": "A rich and creamy pasta with chicken and herbs.",
  "Category": "Dinner",
  "PdfPath": "C:\\Users\\<user>\\Documents\\RecipeAppFiles\\chicken_pasta.pdf",
  "HtmlPath": "C:\\Users\\<user>\\Documents\\RecipeAppFiles\\chicken_pasta.html"
}
```
## Implement `HasPdf` Property for Saved Recipe Cards
The recipes originate from imported PDFs. To help users quickly distinguish them in the UI, we show a small 📄 icon on each recipe card that has an associated PDF file.
1. Add a new boolean property `HasPdf` to the `RecipeIngredient` receive model that indicates whether the recipe has an attached PDF.
2. Update data loading logic so the flag is automatically computed when:
   * A recipe is imported from PDF.
   * A PDF file is added or removed
   3. Display the icon (📄) on the recipe list cards when `HasPdf == true`
### Notes:
* Use `IAutosavingClass` to auto-save this field
* PDF storage may live inside local app or external file path, define this clearly before implemetation.

## Merge Duplicate Recipes created from identical PDF
Model extension
```csharp
[ObservableProperty]
public partial string PdfHash { get; set; }
```
Import flow:
1. `existing` refers to an already stored recipe whose `PdfPath` ends with the same filename
2. If found -> system shows a dialog and stops processing
```csharp
if (existing != null)
            {
                await new ContentDialog
                {
                    Title = "Duplicate PDF Detected",
                    Content = $"A recipe for {Path.GetFileName(savedFilePath)} already exists: {existing.SavedRecipe.Title}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                }.ShowAsync();
                return;
            }
```
If no duplicate is found, the app attaches the PDF and its HTML conversion to the currently selected recipe
```csharp
var selectedRecipe = GetSelectedRecipes().FirstOrDefault();
if (selectedRecipe != null)
{
    selectedRecipe.PdfPath = savedFilePath;
    selectedRecipe.HtmlPath = htmlPath;

    await SavedRecipe.Update(selectedRecipe);
}

```



