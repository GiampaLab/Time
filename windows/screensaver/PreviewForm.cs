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
        _webView.CoreWebView2.NavigateToString(PreviewHtml);
    }

    // The preview thumbnail is ~120x80px so the full Blazor app isn't needed.
    // Virtual host file mapping fails in the cross-process Settings preview context.
    private const string PreviewHtml = """
        <!DOCTYPE html>
        <html>
        <head>
        <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body { background: black; width: 100vw; height: 100vh;
               display: flex; align-items: center; justify-content: center; }
        svg { width: 70%; height: 70%; }
        </style>
        </head>
        <body>
        <svg viewBox="0 0 100 100" xmlns="http://www.w3.org/2000/svg">
          <circle cx="50" cy="50" r="47" stroke="white" stroke-width="2" fill="none"/>
          <line x1="50" y1="50" x2="50" y2="14" stroke="white" stroke-width="5"
                stroke-linecap="round" transform="rotate(180 50 50)"/>
          <line x1="50" y1="50" x2="50" y2="22" stroke="white" stroke-width="5"
                stroke-linecap="round" transform="rotate(225 50 50)"/>
          <circle cx="50" cy="50" r="4" fill="white"/>
        </svg>
        </body>
        </html>
        """;

    [DllImport("user32.dll")] static extern nint SetParent(nint hWnd, nint hWndParent);
    [DllImport("user32.dll")] static extern bool GetClientRect(nint hWnd, ref RECT lpRect);

    struct RECT { public int Left, Top, Right, Bottom; }
}
