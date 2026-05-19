using System.Net;
using System.Runtime.InteropServices;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

public class ScreensaverForm : Form
{
    private readonly WebView2 _webView = new() { Dock = DockStyle.Fill };
    private HttpListener? _http;

    private static Point _startPos;
    private static System.Windows.Forms.Timer? _inputTimer;
    private static int _warmup = 3; // skip first few ticks while screensaver starts

    public ScreensaverForm(Rectangle bounds)
    {
        FormBorderStyle = FormBorderStyle.None;
        Bounds = bounds;
        TopMost = true;
        BackColor = Color.Black;
        Cursor.Hide();
        Controls.Add(_webView);
        Load += async (_, _) => await InitAsync();

        if (_inputTimer == null)
        {
            _startPos = Cursor.Position;
            _inputTimer = new System.Windows.Forms.Timer { Interval = 50 };
            _inputTimer.Tick += InputTick;
            _inputTimer.Start();
        }
    }

    private static void InputTick(object? sender, EventArgs e)
    {
        if (_warmup > 0) { _warmup--; return; }

        var pos = Cursor.Position;
        if (Math.Abs(pos.X - _startPos.X) > 5 || Math.Abs(pos.Y - _startPos.Y) > 5)
            Die();

        if (GetAsyncKeyState(0x01) < 0 || GetAsyncKeyState(0x02) < 0 || GetAsyncKeyState(0x04) < 0)
            Die();

        for (int vk = 8; vk < 256; vk++)
            if (GetAsyncKeyState(vk) < 0) Die();
    }

    private static void Die() => TerminateProcess(GetCurrentProcess(), 0);

    [DllImport("kernel32.dll")] private static extern bool TerminateProcess(nint h, uint code);
    [DllImport("kernel32.dll")] private static extern nint GetCurrentProcess();

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
        _inputTimer?.Stop();
        Cursor.Show();
        _http?.Stop();
        Application.Exit();
    }

    [DllImport("user32.dll")] private static extern short GetAsyncKeyState(int vKey);
}
