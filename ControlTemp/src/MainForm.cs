using System.Diagnostics;
using System.Media;
using Microsoft.Win32;

namespace ControlTemp;

public sealed class MainForm : Form
{
    const string PawnIoUrl = "https://pawnio.eu/";

    readonly AppConfig _cfg = AppConfig.Load();
    readonly HardwareService _hw;
    readonly bool _startHidden;
    bool _exiting;
    bool _trayHintShown;
    bool _shownOnce;
    Snapshot? _last;

    // Pestaña Temperaturas
    readonly TempCard _cpuCard = new("PROCESADOR (CPU)");
    readonly TempCard _gpuCard = new("TARJETA GRÁFICA (GPU)");
    readonly TempCard _diskCard = new("DISCO MÁS CALIENTE");
    readonly TempChart _chart = new();
    readonly ListView _tempList = MakeList(("Sensor", 260), ("Actual", 90), ("Mínimo", 90), ("Máximo", 90));

    // Pestaña Ventiladores
    readonly ListView _fanList = MakeList(("Ventilador", 260), ("Velocidad", 110), ("Mínimo", 100), ("Máximo", 100));
    readonly DataGridView _grid = new();
    readonly Label _noControlLabel = new();
    readonly Label _emergencyLabel = new();
    bool _populatingGrid;

    // Pestaña Alertas
    readonly ListBox _alertLog = new();
    readonly CheckBox _startup = new();
    readonly Dictionary<string, DateTime> _lastAlert = new();

    // Otros
    readonly Panel _banner = new();
    readonly Label _bannerText = new();
    readonly ToolStripStatusLabel _status = new();
    readonly NotifyIcon _tray = new();
    Icon? _trayIcon;

    public MainForm(bool startHidden)
    {
        _startHidden = startHidden;
        Text = "Control de Temperaturas";
        Font = new Font("Segoe UI", 9.75f);
        BackColor = Theme.Background;
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(1000, 720);
        MinimumSize = new Size(820, 600);
        StartPosition = FormStartPosition.CenterScreen;
        try
        {
            using var s = typeof(MainForm).Assembly.GetManifestResourceStream("app.ico");
            if (s is not null) Icon = new Icon(s);
        }
        catch { }

        BuildUi();
        BuildTray();
        CheckPawnIo();

        _ = Handle; // Crea la ventana ya, para poder recibir datos aunque empiece oculta.
        _hw = new HardwareService(_cfg);
        _hw.Updated += snap =>
        {
            if (IsDisposed || !IsHandleCreated) return;
            try { BeginInvoke(() => OnSnapshot(snap)); } catch (InvalidOperationException) { }
        };
        _hw.Start();

        SystemEvents.SessionEnding += (_, _) => _hw.Dispose();
    }

    protected override void SetVisibleCore(bool value)
    {
        // Si se inició con Windows, la app arranca solo como icono junto al reloj.
        if (_startHidden && !_shownOnce)
        {
            _shownOnce = true;
            value = false;
        }
        base.SetVisibleCore(value);
    }

    // ---------------------------------------------------------------- Interfaz

    void BuildUi()
    {
        // Aviso superior (PawnIO)
        _banner.Dock = DockStyle.Top;
        _banner.Height = 44;
        _banner.BackColor = Color.FromArgb(255, 243, 205);
        _banner.Padding = new Padding(12, 0, 12, 0);
        _banner.Visible = false;
        _bannerText.Dock = DockStyle.Fill;
        _bannerText.TextAlign = ContentAlignment.MiddleLeft;
        _bannerText.ForeColor = Color.FromArgb(110, 75, 0);
        var bannerBtn = new Button { Text = "Descargar PawnIO", Dock = DockStyle.Right, Width = 150, FlatStyle = FlatStyle.System };
        bannerBtn.Click += (_, _) => OpenUrl(PawnIoUrl);
        _banner.Controls.Add(_bannerText);
        _banner.Controls.Add(bannerBtn);

        var tabs = new TabControl { Dock = DockStyle.Fill, Padding = new Point(16, 6) };
        tabs.TabPages.Add(BuildTempTab());
        tabs.TabPages.Add(BuildFanTab());
        tabs.TabPages.Add(BuildAlertTab());

        var strip = new StatusStrip { SizingGrip = false };
        _status.Text = "Iniciando sensores…";
        strip.Items.Add(_status);

        Controls.Add(tabs);
        Controls.Add(_banner);
        Controls.Add(strip);
    }

