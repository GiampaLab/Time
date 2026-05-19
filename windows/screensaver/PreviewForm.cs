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

        var iconPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "favicon.png");
        var iconData = File.Exists(iconPath)
            ? "data:image/png;base64," + Convert.ToBase64String(File.ReadAllBytes(iconPath))
            : "";

        _webView.CoreWebView2.NavigateToString($"""
            <!DOCTYPE html>
            <html>
            <head>
            <style>
            * {{ margin: 0; padding: 0; box-sizing: border-box; }}
            body {{ background: black; width: 100vw; height: 100vh;
                   display: flex; align-items: center; justify-content: center; }}
            img {{ width: 70%; height: 70%; object-fit: contain; }}
            </style>
            </head>
            <body>
            <img src="{iconData}" />
            </body>
            </html>
            """);
    }

    [DllImport("user32.dll")] static extern nint SetParent(nint hWnd, nint hWndParent);
    [DllImport("user32.dll")] static extern bool GetClientRect(nint hWnd, ref RECT lpRect);

    struct RECT { public int Left, Top, Right, Bottom; }
}
