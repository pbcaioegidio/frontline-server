using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;

namespace FrontLineBootstrap;

internal static class Program
{
    // Códigos alinhados ao Inno Setup (Partner Center).
    public const int ExitOk = 0;
    public const int ExitInitFail = 1;
    public const int ExitCancel = 5;
    public const int ExitCorrupt = 6;
    public const int ExitInstallFail = 4;

    public const string DefaultZipUrl =
        "https://downloads.frontlinebattle.com.br/FrontLine-Setup-latest.zip";

    [STAThread]
    private static int Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

        var opts = BootstrapOptions.Parse(args);
        try
        {
            using var form = new MainForm(opts);
            Application.Run(form);
            return form.ExitCode;
        }
        catch (Exception ex)
        {
            if (!opts.Silent)
                MessageBox.Show(ex.Message, "FrontLine Setup", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return ExitInitFail;
        }
    }
}

internal sealed class BootstrapOptions
{
    public string ZipUrl { get; init; } = Program.DefaultZipUrl;
    public bool Silent { get; init; }
    public string SetupArgs { get; init; } = "";

    public static BootstrapOptions Parse(string[] args)
    {
        var silent = false;
        var zipUrl = Program.DefaultZipUrl;
        var passthrough = new List<string>();

        for (var i = 0; i < args.Length; i++)
        {
            var a = args[i];
            if (a.Equals("/VERYSILENT", StringComparison.OrdinalIgnoreCase)
                || a.Equals("/SILENT", StringComparison.OrdinalIgnoreCase)
                || a.Equals("/S", StringComparison.OrdinalIgnoreCase))
            {
                silent = true;
                passthrough.Add(a);
                continue;
            }

            if (a.Equals("/ZIPURL", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                zipUrl = args[++i];
                continue;
            }

            passthrough.Add(a);
        }

        // Garantir silent completo para a Store se já pediu silent.
        if (silent)
        {
            Ensure(passthrough, "/VERYSILENT");
            Ensure(passthrough, "/SUPPRESSMSGBOXES");
            Ensure(passthrough, "/NORESTART");
            Ensure(passthrough, "/SP-");
        }

        return new BootstrapOptions
        {
            ZipUrl = zipUrl,
            Silent = silent,
            SetupArgs = string.Join(' ', passthrough.Select(QuoteIfNeeded)),
        };
    }

    private static void Ensure(List<string> list, string flag)
    {
        if (!list.Any(x => x.Equals(flag, StringComparison.OrdinalIgnoreCase)))
            list.Add(flag);
    }

    private static string QuoteIfNeeded(string s) =>
        s.Contains(' ') && !s.StartsWith('"') ? $"\"{s}\"" : s;
}

internal sealed class MainForm : Form
{
    private readonly BootstrapOptions _opts;
    private readonly Label _status = new();
    private readonly ProgressBar _bar = new();
    private readonly Label _detail = new();
    private readonly Button _cancel = new();
    private readonly CancellationTokenSource _cts = new();
    private bool _busy;

    public int ExitCode { get; private set; } = Program.ExitInitFail;

    public MainForm(BootstrapOptions opts)
    {
        _opts = opts;
        Text = "FrontLine — Instalação";
        Width = 520;
        Height = 190;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(5, 8, 12);
        ForeColor = Color.FromArgb(232, 238, 244);
        Font = new Font("Segoe UI", 9.5f);

        _status.AutoSize = false;
        _status.SetBounds(20, 18, 460, 24);
        _status.Text = "A preparar…";

        _bar.SetBounds(20, 52, 460, 22);
        _bar.Style = ProgressBarStyle.Continuous;
        _bar.Minimum = 0;
        _bar.Maximum = 1000;

        _detail.AutoSize = false;
        _detail.SetBounds(20, 84, 460, 22);
        _detail.ForeColor = Color.FromArgb(154, 168, 184);
        _detail.Text = "";

        _cancel.Text = "Cancelar";
        _cancel.SetBounds(380, 118, 100, 28);
        _cancel.FlatStyle = FlatStyle.Flat;
        _cancel.Click += (_, _) =>
        {
            if (!_busy)
            {
                ExitCode = Program.ExitCancel;
                Close();
                return;
            }
            _cts.Cancel();
            _cancel.Enabled = false;
            _status.Text = "A cancelar…";
        };

        Controls.Add(_status);
        Controls.Add(_bar);
        Controls.Add(_detail);
        Controls.Add(_cancel);

        if (_opts.Silent)
        {
            ShowInTaskbar = false;
            Opacity = 0;
            WindowState = FormWindowState.Minimized;
        }

        Load += async (_, _) => await RunAsync();
        FormClosing += (_, e) =>
        {
            if (_busy && !_cts.IsCancellationRequested)
            {
                e.Cancel = true;
                _cts.Cancel();
            }
        };
    }

