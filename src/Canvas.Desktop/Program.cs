using System.Windows;

namespace Canvas.Desktop;

internal static class Program
{
    [STAThread]
    public static void Main()
    {
        new Application().Run(new MainWindow());
    }
}
