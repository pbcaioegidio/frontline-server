namespace Server.Auth.Network.ClientPacket
{
    /// <summary>
    /// Opcode 2518 - Tick/Timestamp packet dari client.
    /// Dikirim berkala, tidak perlu response.
    /// </summary>
    public class PROTOCOL_BASE_TICK_REQ : AuthClientPacket
    {
        public override void Read() { }
        public override void Run() { }
    }
}