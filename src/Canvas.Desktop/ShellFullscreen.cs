using System.Runtime.InteropServices;

namespace Canvas.Desktop;

internal static class ShellFullscreen
{
    public static void Set(nint window, bool fullscreen)
    {
        try
        {
            var taskbar = (ITaskbarList2)new TaskbarList();
            try
            {
                taskbar.HrInit();
                taskbar.MarkFullscreenWindow(window, fullscreen);
            }
            finally
            {
                Marshal.ReleaseComObject(taskbar);
            }
        }
        catch (COMException error)
        {
            throw new InvalidOperationException("Windows could not update the taskbar for the panel.", error);
        }
    }

    [ComImport, Guid("56FDF344-FD6D-11D0-958A-006097C9A090")]
    private class TaskbarList { }

    [ComImport, Guid("602D4995-B13A-429B-A66E-1935E44F4317"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface ITaskbarList2
    {
        void HrInit();
        void AddTab(nint window);
        void DeleteTab(nint window);
        void ActivateTab(nint window);
        void SetActiveAlt(nint window);
        void MarkFullscreenWindow(nint window, [MarshalAs(UnmanagedType.Bool)] bool fullscreen);
    }
}
