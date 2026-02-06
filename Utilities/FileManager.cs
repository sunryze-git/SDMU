using System.IO.Compression;
using SDMU.NewFramework;
using Spectre.Console;

namespace SDMU.Utilities;

internal class FileManager(MediaDevice mediaDevice)
{
    internal readonly string BackupFolder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SDMU_Backup");

    internal static void ExtractZip(string zipPath, string extractPath)
    {
        // Extract the zip file
        ZipFile.ExtractToDirectory(zipPath, extractPath, true);
        File.Delete(zipPath);
    }

    private static void CopyFiles(string source, string target)
    {
        AnsiConsole.Progress()
            .Start(ctx =>
            {
                // Define tasks
                var fileCopy = ctx.AddTask("[green]Copying Files...[/]");

                // Copy files
                var enumerationOptions = new EnumerationOptions
                {
                    RecurseSubdirectories = true,
                    IgnoreInaccessible = true
                };
                var entries = Directory.GetFileSystemEntries(source, "*", enumerationOptions);
                var createdDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                fileCopy.MaxValue = entries.Length;
                foreach (var item in entries)
                {
                    try
                    {
                        var relativePath = Path.GetRelativePath(source, item);
                        var destinationPath = Path.Combine(target, relativePath);
                        var fileAttributes = File.GetAttributes(item);

                        // This is a directory 
                        if (fileAttributes.HasFlag(FileAttributes.Directory))
                        {
                            Directory.CreateDirectory(destinationPath);
                            createdDirectories.Add(destinationPath);
                        }
                        else // not a directory
                        {
                            var parentFolder = Path.GetDirectoryName(destinationPath);
                            if (parentFolder is not null && createdDirectories.Add(parentFolder))
                                Directory.CreateDirectory(parentFolder);
                            File.Copy(item, destinationPath, true);
                        }
                    }
                    catch (Exception)
                    {
                        // idk
                    }

                    fileCopy.Increment(1);
                }
            });
    }

    internal void BackupMedia()
    {
        // Name is current date and time
        var backupTime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        var backupName = Path.Combine(mediaDevice.Device.Name, backupTime);
        var targetDir = Path.Combine(BackupFolder, backupName);

        CopyFiles(mediaDevice.Device.Name, targetDir);
    }

    internal void RestoreMedia()
    {
        var backupFolders = Directory.GetDirectories(BackupFolder);

        // Ask user to select a backup to restore
        var selectedBackup = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("[yellow]Select a backup to restore:[/]")
            .PageSize(10)
            .AddChoices(backupFolders));

        // Format SD Card
        mediaDevice.Format();

        // Copy files from backup to SD Card
        CopyFiles(selectedBackup, mediaDevice.Device.Name);
    }

    internal void DeleteBackup()
    {
        var backupFolders = Directory.GetDirectories(BackupFolder);
        var prompt = new SelectionPrompt<string>()
            .Title("[yellow]Select a backup to delete:[/]")
            .PageSize(10)
            .AddChoices(backupFolders);

        var selectedBackup = AnsiConsole.Prompt(prompt);
        try
        {
            Directory.Delete(selectedBackup, true);
            AnsiConsole.MarkupLine("[bold green]Backup deleted![/]");
        }
        catch (Exception exception)
        {
            AnsiConsole.MarkupLine("[red]Failed to delete backup![/]");
            AnsiConsole.WriteException(exception);
        }

        Thread.Sleep(2000);
    }
}