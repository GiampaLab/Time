using System.Net;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

public class ScreensaverForm : Form
{
    private readonly WebView2 _webView = new() { Dock = DockStyle.Fill };
    private Point _lastMouse = Point.Empty;
    private HttpListener? _http;

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
        _http?.Stop();
        Application.Exit();
    }
}
