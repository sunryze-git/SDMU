using Spectre.Console;

namespace SDMU;
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
}
