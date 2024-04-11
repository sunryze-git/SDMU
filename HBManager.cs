using Spectre.Console;

namespace SDMU;
internal class HBManager
{
    public static void InstallHomebrew()
    {
        // SD Card Preperations
        SDManager.DetermineTargetDrive();

        // Ask user if they would like to format the SD Card
        var formatRequest = AnsiConsole.Confirm("This media will need to be [red]formatted.[/] Is this okay?");
        if (formatRequest)
        {
            SDManager.FormatSDCard();
        }
        else
        {
            return;
        }

        // Ask the user if they want Aroma or Tiramisu or both
        var baseApps = AnsiConsole.Prompt(new MultiSelectionPrompt<string>()
               .Title("[yellow]Which Custom Firmware would you like to install?[/]")
               .PageSize(10)
               .AddChoices(AppTypes.BaseApps.Keys));

        // Ask the user if they want other homebrew apps installed
        var extraApps = AnsiConsole.Prompt(new MultiSelectionPrompt<string>()
               .Title("[yellow]Which additional applications would you like to install?[/]")
               .PageSize(10)
               .AddChoices(AppTypes.ExtraApps)
               .NotRequired());

        // Download and install the selected base apps
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

        // Download and install the selected extra apps
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

        AnsiConsole.MarkupLine("[green]Installation complete![/]");
        Thread.Sleep(5000);
        Console.Clear();
    }

    public static void UpdateHomebrew()
    {

    }

    public static void BackupSDCard()
    {

    }

    public static void RestoreSDCard()
    {

    }

    public static void CleanupSDCard()
    {

    }


}
