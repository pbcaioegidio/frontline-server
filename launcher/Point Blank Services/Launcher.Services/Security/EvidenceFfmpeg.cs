using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Launcher.Services.Security
{
    /// <summary>
    /// Converte ZIP do ring buffer (frame_XXXX.jpg + meta.json) em MP4 via ffmpeg.
    /// Se ffmpeg não estiver instalado, mantém só o ZIP.
    /// </summary>
    public static class EvidenceFfmpeg
    {
        /// <summary>
        /// Tenta gerar .mp4 ao lado do .zip. Retorna caminho do mp4 ou null.
        /// </summary>
        public static string TryConvertClipZip(string zipRelativeOrAbsolute, string baseDirectory)
        {
            try
            {
                string zipPath = Path.IsPathRooted(zipRelativeOrAbsolute)
                    ? zipRelativeOrAbsolute
                    : Path.Combine(baseDirectory, zipRelativeOrAbsolute);

                if (!File.Exists(zipPath) || !zipPath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    return null;

                string ffmpeg = FindFfmpeg();
                if (ffmpeg == null)
                {
                    Console.WriteLine("[FL GUARD] ffmpeg não encontrado — clip fica em ZIP. Instale ffmpeg e coloque no PATH.");
                    return null;
                }

                string work = Path.Combine(Path.GetDirectoryName(zipPath), Path.GetFileNameWithoutExtension(zipPath) + "_frames");
                if (Directory.Exists(work)) Directory.Delete(work, true);
                Directory.CreateDirectory(work);

                ZipFile.ExtractToDirectory(zipPath, work);

                // frame_0000.jpg ...
                string[] frames = Directory.GetFiles(work, "frame_*.jpg").OrderBy(f => f, StringComparer.OrdinalIgnoreCase).ToArray();
                if (frames.Length == 0)
                {
                    TryDelete(work);
                    return null;
                }

                // fps do meta (default 5)
                double fps = 5;
                string metaPath = Path.Combine(work, "meta.json");
                if (File.Exists(metaPath))
                {
                    string meta = File.ReadAllText(metaPath);
                    int i = meta.IndexOf("\"fps_target\":", StringComparison.Ordinal);
                    if (i >= 0)
                    {
                        string num = new string(meta.Substring(i + 13).TakeWhile(c => char.IsDigit(c) || c == '.').ToArray());
                        double.TryParse(num, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out fps);
                        if (fps < 1) fps = 5;
                    }
                }

                // renomeia para frame_%04d.jpg sequencial se precisar
                for (int i = 0; i < frames.Length; i++)
                {
                    string dest = Path.Combine(work, "seq_" + i.ToString("D4") + ".jpg");
                    if (!string.Equals(frames[i], dest, StringComparison.OrdinalIgnoreCase))
                        File.Copy(frames[i], dest, true);
                }

                string mp4 = Path.ChangeExtension(zipPath, ".mp4");
                if (File.Exists(mp4)) File.Delete(mp4);

                // Sem redirect de stdout/stderr: se o buffer enche, WaitForExit trava pra sempre.
                var psi = new ProcessStartInfo
                {
                    FileName = ffmpeg,
                    Arguments = $"-y -hide_banner -loglevel error -framerate {fps.ToString(System.Globalization.CultureInfo.InvariantCulture)} -i \"{Path.Combine(work, "seq_%04d.jpg")}\" -c:v libx264 -pix_fmt yuv420p -crf 23 -preset fast \"{mp4}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = false,
                    RedirectStandardOutput = false
                };

                using (Process p = Process.Start(psi))
                {
                    if (p == null) { TryDelete(work); return null; }
                    if (!p.WaitForExit(120000))
                    {
                        try { p.Kill(); } catch { }
                        TryDelete(work);
                        Console.WriteLine("[FL GUARD] ffmpeg timeout");
                        return null;
                    }
                    if (p.ExitCode != 0 || !File.Exists(mp4) || new FileInfo(mp4).Length < 1024)
                    {
                        Console.WriteLine("[FL GUARD] ffmpeg falhou exit=" + p.ExitCode);
                        try { if (File.Exists(mp4)) File.Delete(mp4); } catch { }
                        TryDelete(work);
                        return null;
                    }
                }

                TryDelete(work);

                // ZIP vira só intermediário — evidencia principal é o MP4
                try { File.Delete(zipPath); } catch { }

                // caminho relativo se baseDirectory for ancestral
                if (mp4.StartsWith(baseDirectory, StringComparison.OrdinalIgnoreCase))
                    return mp4.Substring(baseDirectory.Length).TrimStart('\\', '/');
                return mp4;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[FL GUARD] EvidenceFfmpeg: " + ex.Message);
                return null;
            }
        }

        private static string FindFfmpeg()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string[] candidates =
            {
                Path.Combine(baseDir, "tools", "ffmpeg.exe"),
                Path.Combine(baseDir, "ffmpeg.exe"),
                Path.Combine(baseDir, "bin", "ffmpeg.exe"),
                "ffmpeg",
                @"C:\ffmpeg\bin\ffmpeg.exe",
                @"C:\Program Files\ffmpeg\bin\ffmpeg.exe"
            };
            foreach (string c in candidates)
            {
                try
                {
                    if (c == "ffmpeg")
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = "where",
                            Arguments = "ffmpeg",
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardOutput = true
                        };
                        using (Process p = Process.Start(psi))
                        {
                            if (p == null) continue;
                            string o = p.StandardOutput.ReadToEnd();
                            p.WaitForExit(3000);
                            if (p.ExitCode == 0 && !string.IsNullOrWhiteSpace(o))
                                return o.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                        }
                    }
                    else if (File.Exists(c))
                        return c;
                }
                catch { }
            }
            return null;
        }

        private static void TryDelete(string dir)
        {
            try { if (Directory.Exists(dir)) Directory.Delete(dir, true); } catch { }
        }

        private static string Trunc(string s, int max)
        {
            if (s == null) return "";
            return s.Length <= max ? s : s.Substring(0, max);
        }
    }
}
