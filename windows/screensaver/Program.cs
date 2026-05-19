Application.SetHighDpiMode(HighDpiMode.SystemAware);
Application.EnableVisualStyles();

// In top-level programs, implicit 'args' contains command-line args without the exe name.
// Windows passes: /s (run), /p <hwnd> (preview), /c (settings)
var mode = args.Length > 0 ? args[0].ToLower() : "/s";

if (mode.StartsWith("/s"))
{
    foreach (var screen in Screen.AllScreens)
        new ScreensaverForm(screen.Bounds).Show();
    Application.Run();
}
else if (mode.StartsWith("/p") && args.Length > 1)
{
    var hwnd = nint.Parse(args[1]);
    Application.Run(new PreviewForm(hwnd));
}
else if (mode.StartsWith("/c"))
{
    MessageBox.Show("No settings available.", "Time Screensaver",
        MessageBoxButtons.OK, MessageBoxIcon.Information);
}
