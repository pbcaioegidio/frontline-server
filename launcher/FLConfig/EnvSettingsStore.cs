using System.Globalization;
using System.Text;

namespace FLConfig;

/// <summary>Lê/grava EnvSet/env_settings.ini e DXVersion.ini (mesmo alvo do PBConfig).</summary>
internal sealed class EnvSettingsStore
{
    private readonly string _gameRoot;
    private readonly string _iniPath;
    private readonly string _dxPath;
    private readonly Dictionary<string, Dictionary<string, string>> _sections = new(StringComparer.OrdinalIgnoreCase);

    public EnvSettingsStore(string gameRoot)
    {
        _gameRoot = gameRoot;
        _iniPath = Path.Combine(gameRoot, "EnvSet", "env_settings.ini");
        _dxPath = Path.Combine(gameRoot, "EnvSet", "DXVersion.ini");
    }

    public string IniPath => _iniPath;

    public void Load()
    {
        _sections.Clear();
        EnsureDefaults();
        if (!File.Exists(_iniPath))
            return;

        string section = "Default";
        foreach (var raw in File.ReadAllLines(_iniPath, Encoding.UTF8))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith(';') || line.StartsWith('#'))
                continue;
            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                section = line[1..^1].Trim();
                if (!_sections.ContainsKey(section))
                    _sections[section] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                continue;
            }

            int eq = line.IndexOf('=');
            if (eq <= 0) continue;
            string key = line[..eq].Trim();
            string val = line[(eq + 1)..].Trim();
            if (!_sections.ContainsKey(section))
                _sections[section] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _sections[section][key] = val;
        }
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_iniPath)!);
        var sb = new StringBuilder();
        foreach (var sectionName in new[] { "Default", "Game", "Graphics", "DX11" })
        {
            if (!_sections.TryGetValue(sectionName, out var map))
                continue;
            sb.Append('[').Append(sectionName).AppendLine("]");
            sb.AppendLine();
            foreach (var kv in map)
                sb.Append(kv.Key).Append(" = ").Append(kv.Value).AppendLine();
            sb.AppendLine();
        }
        File.WriteAllText(_iniPath, sb.ToString(), new UTF8Encoding(false));

        int dx = GetInt("Default", "DXVersion", 11);
        File.WriteAllText(_dxPath, $"[DirectX]{Environment.NewLine}Version = {(dx >= 11 ? 11 : 9)}{Environment.NewLine}", new UTF8Encoding(false));
    }

    public string Get(string section, string key, string fallback = "")
    {
        if (_sections.TryGetValue(section, out var map) && map.TryGetValue(key, out var v))
            return v;
        return fallback;
    }

    public int GetInt(string section, string key, int fallback)
        => int.TryParse(Get(section, key, fallback.ToString(CultureInfo.InvariantCulture)), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : fallback;

    public bool GetBool(string section, string key, bool fallback)
    {
        var v = Get(section, key, fallback ? "True" : "False");
        return v.Equals("True", StringComparison.OrdinalIgnoreCase) || v == "1";
    }

    public float GetFloat(string section, string key, float fallback)
        => float.TryParse(Get(section, key, fallback.ToString(CultureInfo.InvariantCulture)), NumberStyles.Float, CultureInfo.InvariantCulture, out var n) ? n : fallback;

    public void Set(string section, string key, string value)
    {
        if (!_sections.ContainsKey(section))
            _sections[section] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        _sections[section][key] = value;
    }

    public void SetInt(string section, string key, int value)
        => Set(section, key, value.ToString(CultureInfo.InvariantCulture));

    public void SetBool(string section, string key, bool value)
        => Set(section, key, value ? "True" : "False");

    public void SetFloat(string section, string key, float value)
        => Set(section, key, value.ToString("0.000000", CultureInfo.InvariantCulture));

    private void EnsureDefaults()
    {
        void Def(string s, string k, string v)
        {
            if (!_sections.ContainsKey(s))
                _sections[s] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!_sections[s].ContainsKey(k))
                _sections[s][k] = v;
        }

        Def("Default", "Version", "1");
        Def("Default", "DXVersion", "11");
        Def("Game", "Language", "6");
        Def("Game", "DefaultNation", "0");
        Def("Game", "EnablePhysX", "False");
        Def("Game", "TeamBand", "False");
        Def("Game", "DisableAccessory", "False");
        Def("Game", "WeaponEffect", "True");
        Def("Game", "HUD_Effect", "True");
        Def("Game", "Enable_MissionIndicator", "True");
        Def("Game", "EnableBulletTrace", "True");
        Def("Game", "EnableBulletSmoke", "True");
        Def("Graphics", "ScreenMode", "0");
        Def("Graphics", "ScreenWidth", "1920");
        Def("Graphics", "ScreenHeight", "1080");
        Def("Graphics", "RefreshRate", "60");
        Def("Graphics", "AntiAlias", "0");
        Def("Graphics", "VideoResolutionWidth", "1920");
        Def("Graphics", "VideoResolutionHeight", "1080");
        Def("Graphics", "ShadowQualityType", "0");
        Def("Graphics", "TextureQualityType", "2");
        Def("Graphics", "SpecularQualityType", "0");
        Def("Graphics", "EffectQuality", "0");
        Def("Graphics", "FPSType", "0");
        Def("Graphics", "FPSVal", "60");
        Def("Graphics", "GammaVal", "50.000000");
        Def("Graphics", "FovValue", "80.000000");
        Def("Graphics", "VSync", "False");
        Def("Graphics", "TriLinearFilter", "True");
        Def("Graphics", "DynamicLight", "False");
        Def("Graphics", "EnableNormalMap", "False");
        Def("Graphics", "EnableTerrainEffect", "True");
        Def("DX11", "HDR", "False");
        Def("DX11", "RimLight", "False");
        Def("DX11", "IBL", "False");
        Def("DX11", "SSAO", "False");
        Def("DX11", "SSR", "False");
    }
}
