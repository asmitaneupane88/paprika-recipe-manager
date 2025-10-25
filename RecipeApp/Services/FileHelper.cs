// cannot be a global using!
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Pdf;
using Windows.Storage.Pickers;  
using PuppeteerSharp;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

    

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

    public static async Task<string> ConvertPdfToTextAsync(string pdfPath)
    {
        if (!File.Exists(pdfPath))
            throw new FileLoadException("PDF not found", pdfPath);
        var sb = new StringBuilder();

        await Task.Run(() =>
        {
            using var document = UglyToad.PdfPig.PdfDocument.Open(pdfPath);
            foreach (var page in document.GetPages())
            {
                sb.AppendLine(page.Text);
            }
        });
        return sb.ToString();
    }

    public static async Task<string> ConvertPdftoHtmlAsync(string pdfPath)
    {
        var text = await ConvertPdfToTextAsync(pdfPath);

        var html = $"""
        <!DOCTYPE html>
        <html>
        <head><meta charset=""UTF-8""><title>Converted PDF</title></head>
        <body><pre>{System.Net.WebUtility.HtmlEncode(text)}</pre></body>
        </html>
        """;
        
        var htmlPath = Path.ChangeExtension(pdfPath, ".html");
        await File.WriteAllTextAsync(htmlPath, html);
        
        return htmlPath;
    }
}
