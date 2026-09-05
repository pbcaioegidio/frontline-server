using System;

namespace Plugin.Core.Logging
{
    /// <summary>
    /// Encaminha eventos do CLogger para o painel de logs da UI (modo silencioso).
    /// Por padrão omite pacotes de rede para não encher a tela.
    /// </summary>
    public sealed class UiLogSink : ILogSink
    {
        private readonly LogLevel _min;
        private readonly bool _showPackets;

        public UiLogSink(LogLevel minLevel = LogLevel.Info, bool showPackets = false)
        {
            _min = minLevel;
            _showPackets = showPackets;
        }

        public void Write(LogEvent e)
        {
            try
            {
                if (e.Level < _min && e.Level != LogLevel.Hack) return;
                if (e.Cat == LogCat.Packet && !_showPackets) return;

                UiLogHub.Push(new UiLogLine
                {
                    Time = e.TsUtc.ToLocalTime(),
                    Level = e.Level,
                    Cat = e.Cat,
                    Text = ConsoleSink.Render(e)
                });
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("[UiLogSink] " + ex.Message);
            }
        }
    }
}
