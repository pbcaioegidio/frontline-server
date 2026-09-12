# Ferramenta para ler/escrever i3VTexImage (VTIH)
# Formato descoberto: header 2048 bytes, payload RGBA (BGRA) 4 bytes/pixel,
# armazenado em TILES de 128x128 (tiles em ordem row-major, pixels dentro do tile row-major).

$vtexCode = @'
using System;using System.Drawing;using System.Drawing.Imaging;using System.IO;using System.Runtime.InteropServices;
public static class VTex {
  public const int HDR = 2048;
  public const int TILE = 128;

  public static int W(byte[] d){ return BitConverter.ToInt32(d,8); }
  public static int H(byte[] d){ return BitConverter.ToInt32(d,12); }
  public static int Fmt(byte[] d){ return BitConverter.ToInt32(d,20); }

  // offset no arquivo do pixel (x,y)
  public static int Off(int x,int y,int w){
    int tpr=w/TILE;
    int tx=x/TILE, ty=y/TILE;
    int t=ty*tpr+tx;
    int r=(y%TILE)*TILE+(x%TILE);
    return HDR+(t*TILE*TILE+r)*4;
  }

  public static string Info(string src){
    byte[] d=File.ReadAllBytes(src);
    return Path.GetFileName(src)+"  w="+W(d)+" h="+H(d)+" fmt="+Fmt(d)+" len="+d.Length;
  }

  // exporta a textura inteira (ou recorte) desembaralhada, composta sobre fundo
  public static void Export(string src,string dst,int x0,int y0,int cw,int ch,int zoom,bool checker){
    byte[] d=File.ReadAllBytes(src); int w=W(d),h=H(d);
    if(cw<=0){x0=0;y0=0;cw=w;ch=h;}
    byte[] px=new byte[cw*ch*4];
    for(int y=0;y<ch;y++)for(int x=0;x<cw;x++){
      int sx=x0+x, sy=y0+y; int q=(y*cw+x)*4;
      if(sx>=w||sy>=h){ px[q+3]=255; continue; }
      int o=Off(sx,sy,w);
      double a=d[o+3]/255.0;
      int br,bgc,bb;
      if(checker){ int c=(((sx/8)+(sy/8))%2==0)?90:130; br=c;bgc=c;bb=c; } else { br=60;bgc=30;bb=20; }
      px[q+0]=(byte)(d[o+0]*a+br*(1-a));
      px[q+1]=(byte)(d[o+1]*a+bgc*(1-a));
      px[q+2]=(byte)(d[o+2]*a+bb*(1-a));
      px[q+3]=255;
    }
    using(Bitmap bm=new Bitmap(cw,ch,PixelFormat.Format32bppArgb)){
      BitmapData bd=bm.LockBits(new Rectangle(0,0,cw,ch),ImageLockMode.WriteOnly,PixelFormat.Format32bppArgb);
      Marshal.Copy(px,0,bd.Scan0,px.Length); bm.UnlockBits(bd);
      if(zoom==1){ bm.Save(dst,ImageFormat.Png); }
      else if(zoom>1){ using(Bitmap b2=new Bitmap(cw*zoom,ch*zoom)){ using(Graphics g=Graphics.FromImage(b2)){ g.InterpolationMode=System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor; g.DrawImage(bm,0,0,cw*zoom,ch*zoom);} b2.Save(dst,ImageFormat.Png);} }
      else { int s=-zoom; using(Bitmap b2=new Bitmap(cw/s,ch/s)){ using(Graphics g=Graphics.FromImage(b2)) g.DrawImage(bm,0,0,cw/s,ch/s); b2.Save(dst,ImageFormat.Png);} }
    }
  }

  // exporta PNG com alpha real (para editar fora)
  public static void ExportRaw(string src,string dst,int x0,int y0,int cw,int ch){
    byte[] d=File.ReadAllBytes(src); int w=W(d);
    byte[] px=new byte[cw*ch*4];
    for(int y=0;y<ch;y++)for(int x=0;x<cw;x++){
      int o=Off(x0+x,y0+y,w); int q=(y*cw+x)*4;
      px[q]=d[o];px[q+1]=d[o+1];px[q+2]=d[o+2];px[q+3]=d[o+3];
    }
    using(Bitmap bm=new Bitmap(cw,ch,PixelFormat.Format32bppArgb)){
      BitmapData bd=bm.LockBits(new Rectangle(0,0,cw,ch),ImageLockMode.WriteOnly,PixelFormat.Format32bppArgb);
      Marshal.Copy(px,0,bd.Scan0,px.Length); bm.UnlockBits(bd); bm.Save(dst,ImageFormat.Png);
    }
  }

  // grava um PNG dentro da textura na posicao (x0,y0)
  public static void Import(string tex,string png,int x0,int y0,int cw,int ch){
    byte[] d=File.ReadAllBytes(tex); int w=W(d);
    using(Bitmap src=new Bitmap(png)){
      using(Bitmap bm=new Bitmap(cw,ch,PixelFormat.Format32bppArgb)){
        using(Graphics g=Graphics.FromImage(bm)){
          g.Clear(Color.Transparent);
          g.InterpolationMode=System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
          g.DrawImage(src,0,0,cw,ch);
        }
        BitmapData bd=bm.LockBits(new Rectangle(0,0,cw,ch),ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);
        byte[] px=new byte[cw*ch*4]; Marshal.Copy(bd.Scan0,px,0,px.Length); bm.UnlockBits(bd);
        for(int y=0;y<ch;y++)for(int x=0;x<cw;x++){
          int o=Off(x0+x,y0+y,w); int q=(y*cw+x)*4;
          d[o]=px[q];d[o+1]=px[q+1];d[o+2]=px[q+2];d[o+3]=px[q+3];
        }
      }
    }
    File.WriteAllBytes(tex,d);
  }

  // preenche retangulo com cor solida (RGBA)
  public static void Fill(string tex,int x0,int y0,int cw,int ch,int r,int g,int b,int a){
    byte[] d=File.ReadAllBytes(tex); int w=W(d);
    for(int y=0;y<ch;y++)for(int x=0;x<cw;x++){
      int o=Off(x0+x,y0+y,w); d[o]=(byte)b;d[o+1]=(byte)g;d[o+2]=(byte)r;d[o+3]=(byte)a;
    }
    File.WriteAllBytes(tex,d);
  }

  public static void Montage(string[] f,string dst,int cell,int cols){
    int rows=(f.Length+cols-1)/cols;
    using(Bitmap bg=new Bitmap(cols*cell,rows*cell)){
      using(Graphics g=Graphics.FromImage(bg)){
        g.Clear(Color.Black);
        for(int i=0;i<f.Length;i++){ using(Bitmap im=new Bitmap(f[i])) g.DrawImage(im,(i%cols)*cell,(i/cols)*cell,cell,cell); }
      }
      bg.Save(dst,ImageFormat.Png);
    }
  }
}
'@

if (-not ([System.Management.Automation.PSTypeName]'VTex').Type) {
  Add-Type -TypeDefinition $vtexCode -ReferencedAssemblies System.Drawing
}
