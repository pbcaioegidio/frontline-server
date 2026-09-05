namespace FLConfig;

internal sealed class MainForm : Form
{
    private static readonly Color Bg = Color.FromArgb(18, 20, 24);
    private static readonly Color PanelBg = Color.FromArgb(28, 32, 38);
    private static readonly Color Line = Color.FromArgb(48, 54, 64);
    private static readonly Color TextMain = Color.FromArgb(230, 234, 240);
    private static readonly Color Muted = Color.FromArgb(140, 150, 165);
    private static readonly Color Accent = Color.FromArgb(46, 140, 210);
    private static readonly Color AccentDark = Color.FromArgb(28, 90, 140);

    private readonly EnvSettingsStore _store;
    private ComboBox _cmbRes = null!;
    private ComboBox _cmbHz = null!;
    private ComboBox _cmbFpsMode = null!;
    private NumericUpDown _numFps = null!;
    private CheckBox _chkVSync = null!;
    private RadioButton _rbFull = null!, _rbBorderless = null!, _rbWindow = null!;
    private RadioButton _rbMin = null!, _rbLow = null!, _rbMid = null!, _rbHigh = null!, _rbMax = null!, _rbManual = null!;
    private CheckBox _chkNormal = null!, _chkDynLight = null!, _chkBullet = null!, _chkTri = null!, _chkTerrain = null!, _chkPhysX = null!;
    private ComboBox _cmbDx = null!;
    private CheckBox _chkHdr = null!, _chkRim = null!, _chkIbl = null!, _chkSsao = null!, _chkSsr = null!;
    private ComboBox _cmbTex = null!, _cmbShadow = null!, _cmbSpec = null!, _cmbAa = null!;
    private bool _suppressPreset;

    public MainForm()
    {
        _store = new EnvSettingsStore(FindGameRoot());
        _store.Load();

        Text = "FRONTLINE - Configurações";
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Icon.ico");
        if (File.Exists(iconPath))
            Icon = new Icon(iconPath);

        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        ClientSize = new Size(780, 700);
        MinimumSize = new Size(780, 700);
        BackColor = Bg;
        ForeColor = TextMain;
        Font = new Font("Segoe UI", 9.25f);

        BuildUi();
        LoadFromStore();
    }

    private static string FindGameRoot()
    {
        string baseDir = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        foreach (var candidate in new[] { baseDir, Path.GetFullPath(Path.Combine(baseDir, "..")) })
        {
            if (File.Exists(Path.Combine(candidate, "FrontLine.exe"))
                || File.Exists(Path.Combine(candidate, "PointBlank.exe"))
                || File.Exists(Path.Combine(candidate, "FLLauncher.exe"))
                || File.Exists(Path.Combine(candidate, "EnvSet", "env_settings.ini")))
                return candidate;
        }
        return baseDir;
    }

    private void BuildUi()
    {
        var header = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = PanelBg };
        header.Controls.Add(new Label
        {
            Text = "FRONTLINE",
            Font = new Font("Segoe UI Semibold", 16f, FontStyle.Bold),
            ForeColor = Accent,
            AutoSize = true,
            Location = new Point(18, 6),
            BackColor = Color.Transparent
        });
        header.Controls.Add(new Label
        {
            Text = "Configurações de vídeo",
            ForeColor = Muted,
            AutoSize = true,
            Location = new Point(20, 34),
            BackColor = Color.Transparent
        });
        Controls.Add(header);

        var footer = new Panel { Dock = DockStyle.Bottom, Height = 54, BackColor = PanelBg };
        var btnSave = Btn("Salvar", true);
        btnSave.Location = new Point(560, 11);
        btnSave.Click += (_, _) => DoSave();
        var btnBack = Btn("Voltar", false);
        btnBack.Location = new Point(668, 11);
        btnBack.Click += (_, _) => Close();
        footer.Controls.Add(btnSave);
        footer.Controls.Add(btnBack);
        Controls.Add(footer);

