using Plugin.Core.Managers;
using Plugin.Core.Models;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using Server.Game.Data.Commands;

namespace Server.Game.Data.Command.Commands
{
    class UpdateCommand : ICommand
    {
        public string Command => "refreshshop";
        public string Description => "Send messages";
        public string Permission => "gamemastercommand";
        public string Args => "";
        public string Execute(string Command, string[] Args, Account Player)
        {
            ShopManager.Reset();
            ShopManager.Load(1);

            ShopCatalog121Sender.SendFullCatalog(Player.Connection, Player, true);
            Player.SendPacket(new PROTOCOL_SHOP_GET_SAILLIST_ACK(true));
            return "Success Refresh shop";
        }
    }
}