    TabPage BuildTempTab()
    {
        var page = new TabPage("Temperaturas") { BackColor = Theme.Background, Padding = new Padding(8) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 130));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));

        var cards = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Margin = Padding.Empty };
        for (int i = 0; i < 3; i++) cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        cards.Controls.Add(_cpuCard, 0, 0);
        cards.Controls.Add(_gpuCard, 1, 0);
        cards.Controls.Add(_diskCard, 2, 0);

        _tempList.Margin = new Padding(6);
        layout.Controls.Add(cards, 0, 0);
        layout.Controls.Add(_chart, 0, 1);
        layout.Controls.Add(_tempList, 0, 2);
        page.Controls.Add(layout);
        return page;
    }

    TabPage BuildFanTab()
    {
        var page = new TabPage("Ventiladores") { BackColor = Theme.Background, Padding = new Padding(8) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6 };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 35));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 65));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        layout.Controls.Add(Heading("Velocidad de los ventiladores"), 0, 0);
        _fanList.Margin = new Padding(6);
        layout.Controls.Add(_fanList, 0, 1);

        layout.Controls.Add(Heading("Control de ventiladores"), 0, 2);

        var gridHost = new Panel { Dock = DockStyle.Fill, Margin = new Padding(6) };
        SetupGrid();
        _noControlLabel.Dock = DockStyle.Fill;
        _noControlLabel.TextAlign = ContentAlignment.MiddleCenter;
        _noControlLabel.ForeColor = Theme.Muted;
        _noControlLabel.BackColor = Theme.Surface;
        _noControlLabel.Text = "Buscando ventiladores que se puedan controlar…";
        gridHost.Controls.Add(_grid);
        gridHost.Controls.Add(_noControlLabel);
        layout.Controls.Add(gridHost, 0, 3);

        _emergencyLabel.AutoSize = true;
        _emergencyLabel.ForeColor = Theme.Hot;
        _emergencyLabel.Font = new Font(Font, FontStyle.Bold);
        _emergencyLabel.Margin = new Padding(8, 4, 8, 0);
        _emergencyLabel.Visible = false;
        layout.Controls.Add(_emergencyLabel, 0, 4);

        var safety = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(4, 6, 4, 0), WrapContents = true };
        safety.Controls.Add(FieldLabel("Temperatura de emergencia (todo al 100 %):"));
        safety.Controls.Add(Number(60, 105, _cfg.EmergencyTemp, v => _cfg.EmergencyTemp = v));
        safety.Controls.Add(FieldLabel("°C      Potencia mínima:"));
        safety.Controls.Add(Number(10, 100, _cfg.MinFanPercent, v => _cfg.MinFanPercent = v));
        safety.Controls.Add(FieldLabel("%"));
        layout.Controls.Add(safety, 0, 5);

        page.Controls.Add(layout);
        return page;
    }

    void SetupGrid()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.BackgroundColor = Theme.Surface;
        _grid.BorderStyle = BorderStyle.None;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AllowUserToResizeRows = false;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.CellSelect;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.RowTemplate.Height = 34;
        _grid.EditMode = DataGridViewEditMode.EditOnEnter;
        _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Ventilador", ReadOnly = true, FillWeight = 190 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Rpm", HeaderText = "Velocidad", ReadOnly = true, FillWeight = 80 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Power", HeaderText = "Potencia", ReadOnly = true, FillWeight = 70 });
        var mode = new DataGridViewComboBoxColumn { Name = "Mode", HeaderText = "Modo", FillWeight = 130, FlatStyle = FlatStyle.Flat };
        mode.Items.AddRange(ModeLabels);
        _grid.Columns.Add(mode);
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Fixed", HeaderText = "% fijo", FillWeight = 60 });
        var source = new DataGridViewComboBoxColumn { Name = "Source", HeaderText = "Curva según", FillWeight = 100, FlatStyle = FlatStyle.Flat };
        source.Items.AddRange(SourceLabels);
        _grid.Columns.Add(source);
        _grid.Columns.Add(new DataGridViewButtonColumn { Name = "Curve", HeaderText = "", Text = "Editar curva…", UseColumnTextForButtonValue = true, FillWeight = 90 });

        _grid.CurrentCellDirtyStateChanged += (_, _) =>
        {
            if (_grid.IsCurrentCellDirty && _grid.CurrentCell is DataGridViewComboBoxCell)
                _grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
        };
        _grid.CellValueChanged += OnGridValueChanged;
        _grid.CellContentClick += OnGridButton;
        _grid.DataError += (_, e) => e.ThrowException = false;
        _grid.Visible = false;
    }

    static readonly string[] ModeLabels = { "Automático (BIOS)", "Fijo", "Curva" };
    static readonly string[] SourceLabels = { "CPU", "GPU", "La más alta" };

    TabPage BuildAlertTab()
    {
        var page = new TabPage("Alertas y opciones") { BackColor = Theme.Background, Padding = new Padding(16) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5 };
        for (int i = 0; i < 4; i++) layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(Heading("Avisarme cuando la temperatura llegue a…"), 0, 0);

        var limits = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(4, 4, 4, 12) };
        limits.Controls.Add(FieldLabel("CPU:"));
        limits.Controls.Add(Number(50, 105, _cfg.CpuAlert, v => { _cfg.CpuAlert = v; _chart.CpuLimit = v; _chart.Invalidate(); }));
        limits.Controls.Add(FieldLabel("°C       GPU:"));
        limits.Controls.Add(Number(50, 105, _cfg.GpuAlert, v => _cfg.GpuAlert = v));
        limits.Controls.Add(FieldLabel("°C       Disco:"));
        limits.Controls.Add(Number(35, 90, _cfg.DiskAlert, v => _cfg.DiskAlert = v));
        limits.Controls.Add(FieldLabel("°C"));
        layout.Controls.Add(limits, 0, 1);
        _chart.CpuLimit = _cfg.CpuAlert;

        var opts = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Margin = new Padding(4, 0, 4, 12) };
        opts.Controls.Add(Check("Mostrar avisos", _cfg.AlertsEnabled, v => _cfg.AlertsEnabled = v));
        opts.Controls.Add(Check("Sonido en los avisos", _cfg.AlertSound, v => _cfg.AlertSound = v));
        _startup.Text = "Iniciar con Windows (queda junto al reloj, sin abrir la ventana)";
        _startup.AutoSize = true;
        _startup.Checked = StartupTask.IsEnabled();
        _startup.CheckedChanged += (_, _) =>
        {
            if (!StartupTask.Set(_startup.Checked))
                MessageBox.Show(this, "No se pudo cambiar el inicio automático.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        };
        opts.Controls.Add(_startup);
        var test = new Button { Text = "Probar aviso", AutoSize = true, Margin = new Padding(3, 8, 3, 3) };
        test.Click += (_, _) => ShowAlert("Prueba", "Así se verán los avisos de temperatura.");
        opts.Controls.Add(test);
        layout.Controls.Add(opts, 0, 2);

        layout.Controls.Add(Heading("Historial de avisos"), 0, 3);
        _alertLog.Dock = DockStyle.Fill;
        _alertLog.BorderStyle = BorderStyle.FixedSingle;
        _alertLog.IntegralHeight = false;
        layout.Controls.Add(_alertLog, 0, 4);

        page.Controls.Add(layout);
        return page;
    }

    void BuildTray()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Abrir", null, (_, _) => ShowWindow());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Salir", null, (_, _) => ExitApp());
        _tray.ContextMenuStrip = menu;
        _tray.Text = "Control de Temperaturas";
        _tray.Icon = Icon ?? SystemIcons.Application;
        _tray.Visible = true;
        _tray.DoubleClick += (_, _) => ShowWindow();
        _tray.BalloonTipClicked += (_, _) => ShowWindow();
    }

    // ---------------------------------------------------------------- Datos

    void OnSnapshot(Snapshot s)
    {
        _last = s;
        _chart.Add(s.Cpu, s.Gpu);
        UpdateTray(s);
        CheckAlerts(s);

        if (!Visible || WindowState == FormWindowState.Minimized) return;

        _cpuCard.SetValue(s.Cpu, s.CpuName, _cfg.CpuAlert);
        _gpuCard.SetValue(s.Gpu, s.GpuName, _cfg.GpuAlert);
        _diskCard.SetValue(s.Disk, s.DiskName, _cfg.DiskAlert);

        UpdateList(_tempList, s.Temps, v => $"{v:0.0} °C");
        UpdateList(_fanList, s.Fans, v => $"{v:0} RPM");
        UpdateGrid(s);

        _emergencyLabel.Visible = s.Emergency;
        _emergencyLabel.Text = "⚠ Temperatura de emergencia: los ventiladores controlados por la app están al 100 %.";

        if (s.Cpu is null && _hw.OpenError is null && !IsPawnIoInstalled())
            _banner.Visible = true;

        _status.Text = _hw.OpenError is not null
            ? $"Error al abrir los sensores: {_hw.OpenError}"
            : $"Actualizado {DateTime.Now:HH:mm:ss} · {s.Temps.Count} sensores de temperatura · {s.Fans.Count} ventiladores";
    }

    static void UpdateList(ListView list, List<SensorReading> readings, Func<float, string> fmt)
    {
        list.BeginUpdate();
        var seen = new HashSet<string>();
        foreach (var r in readings)
        {
            seen.Add(r.Id);
            var group = list.Groups[r.Hardware] ?? list.Groups.Add(r.Hardware, r.Hardware);
            if (list.Items[r.Id] is not ListViewItem item)
            {
                item = new ListViewItem(r.Name) { Name = r.Id, Group = group };
                item.SubItems.Add("");
                item.SubItems.Add("");
                item.SubItems.Add("");
                list.Items.Add(item);
            }
            SetSub(item, 1, r.Value is float v ? fmt(v) : "--");
            SetSub(item, 2, r.Min is float mn ? fmt(mn) : "--");
            SetSub(item, 3, r.Max is float mx ? fmt(mx) : "--");
        }
        for (int i = list.Items.Count - 1; i >= 0; i--)
            if (!seen.Contains(list.Items[i].Name)) list.Items.RemoveAt(i);
        list.EndUpdate();

        static void SetSub(ListViewItem item, int i, string text)
        {
            if (item.SubItems[i].Text != text) item.SubItems[i].Text = text;
        }
    }

    void UpdateGrid(Snapshot s)
    {
        bool any = s.Controls.Count > 0;
        _grid.Visible = any;
        _noControlLabel.Visible = !any;
        if (!any)
        {
            _noControlLabel.Text = IsPawnIoInstalled()
                ? "No se encontraron ventiladores que se puedan controlar desde Windows.\n\n" +
                  "Esto es normal en portátiles y en algunas placas base. En ese caso, ajusta la curva de\n" +
                  "ventiladores en la BIOS o con el programa del fabricante. La app igual te mostrará temperaturas y avisos."
                : "Para controlar ventiladores de la placa base hace falta instalar PawnIO (mira el aviso amarillo arriba).\n" +
                  "Las tarjetas gráficas NVIDIA/AMD compatibles aparecen aquí aunque no esté instalado.";
            return;
        }

        _populatingGrid = true;
        try
        {
            foreach (var c in s.Controls)
            {
                var row = _grid.Rows.Cast<DataGridViewRow>().FirstOrDefault(r => (string?)r.Tag == c.Id);
                if (row is null)
                {
                    var fan = _cfg.GetFan(c.Id);
                    int idx = _grid.Rows.Add($"{c.Name} · {c.Hardware}", "", "",
                        ModeLabels[(int)fan.Mode], fan.FixedPercent.ToString(), SourceLabels[(int)fan.Source]);
                    row = _grid.Rows[idx];
                    row.Tag = c.Id;
                    RefreshRowState(row, fan);
                }
                row.Cells["Rpm"].Value = c.Rpm is float rpm ? $"{rpm:0} RPM" : "--";
                row.Cells["Power"].Value = c.Percent is float pct ? $"{pct:0} %" : "--";
                row.Cells["Power"].Style.ForeColor = c.Managed ? Theme.Cpu : Theme.Muted;
            }
        }
        finally
        {
            _populatingGrid = false;
        }
    }

    static void RefreshRowState(DataGridViewRow row, FanSetting fan)
    {
        var fixedCell = row.Cells["Fixed"];
        fixedCell.ReadOnly = fan.Mode != FanMode.Fixed;
        fixedCell.Style.ForeColor = fan.Mode == FanMode.Fixed ? Theme.Text : Theme.Grid;
        var src = row.Cells["Source"];
        src.ReadOnly = fan.Mode != FanMode.Curve;
        src.Style.ForeColor = fan.Mode == FanMode.Curve ? Theme.Text : Theme.Grid;
    }

    void OnGridValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (_populatingGrid || e.RowIndex < 0) return;
        var row = _grid.Rows[e.RowIndex];
        if (row.Tag is not string id) return;
        var fan = _cfg.GetFan(id);
        string col = _grid.Columns[e.ColumnIndex].Name;
        object? value = row.Cells[e.ColumnIndex].Value;

        switch (col)
        {
            case "Mode":
                var mode = (FanMode)Math.Max(0, Array.IndexOf(ModeLabels, value as string));
                if (mode != FanMode.Auto && !_cfg.FanWarningShown)
                {
                    MessageBox.Show(this,
                        "La app va a controlar este ventilador.\n\n" +
                        $"• Nunca bajará de {_cfg.MinFanPercent} %.\n" +
                        $"• Si la CPU o la GPU llegan a {_cfg.EmergencyTemp} °C, irá al 100 %.\n" +
                        "• Al cerrar la app (Salir), el ventilador vuelve al control automático de la BIOS.\n\n" +
                        "Vigila las temperaturas los primeros minutos después de cambiarlo.",
                        "Control de ventiladores", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _cfg.FanWarningShown = true;
                }
                fan.Mode = mode;
                RefreshRowState(row, fan);
                break;
            case "Fixed":
                if (int.TryParse(value?.ToString()?.Replace("%", "").Trim(), out int pct))
                    fan.FixedPercent = Math.Clamp(pct, _cfg.MinFanPercent, 100);
                _populatingGrid = true;
                row.Cells[e.ColumnIndex].Value = fan.FixedPercent.ToString();
                _populatingGrid = false;
                break;
            case "Source":
                fan.Source = (TempSource)Math.Max(0, Array.IndexOf(SourceLabels, value as string));
                break;
        }
        _cfg.Save();
    }

    void OnGridButton(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || _grid.Columns[e.ColumnIndex].Name != "Curve") return;
        var row = _grid.Rows[e.RowIndex];
        if (row.Tag is not string id) return;
        var fan = _cfg.GetFan(id);
        using var dlg = new CurveDialog(row.Cells["Name"].Value?.ToString() ?? "ventilador", fan.Curve);
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            fan.Curve = dlg.Result;
            _cfg.Save();
        }
    }

    // ---------------------------------------------------------------- Avisos

    void CheckAlerts(Snapshot s)
    {
        if (!_cfg.AlertsEnabled) return;
        Check("CPU", s.Cpu, _cfg.CpuAlert, "El procesador");
        Check("GPU", s.Gpu, _cfg.GpuAlert, "La tarjeta gráfica");
        Check("Disco", s.Disk, _cfg.DiskAlert, "El disco");

        void Check(string key, float? temp, int limit, string who)
        {
            if (temp is null || temp < limit) return;
            if (_lastAlert.TryGetValue(key, out var when) && DateTime.Now - when < TimeSpan.FromMinutes(3)) return;
            _lastAlert[key] = DateTime.Now;
            ShowAlert($"¡{key} a {temp:0} °C!", $"{who} pasó tu límite de {limit} °C.");
        }
    }

    void ShowAlert(string title, string text)
    {
        _tray.ShowBalloonTip(8000, title, text, ToolTipIcon.Warning);
        if (_cfg.AlertSound) SystemSounds.Exclamation.Play();
        _alertLog.Items.Insert(0, $"{DateTime.Now:dd/MM HH:mm:ss}   {title}  {text}");
        while (_alertLog.Items.Count > 200) _alertLog.Items.RemoveAt(_alertLog.Items.Count - 1);
    }

    void UpdateTray(Snapshot s)
    {
        var color = Theme.ForTemp(s.Cpu, _cfg.CpuAlert);
        if (s.Cpu is null && s.Gpu is not null) color = Theme.ForTemp(s.Gpu, _cfg.GpuAlert);
        var old = _trayIcon;
        _trayIcon = TrayIconRenderer.Render(s.Cpu ?? s.Gpu, color);
        _tray.Icon = _trayIcon;
        old?.Dispose();

        string tip = $"CPU {Fmt(s.Cpu)} · GPU {Fmt(s.Gpu)} · Disco {Fmt(s.Disk)}";
        _tray.Text = tip.Length > 120 ? tip[..120] : tip;

        static string Fmt(float? t) => t is null ? "--" : $"{t:0}°C";
    }

    // ---------------------------------------------------------------- Ventana / salida

    void CheckPawnIo()
    {
        if (IsPawnIoInstalled()) return;
        _bannerText.Text = "Falta el controlador gratuito PawnIO. Sin él no se puede leer la CPU ni controlar los ventiladores de la placa base. Instálalo y vuelve a abrir la app.";
        _banner.Visible = true;
    }

    static bool IsPawnIoInstalled()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\PawnIO");
            return key is not null;
        }
        catch { return false; }
    }

    static void OpenUrl(string url)
    {
        try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); } catch { }
    }

    void ShowWindow()
    {
        Show();
        if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
        Activate();
        if (_last is not null) OnSnapshot(_last);
    }

    void ExitApp()
    {
        _exiting = true;
        Close();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // La X esconde la ventana; la app sigue vigilando junto al reloj.
        if (!_exiting && e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
            if (!_trayHintShown)
            {
                _trayHintShown = true;
                _tray.ShowBalloonTip(5000, "Sigo vigilando",
                    "La app sigue funcionando junto al reloj. Para cerrarla del todo: clic derecho en el icono → Salir.",
                    ToolTipIcon.Info);
            }
            return;
        }
        base.OnFormClosing(e);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _cfg.Save();
        _hw.Dispose(); // devuelve los ventiladores a la BIOS
        _tray.Visible = false;
        _tray.Dispose();
        _trayIcon?.Dispose();
        base.OnFormClosed(e);
    }

    // ---------------------------------------------------------------- Ayudantes

    static ListView MakeList(params (string, int)[] cols)
    {
        var list = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            HeaderStyle = ColumnHeaderStyle.Nonclickable,
            BorderStyle = BorderStyle.FixedSingle,
            ShowGroups = true,
        };
        foreach (var (name, width) in cols) list.Columns.Add(name, width);
        typeof(ListView).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(list, true);
        return list;
    }

    Label Heading(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Font = new Font(Font.FontFamily, 10.5f, FontStyle.Bold),
        ForeColor = Theme.Text,
        Margin = new Padding(6, 8, 6, 4),
    };

    static Label FieldLabel(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Margin = new Padding(3, 7, 3, 3),
    };

    NumericUpDown Number(int min, int max, int value, Action<int> onChange)
    {
        var n = new NumericUpDown { Minimum = min, Maximum = max, Value = Math.Clamp(value, min, max), Width = 64 };
        n.ValueChanged += (_, _) => { onChange((int)n.Value); _cfg.Save(); };
        return n;
    }

    CheckBox Check(string text, bool value, Action<bool> onChange)
    {
        var c = new CheckBox { Text = text, AutoSize = true, Checked = value };
        c.CheckedChanged += (_, _) => { onChange(c.Checked); _cfg.Save(); };
        return c;
    }
}
