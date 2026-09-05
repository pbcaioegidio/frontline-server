namespace Server.Game.Network.ServerPacket
{
    /// <summary>
    /// Opcode 1106 — fixed shop filter-tab descriptor required by the 121 client.
    ///
    /// The client parser (ShopSlot sort path around 0xCD85F8) expects this exact
    /// 5-slot framing (tags 3/4/2/6/1). Changing the count or omitting slots causes
    /// ACCESS_VIOLATION when the lobby/shop UI sorts slots (null string compare).
    ///
    /// This packet is UI-category chrome, NOT the per-item shop_tag source.
    /// Per-item tags come exclusively from system_shop / system_shop_effects.shop_tag
    /// via ShopManager.LoadShopItems / LoadShopEffects.
    /// </summary>
    public class PROTOCOL_SHOP_TAG_INFO_ACK : GameServerPacket
    {
        public override void Write()
        {
            this.WriteH((short)1106);
            this.WriteH((short)0);
            this.WriteC((byte)7);
            this.WriteC((byte)5);
            this.WriteH((short)0);
            this.WriteC((byte)0);
            this.WriteD(0);
            this.WriteH((short)0);
            this.WriteC((byte)3);
            this.WriteQ(0L);
            this.WriteC((byte)0);
            this.WriteC((byte)4);
            this.WriteQ(0L);
            this.WriteC((byte)0);
            this.WriteC((byte)2);
            this.WriteQ(0L);
            this.WriteC((byte)0);
            this.WriteC((byte)6);
            this.WriteQ(0L);
            this.WriteC((byte)0);
            this.WriteC((byte)1);
            this.WriteQ(0L);
            this.WriteD(0);
            this.WriteC((byte)0);
            this.WriteC(byte.MaxValue);
            this.WriteC(byte.MaxValue);
            this.WriteC(byte.MaxValue);
            this.WriteC((byte)0);
            this.WriteC(byte.MaxValue);
            this.WriteC((byte)1);
            this.WriteC((byte)7);
            this.WriteC((byte)2);
        }
    }
}
