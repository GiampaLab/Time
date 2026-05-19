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
        var env = await CoreWebView2Environment.CreateAsync(
            userDataFolder: Path.Combine(Path.GetTempPath(), "TimeScreensaver"));
        await _webView.EnsureCoreWebView2Async(env);

        var wwwroot = Path.Combine(
            Path.GetDirectoryName(Application.ExecutablePath)!, "wwwroot");
        _webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
            "time.app", wwwroot, CoreWebView2HostResourceAccessKind.Allow);

        _webView.CoreWebView2.Navigate("https://time.app/index.html");
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
