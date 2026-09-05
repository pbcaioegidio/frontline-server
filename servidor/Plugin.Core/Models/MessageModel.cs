// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.MessageModel
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.Utility;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Plugin.Core.Models
{
    public class MessageModel
    {
        public int ClanId { get; set; }

        public long ObjectId { get; set; }

        public long SenderId { get; set; }

        public long ExpireDate { get; set; }

        public string SenderName { get; set; }

        public string Text { get; set; }

        public NoteMessageState State { get; set; }

        public NoteMessageType Type { get; set; }

        public NoteMessageClan ClanNote { get; set; }

        public int DaysRemaining { get; set; }

        public MessageModel()
        {
            this.SenderName = "";
            this.Text = "";
        }

        public MessageModel(long A_1, DateTime A_2)
        {
            this.SenderName = "";
            this.Text = "";
            this.ExpireDate = A_1;
            this.Method0(A_2);
        }

        
        public MessageModel(double A_1)
        {
            this.SenderName = "";
            this.Text = "";
            DateTime A_1_1 = DateTimeUtil.Now().AddDays(A_1);
            this.ExpireDate = long.Parse(A_1_1.ToString("yyMMddHHmm"));
            this.Method1(A_1_1, DateTimeUtil.Now());
        }

        
        private void Method0(DateTime A_1)
        {
            this.Method1(DateTime.ParseExact(this.ExpireDate.ToString(), "yyMMddHHmm", (IFormatProvider)CultureInfo.InvariantCulture), A_1);
        }

        private void Method1(DateTime A_1, DateTime A_2)
        {
            int num = (int)Math.Ceiling((A_1 - A_2).TotalDays);
            this.DaysRemaining = num < 0 ? 0 : num;
        }
    }
}