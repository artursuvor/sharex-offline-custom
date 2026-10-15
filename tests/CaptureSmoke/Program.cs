using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Newtonsoft.Json;
using ShareX;
using ShareX.HelpersLib;
using SkiaSharp;

if (args.Length != 3) throw new ArgumentException("Usage: CaptureSmoke <ApplicationConfig.json> <expected screenshots folder> <HotkeysConfig.json>");
string runtimeFolder = Path.GetDirectoryName(typeof(TaskHelpers).Assembly.Location)!;
AssemblyLoadContext.Default.Resolving += (_, name) =>
{
    string path = Path.Combine(runtimeFolder, name.Name + ".dll");
    return File.Exists(path) ? AssemblyLoadContext.Default.LoadFromAssemblyPath(path) : null;
};
var jsonSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };
var config = JsonConvert.DeserializeObject<ApplicationConfig>(File.ReadAllText(args[0]), jsonSettings)!;
if (config.DefaultTaskSettings.CaptureSettings.ShowCursor) throw new Exception("Cursor is still enabled.");
typeof(TaskHelpers).Assembly.GetType("ShareX.ApplicationState")!
    .GetProperty("Settings", BindingFlags.Static | BindingFlags.NonPublic)!.SetValue(null, config);
string actualFolder = TaskHelpers.GetScreenshotsFolder();
if (!Path.GetFullPath(actualFolder).Equals(Path.GetFullPath(args[1]), StringComparison.OrdinalIgnoreCase))
    throw new Exception($"Unexpected screenshots folder: {actualFolder}");
Console.WriteLine($"PASS: screenshots folder = {actualFolder}, no cursor overlay.");

var hotkeys = JsonConvert.DeserializeObject<HotkeysConfig>(File.ReadAllText(args[2]), jsonSettings)!;
var printScreen = hotkeys.Hotkeys.Single(key => key.HotkeyInfo.Hotkey == InputKey.PrintScreen && !key.HotkeyInfo.Win);
if (printScreen.TaskSettings.Job != HotkeyType.RectangleRegion)
    throw new Exception("PrintScreen must open region selection.");
if (HotkeyManager.GetDefaultHotkeyList().Single(key => key.HotkeyInfo.Hotkey == InputKey.PrintScreen).TaskSettings.Job != HotkeyType.RectangleRegion)
    throw new Exception("Default PrintScreen mapping must also select a region.");
if (TaskHelpers.GetScreenshot(config.DefaultTaskSettings).CaptureCursor)
    throw new Exception("Screenshot capture still includes the cursor.");
foreach (var hotkey in hotkeys.Hotkeys)
    if (!hotkey.TaskSettings.UseDefaultCaptureSettings && hotkey.TaskSettings.CaptureSettings.ShowCursor)
        throw new Exception("A custom hotkey still includes the cursor.");
Console.WriteLine("PASS: PrintScreen selects a region, screenshot capture excludes cursor.");

foreach (int id in new[] { 32512, 32513, 32515, 32649 })
{
    IntPtr handle = LoadCursor(IntPtr.Zero, id);
    using SKBitmap cursor = WindowsImageInterop.FromIcon(handle);
    using SKBitmap background = SkiaImageHelpers.CreateBitmap(cursor.Width, cursor.Height);
    background.Erase(SKColors.White);
    using (var canvas = new SKCanvas(background)) canvas.DrawBitmap(cursor, 0, 0);
    int transparent = 0, visible = 0;
    for (int y = 0; y < cursor.Height; y++)
    for (int x = 0; x < cursor.Width; x++)
    {
        if (cursor.GetPixel(x, y).Alpha == 0)
        {
            transparent++;
            if (background.GetPixel(x, y) != SKColors.White) throw new Exception("Cursor transparency produced a rectangular artifact.");
        }
        else visible++;
    }
    if (transparent == 0 || visible == 0) throw new Exception("Cursor is a solid rectangle or invisible.");
}
Console.WriteLine("PASS: arrow, text, crosshair and hand cursor transparency preserved on capture background.");

[DllImport("user32.dll")]
static extern IntPtr LoadCursor(IntPtr instance, int id);
