using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace ControlTemp;

public static class Theme
{
    public static readonly Color Background = Color.FromArgb(245, 247, 250);
    public static readonly Color Surface = Color.White;
    public static readonly Color Text = Color.FromArgb(30, 36, 45);
    public static readonly Color Muted = Color.FromArgb(105, 115, 130);
    public static readonly Color Grid = Color.FromArgb(228, 232, 238);
    public static readonly Color Cpu = Color.FromArgb(226, 90, 52);
    public static readonly Color Gpu = Color.FromArgb(46, 120, 214);
    public static readonly Color Ok = Color.FromArgb(34, 150, 90);
    public static readonly Color Warm = Color.FromArgb(214, 140, 20);
    public static readonly Color Hot = Color.FromArgb(205, 45, 45);

    // Verde = bien, naranja = acercándose al límite, rojo = en el límite o encima.
    public static Color ForTemp(float? t, int limit)
    {
        if (t is null) return Muted;
        if (t >= limit) return Hot;
        if (t >= limit - 15) return Warm;
        return Ok;
    }
}

// Tarjeta grande con una temperatura (CPU, GPU o disco).
public sealed class TempCard : Panel
{
    readonly string _title;
    string _value = "--";
    string _subtitle = "Sin datos";
    Color _accent = Theme.Muted;

    public TempCard(string title)
    {
        _title = title;
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Theme.Surface;
        Margin = new Padding(6);
        Dock = DockStyle.Fill;
    }

    public void SetValue(float? temp, string? subtitle, int limit)
    {
        _value = temp is null ? "--" : $"{temp:0} °C";
        _subtitle = subtitle ?? "No detectado";
        _accent = Theme.ForTemp(temp, limit);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

        using (var bar = new SolidBrush(_accent))
            g.FillRectangle(bar, 0, 0, 6, Height);
        using (var border = new Pen(Theme.Grid))
            g.DrawRectangle(border, 0, 0, Width - 1, Height - 1);

        float x = 20;
        using var titleFont = new Font(Font.FontFamily, 10f, FontStyle.Bold);
        using var valueFont = new Font(Font.FontFamily, 28f, FontStyle.Bold);
        using var subFont = new Font(Font.FontFamily, 8.5f);
        using var muted = new SolidBrush(Theme.Muted);
        using var accent = new SolidBrush(_accent);

        g.DrawString(_title, titleFont, muted, x, 10);
        g.DrawString(_value, valueFont, accent, x - 4, 30);
        var subRect = new RectangleF(x, Height - 26, Width - x - 8, 20);
        using var fmt = new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
        g.DrawString(_subtitle, subFont, muted, subRect, fmt);
    }
}

// Gráfico de los últimos 10 minutos de CPU y GPU.
public sealed class TempChart : Panel
{
    const int Capacity = 600;
    readonly float?[] _cpu = new float?[Capacity];
    readonly float?[] _gpu = new float?[Capacity];
    int _count;
    int _head;

    public int CpuLimit { get; set; } = 85;

