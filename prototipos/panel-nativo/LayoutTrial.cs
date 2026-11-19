using System.Diagnostics;
using System.Runtime.InteropServices;
using Placement = WindowPlacementApi.Placement;

internal static class LayoutTrial
{
    const uint Normal = 1;
    const int Maximized = 3;

    readonly record struct Subject(string Name, nint Hwnd, int Pid, nint Monitor, uint Dpi)
    {
        public override string ToString() => $"{Name} HWND=0x{Hwnd:X} PID={Pid} DPI={Dpi}";
    }

    readonly record struct Reading(Placement Placement, Rectangle Visible)
    {
        internal uint State => Placement.ShowCommand;
        internal Rectangle NormalBounds => Placement.NormalBounds;
    }

    readonly record struct Expected(Subject Window, uint State, Rectangle? NormalBounds, Rectangle? Visible)
    {
        internal Expected(Subject window, Reading reading)
            : this(window, reading.State, reading.NormalBounds, reading.Visible) { }
    }

    internal static int Execute(Run run)
    {
        var a = Identify("A", run.Candidate, run.CandidatePid);
        var b = Identify("B", run.Neighbor, run.NeighborPid);
        var area = WorkArea(a.Monitor);
        if (area.Width < 1200 || area.Height < 800)
            throw new InvalidOperationException($"Área de trabajo insuficiente: {area} px.");

        run.Log($"layout área={area} px aplicación=PanelNativo {typeof(LayoutTrial).Assembly.GetName().Version}");
        var startupA = Read(a);
        var startupB = Read(b);
        Reading? originalA = null, originalB = null;
        bool passed = false;
        string stage = "preparación";
        try
        {
            var desktopA = BoundsAt(20, 40, 540, 400);
            var desktopB = BoundsAt(580, 70, 540, 440);
            PlaceNormal(a, desktopA);
            PlaceNormal(b, desktopB);
            var prepared = Wait(stage, new(a, Normal, null, desktopA), new(b, Normal, null, desktopB));

            stage = "D0";
            Change(b, () => ShowWindow(b.Hwnd, Maximized));
            var desktop = Wait(stage, new(a, prepared[0]), new(b, Maximized, prepared[1].NormalBounds, null));
            originalA = desktop[0];
            originalB = desktop[1];

            stage = "C1";
            var canvasA = BoundsAt(30, 80, 480, 500);
            var canvasB = BoundsAt(550, 120, 500, 450);
            PlaceNormal(a, canvasA);
            PlaceNormal(b, canvasB);
            Wait(stage, new(a, Normal, null, canvasA), new(b, Normal, null, canvasB));

            stage = "C2";
            var movedA = BoundsAt(65, 110, 510, 460);
            Change(a, () => WindowPlacementApi.Place(a.Hwnd, movedA));
            var canvas = Wait(stage, new(a, Normal, null, movedA), new(b, Normal, null, canvasB));

            stage = "D1";
            ReturnToDesktop();

            stage = "C3";
            PlaceNormal(a, canvas[0].Visible);
            PlaceNormal(b, canvas[1].Visible);
            Wait(stage, new(a, Normal, null, canvas[0].Visible), new(b, Normal, null, canvas[1].Visible));

            stage = "D2";
            ReturnToDesktop();
            passed = true;
        }
        catch (Exception ex)
        {
            run.Log($"{stage} FALLO {ex.Message}");
        }
        finally
        {
            RestoreAfterTrial(a, originalA ?? startupA);
            RestoreAfterTrial(b, originalB ?? startupB);
        }
        return passed ? 0 : 1;

        Subject Identify(string name, nint hwnd, int pid)
        {
            var subject = new Subject(name, hwnd, pid, 0, 0);
            RequireIdentity(subject);
            var monitor = MonitorFromWindow(hwnd, 0);
            var dpi = Native.GetDpiForWindow(hwnd);
            if (monitor == 0 || dpi == 0)
                throw new InvalidOperationException($"{subject} sin monitor o DPI.");
            return subject with { Monitor = monitor, Dpi = dpi };
        }

        void RequireIdentity(Subject window)
        {
            if (!run.IsOurs(window.Hwnd, window.Pid))
                throw new InvalidOperationException($"{window} perdió su identidad.");
        }

        void RequireEnvironment(Subject window)
        {
            var dpi = Native.GetDpiForWindow(window.Hwnd);
            if (window.Monitor != a.Monitor || MonitorFromWindow(window.Hwnd, 0) != window.Monitor ||
                dpi != window.Dpi || WorkArea(a.Monitor) != area)
                throw new InvalidOperationException($"{window} cambió de monitor, DPI o área de trabajo. DPI obtenido={dpi}.");
        }

        Rectangle BoundsAt(int x, int y, int width, int height) =>
            new(area.Left + x, area.Top + y, width, height);

        void Change(Subject window, Action change)
        {
            RequireIdentity(window);
            RequireEnvironment(window);
            change();
        }

        void PlaceNormal(Subject window, Rectangle bounds)
        {
            Change(window, () => WindowPlacementApi.Normalize(window.Hwnd));
            Change(window, () => WindowPlacementApi.Place(window.Hwnd, bounds));
        }

        void ReturnToDesktop()
        {
            var savedA = originalA!.Value;
            var savedB = originalB!.Value;
            Change(a, () => WindowPlacementApi.Restore(a.Hwnd, savedA.Placement));
            Change(b, () => WindowPlacementApi.Restore(b.Hwnd, savedB.Placement));
            Wait(stage, new(a, savedA), new(b, savedB));
        }

        void RestoreAfterTrial(Subject window, Reading saved)
        {
            try
            {
                RequireIdentity(window);
                WindowPlacementApi.Restore(window.Hwnd, saved.Placement);
                Wait("limpieza", new Expected(window, saved));
            }
            catch (Exception ex)
            {
                passed = false;
                run.Log($"limpieza {window} FALLO {ex.Message}");
            }
        }

        Reading Read(Subject window)
        {
            RequireIdentity(window);
            RequireEnvironment(window);
            return new(WindowPlacementApi.Capture(window.Hwnd), Native.Bounds(window.Hwnd));
        }

        Reading[] Wait(string transition, params Expected[] expected)
        {
            var elapsed = Stopwatch.StartNew();
            var actual = new Reading[expected.Length];
            var previous = new Reading?[expected.Length];
            var verified = new bool[expected.Length];
            do
            {
                for (int i = 0; i < expected.Length; i++)
                {
                    var target = expected[i];
                    actual[i] = Read(target.Window);
                    var reading = actual[i];
                    verified[i] = previous[i] is Reading prior && reading.State == target.State &&
                        reading.State == prior.State && reading.NormalBounds == prior.NormalBounds &&
                        reading.Visible == prior.Visible && Near(target.NormalBounds, reading.NormalBounds) &&
                        Near(target.Visible, reading.Visible);
                    previous[i] = reading;
                }
                if (verified.All(value => value))
                    break;
                if (elapsed.ElapsedMilliseconds <= 2900)
                    Thread.Sleep(100);
                else
                    break;
            } while (elapsed.ElapsedMilliseconds < 3000);

            for (int i = 0; i < expected.Length; i++)
            {
                var target = expected[i];
                var reading = actual[i];
                run.Log($"{transition} {target.Window} estado esperado={target.State} obtenido={reading.State} " +
                    $"normal {Describe(target.NormalBounds, reading.NormalBounds)} " +
                    $"pantalla {Describe(target.Visible, reading.Visible)} " +
                    $"resultado={(verified[i] ? "verificado" : "fallido")}");
            }
            if (!verified.All(value => value))
                throw new TimeoutException("No se obtuvieron dos lecturas consecutivas válidas en 3 s.");
            return actual;
        }
    }

