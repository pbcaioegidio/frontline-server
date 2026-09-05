// Decompiled with JetBrains decompiler
// Type: Server.Game.Data.Commands.ValuesCommand
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.Managers;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Data.Sync.Server;
using Server.Game.Data.Utils;
using Server.Game.Network;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Data.Commands
{
    public class ValuesCommand : ICommand
    {
        public string Command
        {
            
            get => "player";
        }

        public string Description
        {
            
            get => "modify value of player";
        }

        public string Permission
        {
            
            get => "gamemastercommand";
        }

        public string Args
        {
            
            get => "%options1% $options2% %value% %uid%";
        }

        
        public string Execute(string Command, string[] Args, Account Player)
        {
            string lower1 = Args[0].ToLower();
            string lower2 = Args[1].ToLower();

            if (lower1 == "xtx")
                return ExecuteXtx(lower2, Args, Player);

            int GoodId = int.Parse(Args[2]);
            long result;
            bool flag = long.TryParse(Args[3], out result);
            switch (lower1)
            {
                case "gift":
                    Account account1 = flag ? AccountManager.GetAccount(result, 0) : AccountManager.GetAccount(Args[3], 1, 0);
                    switch (lower2)
                    {
                        case "gold":
                            if (account1 == null)
                                return $"Player with {(flag ? $"UID: {result}" : "Nickname: " + Args[3])} doesn't Exist!";
                            if (!DaoManagerSQL.UpdateAccountGold(account1.PlayerId, account1.Gold + GoodId))
                                return ComDiv.ToTitleCase(lower1) + " Command wrong or not founded!";
                            account1.Gold += GoodId;
                            account1.SendPacket(new PROTOCOL_AUTH_GET_POINT_CASH_ACK(0U, account1));
                            SendItemInfo.LoadGoldCash(account1);
                            return $"{ComDiv.ToTitleCase(lower1)} {GoodId} Amount Of {ComDiv.ToTitleCase(lower2)} To UID: {account1.PlayerId} ({account1.Nickname})";
                        case "cash":
                            if (account1 == null)
                                return $"Player with {(flag ? $"UID: {result}" : "Nickname: " + Args[3])} doesn't Exist!";
                            if (!DaoManagerSQL.UpdateAccountCash(account1.PlayerId, account1.Cash + GoodId))
                                return ComDiv.ToTitleCase(lower1) + " Command wrong or not founded!";
                            account1.Cash += GoodId;
                            account1.SendPacket(new PROTOCOL_AUTH_GET_POINT_CASH_ACK(0U, account1));
                            SendItemInfo.LoadGoldCash(account1);
                            return $"{ComDiv.ToTitleCase(lower1)} {GoodId} Amount Of {ComDiv.ToTitleCase(lower2)} To UID: {account1.PlayerId} ({account1.Nickname})";
                        case "tags":
                            if (account1 == null)
                                return $"Player with {(flag ? $"UID: {result}" : "Nickname: " + Args[3])} doesn't Exist!";
                            if (!DaoManagerSQL.UpdateAccountTags(account1.PlayerId, account1.Tags + GoodId))
                                return ComDiv.ToTitleCase(lower1) + " Command wrong or not founded!";
                            account1.Tags += GoodId;
                            account1.SendPacket(new PROTOCOL_AUTH_GET_POINT_CASH_ACK(0U, account1));
                            SendItemInfo.LoadGoldCash(account1);
                            return $"{ComDiv.ToTitleCase(lower1)} {GoodId} Amount Of {ComDiv.ToTitleCase(lower2)} To UID: {account1.PlayerId} ({account1.Nickname})";
                        case "item":
                            if (account1 == null)
                                return $"Player with {(flag ? $"UID: {result}" : "Nickname: " + Args[3])} doesn't Exist!";
                            GoodsItem good = ShopManager.GetGood(GoodId);
                            if (good == null)
                                return $"Goods Id: {GoodId} not founded!";
                            ItemsModel itemsModel = new ItemsModel(good.Item);
                            if (itemsModel == null)
                                return ComDiv.ToTitleCase(lower1) + " Command wrong or not founded!";
                            account1.SendPacket(new PROTOCOL_BASE_NEW_REWARD_POPUP_ACK(Player, itemsModel));
                            if (ComDiv.GetIdStatics(itemsModel.Id, 1) == 6 && Player.Character.GetCharacter(itemsModel.Id) == null)
                                AllUtils.CreateCharacter(Player, itemsModel);
                            else
                                account1.SendPacket(new PROTOCOL_INVENTORY_GET_INFO_ACK(0, account1, itemsModel));
                            return $"{ComDiv.ToTitleCase(lower1)} {itemsModel.Name} To UID: {account1.PlayerId} ({account1.Nickname})";
                    }
                    break;
                case "kick":
                    switch (lower2)
                    {
                        case "uid":
                            Account account2 = AccountManager.GetAccount(result, 0);
                            if (account2 == null)
                                return $"Player with UID: {result} doesn't Exist!";
                            if (account2.PlayerId == Player.PlayerId)
                                return $"Player by UID: {result} failed! (Can't Kick Yourself)";
                            if (account2.Access > Player.Access)
                                return $"Player by UID: {result} failed! (Can't Kick Higher Access Level Than Yours)";
                            account2.SendPacket(new PROTOCOL_AUTH_ACCOUNT_KICK_ACK(2), false);
                            account2.Close(TimeSpan.FromSeconds((double)GoodId).Milliseconds, true);
                            return $"{ComDiv.ToTitleCase(lower1)} Player by UID: {result} Executed in {GoodId} Seconds!";
                        case "nick":
                            Account account3 = AccountManager.GetAccount(Args[3], 1, 0);
                            if (account3 == null)
                                return $"Player with Nickname: {Args[3]} doesn't Exist!";
                            if (account3.Nickname == Player.Nickname)
                                return $"Player by Nickname: {Args[3]} failed! (Can't Kick Yourself)";
                            if (account3.Access > Player.Access)
                                return $"Player by Nickname: {Args[3]} failed! (Can't Kick Higher Access Level Than Yours)";
                            account3.SendPacket(new PROTOCOL_AUTH_ACCOUNT_KICK_ACK(2), false);
                            account3.Close(TimeSpan.FromSeconds((double)GoodId).Milliseconds, true);
                            return $"{ComDiv.ToTitleCase(lower1)} Player by Nickname: {Args[3]} Executed in {GoodId} Seconds!";
                    }
                    break;
                case "ban":
                    Account account4 = flag ? AccountManager.GetAccount(result, 0) : AccountManager.GetAccount(Args[3], 1, 0);
                    switch (lower2)
                    {
                        case "normal":
                            if (account4 == null)
                                return $"Player with {(flag ? $"UID: {result}" : "Nickname: " + Args[3])} doesn't Exist!";
                            if (account4.PlayerId == Player.PlayerId)
                                return $"Player by {(flag ? $"UID: {result}" : "Nickname: " + Args[3])} failed! (Can't Ban Yourself)";
                            if (account4.Access > Player.Access)
                                return $"Player by {(flag ? $"UID: {result}" : "Nickname: " + Args[3])} failed! (Can't Ban Higher Access Level Than Yours)";

                            double num1 = Convert.ToDouble(GoodId);

                            // FL GUARD: hard ban (conta + MAC + IP + hardware) pelo período
                            Plugin.Core.Security.SecurityDao.HardBanResult hard1 = Plugin.Core.Security.SecurityDao.ApplyHardBan(
                                account4.PlayerId, "GM Command", TimeSpan.FromDays(num1), "gm:" + Player.Nickname,
                                Plugin.Core.Security.SecurityDao.SourceGm, Player.PlayerId, "GM_BAN",
                                "{\"gm\":\"" + Player.Nickname + "\",\"gm_id\":" + Player.PlayerId + ",\"days\":" + num1 + ",\"command\":\"player ban normal\"}",
                                Plugin.Core.ConfigLoader.HardBanSubnet);

                            if (hard1.BanId == 0)
                                return ComDiv.ToTitleCase(lower1) + " Command wrong or not founded!";

                            ComDiv.UpdateDB("accounts", "ban_object_id", hard1.BanId, "player_id", account4.PlayerId);

                            using (PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK Packet = new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK($"Player '{account4.Nickname}' has been banned for {num1} Day(s)!"))
                                GameXender.BroadcastToAll(Packet);

                            account4.BanObjectId = hard1.BanId;
                            account4.SendPacket(new PROTOCOL_AUTH_ACCOUNT_KICK_ACK(2), false);
                            account4.Close(1000, true);

                            return $"{ComDiv.ToTitleCase(lower1)} {(flag ? $"UID: {result}" : "Nickname: " + Args[3])} Success for {num1} Day(s)";

                        case "permanent":
                            if (account4 == null)
                                return $"Player with {(flag ? $"UID: {result}" : "Nickname: " + Args[3])} doesn't Exist!";
                            if (account4.PlayerId == Player.PlayerId)
                                return $"Player by {(flag ? $"UID: {result}" : "Nickname: " + Args[3])} failed! (Can't Ban Yourself)";
                            if (account4.Access > Player.Access)
                                return $"Player by {(flag ? $"UID: {result}" : "Nickname: " + Args[3])} failed! (Can't Ban Higher Access Level Than Yours)";

                            double num2 = 999.0;

                            // FL GUARD: hard ban permanente (conta + MAC + IP + hardware)
                            Plugin.Core.Security.SecurityDao.HardBanResult hard2 = Plugin.Core.Security.SecurityDao.ApplyHardBan(
                                account4.PlayerId, "GM Command", TimeSpan.Zero, "gm:" + Player.Nickname,
                                Plugin.Core.Security.SecurityDao.SourceGm, Player.PlayerId, "GM_BAN",
                                "{\"gm\":\"" + Player.Nickname + "\",\"gm_id\":" + Player.PlayerId + ",\"duration\":\"permanent\",\"command\":\"player ban permanent\"}",
                                Plugin.Core.ConfigLoader.HardBanSubnet);

                            if (hard2.BanId == 0)
                                return ComDiv.ToTitleCase(lower1) + " Command wrong or not founded!";

                            ComDiv.UpdateDB("accounts", "ban_object_id", hard2.BanId, "player_id", account4.PlayerId);

                            using (PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK Packet = new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK($"Player '{account4.Nickname}' has been permanently Banned!"))
                                GameXender.BroadcastToAll(Packet);

                            account4.BanObjectId = hard2.BanId;
                            account4.SendPacket(new PROTOCOL_AUTH_ACCOUNT_KICK_ACK(2), false);
                            account4.Close(1000, true);

                            return $"{ComDiv.ToTitleCase(lower1)} {(flag ? $"UID: {result}" : "Nickname: " + Args[3])} Success for {num2} Day(s)";
                    }
                    break;
            }
            return $"Command {ComDiv.ToTitleCase(lower1)} was not founded!";
        }

        private static string ExecuteXtx(string Options2, string[] Args, Account Player)
        {
            switch (Options2)
            {
                case "ci":
                    if (Player == null)
                        return "Invoker player not found!";
                    if (Args.Length < 3)
                        return "Usage: player xtx ci <GoodsId|ItemId>";
                    if (!int.TryParse(Args[2], out int selfValue))
                        return $"Invalid value: {Args[2]}";

                    ItemsModel selfItem = ResolveItem(selfValue);
                    if (selfItem == null)
                        return $"Goods/Item not found. '{selfValue}' is neither a valid GoodsId nor ItemId.";

                    GrantItem(Player, selfItem);
                    return $"Xtx {selfItem.Name ?? selfItem.Id.ToString()} to self ({Player.Nickname})";

                case "cia":
                    if (Args.Length < 4)
                        return "Usage: player xtx cia <GoodsId|ItemId> <UID|Nick>";
                    if (!int.TryParse(Args[2], out int targetValue))
                        return $"Invalid value: {Args[2]}";

                    bool isUid = long.TryParse(Args[3], out long uid);
                    Account Target = isUid ? AccountManager.GetAccount(uid, 0) : AccountManager.GetAccount(Args[3], 1, 0);
                    if (Target == null)
                        return $"Player with {(isUid ? $"UID: {uid}" : "Nickname: " + Args[3])} doesn't Exist!";
                    if (Player != null && Target.PlayerId == Player.PlayerId)
                        return "Use 'ci' to give items to yourself.";

                    ItemsModel targetItem = ResolveItem(targetValue);
                    if (targetItem == null)
                        return $"Goods/Item not found. '{targetValue}' is neither a valid GoodsId nor ItemId.";

                    GrantItem(Target, targetItem);
                    return $"Xtx {targetItem.Name ?? targetItem.Id.ToString()} To UID: {Target.PlayerId} ({Target.Nickname})";
            }

            return $"Command Xtx {Options2} was not founded!";
        }

        private static ItemsModel ResolveItem(int Value)
        {
            GoodsItem goodByGoodsId = ShopManager.GetGood(Value);
            if (goodByGoodsId != null)
                return new ItemsModel(goodByGoodsId.Item);

            GoodsItem goodByItemId = ShopManager.GetItemId(Value);
            if (goodByItemId != null)
                return new ItemsModel(goodByItemId.Item);

            if (Value < 100000)
                return null;

            ItemEquipType equip = ComDiv.GetItemCategory(Value) == ItemCategory.Coupon
                ? ItemEquipType.Durable
                : ItemEquipType.Permanent;

            return new ItemsModel(Value, "Command Item", equip, 1u);
        }

        private static void GrantItem(Account Target, ItemsModel Item)
        {
            Target.SendPacket(new PROTOCOL_BASE_NEW_REWARD_POPUP_ACK(Target, Item));

            if (ComDiv.GetIdStatics(Item.Id, 1) == 6 && Target.Character.GetCharacter(Item.Id) == null)
                AllUtils.CreateCharacter(Target, Item);
            else
                Target.SendPacket(new PROTOCOL_INVENTORY_GET_INFO_ACK(0, Target, Item));
        }
    }
}