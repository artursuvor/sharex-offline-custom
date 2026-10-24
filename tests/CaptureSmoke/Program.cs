using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Newtonsoft.Json;
using ShareX;
using ShareX.HelpersLib;
using SkiaSharp;
using ShareX.ScreenCaptureLib;
using ShareX.ScreenCaptureLib.Presentation.RegionCapture;
using Avalonia;

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
if (config.DefaultTaskSettings.CaptureSettings.RegionCaptureOptions.QuickCapture) throw new Exception("Configured selection still finishes immediately.");
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

if (new RegionCaptureOptions().QuickCapture) throw new Exception("Region selection must remain open for editing.");
foreach (var test in new[]
{
    (new Rect(300, 200, 400, 300), new Rect(0, 0, 1920, 1080), new Size(1000, 120)),
    (new Rect(1500, 800, 400, 260), new Rect(0, 0, 1920, 1080), new Size(1000, 200)),
    (new Rect(2000, 0, 200, 100), new Rect(1920, 0, 1280, 720), new Size(1200, 200))
})
{
    var point = RegionCaptureLayout.PlaceToolbar(test.Item1, test.Item2, test.Item3);
    var toolbarBounds = new Rect(point, test.Item3);
    if (!test.Item2.Contains(toolbarBounds)) throw new Exception("Capture toolbar escaped the monitor.");
    if (toolbarBounds.Intersects(test.Item1)) throw new Exception("Capture toolbar covers the selected image when there is space beside it.");
}
Console.WriteLine("PASS: inline editing stays open and toolbar placement fits normal and offset monitors.");

// Use an owned fixture: shared Windows cursors can be replaced by themes or
// remote desktop tools while a UI test is running.
byte[] mask = Enumerable.Repeat((byte)255, 32 * 4).ToArray();
byte[] colors = new byte[32 * 32 * 4];
for (int y = 8; y < 24; y++)
for (int x = 8; x < 24; x++)
{
    mask[y * 4 + x / 8] &= (byte)~(128 >> (x % 8));
    colors[(y * 32 + x) * 4 + 2] = 255;
    colors[(y * 32 + x) * 4 + 3] = 255;
}
IntPtr handle = CreateIcon(IntPtr.Zero, 32, 32, 1, 32, mask, colors);
if (handle == IntPtr.Zero) throw new Exception("Cannot create native transparency fixture.");
try
{
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
    if (transparent != 768 || visible != 256) throw new Exception($"Cursor fixture lost its transparency (transparent={transparent}, visible={visible}).");
    if (background.GetPixel(16, 16) != SKColors.Red) throw new Exception("Cursor fixture lost its visible pixels.");
}
finally { DestroyIcon(handle); }
Console.WriteLine("PASS: owned native cursor fixture preserves visible pixels and transparent background.");

[DllImport("user32.dll")]
static extern IntPtr CreateIcon(IntPtr instance, int width, int height, byte planes, byte bitsPerPixel, byte[] andMask, byte[] xorMask);

[DllImport("user32.dll")]
static extern bool DestroyIcon(IntPtr handle);
