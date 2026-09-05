using Launcher.PointBlank.Utils;
using System;
using System.IO;
using System.Windows.Forms;

public class Logger
{
    private static readonly string _logPath =
        Path.Combine(Application.StartupPath, "FLLauncher.log");
    private static bool _ended = false;

    // [HH:mm:ss]texto
    public static void Log(string texto)
    {
        Write(texto);
    }

    // [HH:mm:ss]FLLauncher Start - YYYY-M-D HH-mm-ss
    public static void LogStart()
    {
        _ended = false;
        DateTime now = DateTime.Now;
        string date = $"{now.Year}-{now.Month}-{now.Day} {now.Hour}-{now.Minute}-{now.Second}";
        Write($"FLLauncher Start - {date}");
    }

    // [HH:mm:ss]FLLauncher Start - YYYY-M-D HH-mm-ss
    // [HH:mm:ss]## FLLauncher Ver {version}
    public static void LogHeader(string version)
    {
        LogStart();
        Write($"## FLLauncher Ver {version}");
    }

    // [HH:mm:ss]FLLauncher End - YYYY-M-D HH-mm-ss
    public static void LogEnd()
    {
        if (_ended) return;
        _ended = true;
        DateTime now = DateTime.Now;
        string date = $"{now.Year}-{now.Month}-{now.Day} {now.Hour}-{now.Minute}-{now.Second}";
        Write($"FLLauncher End - {date}");
    }

    // [HH:mm:ss][FROM] -> [TO] try state change
    public static void LogState(UpdaterState from, UpdaterState to)
    {
        Write($"[{from}] -> [{to}] try state change");
    }

    // [HH:mm:ss]success : STATE
    public static void LogSuccess(UpdaterState state)
    {
        Write($"success : {state}");
    }

    // [HH:mm:ss]fail : STATE — mensagem
    public static void LogFail(UpdaterState state, string reason = null)
    {
        string msg = reason != null ? $"fail : {state} — {reason}" : $"fail : {state}";
        Write(msg);
    }

    // [HH:mm:ss][SUCC] Login Succ
    public static void LogLoginSuccess()
    {
        Write("[SUCC] Login Succ");
    }

    // [HH:mm:ss]LoginServer Connected Fail
    // [HH:mm:ss][ERROR] Connect to LoginServer
    public static void LogLoginFail()
    {
        Write("LoginServer Connected Fail");
        Write("[ERROR] Connect to LoginServer");
    }

    // [HH:mm:ss]# PBLuancher file Check Exception # FileName : {fileName}
    public static void LogFileCheckException(string fileName)
    {
        Write($"# PBLuancher file Check Exception # FileName : {fileName}");
    }

    private static void Write(string texto)
    {
        try
        {
            using (StreamWriter sw = new StreamWriter(_logPath, true))
            {
                string line = texto == null
                    ? ""
                    : $"[{DateTime.Now:HH:mm:ss}]{texto}";
                sw.WriteLine(line);
                sw.Flush();
            }
        }
        catch { /* não travar o launcher por falha de log */ }
    }
}
