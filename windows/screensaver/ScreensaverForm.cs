using System.Net;
using System.Runtime.InteropServices;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

public class ScreensaverForm : Form
{
    private readonly WebView2 _webView = new() { Dock = DockStyle.Fill };
    private HttpListener? _http;

    // Static so hooks are installed once across all monitor instances
    private static nint _mouseHook;
    private static nint _keyboardHook;
    private static HookProc? _mouseProc;
    private static HookProc? _keyboardProc;
    private static Point _startPos;
    private static bool _hooksInstalled;

    public ScreensaverForm(Rectangle bounds)
    {
        FormBorderStyle = FormBorderStyle.None;
        Bounds = bounds;
        TopMost = true;
        BackColor = Color.Black;
        Cursor.Hide();
        Controls.Add(_webView);
        Load += async (_, _) => await InitAsync();

        if (!_hooksInstalled)
        {
            _startPos = Cursor.Position;
            _mouseProc = MouseHook;
            _keyboardProc = KeyboardHook;
            using var mod = System.Diagnostics.Process.GetCurrentProcess().MainModule!;
            var hMod = GetModuleHandle(mod.ModuleName);
            _mouseHook = SetWindowsHookEx(14 /* WH_MOUSE_LL */, _mouseProc, hMod, 0);
            _keyboardHook = SetWindowsHookEx(13 /* WH_KEYBOARD_LL */, _keyboardProc, hMod, 0);
            _hooksInstalled = true;
        }
    }

    private static nint MouseHook(int code, nint wParam, nint lParam)
    {
        if (code >= 0)
        {
            if (wParam is 0x0201 or 0x0204 or 0x0207 or 0x020A) // button down or wheel
            {
                Application.Exit();
            }
            else if (wParam == 0x0200) // WM_MOUSEMOVE
            {
                var p = Cursor.Position;
                if (Math.Abs(p.X - _startPos.X) > 10 || Math.Abs(p.Y - _startPos.Y) > 10)
                    Application.Exit();
            }
        }
        return CallNextHookEx(_mouseHook, code, wParam, lParam);
    }

    private static nint KeyboardHook(int code, nint wParam, nint lParam)
    {
        if (code >= 0 && wParam == 0x0100) // WM_KEYDOWN
            Application.Exit();
        return CallNextHookEx(_keyboardHook, code, wParam, lParam);
    }

    private async Task InitAsync()
    {
        var wwwroot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
        var port = StartFileServer(wwwroot);

        var env = await CoreWebView2Environment.CreateAsync(
            userDataFolder: Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TimeScreensaver"));
        await _webView.EnsureCoreWebView2Async(env);
        _webView.CoreWebView2.Navigate($"http://localhost:{port}/");
    }

    private int StartFileServer(string wwwroot)
    {
        var probe = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        int port = ((IPEndPoint)probe.LocalEndpoint).Port;
        probe.Stop();

        _http = new HttpListener();
        _http.Prefixes.Add($"http://localhost:{port}/");
        _http.Start();
        _ = ServeAsync(_http, wwwroot);
        return port;
    }

    private static async Task ServeAsync(HttpListener listener, string wwwroot)
    {
        while (listener.IsListening)
        {
            HttpListenerContext ctx;
            try { ctx = await listener.GetContextAsync(); }
            catch { return; }
            _ = Task.Run(() => HandleRequest(ctx, wwwroot));
        }
    }

    private static void HandleRequest(HttpListenerContext ctx, string wwwroot)
    {
        try
        {
            var urlPath = ctx.Request.Url?.LocalPath.TrimStart('/') ?? "";
            if (string.IsNullOrEmpty(urlPath)) urlPath = "index.html";

            var filePath = Path.GetFullPath(
                Path.Combine(wwwroot, urlPath.Replace('/', Path.DirectorySeparatorChar)));

            if (!filePath.StartsWith(wwwroot, StringComparison.OrdinalIgnoreCase) || !File.Exists(filePath))
            {
                ctx.Response.StatusCode = 404;
                ctx.Response.Close();
                return;
            }

            ctx.Response.StatusCode = 200;
            ctx.Response.ContentType = MimeType(filePath);
            using var fs = File.OpenRead(filePath);
            fs.CopyTo(ctx.Response.OutputStream);
        }
        catch { }
        finally { try { ctx.Response.Close(); } catch { } }
    }

    private static string MimeType(string path) => Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".html" => "text/html; charset=utf-8",
        ".js"   => "text/javascript",
        ".css"  => "text/css",
        ".wasm" => "application/wasm",
        ".json" => "application/json",
        ".png"  => "image/png",
        ".ico"  => "image/x-icon",
        ".svg"  => "image/svg+xml",
        ".dat"  => "application/octet-stream",
        _       => "application/octet-stream",
    };

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        if (_hooksInstalled)
        {
            UnhookWindowsHookEx(_mouseHook);
            UnhookWindowsHookEx(_keyboardHook);
            _hooksInstalled = false;
        }
        Cursor.Show();
        _http?.Stop();
        Application.Exit();
    }

    delegate nint HookProc(int code, nint wParam, nint lParam);

    [DllImport("user32.dll")] static extern nint SetWindowsHookEx(int id, HookProc proc, nint hMod, uint threadId);
    [DllImport("user32.dll")] static extern bool UnhookWindowsHookEx(nint hook);
    [DllImport("user32.dll")] static extern nint CallNextHookEx(nint hook, int code, nint wParam, nint lParam);
    [DllImport("kernel32.dll")] static extern nint GetModuleHandle(string? name);
}
