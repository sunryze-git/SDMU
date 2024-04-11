using SDMU;
using Spectre.Console;
AnsiConsole.MarkupLine("[cyan]Welcome to SDMU![/]\n");

// Step 1: SD Card Preperations
SDManager.DetermineTargetDrive();

// Step 3: Check if Homebrew is already present on the SD Card
var homebrewPresent = SDManager.IsHomebrewPresent();
if (homebrewPresent)
{
    var backupMedia = AnsiConsole.Confirm("Homebrew is already present on this SD Card. Would you like to backup before running SDMU?");
    if (backupMedia)
    {
        FileManager.BackupMedia();
    }
}

// Step 4: Ask user if they would like to format the SD Card
var formatRequest = AnsiConsole.Confirm("This media will need to be [red]formatted.[/] Is this okay?");
if (formatRequest)
{
    SDManager.FormatSDCard();
}

// Step 5: Ask the user if they want Aroma or Tiramisu or both
var baseApps = AnsiConsole.Prompt(new MultiSelectionPrompt<string>()
       .Title("[yellow]Which Custom Firmware would you like to install?[/]")
       .PageSize(10)
       .AddChoices(AppTypes.BaseApps.Keys));

// Step 6: Ask the user if they want other homebrew apps installed
var extraApps = AnsiConsole.Prompt(new MultiSelectionPrompt<string>()
       .Title("[yellow]Which additional applications would you like to install?[/]")
       .PageSize(10)
       .AddChoices(AppTypes.ExtraApps)
       .NotRequired());

// Step 7: Download and install the selected base apps
foreach (var app in baseApps)
{
    if (AppTypes.BaseApps.TryGetValue(app, out var action))
    {
        AnsiConsole.Status()
            .Start($"Downloading {app}", ctx =>
            {
                ctx.Spinner(Spinner.Known.Star);
                action().Wait();
            });
    }
    else
    {
        AnsiConsole.MarkupLine($"[red]Failed to download {app}![/]");
    }
}

// Step 8: Download and install the selected extra apps
foreach (var app in extraApps)
{
    if (AppTypes.ExtraApps.Contains(app))
    {
        AnsiConsole.Status()
            .Start($"Downloading {app}", ctx =>
            {
                ctx.Spinner(Spinner.Known.Star);
                Downloader.DownloadPackage(app).Wait();
            });
    }
    else
    {
        AnsiConsole.MarkupLine($"[red]Failed to download {app}![/]");
    }
}