using SDMU.NewFramework;
using Spectre.Console;

namespace SDMU.Utilities;

internal class HbManager(MediaDevice mediaDevice, FileManager fileManager, Downloader downloader, AppTypes appType)
{
    public async Task InstallHomebrew()
    {
        try
        {
            // Ask user if they would like to format the SD Card
            var formatRequest = AnsiConsole.Confirm("This media will need to be [red]formatted.[/] Is this okay?");
            if (formatRequest)
                await mediaDevice.Format();
            else
                return;

            // Ask the user if they want Aroma or Tiramisu or both
            var baseApps = AnsiConsole.Prompt(new MultiSelectionPrompt<string>()
                .Title("[yellow]Which Custom Firmware would you like to install?[/]")
                .PageSize(10)
                .AddChoices(appType.BaseApps.Keys));

            // Ask the user if they want other homebrew apps installed
            var extraApps = AnsiConsole.Prompt(new MultiSelectionPrompt<string>()
                .Title("[yellow]Which additional applications would you like to install?[/]")
                .PageSize(10)
                .AddChoices(appType.ExtraApps)
                .NotRequired());

            // Download and install the selected base apps
            await AnsiConsole.Status().StartAsync("Installing Homebrew...", async ctx =>
            {
                // Process base apps
                foreach (var app in baseApps)
                {
                    if (!appType.BaseApps.TryGetValue(app, out var action)) continue;
                    ctx.Status($"Downloading {app}...");
                    await action();
                }

                // Process extra apps
                foreach (var app in extraApps)
                {
                    ctx.Status($"Downloading {app}...");
                    await downloader.DownloadPackage(app);
                }
            });

            AnsiConsole.MarkupLine("[green]Installation complete![/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
        }
        finally
        {
            AnsiConsole.MarkupLine("[grey]Returning to main menu in 5 seconds...[/]");
            await Task.Delay(5000);
            Console.Clear();
        }
    }

    public Task UpdateHomebrew()
    {
        return Task.CompletedTask;
    }
}