    static bool Near(Rectangle? expected, Rectangle actual) =>
        expected is not Rectangle bounds || Edges(bounds, actual).All(delta => Math.Abs(delta) <= 1);

    static int[] Edges(Rectangle expected, Rectangle actual) =>
        [actual.Left - expected.Left, actual.Top - expected.Top,
         actual.Right - expected.Right, actual.Bottom - expected.Bottom];

    static string Describe(Rectangle? expected, Rectangle actual) => expected is Rectangle bounds
        ? $"esperado={bounds} obtenido={actual} Δ(I,S,D,B)=[{string.Join(",", Edges(bounds, actual))}] px"
        : $"esperado=sin referencia obtenido={actual}";

    static Rectangle WorkArea(nint monitor)
    {
        const uint primary = 1;
        var info = new MonitorInfo { Size = (uint)Marshal.SizeOf<MonitorInfo>() };
        if (!GetMonitorInfoW(monitor, ref info) || (info.Flags & primary) == 0)
            throw new InvalidOperationException("No se pudo medir el área de trabajo del monitor primario.");
        return info.Work.ToRectangle();
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MonitorInfo
    {
        internal uint Size;
        internal Native.Rect Bounds;
        internal Native.Rect Work;
        internal uint Flags;
    }

    [DllImport("user32")]
    static extern nint MonitorFromWindow(nint hwnd, uint flags);

    [DllImport("user32", ExactSpelling = true)]
    static extern bool GetMonitorInfoW(nint monitor, ref MonitorInfo info);

    [DllImport("user32")]
    static extern bool ShowWindow(nint hwnd, int command);
}