    public TempChart()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Theme.Surface;
        Dock = DockStyle.Fill;
        Margin = new Padding(6);
    }

    public void Add(float? cpu, float? gpu)
    {
        _cpu[_head] = cpu;
        _gpu[_head] = gpu;
        _head = (_head + 1) % Capacity;
        if (_count < Capacity) _count++;
        Invalidate();
    }

    float? At(float?[] data, int i) => data[(_head - _count + i + Capacity) % Capacity];

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using (var border = new Pen(Theme.Grid))
            g.DrawRectangle(border, 0, 0, Width - 1, Height - 1);

        var plot = new Rectangle(44, 30, Width - 60, Height - 56);
        if (plot.Width < 50 || plot.Height < 40) return;

        float max = 100;
        for (int i = 0; i < _count; i++)
            max = Math.Max(max, Math.Max(At(_cpu, i) ?? 0, At(_gpu, i) ?? 0));
        float min = 20;
        max = (float)Math.Ceiling(max / 10) * 10;

        using var small = new Font(Font.FontFamily, 8f);
        using var muted = new SolidBrush(Theme.Muted);
        using var gridPen = new Pen(Theme.Grid);

        for (float v = min; v <= max; v += 20)
        {
            float y = Y(v);
            g.DrawLine(gridPen, plot.Left, y, plot.Right, y);
            g.DrawString($"{v:0}°", small, muted, 6, y - 7);
        }

        float Y(float v) => plot.Bottom - (v - min) / (max - min) * plot.Height;
        float X(int i) => plot.Right - (_count - 1 - i) * (plot.Width / (float)(Capacity - 1));

        // Línea del límite de alerta de la CPU.
        using (var limitPen = new Pen(Color.FromArgb(150, Theme.Hot), 1) { DashStyle = DashStyle.Dash })
        {
            float ly = Y(CpuLimit);
            g.DrawLine(limitPen, plot.Left, ly, plot.Right, ly);
            g.DrawString($"Límite CPU {CpuLimit}°", small, muted, plot.Left + 4, ly - 15);
        }

        DrawSeries(g, _cpu, Theme.Cpu, X, Y);
        DrawSeries(g, _gpu, Theme.Gpu, X, Y);

        g.DrawString("hace 10 min", small, muted, plot.Left, plot.Bottom + 6);
        var nowSize = g.MeasureString("ahora", small);
        g.DrawString("ahora", small, muted, plot.Right - nowSize.Width, plot.Bottom + 6);

        // Leyenda
        using var legendFont = new Font(Font.FontFamily, 9f, FontStyle.Bold);
        float lx = plot.Left;
        foreach (var (label, color) in new[] { ("CPU", Theme.Cpu), ("GPU", Theme.Gpu) })
        {
            using var b = new SolidBrush(color);
            g.FillRectangle(b, lx, 10, 14, 4);
            g.DrawString(label, legendFont, b, lx + 18, 4);
            lx += 70;
        }
    }

    void DrawSeries(Graphics g, float?[] data, Color color, Func<int, float> x, Func<float, float> y)
    {
        using var pen = new Pen(color, 2f) { LineJoin = LineJoin.Round };
        var pts = new List<PointF>();
        for (int i = 0; i < _count; i++)
        {
            var v = At(data, i);
            if (v is null)
            {
                if (pts.Count > 1) g.DrawLines(pen, pts.ToArray());
                pts.Clear();
                continue;
            }
            pts.Add(new PointF(x(i), y(v.Value)));
        }
        if (pts.Count > 1) g.DrawLines(pen, pts.ToArray());
    }
}

// Dibuja el icono junto al reloj de Windows con la temperatura de la CPU.
public static class TrayIconRenderer
{
    [DllImport("user32.dll")]
    static extern bool DestroyIcon(IntPtr handle);

    public static Icon Render(float? temp, Color color)
    {
        int size = Math.Max(16, SystemInformation.SmallIconSize.Width);
        using var bmp = new Bitmap(size, size);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            using var bg = new SolidBrush(color);
            using var path = RoundRect(new Rectangle(0, 0, size - 1, size - 1), size / 4);
            g.FillPath(bg, path);

            string text = temp is null ? "?" : Math.Min(99, (int)Math.Round(temp.Value)).ToString();
            using var font = new Font("Segoe UI", size * 0.52f, FontStyle.Bold, GraphicsUnit.Pixel);
            using var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(text, font, Brushes.White, new RectangleF(0, 1, size, size), fmt);
        }
        IntPtr h = bmp.GetHicon();
        try
        {
            using var tmp = Icon.FromHandle(h);
            return (Icon)tmp.Clone();
        }
        finally
        {
            DestroyIcon(h);
        }
    }

    static GraphicsPath RoundRect(Rectangle r, int radius)
    {
        var p = new GraphicsPath();
        int d = radius * 2;
        p.AddArc(r.X, r.Y, d, d, 180, 90);
        p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        p.CloseFigure();
        return p;
    }
}
