# ShareX Offline Custom

A personal Windows x64 fork of [ShareX](https://github.com/ShareX/ShareX), focused on local screenshots, annotation, clipboard copying, and file saving. This build adds enforced offline behavior, larger annotation controls, reduced background activity, and an interactive region capture workflow.

## What's new in this fork

### Enforced offline behavior

- Uploads and automatic update checks are forced off, regardless of user settings.
- HTTP requests through ShareX's shared HTTP client are rejected before a connection is created. FTP, SFTP, email sending, and ping operations are also blocked.
- Opening external links and loading images from URLs are disabled. Online image analysis providers are blocked as well.
- Local capture, image editing, clipboard operations, and saving files remain available.

The offline policy is part of this build and cannot be enabled or disabled through configuration. Restoring build dependencies still requires internet access.

### More flexible annotation controls

- Enter custom brush widths and text sizes up to **4096**, with additional preset sizes for quick selection.
- Use the **100%** button to make the stroke and shape fill colors fully opaque.

### Less background activity

- Start in the system tray by default. The main window is created when its window or tray menu is first opened.
- Load image editor styles when the editor is opened.
- Handle global hotkeys through Windows messages instead of continuously polling the keyboard.
- Disable diagnostic log writes to disk. Settings and screenshots are still saved when needed; existing files are preserved.

### Interactive region capture

- **PrintScreen** and **Ctrl+PrintScreen** open region capture by default.
- After selecting a region, keep the capture overlay open to move the selection, drag its resize handles, or enter an exact width and height.
- Annotate directly on the capture overlay using the shared editor toolbar, positioned beside the selection, with undo and redo controls.
- Copy, save, finish, or cancel using the buttons below the tools. Interactive capture finishes without opening a separate editor window, even when no annotations were added.
- Exclude the mouse cursor by default. When cursor capture is enabled, draw it directly onto the frozen screenshot before the overlay opens, avoiding artifacts from a separate cursor annotation.

The interactive workflow applies when **Quick capture** is disabled (the default in this fork). Existing settings can override defaults.

| Shortcut | Action in the capture overlay |
| --- | --- |
| Ctrl+C | Finish capture and copy the result to the clipboard |
| Ctrl+S | Finish capture and save the result to the configured screenshot folder |
| Ctrl+Enter | Finish capture using the configured after-capture workflow |
| Ctrl+Z | Undo an annotation change |
| Ctrl+Y or Ctrl+Shift+Z | Redo an annotation change |

Copy and save actions are available when capture annotations are enabled. While a text field is focused, Ctrl+C and undo/redo shortcuts retain their text-editing behavior.

## Build and run

Requirements: Windows and the **.NET 10 SDK**. Run these commands from the repository root:

```powershell
dotnet restore ShareX/ShareX.csproj -p:Platform=x64
dotnet build ShareX/ShareX.csproj -c Release -p:Platform=x64 --no-restore
dotnet publish ShareX/ShareX.csproj -c Release -p:Platform=x64 -r win-x64 --self-contained true -o artifacts/ShareX-Offline
New-Item -ItemType File -Force artifacts/ShareX-Offline/Portable
```

Launch `artifacts/ShareX-Offline/ShareX.exe`. Keep the entire published folder together. The `Portable` marker stores application settings beside the executable; the self-contained build includes the .NET runtime.

To save screenshots directly in Downloads, select your Windows **Downloads** folder as the screenshot folder and clear the subfolder patterns for both regular and window screenshots. Personal configuration files are kept locally and are not included in this repository.

## Verification

After building Release for x64, run the offline smoke check against the built DLLs:

```powershell
dotnet run --project tests/OfflineSmoke/OfflineSmoke.csproj -c Release
```

It checks that HTTP is blocked before connecting, ping is blocked, uploads and update checks are forced off, and disk logging stays disabled.

To check your local capture configuration and capture helpers:

```powershell
dotnet run --project tests/CaptureSmoke/CaptureSmoke.csproj -c Release -- "<path to ApplicationConfig.json>" "<Downloads folder>" "<path to HotkeysConfig.json>"
```

This checks screenshot paths, the PrintScreen binding, cursor defaults, toolbar placement, and native cursor transparency. It reads configuration files without modifying them or capturing the screen.

## Upstream and documentation

Based on ShareX revision `cb360c1`. This is a personal custom build, not an official ShareX release.

- [Original ShareX README](README.upstream.md)
- [Custom build notes in Russian](CUSTOM.md)
- [License](LICENSE.txt)

Inherited GitHub workflows are archived in `.github/upstream-workflows` and do not run automatically. Personal settings, screenshots, and compiled builds are excluded from the repository.
