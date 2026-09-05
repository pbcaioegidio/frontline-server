namespace Server.Game.Rcon.Admin
{
    /// <summary>
    /// Push the current limited sale to every connected client, the same broadcast a
    /// purchase triggers.
    /// </summary>
    public class RconFlashSaleAdmin : RconReceive
    {
        private string Token;

        public override void Run()
        {
            if (IsJsonMode)
                Token = PopString("token");

            if (!RconCommand.CheckToken(Token) || Token == "")
            {
                RconLogger.LogsPanel("An error occurred in the process, please try again later. ", 1);
                return;
            }

            GameXender.BroadcastLimitedSaleSync();
            RconLogger.LogsPanel("[!] Flash sale synchronised to all clients", 0);
        }
    }
}