        var scroll = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = false,
            Padding = new Padding(0, 10, 0, 8),
            BackColor = Bg
        };
        Controls.Add(scroll);
        scroll.BringToFront();
        header.SendToBack();

        const int cardW = 680;
        const int sidePad = 20;
        var cards = new List<Panel>();
        int y = 4;

        void CenterCards()
        {
            int viewW = scroll.ClientSize.Width;
            if (scroll.VerticalScroll.Visible)
                viewW -= SystemInformation.VerticalScrollBarWidth;
            int x = Math.Max(12, (viewW - cardW) / 2);
            foreach (var c in cards)
                c.Left = x;
        }

        // ---- GRÁFICOS ----
        var g = BeginCard(scroll, ref y, "GRÁFICOS", cardW);
        cards.Add(g);
        int gy = 36;

        g.Controls.Add(Lbl("Resolução", sidePad, gy + 4));
        _cmbRes = Cmb(220);
        _cmbRes.Location = new Point(110, gy);
        _cmbRes.SelectedIndexChanged += (_, _) =>
        {
            RefillHzForSelectedResolution(keepSelection: true);
            MarkManual();
        };
        FillResolutions(_cmbRes);
        g.Controls.Add(_cmbRes);

        g.Controls.Add(Lbl("Taxa de varredura", 340, gy + 4));
        _cmbHz = Cmb(100);
        _cmbHz.Location = new Point(470, gy);
        _cmbHz.SelectedIndexChanged += (_, _) => MarkManual();
        g.Controls.Add(_cmbHz);
        gy += 36;

        g.Controls.Add(Lbl("FPS", sidePad, gy + 4));
        _cmbFpsMode = Cmb(110);
        _cmbFpsMode.Location = new Point(110, gy);
        _cmbFpsMode.Items.AddRange(new object[] { "AUTO", "Manual" });
        _cmbFpsMode.SelectedIndexChanged += (_, _) =>
        {
            _numFps.Enabled = _cmbFpsMode.SelectedIndex == 1;
            MarkManual();
        };
        g.Controls.Add(_cmbFpsMode);
        _numFps = new NumericUpDown
        {
            Location = new Point(232, gy),
            Width = 70,
            Minimum = 30,
            Maximum = 360,
            Value = 60,
            BackColor = Bg,
            ForeColor = TextMain
        };
        _numFps.ValueChanged += (_, _) => MarkManual();
        g.Controls.Add(_numFps);
        gy += 36;

        _chkVSync = AddCheck(g, ref gy, "Sincronização vertical (V-Sync)");
        g.Controls.Add(Lbl("Tela", sidePad, gy));
        gy += 22;
        _rbFull = Rad("Tela cheia");
        _rbBorderless = Rad("Tela cheia em janela");
        _rbWindow = Rad("Em janela");
        PlaceRadios(g, sidePad, gy, _rbFull, _rbBorderless, _rbWindow);
        foreach (var r in new[] { _rbFull, _rbBorderless, _rbWindow })
            r.CheckedChanged += (_, _) => { if (r.Checked) MarkManual(); };
        gy += 34;
        EndCard(g, ref y, gy);

        y += 10;

        // ---- AVANÇADAS ----
        var a = BeginCard(scroll, ref y, "OPÇÕES AVANÇADAS", cardW);
        cards.Add(a);
        int ay = 36;
        a.Controls.Add(Lbl("Preset de vídeo", sidePad, ay));
        ay += 22;
        _rbMin = Rad("Mínimo");
        _rbLow = Rad("Baixo");
        _rbMid = Rad("Médio");
        _rbHigh = Rad("Alto");
        _rbMax = Rad("Máximo");
        _rbManual = Rad("Manual");
        PlaceRadios(a, sidePad, ay, _rbMin, _rbLow, _rbMid, _rbHigh, _rbMax, _rbManual);
        foreach (var r in new[] { _rbMin, _rbLow, _rbMid, _rbHigh, _rbMax, _rbManual })
            r.CheckedChanged += PresetChanged;
        ay += 34;

        int leftCol = ay;
        _chkNormal = AddCheckAt(a, sidePad, ref leftCol, "Mapeamento normal");
        _chkDynLight = AddCheckAt(a, sidePad, ref leftCol, "Luz dinâmica");
        _chkBullet = AddCheckAt(a, sidePad, ref leftCol, "Rastro de bala");
        _chkTri = AddCheckAt(a, sidePad, ref leftCol, "Filtro tri-linear");
        _chkTerrain = AddCheckAt(a, sidePad, ref leftCol, "Efeito de terreno");
        _chkPhysX = AddCheckAt(a, sidePad, ref leftCol, "PhysX");

        int rightColX = 360;
        int right = ay;
        a.Controls.Add(Lbl("DirectX", rightColX, right));
        right += 18;
        _cmbDx = Cmb(180);
        _cmbDx.Location = new Point(rightColX, right);
        _cmbDx.Items.AddRange(new object[] { "DX9", "DX11 (Beta)" });
        _cmbDx.SelectedIndexChanged += (_, _) =>
        {
            MarkManual();
            UpdateDx11Enabled();
        };
        a.Controls.Add(_cmbDx);
        right += 32;
        _chkHdr = AddCheckAt(a, rightColX, ref right, "HDR e Bloom");
        _chkRim = AddCheckAt(a, rightColX, ref right, "Luz de borda");
        _chkIbl = AddCheckAt(a, rightColX, ref right, "Luz baseada em imagem");
        _chkSsao = AddCheckAt(a, rightColX, ref right, "SSAO");
        _chkSsr = AddCheckAt(a, rightColX, ref right, "Reflexo de tela");

        int by = Math.Max(leftCol, right) + 14;
        // 4 selects com margem igual esquerda/direita
        int comboW = 145;
        int inner = cardW - sidePad * 2;
        int gap = (inner - comboW * 4) / 3;
        int x0 = sidePad;
        int x1 = x0 + comboW + gap;
        int x2 = x1 + comboW + gap;
        int x3 = x2 + comboW + gap;

        a.Controls.Add(Lbl("Textura", x0, by));
        a.Controls.Add(Lbl("Sombra", x1, by));
        a.Controls.Add(Lbl("Brilho", x2, by));
        a.Controls.Add(Lbl("Anti-aliasing", x3, by));
        by += 20;

        _cmbTex = Cmb(comboW);
        _cmbTex.Location = new Point(x0, by);
        _cmbTex.Items.AddRange(new object[] { "Alto", "Médio", "Baixo", "Mínimo" });
        a.Controls.Add(_cmbTex);

        _cmbShadow = Cmb(comboW);
        _cmbShadow.Location = new Point(x1, by);
        _cmbShadow.Items.AddRange(new object[] { "Nada", "Baixo", "Médio", "Alto" });
        a.Controls.Add(_cmbShadow);

        _cmbSpec = Cmb(comboW);
        _cmbSpec.Location = new Point(x2, by);
        _cmbSpec.Items.AddRange(new object[] { "Baixo", "Médio", "Alto" });
        a.Controls.Add(_cmbSpec);

        _cmbAa = Cmb(comboW);
        _cmbAa.Location = new Point(x3, by);
        _cmbAa.Items.AddRange(new object[] { "Nada", "MSAA x2", "MSAA x4", "FXAA" });
        a.Controls.Add(_cmbAa);
        by += 40;

        foreach (var cb in new[] { _cmbTex, _cmbShadow, _cmbSpec, _cmbAa })
            cb.SelectedIndexChanged += (_, _) => MarkManual();
        foreach (var c in new[] { _chkNormal, _chkDynLight, _chkBullet, _chkTri, _chkTerrain, _chkPhysX, _chkHdr, _chkRim, _chkIbl, _chkSsao, _chkSsr })
        {
            c.CheckedChanged += (_, _) => MarkManual();
            c.BringToFront();
        }

        EndCard(a, ref y, by);
        scroll.Resize += (_, _) => CenterCards();
        CenterCards();
        UpdateDx11Enabled();
    }

    private void UpdateDx11Enabled()
    {
        bool dx11 = _cmbDx.SelectedIndex == 1;
        foreach (var c in new[] { _chkHdr, _chkRim, _chkIbl, _chkSsao, _chkSsr })
        {
            c.Enabled = dx11;
            c.ForeColor = dx11 ? TextMain : Muted;
        }
        // Normal map e demais opções de graphics ficam sempre ativas
        foreach (var c in new[] { _chkNormal, _chkDynLight, _chkBullet, _chkTri, _chkTerrain, _chkPhysX })
        {
            c.Enabled = true;
            c.ForeColor = TextMain;
        }
    }

    private Panel BeginCard(Control parent, ref int y, string title, int width)
    {
        var card = new Panel
        {
            Location = new Point(0, y),
            Size = new Size(width, 80),
            BackColor = PanelBg
        };
        card.Paint += (_, e) =>
        {
            using var pen = new Pen(Line);
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            using var accent = new Pen(Accent, 2);
            e.Graphics.DrawLine(accent, 0, 0, 0, card.Height);
        };
        card.Controls.Add(new Label
        {
            Text = title,
            Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold),
            ForeColor = Accent,
            Location = new Point(14, 10),
            AutoSize = true,
            BackColor = Color.Transparent
        });
        parent.Controls.Add(card);
        return card;
    }

    private static void EndCard(Panel card, ref int y, int contentBottom)
    {
        card.Height = contentBottom + 14;
        y = card.Bottom;
    }

    private static Label Lbl(string t, int x, int y) => new()
    {
        Text = t,
        ForeColor = Muted,
        Location = new Point(x, y),
        AutoSize = true,
        BackColor = Color.Transparent
    };

    private ComboBox Cmb(int w) => new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Width = w,
        FlatStyle = FlatStyle.Flat,
        BackColor = Bg,
        ForeColor = TextMain
    };

    private RadioButton Rad(string t) => new()
    {
        Text = t,
        AutoSize = true,
        ForeColor = TextMain,
        BackColor = Color.Transparent
    };

    private CheckBox Chk(string t)
    {
        var c = new CheckBox
        {
            Text = t,
            AutoSize = true,
            ForeColor = TextMain,
            BackColor = Color.Transparent,
            FlatStyle = FlatStyle.Standard,
            UseVisualStyleBackColor = false,
            Cursor = Cursors.Hand,
            Enabled = true
        };
        return c;
    }

    private Button Btn(string t, bool primary)
    {
        var b = new Button
        {
            Text = t,
            Size = new Size(96, 32),
            FlatStyle = FlatStyle.Flat,
            ForeColor = TextMain,
            BackColor = primary ? AccentDark : Color.FromArgb(40, 44, 52),
            Cursor = Cursors.Hand
        };
        b.FlatAppearance.BorderColor = primary ? Accent : Line;
        return b;
    }

    private CheckBox AddCheck(Panel p, ref int y, string text)
    {
        var c = Chk(text);
        c.Location = new Point(20, y);
        p.Controls.Add(c);
        y += 26;
        return c;
    }

    private CheckBox AddCheckAt(Panel p, int x, ref int y, string text)
    {
        var c = Chk(text);
        c.Location = new Point(x, y);
        p.Controls.Add(c);
        y += 26;
        return c;
    }

    private static void PlaceRadios(Control parent, int x, int y, params RadioButton[] radios)
    {
        int cx = x;
        foreach (var r in radios)
        {
            r.Location = new Point(cx, y);
            parent.Controls.Add(r);
            cx += TextRenderer.MeasureText(r.Text, r.Font).Width + 28;
        }
    }

    private void FillResolutions(ComboBox cmb)
    {
        foreach (var (w, h) in DisplayModes.GetResolutions())
            cmb.Items.Add($"{w} x {h}");
    }

    private bool TryParseSelectedResolution(out int w, out int h)
    {
        w = 0; h = 0;
        string? res = _cmbRes.SelectedItem?.ToString();
        if (string.IsNullOrWhiteSpace(res))
            return false;
        var parts = res.Split('x', StringSplitOptions.TrimEntries);
        return parts.Length == 2 && int.TryParse(parts[0], out w) && int.TryParse(parts[1], out h);
    }

    private void RefillHzForSelectedResolution(bool keepSelection, int? preferHz = null)
    {
        int? current = preferHz;
        if (keepSelection && _cmbHz.SelectedItem is int selected)
            current = selected;

        _cmbHz.Items.Clear();

        IReadOnlyList<int> rates;
        if (TryParseSelectedResolution(out int w, out int h))
            rates = DisplayModes.GetRefreshRates(w, h);
        else
            rates = DisplayModes.GetRefreshRates();

        foreach (var hz in rates)
            _cmbHz.Items.Add(hz);

        if (current is int want)
        {
            if (!_cmbHz.Items.Contains(want))
                _cmbHz.Items.Insert(0, want);
            _cmbHz.SelectedItem = want;
        }
        else if (_cmbHz.Items.Count > 0)
        {
            _cmbHz.SelectedIndex = 0;
        }
    }

    private void LoadFromStore()
    {
        _suppressPreset = true;
        int w = _store.GetInt("Graphics", "ScreenWidth", 1920);
        int h = _store.GetInt("Graphics", "ScreenHeight", 1080);
        string res = $"{w} x {h}";
        if (!_cmbRes.Items.Contains(res))
            _cmbRes.Items.Insert(0, res);
        _cmbRes.SelectedItem = res;

        int hz = _store.GetInt("Graphics", "RefreshRate", 60);
        RefillHzForSelectedResolution(keepSelection: false, preferHz: hz);

        _cmbFpsMode.SelectedIndex = _store.GetInt("Graphics", "FPSType", 0) == 0 ? 0 : 1;
        _numFps.Value = Math.Clamp(_store.GetInt("Graphics", "FPSVal", 60), 30, 360);
        _numFps.Enabled = _cmbFpsMode.SelectedIndex == 1;
        _chkVSync.Checked = _store.GetBool("Graphics", "VSync", false);

        // i3/PB: 0=Janela, 1=Tela cheia em janela, 2=Tela cheia
        switch (_store.GetInt("Graphics", "ScreenMode", 0))
        {
            case 2: _rbFull.Checked = true; break;
            case 1: _rbBorderless.Checked = true; break;
            default: _rbWindow.Checked = true; break;
        }

        _chkNormal.Checked = _store.GetBool("Graphics", "EnableNormalMap", false);
        _chkDynLight.Checked = _store.GetBool("Graphics", "DynamicLight", false);
        _chkBullet.Checked = _store.GetBool("Game", "EnableBulletTrace", true);
        _chkTri.Checked = _store.GetBool("Graphics", "TriLinearFilter", true);
        _chkTerrain.Checked = _store.GetBool("Graphics", "EnableTerrainEffect", true);
        _chkPhysX.Checked = _store.GetBool("Game", "EnablePhysX", false);

        _cmbDx.SelectedIndex = _store.GetInt("Default", "DXVersion", 11) >= 11 ? 1 : 0;
        _chkHdr.Checked = _store.GetBool("DX11", "HDR", false);
        _chkRim.Checked = _store.GetBool("DX11", "RimLight", false);
        _chkIbl.Checked = _store.GetBool("DX11", "IBL", false);
        _chkSsao.Checked = _store.GetBool("DX11", "SSAO", false);
        _chkSsr.Checked = _store.GetBool("DX11", "SSR", false);

        _cmbTex.SelectedIndex = Math.Clamp(_store.GetInt("Graphics", "TextureQualityType", 2), 0, 3);
        _cmbShadow.SelectedIndex = Math.Clamp(_store.GetInt("Graphics", "ShadowQualityType", 0), 0, 3);
        _cmbSpec.SelectedIndex = Math.Clamp(_store.GetInt("Graphics", "SpecularQualityType", 0), 0, 2);
        _cmbAa.SelectedIndex = Math.Clamp(_store.GetInt("Graphics", "AntiAlias", 0), 0, 3);
        _rbManual.Checked = true;
        _suppressPreset = false;
    }

    private void MarkManual()
    {
        if (!_suppressPreset)
            _rbManual.Checked = true;
    }

    private void PresetChanged(object? sender, EventArgs e)
    {
        if (_suppressPreset || sender is not RadioButton { Checked: true } rb || ReferenceEquals(rb, _rbManual))
            return;
        _suppressPreset = true;
        if (ReferenceEquals(rb, _rbMin))
            ApplyPreset(3, 0, 0, 0, false, false, false, false, false, false, false, false, false, false);
        else if (ReferenceEquals(rb, _rbLow))
            ApplyPreset(2, 0, 0, 0, false, false, true, true, false, false, false, false, false, true);
        else if (ReferenceEquals(rb, _rbMid))
            ApplyPreset(1, 1, 1, 1, true, true, true, true, false, false, false, false, false, true);
        else if (ReferenceEquals(rb, _rbHigh))
            ApplyPreset(0, 2, 2, 2, true, true, true, true, true, true, false, true, false, true);
        else if (ReferenceEquals(rb, _rbMax))
            ApplyPreset(0, 3, 2, 3, true, true, true, true, true, true, true, true, true, true);
        _suppressPreset = false;
    }

    private void ApplyPreset(int texture, int shadow, int spec, int aa, bool normal, bool dyn, bool tri, bool terrain,
        bool hdr, bool rim, bool ibl, bool ssao, bool ssr, bool dx11)
    {
        _cmbTex.SelectedIndex = texture;
        _cmbShadow.SelectedIndex = shadow;
        _cmbSpec.SelectedIndex = spec;
        _cmbAa.SelectedIndex = aa;
        _chkNormal.Checked = normal;
        _chkDynLight.Checked = dyn;
        _chkTri.Checked = tri;
        _chkTerrain.Checked = terrain;
        _chkHdr.Checked = hdr;
        _chkRim.Checked = rim;
        _chkIbl.Checked = ibl;
        _chkSsao.Checked = ssao;
        _chkSsr.Checked = ssr;
        _cmbDx.SelectedIndex = dx11 ? 1 : 0;
    }

    private void DoSave()
    {
        try
        {
            string res = _cmbRes.SelectedItem?.ToString() ?? "1920 x 1080";
            var parts = res.Split('x', StringSplitOptions.TrimEntries);
            int w = int.Parse(parts[0]);
            int h = int.Parse(parts[1]);
            int hz = _cmbHz.SelectedItem is int hzInt ? hzInt : 60;

            _store.SetInt("Graphics", "ScreenWidth", w);
            _store.SetInt("Graphics", "ScreenHeight", h);
            _store.SetInt("Graphics", "VideoResolutionWidth", w);
            _store.SetInt("Graphics", "VideoResolutionHeight", h);
            _store.SetInt("Graphics", "RefreshRate", hz);
            _store.SetInt("Graphics", "FPSType", _cmbFpsMode.SelectedIndex == 0 ? 0 : 1);
            _store.SetInt("Graphics", "FPSVal", (int)_numFps.Value);
            _store.SetBool("Graphics", "VSync", _chkVSync.Checked);
            // i3/PB: 0=Janela, 1=Tela cheia em janela, 2=Tela cheia
            _store.SetInt("Graphics", "ScreenMode", _rbFull.Checked ? 2 : _rbBorderless.Checked ? 1 : 0);

            _store.SetBool("Graphics", "EnableNormalMap", _chkNormal.Checked);
            _store.SetBool("Graphics", "DynamicLight", _chkDynLight.Checked);
            _store.SetBool("Game", "EnableBulletTrace", _chkBullet.Checked);
            _store.SetBool("Graphics", "TriLinearFilter", _chkTri.Checked);
            _store.SetBool("Graphics", "EnableTerrainEffect", _chkTerrain.Checked);
            _store.SetBool("Game", "EnablePhysX", _chkPhysX.Checked);

            _store.SetInt("Default", "DXVersion", _cmbDx.SelectedIndex == 1 ? 11 : 9);
            _store.SetBool("DX11", "HDR", _chkHdr.Checked);
            _store.SetBool("DX11", "RimLight", _chkRim.Checked);
            _store.SetBool("DX11", "IBL", _chkIbl.Checked);
            _store.SetBool("DX11", "SSAO", _chkSsao.Checked);
            _store.SetBool("DX11", "SSR", _chkSsr.Checked);

            _store.SetInt("Graphics", "TextureQualityType", _cmbTex.SelectedIndex);
            _store.SetInt("Graphics", "ShadowQualityType", _cmbShadow.SelectedIndex);
            _store.SetInt("Graphics", "SpecularQualityType", _cmbSpec.SelectedIndex);
            _store.SetInt("Graphics", "AntiAlias", _cmbAa.SelectedIndex);

            _store.Save();
            MessageBox.Show("Configurações salvas.", "FRONTLINE", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Falha ao salvar: " + ex.Message, "FRONTLINE", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
