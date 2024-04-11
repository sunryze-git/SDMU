using Spectre.Console;

namespace SDMU.Utilities;
internal class FileManager
{
    internal static void ExtractZip(string zipPath, string extractPath)
    {
        // Extract the zip file
        System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, extractPath, true);
    }

    internal static void CopyFiles(string source, string target)
    {
        AnsiConsole.Progress()
            .Start(ctx =>
            {
                // Define tasks
                var directoryCopy = ctx.AddTask("[green]Creating Directories...[/]");
                var fileCopy = ctx.AddTask("[green]Copying Files...[/]");

                while (!ctx.IsFinished)
                {
                    // Copy all directories (directory structure)
                    foreach (string dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
                    {
                        Directory.CreateDirectory(dirPath.Replace(source, target));
                        directoryCopy.Increment(1);
                    }

                    // Copy all files
                    foreach (string newPath in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories))
                    {
                        File.Copy(newPath, newPath.Replace(source, target), true);
                        fileCopy.Increment(1);
                    }
                }
            });
    }

    internal static void BackupMedia()
    {
        var targetDrive = SDManager._targetDrive?.Name;
        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var parentBackupFolder = $"{documents}\\SDMU_Backup\\";

        // Null Checking
        if (targetDrive is null)
        {
            AnsiConsole.MarkupLine("[red]No target drive found![/]");
            return;
        }

        // Name is current date and time
        var backupName = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        Directory.CreateDirectory($"{parentBackupFolder}\\{backupName}");

        var targetDirectory = $"{parentBackupFolder}\\{backupName}\\";

        CopyFiles(targetDrive, targetDirectory);
    }

    internal static void RestoreMedia()
    {
        var targetDrive = SDManager._targetDrive?.Name;
        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var parentBackupFolder = $"{documents}\\SDMU_Backup\\";
        var backupFolders = Directory.GetDirectories(parentBackupFolder);

        // Null Checking
        if (targetDrive is null)
        {
            AnsiConsole.MarkupLine("[red]No target drive found![/]");
            return;
        }

        // Ask user to select a backup to restore
        var selectedBackup = AnsiConsole.Prompt(new SelectionPrompt<string>()
                       .Title("[yellow]Select a backup to restore:[/]")
                       .PageSize(10)
                       .AddChoices(backupFolders));

        // Format SD Card
        SDManager.FormatSDCard();

        // Copy files from backup to SD Card
        CopyFiles(selectedBackup, targetDrive);
    }

    internal static string GetBackupsDirectory()
    {
        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        return $"{documents}\\SDMU_Backup\\";
    }

    internal static void DeleteBackup()
    {
        var backupsDir = GetBackupsDirectory();
        var backupFolders = Directory.GetDirectories(backupsDir);

        var prompt = new SelectionPrompt<string>()
            .Title("[yellow]Select a backup to delete:[/]")
            .PageSize(10)
            .AddChoices(backupFolders);

        var selectedBackup = AnsiConsole.Prompt(prompt);

        Directory.Delete(selectedBackup, true);

        AnsiConsole.MarkupLine("[bold green]Backup deleted![/]");
        Thread.Sleep(2000);
    }
}
