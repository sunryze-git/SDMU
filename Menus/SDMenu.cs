using SDMU.NewFramework;
using SDMU.Utilities;
using Spectre.Console;
using System.IO;

namespace SDMU.Menus;
internal class SDMenu
{
    // Get table of backups within the backup directory
    private static Table GetBackups()
    {
        var backupDir = FileManager.GetBackupsDirectory();
        var backups = Directory.GetDirectories(backupDir);

        var table = new Table();
        table.AddColumn("Backup Name");
        table.AddColumn("Date Created");

        foreach (var backup in backups)
        {
            var backupName = Path.GetFileName(backup);
            var dateCreated = Directory.GetCreationTime(backup).ToString("yyyy-MM-dd HH:mm:ss");

            table.AddRow(backupName, dateCreated);
        }

        return table;
    }

    // Show Menu Screen
    internal static void Show()
    {
        while (true)
        {
            Console.Clear();

            // Write Header
            AnsiConsole.Write(
                new Panel(
                    new FigletText("SD Card")
                        .Centered()
                        .Color(Color.LightSteelBlue))
                .Expand()
                .Border(BoxBorder.Rounded)
                .Header("[yellow]Always make sure to reference the [link=https://wiiu.hacks.guide#/][blue]Wii U Hacks Guide![/][/][/]")
                .HeaderAlignment(Justify.Center)
                .BorderStyle(new Style(Color.White)));

            AnsiConsole.MarkupLine($"[yellow]SD Card: {MediaDevice.Device?.Name}[/]\n");
            // 

            var promptItems = new List<(string Name, string Id)>();
            promptItems.AddRange(new[]
            {
                ("Backup SD Card", "backup"),
                ("Restore SD Card", "restore"),
                ("Delete Backup", "delete"),
                ("Format SD Card", "format"),
                (" ", "spacer"),
                ("Return to Main Menu", "back")
            });

            var prompt = new SelectionPrompt<(string Name, string Id)>()
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

    internal static void HandleSelection(string selectionId)
    {
        switch (selectionId)
        {
            case "backup":
                FileManager.BackupMedia();
                AnsiConsole.MarkupLine("[bold green]Backup complete![/]");
                Thread.Sleep(2000);
                break;
            case "restore":
                FileManager.RestoreMedia();
                AnsiConsole.MarkupLine("[bold green]Restore complete![/]");
                Thread.Sleep(2000);
                break;
            case "delete":
                FileManager.DeleteBackup();
                break;
            case "format":
                MediaDevice.Format();
                AnsiConsole.MarkupLine("[bold green]SD Card formatted![/]");
                Thread.Sleep(2000);
                break;
        }
    }
}