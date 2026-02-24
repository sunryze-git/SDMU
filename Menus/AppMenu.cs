namespace SDMU.Menus;

using NewFramework;
using Utilities;
using Spectre.Console;

internal class AppMenu(
    MediaDevice mediaDevice, Downloader downloader, AppTypes appType, Updater updater)
{
    internal async Task Show()
    {
        var running = true;
        while (running)
        {
            Console.Clear();

            // Write Header
            AnsiConsole.Write(
                new Panel(
                    new FigletText("Applications")
                        .Centered()
                        .Color(Color.LightSteelBlue))
                .Expand()
                .Border(BoxBorder.Rounded)
                .Header("[yellow]Always make sure to reference the [link=https://wiiu.hacks.guide#/][blue]Wii U Hacks Guide![/][/][/]")
                .HeaderAlignment(Justify.Center)
                .BorderStyle(new Style(Color.White)));

            AnsiConsole.MarkupLine($"[yellow]SD Card: {mediaDevice.Device.Name}[/]\n");
            // 

            // Get list of applications installed
            var apps = await mediaDevice.GetInstalledPackages();
            AnsiConsole.MarkupLine($"[grey]Applications: {apps.Length}[/]\n");

            var appTable = new Table();
            appTable.AddColumn("Name");
            appTable.AddColumn("Version");
            appTable.AddColumn("Category");
            appTable.AddColumn("Author");
            appTable.AddColumn("Updated");

            // Add each application to the panel
            foreach (var app in apps)
            {
                appTable.AddRow(
                    new Markup($"[cyan]{app.Name}[/]"),
                    new Markup($"[grey78]{app.Version}[/]"),
                    new Markup($"[yellow]{app.Category}[/]"),
                    new Markup($"[grey78]{app.Author}[/]"),
                    new Markup($"{app.Updated}")
                );
            }

            if (apps.Length == 0)
            {
                AnsiConsole.MarkupLine("[red]No applications found on SD card.[/]");
            }
            else
            {
                AnsiConsole.Write(appTable);
            }

            var promptItems = new List<(string Name, Func<Task> Id)>
            {
                ("Install Application", InstallApp),
                ("Update Application", updater.UpdatePackages),
                ("Uninstall Application", UninstallApp), // Uninstall not implemented yet
                ("Return to Main Menu", () => { running = false; return Task.CompletedTask; })
            };

            var prompt = new SelectionPrompt<(string Name, Func<Task> Id)>()
                .PageSize(10)
                .UseConverter(item => item.Name)
                .AddChoices(promptItems);

            var selectedItem = AnsiConsole.Prompt(prompt);
            await selectedItem.Id.Invoke();
        }
    }

    private async Task UninstallApp()
    {
        var apps = await mediaDevice.GetInstalledPackages();
    
        if (apps.Length == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No applications found to uninstall.[/]");
            Thread.Sleep(2000);
            return;
        }

        var toUninstall = AnsiConsole.Prompt(
            new MultiSelectionPrompt<Package>()
                .Title("Select applications to [red]uninstall[/]:")
                .UseConverter(p => p.Name!)
                .AddChoices(apps));

        if (toUninstall.Count == 0) return;

        if (!AnsiConsole.Confirm($"Are you sure you want to delete {toUninstall.Count} apps?")) 
            return;

        foreach (var app in toUninstall)
        {
            try
            {
                // Assuming your Package object has the folder path or 
                // you can derive it from the name
                var appPath = Path.Combine(mediaDevice.Device.Name, "wiiu", "apps", app.Name!);

                if (!Directory.Exists(appPath)) continue;
                Directory.Delete(appPath, true);
                AnsiConsole.MarkupLine($"[green]✔[/] Uninstalled {app.Name}");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]✘[/] Failed to delete {app.Name}: {ex.Message}");
            }
        }

        AnsiConsole.MarkupLine("\n[yellow]Press any key to return...[/]");
        Console.ReadKey();
    }

    private async Task InstallApp()
    {
        var extraApps = AnsiConsole.Prompt(new MultiSelectionPrompt<string>()
            .Title("[yellow]Which additional applications would you like to install?[/]")
            .AddChoices(appType.ExtraApps)
            .NotRequired());

        if (extraApps.Count == 0) return;

        // Use Status for a clean, single-item UI since we are doing one at a time
        await AnsiConsole.Status()
            .StartAsync("Initializing downloads...", async ctx =>
            {
                foreach (var app in extraApps)
                {
                    ctx.Status($"Downloading [bold]{app}[/]");
                    ctx.Spinner(Spinner.Known.Dots);

                    try
                    {
                        await downloader.DownloadPackage(app);
                        AnsiConsole.MarkupLine($"[green]✔[/] {app} installed.");
                    }
                    catch (Exception ex)
                    {
                        // Print error but keep the loop going for the next apps
                        AnsiConsole.MarkupLine($"[red]✘[/] {app} failed: {ex.Message}");
                    }
                }
            });

        AnsiConsole.MarkupLine("[bold green]Done![/]");
    }
}