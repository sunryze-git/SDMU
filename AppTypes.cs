namespace SDMU;
internal class AppTypes
{
    public enum baseApps
    {
        Aroma, // Modern Aroma Environment with Plugins
        Tiramisu // Legacy Tiramisu Environment
    }

    public enum aromaPlugins
    {
        Padcon, // Gamepad Stuff
        sdcafiine, // On Demand File Replacer
        SwipSwapMe, // Swap DRC
        Inkay, // Pretendo 
        HaltFix, // BSOD Thing on PPCHALT
        Screenshot // Screenshot Service
    }

    public enum extraApps
    {
        SaveMii, // Save File Manager
        Bloopair, // Bluetooth Controllers
        Dumpling, // Disc Dumper
        NUSspli, // WUP / NUS Downloader
        WUDD, // Disc Dumper
        WiiUIdent, // Hardware Identifier
        envSwap, // Swap between Aroma and Tiramisu environments
        WiiUReboot, // Reboot Console App
        WiiUScreenshotManager // Screenshot Manager
    }

}
