# Core UI
This section contains lists the core UI components of the application.

## Shell
This container is responsible for the navigation menu and the header.
The content of the shell is determined by the current page of the @RecipeApp.Models.Navigator
which handles all navigation and keeping track of the current page along with history.

View the @RecipeApp.Controls.Shell for details.

## INavigatorPage
This is an abstract class that inherits from a standard page. This is used in the @RecipeApp.Models.Navigator
to pass the Navigator object from page to page and implement a few extra features to make life easier.

**Highlighted Methods:**

| Returns       | Name        | Parameters  | Notes                                                         |
|---------------|-------------|-------------|---------------------------------------------------------------|
| Task          | Restore     | None        | Can be overridden to update values when returning to the page |
| NavigatorPage | Constructor | Navigator?  | Base constructor that sets the navigator for the page.        |

**Highlighted Properties:**

| Type       | Name       | Notes                                                     |
|------------|------------|-----------------------------------------------------------|
| Navigator  | Navigator  | The main navigator that is passed around the application. |

**Example Implementation:**

ExamplePage.xaml:
```xaml
<!-- Lot of boilerplate that should be created by the IDE is up here in the xaml header section -->
<local:NavigatorPage
    x:Class="RecipeApp.Controls.Pages.ExamplePage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:local="using:RecipeApp.Controls.Pages"
    Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">

    <!-- The controls to display go here -->
    <StackPanel VerticalAlignment="Center" HorizontalAlignment="Center">
        <TextBlock Text="Counter 2000" FontSize="24" TextAlignment="Center"/>

        <!-- We can use a binding here to display a value -->
        <!-- It is important to change the mode to either OneWay or TwoWay -->
        <!-- The following can be added to the TextBox control's binding to allow for text to update (in C#) as the user types: -->
        <!-- UpdateSourceTrigger=PropertyChanged -->
        <TextBlock Text="{x:Bind Count, Mode=OneWay}" FontSize="20" TextAlignment="Center" Margin="15"/>

        <!-- We can also use events to assign functions that should be called whenever an action happens, such as a click -->
        <Button Content="Increment" Click="ButtonBase_OnClick" HorizontalAlignment="Center"/>

    </StackPanel>
</local:NavigatorPage>
```

ExamplePage.xaml.cs:
```csharp
namespace RecipeApp.Controls.Pages;

public sealed partial class ExamplePage : NavigatorPage
{
    // ObservableProperty does a lot of the boilerplate code for binding to the UI
    [ObservableProperty] public partial int Count { get; set; } = 0;

    // any other properties here

    public ExamplePage(Navigator? nav = null) : base(nav)
    {
        this.InitializeComponent();

        // any other initialization here
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        => Count++;

    // any other methods here
}

```


View the @RecipeApp.Interfaces.INavigatorPage for details.
