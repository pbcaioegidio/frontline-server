using Plugin.Core.Models;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using System;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_CHAR_DELETE_CHARA_ACK : GameServerPacket
    {
        private readonly uint Error;
        private readonly int Slot;
        private readonly ItemsModel Item;
        private readonly CharacterModel Chara;
        private Account Player;

        public PROTOCOL_CHAR_DELETE_CHARA_ACK(uint Error, int Slot, Account Player, ItemsModel Item)
        {
            this.Error = Error;
            this.Slot = Slot;
            this.Item = Item;
            this.Player = Player;
            if (Player != null && Item != null)
            {
                Chara = Player.Character.GetCharacter(Item.Id);
            }
        }
        public override void Write()
        {
            WriteH(6152);
            WriteD(Error);

            if (Error == 0)
            {
       
                int Type = ComDiv.GetIdStatics(Item.Id, 2);

                // Idol Chara Checker
                if (Item.Id >= 632001 && Item.Id <= 632999) 
                {
                    Type = 1;
                }
                else if (Item.Id >= 664001 && Item.Id <= 664999)
                {
                    Type = 2;
                }

                WriteC((byte)Slot);
                WriteD((uint)Item.ObjectId);

                if (Type == 1)
                {
                    WriteD(Player.Character.GetCharacter(Player.Equipment.CharaRedId).Slot);
                }
                else if (Type == 2)
                {
                    WriteD(Player.Character.GetCharacter(Player.Equipment.CharaBlueId).Slot);
                }
            }
        }

    }
}