    private async Task RunAsync()
    {
        _busy = true;
        var workRoot = Path.Combine(Path.GetTempPath(), "FrontLine-Store-Setup");
        var zipPath = Path.Combine(workRoot, "FrontLine-Setup-latest.zip");
        var extractDir = Path.Combine(workRoot, "payload");

        try
        {
            Directory.CreateDirectory(workRoot);

            SetUi("A verificar espaço em disco…", 0, "");
            // ZIP ~6 GB + extract ~6 GB + margem; install final fica em Program Files.
            const long needBytes = 14L * 1024 * 1024 * 1024;
            var drive = new DriveInfo(Path.GetPathRoot(workRoot) ?? "C:\\");
            if (drive.AvailableFreeSpace < needBytes)
                throw new InvalidOperationException(
                    $"Espaço insuficiente em {drive.Name}. Livre pelo menos ~14 GB temporários.");

            SetUi("A descarregar o instalador (~6 GB)…", 0, _opts.ZipUrl);
            await DownloadAsync(_opts.ZipUrl, zipPath, _cts.Token);

            SetUi("A extrair ficheiros…", 850, Path.GetFileName(zipPath));
            if (Directory.Exists(extractDir))
                Directory.Delete(extractDir, true);
            Directory.CreateDirectory(extractDir);
            await Task.Run(() => ZipFile.ExtractToDirectory(zipPath, extractDir, overwriteFiles: true), _cts.Token);

            var setupExe = FindSetupExe(extractDir);
            if (setupExe is null)
                throw new FileNotFoundException("FrontLine-Setup-latest.exe não encontrado no ZIP.");

            SetUi("A instalar o FrontLine…", 950, Path.GetFileName(setupExe));
            var code = await RunSetupAsync(setupExe, _opts.SetupArgs, _cts.Token);
            ExitCode = code;
            SetUi(code == Program.ExitOk ? "Concluído." : $"Instalador terminou com código {code}.", 1000, "");
        }
        catch (OperationCanceledException)
        {
            ExitCode = Program.ExitCancel;
            SetUi("Cancelado.", 0, "");
        }
        catch (HttpRequestException ex)
        {
            ExitCode = Program.ExitInitFail;
            SetUi("Falha de rede.", 0, ex.Message);
            if (!_opts.Silent)
                MessageBox.Show($"Falha de rede:\n{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            ExitCode = Program.ExitInstallFail;
            SetUi("Erro.", 0, ex.Message);
            if (!_opts.Silent)
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _busy = false;
            _cancel.Enabled = true;
            try
            {
                // Mantém ZIP em cache se sucesso parcial futuro — limpa extract.
                if (Directory.Exists(extractDir))
                    Directory.Delete(extractDir, true);
            }
            catch { /* ignore */ }

            // Em silent (Store), fecha logo; com UI dá 1s para ler.
            if (_opts.Silent)
                Close();
            else
            {
                await Task.Delay(800);
                Close();
            }
        }
    }

    private static string? FindSetupExe(string dir)
    {
        var preferred = Path.Combine(dir, "FrontLine-Setup-latest.exe");
        if (File.Exists(preferred)) return preferred;
        return Directory.EnumerateFiles(dir, "*.exe", SearchOption.TopDirectoryOnly)
            .FirstOrDefault(f => Path.GetFileName(f).StartsWith("FrontLine-Setup", StringComparison.OrdinalIgnoreCase)
                                 || Path.GetFileName(f).StartsWith("Instalador-FrontLine", StringComparison.OrdinalIgnoreCase));
    }

    private async Task DownloadAsync(string url, string dest, CancellationToken ct)
    {
        // Reaproveita ZIP completo se já existir e tiver tamanho > 1 GB.
        if (File.Exists(dest) && new FileInfo(dest).Length > 1_000_000_000)
        {
            SetUi("ZIP em cache — a reutilizar…", 800, dest);
            return;
        }

        using var http = new HttpClient { Timeout = TimeSpan.FromHours(6) };
        http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("FrontLineBootstrap", "1.0"));

        using var resp = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
        resp.EnsureSuccessStatusCode();
        var total = resp.Content.Headers.ContentLength;

        var partial = dest + ".partial";
        await using (var input = await resp.Content.ReadAsStreamAsync(ct))
        await using (var output = new FileStream(partial, FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 256, true))
        {
            var buffer = new byte[1024 * 256];
            long read = 0;
            int n;
            var lastUi = DateTime.UtcNow;
            while ((n = await input.ReadAsync(buffer.AsMemory(0, buffer.Length), ct)) > 0)
            {
                await output.WriteAsync(buffer.AsMemory(0, n), ct);
                read += n;
                if ((DateTime.UtcNow - lastUi).TotalMilliseconds > 200)
                {
                    lastUi = DateTime.UtcNow;
                    double pct = total is > 0 ? read / (double)total.Value : 0;
                    var permille = (int)Math.Clamp(pct * 800, 0, 800); // 0–80% da barra = download
                    var mb = read / (1024.0 * 1024.0);
                    var totalMb = total is > 0 ? total.Value / (1024.0 * 1024.0) : 0;
                    var detail = total is > 0
                        ? $"{mb:0}/{totalMb:0} MB"
                        : $"{mb:0} MB";
                    SetUi("A descarregar o instalador…", permille, detail);
                }
            }
        }

        if (File.Exists(dest)) File.Delete(dest);
        File.Move(partial, dest);
    }

    private static async Task<int> RunSetupAsync(string exe, string args, CancellationToken ct)
    {
        var psi = new ProcessStartInfo
        {
            FileName = exe,
            Arguments = args,
            UseShellExecute = false,
            WorkingDirectory = Path.GetDirectoryName(exe)!,
        };
        using var p = Process.Start(psi) ?? throw new InvalidOperationException("Não foi possível iniciar o Setup.");
        await p.WaitForExitAsync(ct);
        return p.ExitCode;
    }

    private void SetUi(string status, int permille, string detail)
    {
        if (IsDisposed) return;
        void apply()
        {
            _status.Text = status;
            _bar.Value = Math.Clamp(permille, 0, 1000);
            _detail.Text = detail;
        }
        if (InvokeRequired) BeginInvoke(apply);
        else apply();
    }
}
