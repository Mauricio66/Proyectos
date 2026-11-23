using System.Text.Json;
using System.Text.Json.Serialization;

namespace ControlTemp;

public enum FanMode { Auto, Fixed, Curve }

public enum TempSource { Cpu, Gpu, Max }

public sealed class CurvePoint
{
    public int Temp { get; set; }
    public int Percent { get; set; }
}

public sealed class FanSetting
{
    public FanMode Mode { get; set; } = FanMode.Auto;
    public int FixedPercent { get; set; } = 60;
    public TempSource Source { get; set; } = TempSource.Cpu;
    public List<CurvePoint> Curve { get; set; } = DefaultCurve();

    public static List<CurvePoint> DefaultCurve() => new()
    {
        new() { Temp = 40, Percent = 30 },
        new() { Temp = 60, Percent = 45 },
        new() { Temp = 70, Percent = 65 },
        new() { Temp = 80, Percent = 85 },
        new() { Temp = 85, Percent = 100 },
    };
}

public sealed class AppConfig
{
    public bool AlertsEnabled { get; set; } = true;
    public bool AlertSound { get; set; } = true;
    public int CpuAlert { get; set; } = 85;
    public int GpuAlert { get; set; } = 83;
    public int DiskAlert { get; set; } = 55;

    // Si la CPU o la GPU llegan a esta temperatura, los ventiladores controlados por la app van al 100 %.
    public int EmergencyTemp { get; set; } = 90;

    // Ningún ventilador controlado por la app baja de este porcentaje.
    public int MinFanPercent { get; set; } = 25;

    public bool FanWarningShown { get; set; }

    public Dictionary<string, FanSetting> Fans { get; set; } = new();

    static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    static string FolderPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ControlTemp");

    static string FilePath => Path.Combine(FolderPath, "config.json");

    public static AppConfig Load()
    {
        try
        {
            if (File.Exists(FilePath))
                return JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(FilePath), JsonOptions) ?? new();
        }
        catch
        {
            // Archivo dañado: se empieza con valores por defecto.
        }
        return new();
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(FolderPath);
            string json;
            lock (Fans) json = JsonSerializer.Serialize(this, JsonOptions);
            File.WriteAllText(FilePath, json);
        }
        catch
        {
            // No es grave si no se puede guardar.
        }
    }

    public FanSetting GetFan(string id)
    {
        lock (Fans)
        {
            if (!Fans.TryGetValue(id, out var fan))
            {
                fan = new FanSetting();
                Fans[id] = fan;
            }
            return fan;
        }
    }
}
