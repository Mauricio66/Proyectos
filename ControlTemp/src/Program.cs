using System.Diagnostics;

namespace ControlTemp;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        using var mutex = new Mutex(true, "ControlTemp_UnaSolaInstancia", out bool first);
        if (!first)
        {
            MessageBox.Show("Control de Temperaturas ya está abierto. Búscalo junto al reloj de Windows.",
                "Control de Temperaturas", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        ApplicationConfiguration.Initialize();
        bool hidden = args.Any(a => a.Equals("--minimized", StringComparison.OrdinalIgnoreCase));
        Application.Run(new MainForm(hidden));
    }
}

// Inicio automático con Windows mediante el Programador de tareas (necesario porque la app pide administrador).
static class StartupTask
{
    const string TaskName = "ControlTemp";

    public static bool IsEnabled() => Run($"/Query /TN \"{TaskName}\"") == 0;

    public static bool Set(bool enabled)
    {
        if (!enabled) return Run($"/Delete /F /TN \"{TaskName}\"") == 0;
        string exe = Environment.ProcessPath ?? Application.ExecutablePath;
        return Run($"/Create /F /TN \"{TaskName}\" /SC ONLOGON /RL HIGHEST /TR \"\\\"{exe}\\\" --minimized\"") == 0;
    }

    static int Run(string args)
    {
        try
        {
            using var p = Process.Start(new ProcessStartInfo("schtasks.exe", args)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            });
            if (p is null) return -1;
            p.WaitForExit(10000);
            return p.ExitCode;
        }
        catch
        {
            return -1;
        }
    }
}
