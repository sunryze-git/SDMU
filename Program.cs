using System.Security.Principal;
using SDMU.Menus;

namespace SDMU;

// Todo: would be a good idea to not use sudo / admin
public static class Program
{
    public static void Main(string[] args)
    {
        var menu = new MainMenu();
        var os = Environment.OSVersion.Platform;
        switch (os)
        {
            // Determine if we are running as sudo on macOS or Linux
            case PlatformID.Unix:
            {
                if (Environment.GetEnvironmentVariable("SUDO_USER") is null)
                {
                    Console.WriteLine("SDMU must be ran with sudo. Exiting...");
                    Thread.Sleep(2000);
                    Environment.Exit(1);
                }

                break;
            }
            // Determine if we are running as superuser
            case PlatformID.Win32NT:
            {
                if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
                {
                    Console.WriteLine("SDMU must be ran as an administrator. Exiting...");
                    Thread.Sleep(2000);
                    Environment.Exit(1);
                }

                break;
            }
            case PlatformID.Win32S:
            case PlatformID.Win32Windows:
            case PlatformID.WinCE:
            case PlatformID.Xbox:
            case PlatformID.MacOSX:
            case PlatformID.Other:
            default:
                Console.WriteLine("Unsupported platform. Exiting.");
                Thread.Sleep(2000);
                Environment.Exit(1);
                break;
        }

        menu.Show();
    }
}