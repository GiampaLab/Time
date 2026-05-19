Application.SetHighDpiMode(HighDpiMode.SystemAware);
Application.EnableVisualStyles();

var args = Environment.GetCommandLineArgs();
var mode = args.Length > 1 ? args[1].ToLower() : "/s";

if (mode.StartsWith("/s"))
{
    foreach (var screen in Screen.AllScreens)
        new ScreensaverForm(screen.Bounds).Show();
    Application.Run();
}
else if (mode.StartsWith("/p") && args.Length > 2)
{
    var hwnd = nint.Parse(args[2]);
    Application.Run(new PreviewForm(hwnd));
}
else if (mode.StartsWith("/c"))
{
    MessageBox.Show("No settings available.", "Time Screensaver",
        MessageBoxButtons.OK, MessageBoxIcon.Information);
}
