using System.Runtime.InteropServices;

namespace Canvas.Desktop;

internal static class MapBackdrop
{
    public static bool TryEnable(nint window)
    {
        const int ImmersiveDarkMode = 20;
        const int WindowCornerPreference = 33;
        const int SystemBackdropType = 38;
        const int Enabled = 1;
        const int Round = 2;
        const int DesktopAcrylic = 3;
        const int None = 1;

        DwmSetWindowAttribute(window, ImmersiveDarkMode, Enabled, sizeof(int));
        DwmSetWindowAttribute(window, WindowCornerPreference, Round, sizeof(int));
        if (DwmSetWindowAttribute(window, SystemBackdropType, DesktopAcrylic, sizeof(int)) < 0)
        {
            return false;
        }

        var sheet = new Margins(-1, -1, -1, -1);
        if (DwmExtendFrameIntoClientArea(window, in sheet) >= 0)
        {
            return true;
        }

        DwmSetWindowAttribute(window, SystemBackdropType, None, sizeof(int));
        return false;
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(nint window, int attribute, in int value, int size);

    [DllImport("dwmapi.dll")]
    private static extern int DwmExtendFrameIntoClientArea(nint window, in Margins margins);

    [StructLayout(LayoutKind.Sequential)]
    private readonly record struct Margins(int Left, int Right, int Top, int Bottom);
}
