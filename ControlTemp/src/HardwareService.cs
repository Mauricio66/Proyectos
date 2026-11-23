using System.Diagnostics;
using LibreHardwareMonitor.Hardware;

namespace ControlTemp;

public sealed record SensorReading(
    string Id, string Hardware, HardwareType HwType, string Name, float? Value, float? Min, float? Max);

public sealed record ControlReading(
    string Id, string Name, string Hardware, float? Percent, float? Rpm, bool Managed);

public sealed class Snapshot
{
    public List<SensorReading> Temps { get; } = new();
    public List<SensorReading> Fans { get; } = new();
    public List<ControlReading> Controls { get; } = new();

    public float? Cpu { get; set; }
    public float? Gpu { get; set; }
    public float? Disk { get; set; }
    public string? CpuName { get; set; }
    public string? GpuName { get; set; }
    public string? DiskName { get; set; }

    public bool Emergency { get; set; }
}

// Lee los sensores en un hilo aparte (cada segundo) y aplica el control de ventiladores.
public sealed class HardwareService : IDisposable
{
    readonly List<Computer> _groups;
    readonly FanController _fans;
    readonly AutoResetEvent _wake = new(false);
    Thread? _thread;
    volatile bool _running;

    public event Action<Snapshot>? Updated;
    public string? OpenError { get; private set; }

    public HardwareService(AppConfig cfg)
    {
        _fans = new FanController(cfg);
        // Un grupo por tipo de hardware: si uno falla al abrirse, los demás siguen funcionando.
        _groups = new()
        {
            new Computer { IsCpuEnabled = true },
            new Computer { IsGpuEnabled = true },
            new Computer { IsMotherboardEnabled = true },
            new Computer { IsStorageEnabled = true },
            new Computer { IsControllerEnabled = true },
        };
    }

    public void Start()
    {
        _running = true;
        _thread = new Thread(Loop) { IsBackground = true, Name = "Sensores" };
        _thread.Start();
    }

    void Loop()
    {
        var opened = new List<Computer>();
        var errors = new List<string>();
        foreach (var pc in _groups)
        {
            try { pc.Open(); opened.Add(pc); }
            catch (Exception ex) { errors.Add(ex.Message); }
        }
        if (opened.Count == 0 && errors.Count > 0) OpenError = errors[0];

        while (_running)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var snap = Read(opened);
                Updated?.Invoke(snap);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            int wait = 1000 - (int)sw.ElapsedMilliseconds;
            if (wait > 0) _wake.WaitOne(wait);
        }

        // Al salir, los ventiladores vuelven a control de la BIOS.
        try { _fans.RestoreAll(); } catch { }
        foreach (var pc in opened)
        {
            try { pc.Close(); } catch { }
        }
    }

    Snapshot Read(List<Computer> computers)
    {
        var snap = new Snapshot();
        var controls = new List<ISensor>();
        foreach (var pc in computers)
            foreach (var hw in pc.Hardware) Visit(hw, snap, controls);

        (snap.Cpu, snap.CpuName) = PickCpu(snap.Temps);
        (snap.Gpu, snap.GpuName) = PickGpu(snap.Temps);
        var disk = snap.Temps.Where(t => t.HwType == HardwareType.Storage).MaxBy(t => t.Value);
        snap.Disk = disk?.Value;
        snap.DiskName = disk?.Hardware;

        _fans.Apply(snap, controls);
        return snap;
    }

    static void Visit(IHardware hw, Snapshot snap, List<ISensor> controls)
    {
        try { hw.Update(); } catch { }
        foreach (var sub in hw.SubHardware) Visit(sub, snap, controls);

        foreach (var s in hw.Sensors)
        {
            switch (s.SensorType)
            {
                case SensorType.Temperature when s.Value is > 0 and < 150:
                    snap.Temps.Add(Reading(hw, s));
                    break;
                case SensorType.Fan:
                    snap.Fans.Add(Reading(hw, s));
                    break;
                case SensorType.Control when s.Control is not null:
                    controls.Add(s);
                    break;
            }
        }
    }

    static SensorReading Reading(IHardware hw, ISensor s) =>
        new(s.Identifier.ToString(), hw.Name, hw.HardwareType, s.Name, s.Value, s.Min, s.Max);

    static readonly string[] CpuPreferred =
    {
        "CPU Package", "Core (Tctl/Tdie)", "Core (Tctl)", "Core (Tdie)", "Tctl/Tdie", "Tdie", "Tctl", "Core Max", "Core Average",
    };

    static (float?, string?) PickCpu(List<SensorReading> temps)
    {
        var cpu = temps.Where(t => t.HwType == HardwareType.Cpu).ToList();
        foreach (var name in CpuPreferred)
        {
            var hit = cpu.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (hit is not null) return (hit.Value, hit.Hardware);
        }
        var max = cpu.MaxBy(t => t.Value);
        if (max is not null) return (max.Value, max.Hardware);

        // Si no hay sensor de la CPU, se usa el de la placa base.
        var board = temps.FirstOrDefault(t =>
            t.HwType is HardwareType.SuperIO or HardwareType.Motherboard &&
            t.Name.Contains("CPU", StringComparison.OrdinalIgnoreCase));
        return (board?.Value, board is null ? null : "CPU (sensor de placa base)");
    }

    static (float?, string?) PickGpu(List<SensorReading> temps)
    {
        var gpu = temps.Where(t => t.HwType is HardwareType.GpuNvidia or HardwareType.GpuAmd or HardwareType.GpuIntel).ToList();
        var core = gpu.FirstOrDefault(t => t.Name.Equals("GPU Core", StringComparison.OrdinalIgnoreCase))
                   ?? gpu.MaxBy(t => t.Value);
        return (core?.Value, core?.Hardware);
    }

    public void Dispose()
    {
        _running = false;
        _wake.Set();
        _thread?.Join(5000);
    }
}

