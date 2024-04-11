using SDMU;
using Spectre.Console;

internal static class MainMenu
{
    public static void Show()
    {
        while (true)
        {
            var homebrewDetected = SDManager.IsHomebrewPresent();
            var mainMenuItems = new List<(string Name, string Id)>();

            // Conditional items based on Homebrew detection
            if (!homebrewDetected)
            {
                mainMenuItems.Add(("Install Homebrew", "install"));
            }
            else
            {
                mainMenuItems.AddRange(new[]
                {
                    ("Update Homebrew", "update")
                });
            }

            // Adding spacers
            mainMenuItems.Add((" ", "spacer1"));

            // Common items
            mainMenuItems.AddRange(new[]
            {
                ("Backup SD Card", "backup"),
                ("Restore SD Card", "restore"),
                ("Cleanup SD Card", "cleanup"),
                (" ", "spacer2"), // Another spacer
                ("Exit", "exit")
            });

            var prompt = new SelectionPrompt<(string Name, string Id)>()
                .Title("SDMU Main Menu:")
                .PageSize(10)
                .UseConverter(item => item.Name)
                .AddChoices(mainMenuItems);

            var selectedItem = AnsiConsole.Prompt(prompt);

            // Exit the loop and program if "Exit" is selected
            if (selectedItem.Id == "exit") break;

            // Ignore selections of spacers
            if (selectedItem.Id.StartsWith("spacer")) continue;

            HandleSelection(selectedItem.Id, homebrewDetected);
        }
    }

    private static void HandleSelection(string selectionId, bool homebrewDetected)
    {
        switch (selectionId)
        {
            case "install":
                HBManager.InstallHomebrew();
                break;
            case "update":
                HBManager.UpdateHomebrew();
                break;
            case "backup":
                HBManager.BackupSDCard();
                break;
            case "restore":
                HBManager.RestoreSDCard();
                break;
            case "cleanup":
                HBManager.CleanupSDCard();
                break;
            default:
                AnsiConsole.WriteLine("Invalid selection.");
                break;
        }
    }
}
