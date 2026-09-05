// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.ClanInvite
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll


namespace Plugin.Core.Models
{
    public class ClanInvite
    {
        public int Id { get; set; }

        public uint InviteDate { get; set; }

        public long PlayerId { get; set; }

        public string Text { get; set; }
    }
}