using System.Security.Principal;

var os = Environment.OSVersion.Platform;

// Determine if we are running as sudo on macOS or Linux
if (os == PlatformID.Unix)
{
    if (Environment.GetEnvironmentVariable("SUDO_USER") is null)
    {
        Console.WriteLine("SDMU must be ran with sudo. Exiting...");
        Thread.Sleep(2000);
        Environment.Exit(1);
    }
}

// Determine if we are running as superuser
if (os == PlatformID.Win32NT)
{
    if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
    {
        Console.WriteLine("SDMU must be ran as an administrator. Exiting...");
        Thread.Sleep(2000);
        Environment.Exit(1);
    }
}


MainMenu.Show();