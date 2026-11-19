using System.Diagnostics;
using System.Text.Json;

internal static class Program
{
    [STAThread]
    static int Main(string[] args)
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        if (args is ["--fixture", var kind, var path])
        {
            Application.Run(new Fixture(kind, path));
            return 0;
        }
        bool layout = args is ["layout"] or ["layout-code"];
        if (!layout && (args.Length == 0 || args[0] is not ("control" or "code") ||
            args.Skip(1).Any(option => option != "--auto")))
        {
            Console.Error.WriteLine("PanelNativo.exe layout | layout-code | control|code [--auto]");
            return 2;
        }
        var run = new Run(args is ["layout"] ? "control" : args[0]);
        int result = 1;
        try
        {
            run.Start();
            if (layout)
                result = LayoutTrial.Execute(run);
            else
            {
                using var host = new Host(run, args.Contains("--auto"));
                Application.Run(host);
                result = host.Failed ? 1 : 0;
            }
        }
        catch (Exception ex)
        {
            run.Log("ERROR " + ex.Message);
        }
        finally { run.Dispose(); }
        return run.Clean ? result : 1;
    }
}

internal sealed class Run : IDisposable
{
    readonly string id = Guid.NewGuid().ToString("N")[..12];
    readonly List<Process> children = [];
    readonly Dictionary<int, string> documents = [];
    readonly string temp;
    readonly string output;
    readonly string mode;
    internal readonly string Document;
    internal nint Candidate, Neighbor;
    internal int CandidatePid, NeighborPid;
    internal bool Clean { get; private set; } = true;
    internal string Evidence(string name) => Path.Combine(output, name);

    internal Run(string mode)
    {
        this.mode = mode;
        temp = Directory.CreateTempSubdirectory("PanelNativo-").FullName;
        output = Directory.CreateDirectory(Path.Combine("evidencia-local", mode + "-" + id)).FullName;
        Document = Path.Combine(temp, "panel-" + id + ".txt");
    }

    internal void Log(string message)
    {
        Console.WriteLine(message);
        File.AppendAllText(Evidence("sesion.txt"), message + Environment.NewLine);
    }

    Process Launch(string document, string exe, params string[] args)
    {
        var info = new ProcessStartInfo(exe) { UseShellExecute = false, CreateNoWindow = true, WorkingDirectory = temp };
        foreach (var arg in args) info.ArgumentList.Add(arg);
        var process = Process.Start(info) ?? throw new InvalidOperationException("No arrancó " + exe);
        children.Add(process);
        documents.Add(process.Id, Path.GetFileName(document));
        return process;
    }

    internal void Start()
    {
        Log($"caso={mode} id={id} Windows={Environment.OSVersion.Version} runtime={Environment.Version} sesión={Process.GetCurrentProcess().SessionId} interactiva={Environment.UserInteractive} salida={output}");
        File.WriteAllText(Document, string.Concat(Enumerable.Repeat("muestra de texto para observar el ajuste de línea ", 7)) + "\r\n");
        string self = Environment.ProcessPath!;
        string neighborDocument = mode == "layout-code"
            ? Path.Combine(temp, "neighbor-" + id + ".txt") : Document;
        if (mode == "layout-code") File.Copy(Document, neighborDocument);
        var neighbor = mode == "layout-code"
            ? LaunchCode(neighborDocument, "B")
            : Launch(Document, self, "--fixture", "vecino", Document);
        NeighborPid = neighbor.Id;
        Neighbor = WaitWindow(neighbor);
        var candidate = mode == "control"
            ? Launch(Document, self, "--fixture", "editor", Document)
            : LaunchCode(Document, "A");
        CandidatePid = candidate.Id;
        Candidate = WaitWindow(candidate);
        Log($"A HWND=0x{Candidate:X} PID={CandidatePid} documento={Path.GetFileName(Document)}");
        Log($"B HWND=0x{Neighbor:X} PID={NeighborPid} documento={Path.GetFileName(neighborDocument)}");
    }

    Process LaunchCode(string document, string subject)
    {
        var exe = Environment.GetEnvironmentVariable("PANEL_CODE_EXE");
        exe ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Microsoft VS Code", "Code.exe");
        Log("VS Code " + FileVersionInfo.GetVersionInfo(exe).ProductVersion);
        var profile = Path.Combine(temp, "perfil-" + subject);
        var user = Directory.CreateDirectory(Path.Combine(profile, "User")).FullName;
        var settings = new Dictionary<string, object>
        {
            ["window.title"] = "${activeEditorShort}", ["window.restoreWindows"] = "none",
            ["workbench.startupEditor"] = "none", ["editor.wordWrap"] = "on",
            ["editor.fontSize"] = 16, ["editor.lineHeight"] = 24, ["editor.minimap.enabled"] = false,
            ["chat.disableAIFeatures"] = true, ["workbench.secondarySideBar.defaultVisibility"] = "hidden",
            ["files.simpleDialog.enable"] = false, ["update.mode"] = "none", ["telemetry.telemetryLevel"] = "off"
        };
        File.WriteAllText(Path.Combine(user, "settings.json"), JsonSerializer.Serialize(settings));
        return Launch(document, exe, "--user-data-dir", profile, "--extensions-dir", Path.Combine(temp, "ext-" + subject),
            "--disable-extensions", "--new-window", "--skip-welcome", "--skip-release-notes", document);
    }

    nint WaitWindow(Process process)
    {
        var elapsed = Stopwatch.StartNew();
        while (elapsed.Elapsed < TimeSpan.FromSeconds(35) && !process.HasExited)
        {
            process.Refresh();
            var hwnd = process.MainWindowHandle;
            if (IsOurs(hwnd, process.Id)) return hwnd;
            Thread.Sleep(150);
        }
        throw new TimeoutException($"Falta ventana con PID {process.Id} y documento {documents[process.Id]}.");
    }

    internal bool IsOurs(nint hwnd, int pid) => documents.TryGetValue(pid, out var document)
        && children.Any(child => child.Id == pid && !child.HasExited)
        && hwnd != 0 && Native.Pid(hwnd) == pid && Native.Title(hwnd).Contains(document, StringComparison.Ordinal);

    public void Dispose()
    {
        foreach (var child in children.AsEnumerable().Reverse())
        {
            try
            {
                if (!child.HasExited)
                {
                    child.Refresh();
                    if (IsOurs(child.MainWindowHandle, child.Id)) child.CloseMainWindow();
                    else Log($"cierre de ventana omitido: identidad no verificada PID={child.Id}");
                    if (!child.WaitForExit(2500))
                    {
                        Log($"cierre forzado del proceso propio PID={child.Id}");
                        child.Kill(entireProcessTree: true);
                        child.WaitForExit(4000);
                    }
                }
                Log($"cierre PID={child.Id} terminado={child.HasExited}");
                Clean &= child.HasExited;
            }
            catch (Exception ex) { Log($"error de cierre PID={child.Id} {ex.Message}"); Clean = false; }
            child.Dispose();
        }
        for (int attempt = 0; attempt < 10; attempt++)
        {
            try { Directory.Delete(temp, true); break; }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { Thread.Sleep(200); }
        }
        bool removed = !Directory.Exists(temp);
        Clean &= removed;
        Log($"temporal eliminado={removed}");
    }
}
