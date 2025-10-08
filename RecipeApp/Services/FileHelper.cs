using Windows.Storage.Pickers;
using PuppeteerSharp;

namespace RecipeApp.Services;

public class FileHelper
{
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
