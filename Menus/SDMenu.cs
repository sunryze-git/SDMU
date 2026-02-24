using SDMU.NewFramework;
using SDMU.Utilities;
using Spectre.Console;

namespace SDMU.Menus;
internal class SdMenu(FileManager fileManager, MediaDevice device)
{
    // Get table of backups within the backup directory
    private Table GetBackups()
    {
        var backups = Directory.GetDirectories(FileManager.BackupFolder);

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
    internal async Task Show()
    {
        var running = true;
        while (running)
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

            AnsiConsole.MarkupLine($"[yellow]SD Card: {device.Device?.Name}[/]\n");
            AnsiConsole.MarkupLine($"[grey]Backups stored in: {FileManager.BackupFolder}[/]");
            // 

            var promptItems = new List<(string Name, Func<Task> Id)>()
            {
                ("Backup SD Card", fileManager.BackupMedia),
                ("Restore SD Card", fileManager.RestoreMedia),
                ("Delete Backup", fileManager.DeleteBackup),
                ("Format SD Card", device.Format),
                ("Return to Main Menu", () => { running = false; return Task.CompletedTask; })
            };

            var prompt = new SelectionPrompt<(string Name, Func<Task> Id)>()
                .PageSize(10)
                .UseConverter(item => item.Name)
                .AddChoices(promptItems);

            var selectedItem = AnsiConsole.Prompt(prompt);
            try
            {
                await selectedItem.Id.Invoke();
                if (running)
                {
                    AnsiConsole.MarkupLine("\n[green]Operation completed successfully! Press any key to continue...[/]");
                    Console.ReadKey(true);
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
                Console.ReadKey(true);
            }
        }
    }
}