using Windows.Storage.Pickers;
using PuppeteerSharp;

namespace RecipeApp.Services;

/// <summary>
/// Helper functions related to file management, file saving, and file converting.
/// </summary>
public class FileHelper
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="html"></param>
    /// <param name="filePath">file to save to if known or null for a file picker to open before saving
    /// (user can cancel saving when using a file dialog)</param>
    public static async Task SaveHtmlAsPdf(string html, string? filePath = null)
    {
        if (filePath is null)
        {
            var savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SuggestedFileName = "Recipes",
                CommitButtonText = "Save",
                FileTypeChoices = { ["PDF Document"] = [".pdf"] }
            };

            var result = await savePicker.PickSaveFileAsync();
            
            if (result is null) return;
            
            filePath = result.Path;
        }
        
        if (!filePath.EndsWith(".pdf", StringComparison.InvariantCultureIgnoreCase))
            filePath += ".pdf";
        
        await new BrowserFetcher().DownloadAsync();
        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
        await using var page = await browser.NewPageAsync();
        await page.SetContentAsync(html);
        await page.PdfAsync(filePath);
    }
}
