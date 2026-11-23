using System.Drawing.Drawing2D;

namespace ControlTemp;

// Ventana para editar la curva temperatura → % de ventilador.
public sealed class CurveDialog : Form
{
    const int Points = 5;
    readonly NumericUpDown[] _temps = new NumericUpDown[Points];
    readonly NumericUpDown[] _percents = new NumericUpDown[Points];
    readonly Panel _preview;

    public List<CurvePoint> Result { get; private set; } = new();

    public CurveDialog(string fanName, List<CurvePoint> curve)
    {
        Text = $"Curva de {fanName}";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        Font = new Font("Segoe UI", 9.75f);
        BackColor = Theme.Background;
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(560, 380);

        var help = new Label
        {
            Text = "Elige qué potencia (%) tendrá el ventilador a cada temperatura. Entre dos puntos, la potencia sube de forma gradual.",
            Location = new Point(16, 12),
            Size = new Size(528, 40),
            ForeColor = Theme.Muted,
        };
        Controls.Add(help);

        Controls.Add(new Label { Text = "Temperatura", Location = new Point(16, 58), AutoSize = true, Font = new Font(Font, FontStyle.Bold) });
        Controls.Add(new Label { Text = "Potencia", Location = new Point(126, 58), AutoSize = true, Font = new Font(Font, FontStyle.Bold) });

        var src = curve.OrderBy(p => p.Temp).ToList();
        while (src.Count < Points) src = FanSetting.DefaultCurve();

        _preview = new DoubleBufferedPanel { Location = new Point(240, 58), Size = new Size(304, 240), BackColor = Theme.Surface };
        _preview.Paint += DrawPreview;

        for (int i = 0; i < Points; i++)
        {
            int y = 86 + i * 40;
            _temps[i] = new NumericUpDown { Minimum = 20, Maximum = 105, Value = Math.Clamp(src[i].Temp, 20, 105), Location = new Point(16, y), Width = 70 };
            _percents[i] = new NumericUpDown { Minimum = 0, Maximum = 100, Value = Math.Clamp(src[i].Percent, 0, 100), Location = new Point(126, y), Width = 70 };
            Controls.Add(_temps[i]);
            Controls.Add(new Label { Text = "°C", Location = new Point(90, y + 3), AutoSize = true });
            Controls.Add(_percents[i]);
            Controls.Add(new Label { Text = "%", Location = new Point(200, y + 3), AutoSize = true });
            _temps[i].ValueChanged += (_, _) => _preview.Invalidate();
            _percents[i].ValueChanged += (_, _) => _preview.Invalidate();
        }

        Controls.Add(_preview);

        var reset = new Button { Text = "Restablecer", Location = new Point(16, 330), Size = new Size(110, 32) };
        reset.Click += (_, _) =>
        {
            var d = FanSetting.DefaultCurve();
            for (int i = 0; i < Points; i++) { _temps[i].Value = d[i].Temp; _percents[i].Value = d[i].Percent; }
        };
        var ok = new Button { Text = "Guardar", Location = new Point(340, 330), Size = new Size(100, 32), DialogResult = DialogResult.OK };
        var cancel = new Button { Text = "Cancelar", Location = new Point(444, 330), Size = new Size(100, 32), DialogResult = DialogResult.Cancel };
        ok.Click += (_, _) => Result = Current();
        Controls.AddRange(new Control[] { reset, ok, cancel });
        AcceptButton = ok;
        CancelButton = cancel;
    }

    List<CurvePoint> Current() =>
        Enumerable.Range(0, Points)
            .Select(i => new CurvePoint { Temp = (int)_temps[i].Value, Percent = (int)_percents[i].Value })
            .OrderBy(p => p.Temp)
            .ToList();

    void DrawPreview(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var r = new Rectangle(34, 10, _preview.Width - 46, _preview.Height - 36);
        using var gridPen = new Pen(Theme.Grid);
        using var small = new Font(Font.FontFamily, 7.5f);
        using var muted = new SolidBrush(Theme.Muted);

        float X(float t) => r.Left + (t - 20) / 85f * r.Width;
        float Y(float p) => r.Bottom - p / 100f * r.Height;

        for (int p = 0; p <= 100; p += 25)
        {
            g.DrawLine(gridPen, r.Left, Y(p), r.Right, Y(p));
            g.DrawString($"{p}%", small, muted, 2, Y(p) - 6);
        }
        for (int t = 20; t <= 100; t += 20)
            g.DrawString($"{t}°", small, muted, X(t) - 8, r.Bottom + 4);

        var pts = Current();
        var line = new List<PointF> { new(r.Left, Y(pts[0].Percent)) };
        line.AddRange(pts.Select(p => new PointF(X(p.Temp), Y(p.Percent))));
        line.Add(new PointF(r.Right, Y(pts[^1].Percent)));
        using var pen = new Pen(Theme.Cpu, 2.5f);
        g.DrawLines(pen, line.ToArray());
        using var dot = new SolidBrush(Theme.Cpu);
        foreach (var p in pts) g.FillEllipse(dot, X(p.Temp) - 4, Y(p.Percent) - 4, 8, 8);
    }

    sealed class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel() { DoubleBuffered = true; ResizeRedraw = true; }
    }
}
