using System.Diagnostics;
using Microsoft.Extensions.AI;
using PuppeteerSharp;
using Python.Runtime;
using Python.Included;
using RecipeApp.Services;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RecipeApp.Controls.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class WebBrowserPage : NavigatorPage
{
    [ObservableProperty] private partial string SearchBarText { get; set; } = "";
    [ObservableProperty] private partial string DownloadStatus { get; set; } = "";


    [ObservableProperty] private partial Visibility ProgressBarVis { get; set; } = Visibility.Collapsed;
    [ObservableProperty] private partial Visibility DownloadVis { get; set; } = Visibility.Visible;
    
    public WebBrowserPage(Navigator? nav = null) : base(nav)
    {
        this.InitializeComponent();

        InitializeWebView();
    }

    private async void InitializeWebView()
    {
        try
        {
            await WebViewControl.EnsureCoreWebView2Async();
            WebViewControl.CoreWebView2.Navigate("https://google.com");
            WebViewControl.CoreWebView2.SourceChanged += (_, _) 
                => { SearchBarText=WebViewControl.CoreWebView2.Source; };
            WebViewControl.CoreWebView2.NewWindowRequested += (_, args) =>
            {
                args.Handled = true;
                WebViewControl.CoreWebView2.Navigate(args.Uri);
            };
        }
        catch (Exception ex)
        {
            // Handle initialization errors
        }
    }

    private void ButtonBack_OnClick(object sender, RoutedEventArgs e)
    {
        WebViewControl.GoBack();
    }

    private void ButtonForward_OnClick(object sender, RoutedEventArgs e)
    {
        WebViewControl.GoForward();

    }

    private void ButtonGo_OnClick(object sender, RoutedEventArgs e)
    {
        if (Uri.TryCreate(SearchBarText, UriKind.Absolute, out var uri))
            WebViewControl.CoreWebView2.Navigate(SearchBarText);
        else
            WebViewControl.CoreWebView2.Navigate($"https://google.com/search?q={SearchBarText}");
    }
    
    
    private static bool _pythonInitialized = false;
    private async void ButtonDownload_OnClick(object sender, RoutedEventArgs e)
    {
        DownloadVis = Visibility.Collapsed;
        ProgressBarVis = Visibility.Visible;
        
        DownloadStatus = "Downloading HTML...";

        var source = WebViewControl.Source;
        
        var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();
    
        var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true
        });
    
        var page = await browser.NewPageAsync();
        await page.GoToAsync(source.ToString(), WaitUntilNavigation.Networkidle2);
    
        var html = await page.GetContentAsync();
        
        await page.CloseAsync();
        await browser.CloseAsync();
        
        // used Claude Sonnet 4.5 to generate a lot of the boilerplate for interacting with python using pythonnet (and python.included)
        // had to go through and really work to get his to work.
        try
        {
            var recipeInfo = await Task.Run(async () =>
            {
                if (!_pythonInitialized)
                {
                    DownloadStatus = "Verifying python installation...";

                    if (!Installer.IsPythonInstalled())
                    {
                        Debug.WriteLine("Installing python...");
                    }

                    await Installer.SetupPython();

                    DownloadStatus = "Verifying python installation...";

                    if (!Installer.IsPipInstalled())
                    {
                        Debug.WriteLine("Installing pip...");
                        await Installer.TryInstallPip();
                    }

                    DownloadStatus = "Verifying python installation...";

                    if (!Installer.IsModuleInstalled("setuptools"))
                    {
                        Debug.WriteLine("Installing setuptools...");
                        await Installer.PipInstallModule("setuptools");
                    }

                    DownloadStatus = "Verifying python installation...";

                    if (!Installer.IsModuleInstalled("wheel"))
                    {
                        Debug.WriteLine("Installing wheel...");
                        await Installer.PipInstallModule("wheel");
                    }

                    DownloadStatus = "Verifying python installation...";

                    // This is way too complicated, why can't it be simple for just once.
                    // have to use the no build isolation flag for this to work due to the embedded python runtime
                    if (!Installer.IsModuleInstalled("recipe_scrapers"))
                    {
                        DownloadStatus = "Installing recipe scrapers...";

                        var process = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = Path.Combine(Installer.EmbeddedPythonHome, "python.exe"),
                                Arguments = "-m pip install --no-build-isolation recipe-scrapers",
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                CreateNoWindow = true,
                                WorkingDirectory = Installer.EmbeddedPythonHome
                            }
                        };
                        
                        process.Start();
                        
                        var outputTask = process.StandardOutput.ReadToEndAsync();
                        var errorTask = process.StandardError.ReadToEndAsync();
    
                        await process.WaitForExitAsync();
    
                        string output = await outputTask;
                        string error = await errorTask;
    
                        Debug.WriteLine($"Exit Code: {process.ExitCode}");
                        Debug.WriteLine($"Output: {output}");
                        Debug.WriteLine($"Error: {error}");
                    }

                    PythonEngine.Initialize();
                    PythonEngine.BeginAllowThreads();
                    
                    _pythonInitialized = true;
                }
            
                DownloadStatus = "Extracting Recipe...";
            
                using (Py.GIL())
                {
                    dynamic scrapers = Py.Import("recipe_scrapers");
                    dynamic scraper = scrapers.scrape_html(html: html, org_url: source.ToString());

                    // can't serialize until converting it to a C# list due to pointers to the python objects.
                    var ingredients = scraper.ingredients();
                    var ingredientsList = new List<string>();
                    
                    if (ingredients != null)
                    {
                        using PyObject pyIngredients = ingredients;
                        var length = (int)pyIngredients.Length();
                            
                        for (var i = 0; i < length; i++)
                        {
                            using PyObject item = pyIngredients[i];
                            ingredientsList.Add(item.ToString());
                        }
                    }

                    return new
                    {
                        Title = scraper.title()?.ToString(),
                        Ingredients = ingredientsList,
                        Instructions = scraper.instructions()?.ToString(),
                        TotalTime = scraper.total_time()?.ToString(),
                        Yields = scraper.yields()?.ToString(),
                        Image = scraper.image()?.ToString(),
                        Author = scraper.author()?.ToString(),
                        Description = scraper.description()?.ToString(),
                        PrepTime = scraper.prep_time()?.ToString(),
                        CookTime = scraper.cook_time()?.ToString()
                    };
                }
            });
            
            DownloadStatus = "Processing Recipe...";
            
            var aiResponse = await AiHelper.StringToSavedRecipe(JsonSerializer.Serialize(recipeInfo));
            
            if (aiResponse is null)
                throw new Exception("Error processing recipe");
            
            await SavedRecipe.Add(aiResponse);
            DownloadStatus = "Recipe Saved!";
            Navigator.Navigate(new EditRecipe(Navigator, aiResponse), $"Edit Recipe: {aiResponse.Title}");
        }
        catch (Exception exception)
        {
            DownloadStatus = $"Error: {exception.Message}";
            await Task.Delay(5000);
        }
        
        DownloadVis = Visibility.Visible;
        ProgressBarVis = Visibility.Collapsed;
    }
}
