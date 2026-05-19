static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();

        var mode = args.Length > 0 ? args[0].ToLower() : "/s";

        if (mode.StartsWith("/s"))
        {
            foreach (var screen in Screen.AllScreens)
                new ScreensaverForm(screen.Bounds).Show();
            Application.Run();
            Environment.Exit(0);
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
    }
}
