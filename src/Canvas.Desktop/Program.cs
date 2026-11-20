using Canvas.Domain;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Canvas.Desktop;

internal static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        var restoreOnly = args is ["--restore-windows"];
        var title = restoreOnly ? "Devolver ventanas" : "Escritorio Canvas";
        try
        {
            var localData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            using var recovery = new WindowRecovery(WindowRecovery.GetPath(localData), new NativeWindowPlacement());
            if (!TryRestore(recovery, title))
            {
                return 1;
            }

            if (restoreOnly)
            {
                MessageBox.Show("Se han procesado todas las devoluciones pendientes de esta sesión de Windows.", title,
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return 0;
            }

            var exitCode = new Application().Run(new MainWindow());
            return TryRestore(recovery, title) ? exitCode : 1;
        }
        catch (InvalidDataException error)
        {
            Trace.TraceError(error.ToString());
            return ShowError(title, "El registro de devolución no se puede leer. Se ha conservado para revisarlo.");
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or Win32Exception)
        {
            Trace.TraceError(error.ToString());
            return ShowError(title, "No se pudo acceder al registro de devolución. Comprueba que Escritorio Canvas esté cerrado y que el registro sea accesible.");
        }
    }

    private static bool TryRestore(WindowRecovery recovery, string title)
    {
        var failures = recovery.RestoreAll();
        if (failures.Count == 0)
        {
            return true;
        }

        foreach (var failure in failures)
        {
            Trace.TraceError(failure.ToString());
        }

        ShowError(title, "Quedan ventanas por devolver. Vuelve a ejecutar «Devolver ventanas» con Escritorio Canvas cerrado.");
        return false;
    }

    private static int ShowError(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        return 1;
    }
}
