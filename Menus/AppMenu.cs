namespace SDMU.Menus;
using SDMU.Utilities;
using Spectre.Console;

internal class AppMenu
{
    internal static void Show()
    {
        while (true)
        {
            Console.Clear();

            // Get list of applications installed
            var apps = Downloader.GetInstalledPackages();

            var appTable = new Table();
            appTable.AddColumn("Name");
            appTable.AddColumn("Version");
            appTable.AddColumn("Category");
            appTable.AddColumn("Author");
            appTable.AddColumn("Updated");

            // Add each application to the panel
            foreach (var app in apps)
            {
                appTable.AddRow(app.Name, app.Version, app.Category, app.Author, app.Updated);
            }

            if (apps.Count() < 1)
            {
                appTable.AddRow("No Applications Installed");
            }

            // Display Table
            AnsiConsole.Write(appTable);

            var promptItems = new List<(string Name, string Id)>();

            promptItems.AddRange(new[]
            {
                ("Install Application", "install"),
                ("Update Application", "update"),
                ("Uninstall Application", "uninstall"),
                (" ", "spacer"),
                ("Return to Main Menu", "back")
            });

            var prompt = new SelectionPrompt<(string Name, string Id)>()
                .Title("SDMU Application Menu:")
                .PageSize(10)
                .UseConverter(item => item.Name)
                .AddChoices(promptItems);

            var selectedItem = AnsiConsole.Prompt(prompt);

            // Exit the loop and program if "Exit" is selected
            if (selectedItem.Id == "back") break;

            // Ignore selections of spacers
            if (selectedItem.Id == "spacer") continue;

            HandleSelection(selectedItem.Id);
        }
    }

    private static void HandleSelection(string id)
    {
        switch (id)
        {
            case "install":
                // Ask the user if they want other homebrew apps installed
                var extraApps = AnsiConsole.Prompt(new MultiSelectionPrompt<string>()
                       .Title("[yellow]Which additional applications would you like to install?[/]")
                       .PageSize(10)
                       .AddChoices(AppTypes.ExtraApps)
                       .NotRequired());

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
                break;
            case "update":
                break;
            case "uninstall":
                break;
        }
    }
}