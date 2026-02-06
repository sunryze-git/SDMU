namespace SDMU.Menus;

using NewFramework;
using Utilities;
using Spectre.Console;

internal class AppMenu(
    MediaDevice mediaDevice, FileManager fileManager, Downloader downloader, AppTypes appType, Updater updater)
{
    internal async Task Show()
    {
        while (true)
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
            var apps = mediaDevice.InstalledPackages;

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
                appTable.AddRow("No Applications Installed");
            }

            // Display Table
            AnsiConsole.Write(appTable);

            var promptItems = new List<(string Name, string Id)>();

            promptItems.AddRange([
                ("Install Application", "install"),
                ("Update Application", "update"),
                ("Uninstall Application", "uninstall"),
                (" ", "spacer"),
                ("Return to Main Menu", "back")
            ]);

            var prompt = new SelectionPrompt<(string Name, string Id)>()
                .PageSize(10)
                .UseConverter(item => item.Name)
                .AddChoices(promptItems);

            var selectedItem = AnsiConsole.Prompt(prompt);

            // Exit the loop and program if "Exit" is selected
            if (selectedItem.Id == "back") break;

            // Ignore selections of spacers
            if (selectedItem.Id == "spacer") continue;

            await HandleSelection(selectedItem.Id);
        }
    }

    private async Task HandleSelection(string id)
    {
        switch (id)
        {
            case "install":
                // Ask the user if they want other homebrew apps installed
                var extraApps = AnsiConsole.Prompt(new MultiSelectionPrompt<string>()
                       .Title("[yellow]Which additional applications would you like to install?[/]")
                       .PageSize(10)
                       .AddChoices(appType.ExtraApps)
                       .NotRequired());

                foreach (var app in extraApps)
                {
                    if (appType.ExtraApps.Contains(app))
                    {
                        AnsiConsole.Status()
                            .Start($"Downloading {app}", ctx =>
                            {
                                ctx.Spinner(Spinner.Known.Star);
                                downloader.DownloadPackage(app).Wait();
                            });
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[red]Failed to download {app}![/]");
                    }
                }
                break;
            case "update":
                await updater.ComparePackageHash();
                break;
            case "uninstall":
                break;
        }
    }
}