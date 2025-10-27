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
private async void OnButtonUploadPdfClick(object sender, RoutedEventArgs e)
    => await UploadRecipePdfAsync();

private async Task UploadRecipePdfAsync()
{
    var picker = new FileOpenPicker();
    picker.FileTypeFilter.Add(".pdf");
    picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

#if WINDOWS
    WinRT.Interop.InitializeWithWindow.Initialize(picker,
        WinRT.Interop.WindowNative.GetWindowHandle(App.Instance));
#endif

    var file = await picker.PickSingleFileAsync();
    if (file == null)
        return;

    try
    {
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

        // Step 4 – Confirmation
        await new ContentDialog
        {
            Title = "PDF Upload & Converted",
            Content = $"File saved:\n{savedFilePath}\n\nHTML created:\n{htmlPath}",
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        }.ShowAsync();
    }
    catch (Exception ex)
    {
        await new ContentDialog
        {
            Title = "PDF Upload Failed",
            Content = ex.Message,
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        }.ShowAsync();
    }
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
## Next Steps
1. Implement `HasPdf` property to display 📄 icon on recipe cards.
2. Add offline detection when LM Studio server is not running (Optional)
3. Merge duplicate recipes created from identical PDFs.




