// Decompiled with JetBrains decompiler
// Type: Server.Game.Data.Commands.TitleCommand
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using Plugin.Core.XML;
using Server.Game.Data.Models;
using Server.Game.Network;
using Server.Game.Network.ServerPacket;
using System.Runtime.CompilerServices;


namespace Server.Game.Data.Commands
{
    public class TitleCommand : ICommand
    {
        public string Command
        {
            
            get => "title";
        }

        public string Description
        {
            
            get => "Unlock all title";
        }

        public string Permission
        {
            
            get => "hostcommand";
        }

        public string Args => "";

        
        public string Execute(string Command, string[] Args, Account Player)
        {
            if (Player.Title.OwnerId == 0L)
            {
                DaoManagerSQL.CreatePlayerTitlesDB(Player.PlayerId);
                Player.Title = new PlayerTitles()
                {
                    OwnerId = Player.PlayerId
                };
            }
            PlayerTitles title1 = Player.Title;
            int num = 0;
            for (int titleId = 1; titleId <= 44; ++titleId)
            {
                TitleModel title2 = TitleSystemXML.GetTitle(titleId);
                if (title2 != null && !title1.Contains(title2.Flag))
                {
                    ++num;
                    title1.Add(title2.Flag);
                    if (title1.Slots < title2.Slot)
                        title1.Slots = title2.Slot;
                }
            }
            if (num > 0)
            {
                ComDiv.UpdateDB("player_titles", "slots", (object)title1.Slots, "owner_id", (object)Player.PlayerId);
                DaoManagerSQL.UpdatePlayerTitlesFlags(Player.PlayerId, title1.Flags);
                Player.SendPacket(new PROTOCOL_BASE_USER_TITLE_INFO_ACK(Player));
            }
            return "All title Successfully Opened!";
        }
    }
}