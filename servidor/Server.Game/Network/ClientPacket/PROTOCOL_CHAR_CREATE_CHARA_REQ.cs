// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_CHAR_CREATE_CHARA_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Managers;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_CHAR_CREATE_CHARA_REQ : GameClientPacket
    {
        private string Field0;
        private List<CartGoods> Field1 = new List<CartGoods>();

        public override void Read()
        {
            // ===== CREATECHARA-DBG: dump raw bytes BEFORE parsing =====
            try
            {
                string hexRaw = (this._raw != null) ? System.BitConverter.ToString(this._raw) : "(null)";
                CLogger.Print($"[CREATECHARA-DBG] RAW len={(this._raw != null ? this._raw.Length : -1)} bytes={hexRaw}", LoggerType.Info);
            }
            catch (Exception exr) { CLogger.Print("[CREATECHARA-DBG] raw-log err: " + exr.Message, LoggerType.Error, exr); }

            int num1 = (int)this.ReadC();
            int nameLen = (int)this.ReadC();
            this.Field0 = this.ReadU(nameLen * 2);
            int num2 = (int)this.ReadC();
            int dbgGoodId = this.ReadD();
            int dbgBuyType = (int)this.ReadC();
            this.Field1.Add(new CartGoods()
            {
                GoodId = dbgGoodId,
                BuyType = dbgBuyType
            });
            int num3 = (int)this.ReadC();

            // ===== CREATECHARA-DBG: parsed fields + final stream position =====
            try
            {
                long pos = (this.MStream != null) ? this.MStream.Position : -1L;
                long blen = (this.MStream != null) ? this.MStream.Length : -1L;
                CLogger.Print($"[CREATECHARA-DBG] PARSED num1={num1} nameLen={nameLen} name='{this.Field0}' num2={num2} goodId={dbgGoodId} buyType={dbgBuyType} num3={num3} | streamPos={pos} streamLen={blen} (leftover={blen - pos})", LoggerType.Info);
            }
            catch (Exception exp) { CLogger.Print("[CREATECHARA-DBG] parse-log err: " + exp.Message, LoggerType.Error, exp); }
        }

        public override void Run()
        {
            try
            {
                Account player = this.Client.Player;
                if (player == null)
                {
                    CLogger.Print("[CREATECHARA-DBG] RUN abort: player == null", LoggerType.Info);
                    return;
                }
                CLogger.Print($"[CREATECHARA-DBG] RUN player={player.PlayerId} gold={player.Gold} cash={player.Cash} tags={player.Tags} invCount={player.Inventory.Items.Count} charCount={player.Character.Characters.Count}", LoggerType.Info);
                if (player.Inventory.Items.Count < 1500 && player.Character.Characters.Count < 150 )
                {
                    int GoldPrice;
                    int CashPrice;
                    int TagsPrice;
                    List<GoodsItem> goods = ShopManager.GetGoods(this.Field1, out GoldPrice, out CashPrice, out TagsPrice);
                    CLogger.Print($"[CREATECHARA-DBG] GetGoods reqGoodId={(this.Field1.Count > 0 ? this.Field1[0].GoodId.ToString() : "none")} reqBuyType={(this.Field1.Count > 0 ? this.Field1[0].BuyType.ToString() : "none")} -> goods.Count={goods.Count} GoldPrice={GoldPrice} CashPrice={CashPrice} TagsPrice={TagsPrice}", LoggerType.Info);
                    foreach (GoodsItem dbgG in goods)
                        CLogger.Print($"[CREATECHARA-DBG]   resolved Id={dbgG.Id} itemId={dbgG.Item.Id} count={dbgG.Item.Count} cash={dbgG.PriceCash} gold={dbgG.PriceGold} auth={dbgG.AuthType}", LoggerType.Info);
                    if (goods.Count != 0)
                    {
                        if (0 <= player.Gold - GoldPrice && 0 <= player.Cash - CashPrice && 0 <= player.Tags - TagsPrice)
                        {
                            if (!DaoManagerSQL.UpdateAccountValuable(player.PlayerId, player.Gold - GoldPrice, player.Cash - CashPrice, player.Tags - TagsPrice))
                            {
                                CLogger.Print("[CREATECHARA-DBG] BRANCH=UpdateAccountValuable FAILED -> ACK 2147487769", LoggerType.Info);
                                this.Client.SendPacket(new PROTOCOL_CHAR_CREATE_CHARA_ACK(2147487769U, byte.MaxValue, (CharacterModel)null, (Account)null));
                            }
                            else
                            {
                                player.Gold -= GoldPrice;
                                player.Cash -= CashPrice;
                                player.Tags -= TagsPrice;
                                this.Client.SendPacket(new PROTOCOL_INVENTORY_GET_INFO_ACK(0, player, goods));
                                CharacterModel characterModel = this.Method0(goods, player.Character.Characters.Count);
                                CLogger.Print($"[CREATECHARA-DBG] BRANCH=SUCCESS deducted (newGold={player.Gold} newCash={player.Cash}); characterModel={(characterModel == null ? "NULL" : ("id=" + characterModel.Id + " slot=" + characterModel.Slot))}", LoggerType.Info);
                                if (characterModel != null)
                                {
                                    ItemsModel ownedItem = player.Inventory.GetItem(characterModel.Id);
                                    CLogger.Print($"[CREATECHARA-DBG]   ownedItem={(ownedItem == null ? "NULL (item not in inventory!)" : ("objId=" + ownedItem.ObjectId))}", LoggerType.Info);
                                    if (ownedItem != null)
                                        characterModel.ObjectId = ownedItem.ObjectId;
                                    player.Character.AddCharacter(characterModel);
                                    bool persisted = player.Character.GetCharacter(characterModel.Id) != null;
                                    CLogger.Print($"[CREATECHARA-DBG]   AddCharacter done; GetCharacter!=null={persisted}; objId={characterModel.ObjectId}", LoggerType.Info);
                                    if (persisted)
                                        DaoManagerSQL.CreatePlayerCharacter(characterModel, player.PlayerId);
                                }
                                this.Client.SendPacket(new PROTOCOL_CHAR_CREATE_CHARA_ACK(0U, (byte)1, characterModel, player));
                            }
                        }
                        else
                        {
                            CLogger.Print($"[CREATECHARA-DBG] BRANCH=INSUFFICIENT funds (gold {player.Gold}-{GoldPrice}, cash {player.Cash}-{CashPrice}, tags {player.Tags}-{TagsPrice}) -> ACK 2147487768", LoggerType.Info);
                            this.Client.SendPacket(new PROTOCOL_CHAR_CREATE_CHARA_ACK(2147487768U, byte.MaxValue, (CharacterModel)null, (Account)null));
                        }
                    }
                    else
                    {
                        CLogger.Print("[CREATECHARA-DBG] BRANCH=GOODS_NOT_FOUND (goods.Count==0) -> ACK 2147487767", LoggerType.Info);
                        this.Client.SendPacket(new PROTOCOL_CHAR_CREATE_CHARA_ACK(2147487767U, byte.MaxValue, (CharacterModel)null, (Account)null));
                    }
                }
                else
                {
                    CLogger.Print($"[CREATECHARA-DBG] BRANCH=INV_OR_CHAR_FULL (inv={player.Inventory.Items.Count} char={player.Character.Characters.Count}) -> ACK 2147487929", LoggerType.Info);
                    this.Client.SendPacket(new PROTOCOL_CHAR_CREATE_CHARA_ACK(2147487929U, byte.MaxValue, (CharacterModel)null, (Account)null));
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("[CREATECHARA-DBG] EXCEPTION in Run(): " + ex.Message, LoggerType.Error, ex);
            }
        }

        private CharacterModel Method0(List<GoodsItem> A_1, int A_2)
        {
            foreach (GoodsItem goodsItem in A_1)
            {
                if (goodsItem != null && goodsItem.Item.Id != 0)
                    return new CharacterModel()
                    {
                        Id = goodsItem.Item.Id,
                        Slot = A_2++,
                        Name = this.Field0,
                        CreateDate = uint.Parse(DateTimeUtil.Now("yyMMddHHmm")),
                        PlayTime = 0
                    };
            }
            return (CharacterModel)null;
        }
    }
}