using System.Runtime.InteropServices;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

public class PreviewForm : Form
{
    private readonly WebView2 _webView = new() { Dock = DockStyle.Fill };

    public PreviewForm(nint previewHwnd)
    {
        FormBorderStyle = FormBorderStyle.None;
        var parentRect = new RECT();
        GetClientRect(previewHwnd, ref parentRect);
        SetParent(Handle, previewHwnd);
        Size = new Size(parentRect.Right, parentRect.Bottom);
        Location = Point.Empty;
        BackColor = Color.Black;
        Controls.Add(_webView);
        Load += async (_, _) => await InitAsync();
    }

    private async Task InitAsync()
    {
        var env = await CoreWebView2Environment.CreateAsync(
            userDataFolder: Path.Combine(Path.GetTempPath(), "TimeScreensaverPreview"));
        await _webView.EnsureCoreWebView2Async(env);

        var wwwroot = Path.Combine(
            Path.GetDirectoryName(Application.ExecutablePath)!, "wwwroot");
        _webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
            "time.app", wwwroot, CoreWebView2HostResourceAccessKind.Allow);

        _webView.CoreWebView2.Navigate("https://time.app/index.html");
    }

    [DllImport("user32.dll")] static extern nint SetParent(nint hWnd, nint hWndParent);
    [DllImport("user32.dll")] static extern bool GetClientRect(nint hWnd, ref RECT lpRect);

    struct RECT { public int Left, Top, Right, Bottom; }
}
