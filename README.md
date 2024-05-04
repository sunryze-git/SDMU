# SDMU (SD Manager (Wii) U)

![screenshot](image.png)

SDMU is a C# application designed to simplify the management and setup of Wii U Homebrew. With SDMU, users can effortlessly handle tasks such as backing up, cleaning, and restoring their SD card, as well as downloading and updating SD card files without requiring their console to be online. Additionally, SDMU provides the functionality to install packages from the Homebrew App Store directly from your PC and manage NAND backups.

## Features

- **SD Card Management:** Easily create backups, clean up, and restore your SD card contents.
- **Offline File Updates:** Download and update SD card files without needing to use Wii U. (*Can only update Applications at the moment*)
- **Homebrew App Store Integration:** Install packages from the Homebrew App Store directly to your SD Card from your PC.
- ~~**NAND Backup Management:** Manage NAND backups effortlessly to reduce SD Card space usage.~~ *Not Implimented Yet*

## Installation

### Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download)

### Building from Source

1. Clone the repository:
   ```bash
   git clone https://github.com/sunryze-git/SDMU.git
   ```

2. Navigate to the project directory:
   ```bash
   cd SDMU
   ```

3. Build the project:
   ```bash
   dotnet build
   ```

## Usage

1. Run the compiled executable or launch the application from your preferred development environment.
2. Follow the on-screen instructions.

## Contributing

Contributions are welcome! If you'd like to contribute to SDMU, feel free to fork the repository and submit pull requests.

## License

This project is licensed under the [MIT License](LICENSE.txt).

## Acknowledgements

- Aroma, Tiramisu, HB App Store
- Any other package that is downloaded through this software


### FYI
- Portions of this readme were generated by ChatGPT.


## Roadmap

[] Minimum SD Card Size Check
[] 100% Multi-Platform app compatibility
[] Re-Install Homebrew Files
