using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

public class ScreensaverForm : Form
{
    private readonly WebView2 _webView = new() { Dock = DockStyle.Fill };
    private Point _lastMouse = Point.Empty;

    public ScreensaverForm(Rectangle bounds)
    {
        FormBorderStyle = FormBorderStyle.None;
        Bounds = bounds;
        TopMost = true;
        BackColor = Color.Black;
        Cursor.Hide();
        Controls.Add(_webView);
        Load += async (_, _) => await InitAsync();
    }

    private async Task InitAsync()
    {
        try
        {
            var env = await CoreWebView2Environment.CreateAsync(
                userDataFolder: Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "TimeScreensaver"));
            await _webView.EnsureCoreWebView2Async(env);

            var wwwroot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
            _webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "time.app", wwwroot, CoreWebView2HostResourceAccessKind.Allow);

            _webView.CoreWebView2.NavigationCompleted += OnNavCompleted;
            _webView.CoreWebView2.Navigate("https://time.app/index.html");
        }
        catch (Exception ex)
        {
            ShowDiag($"Init failed: {ex.Message}");
        }
    }

    private void OnNavCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        if (e.IsSuccess) return;
        _webView.CoreWebView2.NavigationCompleted -= OnNavCompleted;

        var wwwroot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
        var indexExists = File.Exists(Path.Combine(wwwroot, "index.html"));
        ShowDiag(
            $"Navigation failed ({e.WebErrorStatus})\n" +
            $"wwwroot path: {wwwroot}\n" +
            $"wwwroot exists: {Directory.Exists(wwwroot)}\n" +
            $"index.html exists: {indexExists}\n" +
            $"BaseDirectory: {AppContext.BaseDirectory}");
    }

    private void ShowDiag(string msg)
    {
        if (_webView.CoreWebView2 == null) return;
        var html = $"<body style='background:black;color:white;font:16px monospace;padding:30px'>" +
                   $"<pre>{System.Net.WebUtility.HtmlEncode(msg)}</pre></body>";
        _webView.CoreWebView2.NavigateToString(html);
    }

    protected override void OnKeyDown(KeyEventArgs e) => Close();
    protected override void OnMouseDown(MouseEventArgs e) => Close();
    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (_lastMouse != Point.Empty && _lastMouse != e.Location) Close();
        _lastMouse = e.Location;
    }
    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        Application.Exit();
    }
}
