using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;

internal static class Program
{
    const int HeaderSize = 2048;
    const int Atlas = 1024;

    static int Main(string[] args)
    {
        string path = args.Length > 0
            ? args[0]
            : @"c:\Users\pbcai\Downloads\source\client\Locale\Brazil\UI_V12\VTexList\text_01.i3VTexImage";
        if (!File.Exists(path)) { Console.WriteLine("missing " + path); return 1; }

        byte[] file = File.ReadAllBytes(path);
        if (file.Length < HeaderSize + Atlas * Atlas * 4)
        {
            Console.WriteLine("unexpected size " + file.Length);
            return 1;
        }

        var fmt = PixelFormat.Format32bppArgb;
        using (var bmp = new Bitmap(Atlas, Atlas, fmt))
        {
            var rect = new Rectangle(0, 0, Atlas, Atlas);
            BitmapData bd = bmp.LockBits(rect, ImageLockMode.WriteOnly, fmt);
            Marshal.Copy(file, HeaderSize, bd.Scan0, Atlas * Atlas * 4);
            bmp.UnlockBits(bd);

            // PB_logo: 169x30 @ (10,154)
            if (Path.GetFileName(path).StartsWith("btn_text02", StringComparison.OrdinalIgnoreCase))
                PaintWordmark(bmp, 7, 876, 169, 30);
            else
                PaintWordmark(bmp, 10, 154, 169, 30);

            bd = bmp.LockBits(rect, ImageLockMode.ReadOnly, fmt);
            Marshal.Copy(bd.Scan0, file, HeaderSize, Atlas * Atlas * 4);
            bmp.UnlockBits(bd);

            string previewDir = Path.Combine(Path.GetDirectoryName(path), "_preview");
            Directory.CreateDirectory(previewDir);
            bmp.Clone(new Rectangle(10, 154, 169, 30), fmt).Save(Path.Combine(previewDir, "PB_logo_frontline.png"));
        }

        File.WriteAllBytes(path, file);
        Console.WriteLine("ok " + path);
        return 0;
    }

    static void PaintWordmark(Bitmap atlas, int x, int y, int w, int h)
    {
        using (var g = Graphics.FromImage(atlas))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            // limpa o recorte (transparente)
            g.CompositingMode = CompositingMode.SourceCopy;
            using (var clear = new SolidBrush(Color.FromArgb(0, 0, 0, 0)))
                g.FillRectangle(clear, x, y, w, h);
            g.CompositingMode = CompositingMode.SourceOver;

            // fundo sutil + texto metálico cinza (estilo listrado original)
            var rect = new RectangleF(x, y, w, h);
            using (var font = BestFont(h))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    FormatFlags = StringFormatFlags.NoWrap
                };
                // sombra
                using (var sh = new SolidBrush(Color.FromArgb(180, 20, 20, 20)))
                    g.DrawString("FRONTLINE", font, sh, new RectangleF(x + 1, y + 1, w, h), sf);
                using (var br = new LinearGradientBrush(rect,
                    Color.FromArgb(255, 210, 214, 220),
                    Color.FromArgb(255, 120, 126, 136),
                    90f))
                    g.DrawString("FRONTLINE", font, br, rect, sf);
            }
        }
    }

    static Font BestFont(int boxH)
    {
        float size = Math.Max(10f, boxH * 0.72f);
        string[] faces = { "Impact", "Arial Black", "Segoe UI Black", "Arial" };
        foreach (var face in faces)
        {
            try { return new Font(face, size, FontStyle.Bold, GraphicsUnit.Pixel); }
            catch { }
        }
        return new Font("Arial", size, FontStyle.Bold, GraphicsUnit.Pixel);
    }
}
