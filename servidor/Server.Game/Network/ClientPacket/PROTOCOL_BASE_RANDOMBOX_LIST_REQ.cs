using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Managers;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using Plugin.Core.XML;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;

namespace Server.Game.Network.ClientPacket
{
    // Token: 0x02000152 RID: 338
    public class PROTOCOL_BASE_RANDOMBOX_LIST_REQ : GameClientPacket
    {
        private const int RandomBoxChunkSize = 8000;

        // Token: 0x06000365 RID: 869 RVA: 0x0001CE99 File Offset: 0x0001B099
        public override void Read()
        {
            base.ReadC(); // fix: count:u8 was being consumed as the first hash byte, dropping hash[32]
            base.ReadS(33);
        }

        // Token: 0x06000366 RID: 870 RVA: 0x0001CEAC File Offset: 0x0001B0AC

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                bool flag = player != null;
                if (flag)
                {
                    bool flag2 = !player.LoadedShop;
                    if (flag2)
                    {
                        player.LoadedShop = true;
                        player.LoadedPackedGoods = true;
                        ShopCatalog121Sender.SendFullCatalog(this.Client, player, true);
                        Throw2UnlockHelper.TrySendAfterShopCatalog(this.Client, player);
                    }
                    SendPackedRandomBoxList();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_BASE_RANDOMBOX_LIST_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }

        private void SendPackedRandomBoxList()
        {
            byte[] packed = RandomBoxXML.PackedRandomBoxBuffer;
            if (packed == null || packed.Length == 0)
            {
                this.Client.SendPacket(new PROTOCOL_BASE_RANDOMBOX_LIST_ACK());
                return;
            }

            for (int offset = 0; offset < packed.Length; offset += RandomBoxChunkSize)
            {
                int len = Math.Min(RandomBoxChunkSize, packed.Length - offset);
                byte[] chunk = new byte[len];
                Array.Copy(packed, offset, chunk, 0, len);
                this.Client.SendPacket(new PROTOCOL_BASE_RANDOMBOX_LIST_ACK(packed.Length, offset, chunk));
            }
        }
    }
}
