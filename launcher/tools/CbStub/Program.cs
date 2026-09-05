using System;
using System.Threading;
using System.Windows.Forms;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        // Stub: satisfaz CreateProcess do client sem anti-cheat.
        // Mantem processo vivo (alguns clients esperam o CB aberto).
        var ctx = new ApplicationContext();
        // sem janela
        ThreadPool.QueueUserWorkItem(_ =>
        {
            Thread.Sleep(Timeout.Infinite);
        });
        Application.Run(ctx);
    }
}