public sealed class FanController
{
    readonly AppConfig _cfg;
    readonly Dictionary<string, ISensor> _managed = new();
    readonly Dictionary<string, float> _last = new();

    public FanController(AppConfig cfg) => _cfg = cfg;

    public void Apply(Snapshot snap, List<ISensor> controls)
    {
        snap.Emergency = snap.Cpu >= _cfg.EmergencyTemp || snap.Gpu >= _cfg.EmergencyTemp;

        foreach (var c in controls)
        {
            string id = c.Identifier.ToString();
            var fan = _cfg.GetFan(id);
            var ctl = c.Control;

            float? target = fan.Mode switch
            {
                FanMode.Fixed => fan.FixedPercent,
                FanMode.Curve => CurveTarget(fan, snap),
                _ => null,
            };

            try
            {
                if (target is null)
                {
                    if (_managed.Remove(id))
                    {
                        ctl.SetDefault();
                        _last.Remove(id);
                    }
                }
                else
                {
                    float t = snap.Emergency ? 100 : target.Value;
                    t = Math.Clamp(t, _cfg.MinFanPercent, 100);

                    // Con curva, sube rápido pero baja poco a poco para que no "suba y baje" constantemente.
                    if (fan.Mode == FanMode.Curve && !snap.Emergency &&
                        _last.TryGetValue(id, out var prev) && t < prev)
                        t = Math.Max(t, prev - 3);

                    float lo = Math.Min(ctl.MinSoftwareValue, ctl.MaxSoftwareValue);
                    float hi = Math.Max(ctl.MinSoftwareValue, ctl.MaxSoftwareValue);
                    if (hi > lo) t = Math.Clamp(t, lo, hi);

                    bool isNew = !_managed.ContainsKey(id);
                    if (isNew || !_last.TryGetValue(id, out var last) || Math.Abs(last - t) >= 1)
                    {
                        ctl.SetSoftware(t);
                        _last[id] = t;
                    }
                    _managed[id] = c;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            float? rpm = c.Hardware.Sensors
                .FirstOrDefault(f => f.SensorType == SensorType.Fan && f.Index == c.Index)?.Value;
            snap.Controls.Add(new ControlReading(id, c.Name, c.Hardware.Name, c.Value, rpm, _managed.ContainsKey(id)));
        }
    }

    static float CurveTarget(FanSetting fan, Snapshot snap)
    {
        float? temp = fan.Source switch
        {
            TempSource.Cpu => snap.Cpu,
            TempSource.Gpu => snap.Gpu,
            _ => Max(snap.Cpu, snap.Gpu),
        };
        // Sin lectura de temperatura no se arriesga: ventilador al máximo.
        return temp is null ? 100 : FanCurve.Evaluate(fan.Curve, temp.Value);
    }

    static float? Max(float? a, float? b) =>
        a is null ? b : b is null ? a : Math.Max(a.Value, b.Value);

    public void RestoreAll()
    {
        foreach (var c in _managed.Values)
        {
            try { c.Control.SetDefault(); } catch { }
        }
        _managed.Clear();
        _last.Clear();
    }
}
