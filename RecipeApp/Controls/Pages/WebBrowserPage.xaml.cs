using System.Diagnostics;
using Python.Runtime;
using Python.Included;

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

    private async void ButtonDownload_OnClick(object sender, RoutedEventArgs e)
    {
        DownloadVis = Visibility.Collapsed;
        ProgressBarVis = Visibility.Visible;
        
        DownloadStatus = "Downloading HTML...";

        var source = WebViewControl.Source;
        string html = await (WebViewControl.CoreWebView2.ExecuteScriptAsync("document.documentElement.outerHTML")?.AsTask() ?? new Task<string>(() => ""));
        
        // used Claude Sonnet 4.5 to generate a lot of the boilerplate for interacting with python using pythonnet (and python.included)
        var recipeInfo = await Task.Run(async () =>
        {
            DownloadStatus = "Updating Python...";
            
            await Installer.SetupPython();
            await Installer.InstallPip();
            await Installer.PipInstallModule("recipe-scrapers");
            
            var result2 = Installer.IsModuleInstalled("recipe-scrapers");
            Debug.WriteLine(result2);
            
            PythonEngine.Initialize();
            
            DownloadStatus = "Extracting Recipe...";
            
            using (Py.GIL())
            {
                dynamic scrapers = Py.Import("recipe_scrapers");
                dynamic scraper = scrapers.scrape_html(html: html, org_url: source.ToString());

                return new
                {
                    Title = scraper.title().ToString(),
                    Ingredients = ((IEnumerable<dynamic>)scraper.ingredients())
                        .Select(i => i.ToString()).ToList(),
                    Instructions = scraper.instructions().ToString(),
                    TotalTime = scraper.total_time()?.ToString(),
                    Yields = scraper.yields()?.ToString()
                };
            }
        });
        
        PythonEngine.Shutdown();
        
        DownloadStatus = "Processing Recipe...";

    }
}

