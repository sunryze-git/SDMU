using SDMU;
using Spectre.Console;
AnsiConsole.Markup("Welcome to [cyan]SDMU![/]\n");

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
       .Title("Which Custom Firmware would you like to install?")
       .PageSize(10)
       .AddChoices(Enum.GetNames(typeof(AppTypes.baseApps))));

// Step 6: Ask the user if they want other homebrew apps installed
var extraApps = AnsiConsole.Prompt(new MultiSelectionPrompt<AppTypes.extraApps>()
       .Title("Which additional applications would you like to install?")
       .PageSize(10)
       .AddChoices(Enum.GetValues(typeof(AppTypes.extraApps)).Cast<AppTypes.extraApps>()));

// Step 7: Get HB Repository
var downloader = new Downloader();
// downloader.GetPackages();

// Step 8: Download and install base apps
await downloader.DownloadTiramisu();
await downloader.DownloadAroma();