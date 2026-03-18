# X6ProUnLocker

<p align="center">
  <img src="src/X6ProUnLocker/Assets/logo.png" alt="X6ProUnLocker Logo" width="200"/>
</p>

**X6ProUnLocker** is a powerful open‑source tool for Windows system recovery. It allows you to replace system utilities, manage processes, scan for malware, restore fonts, configure autorun, and much more. Originally written in **C++ (Qt)**, the project has been fully rewritten in **C# (WPF)** for better integration with the Windows API and easier distribution.

## Features

- **Advanced Task Manager**  
  – View detailed information about running processes: PID, name, executable path, memory usage (MB), CPU usage (%), priority class, status (system/running), and digital signature company.  
  – End processes gracefully or force‑kill them.  
  – Open the file location or view file properties directly from the interface.

- **Built‑in CMD Terminal**  
  – Full command‑line experience with a native Windows `cmd.exe` engine.  
  – Command history (navigate with Up/Down keys) and auto‑completion.  
  – Change working directory (`cd`) and see the current path in the prompt.  
  – Supports all standard commands (`dir`, `ipconfig`, `tasklist`, `netstat`, etc.).  
  – Colored output and scrollable log.

- **System Utility Replacement (Sticky Keys / Ease of Access)**  
  – Replace critical system executables (`cmd.exe`, `sethc.exe`, `utilman.exe`) with any custom `.exe` file.  
  – Gain administrative access at the Windows login screen (e.g., by pressing Shift five times or Win+U).  
  – Automatic backup of original files (`.bak`) and restoration of original permissions.  
  – Take ownership of system files and restore default DACLs after replacement.

- **System Font Restoration**  
  – Restore default Windows fonts if they are missing or corrupted.  
  – Useful for fixing display issues in applications and system dialogs.

- **6 Autorun Methods**  
  – Add the program to startup using multiple persistence techniques:  
    1. **Registry** – `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run`  
    2. **Registry (machine‑wide)** – `HKEY_LOCAL_MACHINE\...\Run`  
    3. **Startup folder** – creates a shortcut in the user's Startup folder.  
    4. **Task Scheduler** – creates a logon trigger task.  
    5. **Windows Service** – registers the program as a service (auto‑start).  
    6. **win.ini** – adds the program to the `load=` line in the classic `win.ini` file.  
  – One‑click removal of all autorun entries.

- **Malware Scanner**  
  – Scan individual files for known malware using SHA‑256 hash matching against a built‑in database.  
  – Verify digital signatures with `WinVerifyTrust` API to detect unsigned or tampered files.  
  – Asynchronous scanning with event‑driven result reporting.  
  – (Planned) Full system scan to enumerate all files and report suspicious ones.

- **Environment Detection**  
  – Automatically detect the current execution environment:      – Normal Windows mode  
    – Safe Mode (via `GetSystemMetrics(SM_CLEANBOOT)`)  
    – Windows Recovery Environment (WinRE)  
    – Windows Preinstallation Environment (WinPE)  
  – Check for administrator privileges and warn if missing.  
  – Visual indicator in the main window with color‑coded status.

- **Early Boot Service**  
  – Register a Windows service that starts early during boot (`X6ProUnLockerEarlyBoot`).  
  – Check critical system files (`cmd.exe`, `sethc.exe`, `utilman.exe`, `explorer.exe`, `winlogon.exe`) for existence and valid digital signatures.  
  – Log results for forensic analysis.

- **Comprehensive Logging**  
  – All actions, errors, and results are logged in a dedicated tab with color highlighting (green for success, red for errors, yellow for warnings, blue for informational).  
  – Timestamps for each entry.  
  – Logs can be reviewed and copied for troubleshooting.

- **Modern WPF Interface**  
  – Dark theme with accent colors (gold and blue).  
  – Tab‑based navigation for easy access to all features.  
  – Responsive layout with progress bar and status updates.  
  – Fully resizable window with minimum size constraints.

- **Process Properties and File Management**  
  – Right‑click or use buttons to open file properties dialog (Shell‑style) directly from the process list.  
  – Open the containing folder in Windows Explorer.  
  – Display company name and digital signer information where available.

- **Security and Permissions Handling**  
  – Properly request and enable `SeTakeOwnershipPrivilege` when needed.  
  – Restore default security descriptors after file operations.  
  – All privileged operations check for administrator rights and notify the user.

- **Critical System File Check**  
  – Verify the integrity and signature of essential Windows files.  
  – Detect missing or replaced system files and alert the user.

- **User-Friendly Status Bar**  
  – Real‑time status messages and a progress bar for long operations.

## Project History

The project started as a C++ Qt application to maintain compatibility with older Windows versions. However, to simplify development and better leverage modern Windows features (WinAPI, .NET), it was rewritten in **C#** using **WPF**. This rewrite brought:

- More efficient system calls via P/Invoke.
- Smaller self‑contained executable size.
- Easier integration with Task Scheduler, registry, and services.
- Cleaner, more maintainable code.

## Requirements
- Operating System: Windows 7 or later (x64)
- [.NET 8.0 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) (if not using self‑contained build) or the self‑contained executable which includes everything.
- Administrator rights for most features.

## Building from Source

### Using .NET CLI

1. Install the [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).
2. Clone the repository:
```bash
   git clone https://github.com/TIBI624/X6ProUnLocker.git
   cd X6ProUnLocker
```
3. Restore dependencies and build:
```bash
   dotnet restore src/X6ProUnLocker.sln
   dotnet build src/X6ProUnLocker.sln -c Release
```
4. To create a self‑contained `.exe`:
```bash
   dotnet publish src/X6ProUnLocker/X6ProUnLocker.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
```
   The final `X6ProUnLocker.exe` will be in the `publish` folder.

### Using Visual Studio 2022

1. Open the solution file `src/X6ProUnLocker.sln`.
2. Select **Release** configuration and **x64** platform.
3. Build the solution (Build → Build Solution).
4. To publish, right‑click the project → **Publish** and follow the wizard.

## Usage

1. Run the program **as Administrator** (recommended).
2. The **Task Manager** tab lets you view and kill processes.
3. The **CMD Terminal** tab provides a built‑in command prompt.
4. On the **System Tools** tab you can:
   - **Restore system fonts**
   - **Replace system utilities** (select an `.exe` and replace `cmd.exe`, `sethc.exe`, or `utilman.exe`)
   - Choose from **6 autorun methods** and apply them.
5. The **Logs** tab shows all actions with color coding.

## License

This project is licensed under the **MIT License**. See the [LICENSE](LICENSE) file for details.

## Author
**TIBI624**  
Developed and maintained by the TIBI624 (2026).

## Contributing

If you'd like to contribute, feel free to [fork the repository](https://github.com/TIBI624/X6ProUnLocker/fork) on GitHub and submit a pull request. Any help is welcome!