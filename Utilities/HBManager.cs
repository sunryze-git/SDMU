﻿using SDMU.NewFramework;
using Spectre.Console;

namespace SDMU.Utilities;
internal class HBManager
{
    public static void InstallHomebrew()
    {
        // Ask user if they would like to format the SD Card
        var formatRequest = AnsiConsole.Confirm("This media will need to be [red]formatted.[/] Is this okay?");
        if (formatRequest)
        {
            MediaDevice.Format();
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
        FileManager.BackupMedia();
        AnsiConsole.MarkupLine("[#5D8AA8]Backup completed successfully.[/]");
        Thread.Sleep(5000);
    }

    public static void RestoreSDCard()
    {
        FileManager.RestoreMedia();
        AnsiConsole.MarkupLine("[#5D8AA8]Restore completed successfully.[/]");
        Thread.Sleep(5000);
    }

    public static void CleanupSDCard()
    {
        // Not Implimented yet
        // Will provide a list of WUPS that are not installed (like in wiiu\install)
        // Will also ask about backups on the SD card and if you would like to move them to PC
    }

    public static void FormatSDCard()
    {
        MediaDevice.Format();
        AnsiConsole.MarkupLine("[#5D8AA8]Format completed successfully.[/]");
        Thread.Sleep(5000);
    }
}
