# Game Data Exporter for Playnite

A Playnite plugin that exports your game library data to a JSON file and provides a custom URI scheme to launch specific game actions.

## Features

- **Automatic Library Export:** Automatically exports your game library to a `library.json` file whenever a library update occurs (e.g., on application start, library updates, game installation/uninstallation).
- **Manual Export:** A main menu item allows you to manually trigger the library export at any time.
- **Custom URI Handler:** Launch any game action (including non-primary actions) using a custom URI. This is useful for creating desktop shortcuts or for integration with external applications.
    - URI format: `playnite://gamedataexporter/start_gameaction/{gameId}/{gameAction.Name}`

## Installation

As this plugin is under development, there is no official release package yet. You need to build it from source.

1.  **Clone the repository.**
2.  **Open the Solution:** Open the `GameDataExporter.sln` file in Visual Studio.
3.  **Build the Project:** Build the solution in `Release` mode.
4.  **Copy the Plugin Files:** Navigate to the `bin\Release` directory and copy all the generated files.
5.  **Paste into Playnite:** Paste the copied files into the `Extensions\GameDataExporter` directory inside your Playnite installation folder. If the `GameDataExporter` folder doesn't exist, create it.
6.  **Restart Playnite:** Restart Playnite to load the new plugin.

## Usage

### JSON Export

The `library.json` file is automatically created and updated in the plugin's data directory. You can access this directory from within Playnite by going to `About Playnite` > `Go to...` > `User data folder`, and then navigating to `ExtensionsData\GameDataExporter_66b8eca4-3f39-4b79-a359-3cb98d5b18fd`.

The JSON file contains a list of all your games with detailed information, including game actions.

### Custom URI

To use the custom URI, you can create a shortcut or use a command line tool.

**Format:**
`playnite://gamedataexporter/start_gameaction/{gameId}/{gameAction.Name}`

**Example:**
If you have a game with ID `b8f69b32-84a5-4813-911e-0a5255c2f5d1` and a game action named "Launch Configuration Tool", you can use the following URI to launch it:
`playnite://gamedataexporter/start_gameaction/b8f69b32-84a5-4813-911e-0a5255c2f5d1/Launch Configuration Tool`

You can find the Game ID in Playnite by right-clicking a game > `Edit...` > `Information` tab.

## Building from Source

### Prerequisites

-   Visual Studio (with .NET desktop development workload)
-   .NET Framework 4.6.2 Developer Pack

### Steps

1.  Clone this repository.
2.  Open `GameDataExporter.sln` in Visual Studio.
3.  Build the solution. The plugin will be built in the `bin` directory.
