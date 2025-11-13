using System.Diagnostics;
using System.Text.RegularExpressions;
using Python.Runtime;
using Python.Included;
using RecipeApp.Services;

namespace RecipeApp.Controls.Pages;

public sealed partial class WebBrowserPage : NavigatorPage
{
    
    private static bool _pythonInitialized = false;

    [ObservableProperty] private partial string SearchBarText { get; set; } = "";
    [ObservableProperty] private partial string DownloadStatus { get; set; } = "";


    [ObservableProperty] private partial Visibility ProgressBarVis { get; set; } = Visibility.Collapsed;
    [ObservableProperty] private partial Visibility DownloadVis { get; set; } = Visibility.Visible;
    
    public WebBrowserPage(Navigator? nav = null) : base(nav)
    {
        this.InitializeComponent();

        InitializeWebView();
    }

    /// <summary>
    /// sets up some events and navigates to google
    /// </summary>
    private async void InitializeWebView()
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

    /// <summary>
    /// routes to webview go back
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonBack_OnClick(object sender, RoutedEventArgs e)
    {
        WebViewControl.GoBack();
    }

    /// <summary>
    /// routes to webview go forward
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonForward_OnClick(object sender, RoutedEventArgs e)
    {
        WebViewControl.GoForward();
    }

    /// <summary>
    /// checks if the text is a valid url, if it is it navigates to it, otherwise it navigates to google.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonGo_OnClick(object sender, RoutedEventArgs e)
    {
        if (Uri.TryCreate(SearchBarText, UriKind.Absolute, out var uri))
            WebViewControl.CoreWebView2.Navigate(SearchBarText);
        else
            WebViewControl.CoreWebView2.Navigate($"https://google.com/search?q={SearchBarText}");
    }
    
    
    /// <summary>
    /// handles downloading the html, setting up python with the recipe scrapers module, passing the recipe to the ai processing model, and saving the recipe.
    /// Do not change any package versions for the python stuff, this barely works.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="Exception"></exception>
    private async void ButtonDownload_OnClick(object sender, RoutedEventArgs e)
    {
        DownloadVis = Visibility.Collapsed;
        ProgressBarVis = Visibility.Visible;
        
        DownloadStatus = "Downloading HTML...";

        var html = await WebViewControl.ExecuteScriptAsync("document.documentElement.outerHTML;");

        html = Regex.Unescape(html);
        html = html.Substring(1, html.Length - 2);
        
        var source = WebViewControl.Source;
        
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

                    string TryValue(Func<dynamic> getter) {
                        try { return getter()?.ToString(); }catch { return null; }
                    }
                    
                    // can't serialize until converting it to a C# list due to pointers to the python objects.
                    List<string> TryList(Func<dynamic> getter) {
                        try
                        {
                            var result = getter();
                            if (result == null) return new List<string>();
        
                            using PyObject pyList = result;
                            var list = new List<string>();
                            var length = (int)pyList.Length();
        
                            for (var i = 0; i < length; i++)
                            {
                                using PyObject item = pyList[i];
                                list.Add(item.ToString());
                            }
                            return list;
                        }
                        catch
                        {
                            return new List<string>();
                        }
                    }
                    
                    return new
                    {
                        Title = TryValue(() => scraper.title()),
                        Ingredients = TryList(() => scraper.ingredients()),
                        InstructionsList = TryList(() => scraper.instructions_list()),
                        Instructions = TryValue(() => scraper.instructions()),
                        TotalTime = TryValue(() => scraper.total_time()),
                        Yields = TryValue(() => scraper.yields()),
                        Image = TryValue(() => scraper.image()),
                        Images = TryList(() => scraper.images()),
                        Author = TryValue(() => scraper.author()),
                        Description = TryValue(() => scraper.description()),
                        PrepTime = TryValue(() => scraper.prep_time()),
                        CookTime = TryValue(() => scraper.cook_time()),
                        Ratings = TryValue(() => scraper.ratings()),
                        Reviews = TryValue(() => scraper.reviews()),
                        Cuisine = TryValue(() => scraper.cuisine()),
                        Category = TryValue(() => scraper.category()),
                        Nutrients = TryValue(() => scraper.nutrients()),
                        Language = TryValue(() => scraper.language()),
                        CanonicalUrl = TryValue(() => scraper.canonical_url()),
                        Host = TryValue(() => scraper.host()),
                        Equipment = TryList(() => scraper.equipment())
                    };
                }
            });
            
            DownloadStatus = "Processing Recipe...";
            
            var aiResponse = await AiHelper.StringToSavedRecipe(JsonSerializer.Serialize(recipeInfo));
            
            if (aiResponse is null)
                throw new Exception("Error processing recipe");
            
            await SavedRecipe.Add(aiResponse);
            DownloadStatus = "Recipe Saved!";
            Navigator.Navigate(new RecipeDetailsV2(Navigator, aiResponse), $"Recipe Details: {aiResponse.Title}");
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
