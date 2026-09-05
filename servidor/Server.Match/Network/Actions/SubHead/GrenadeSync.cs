// Decompiled with JetBrains decompiler
// Type: Server.Match.Network.Actions.SubHead.GrenadeSync
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using Server.Match.Data.Models.SubHead;
using System.Runtime.CompilerServices;


namespace Server.Match.Network.Actions.SubHead
{
    public class GrenadeSync
    {
        
        public static GrenadeInfo ReadInfo(SyncClientPacket C, bool GenLog, bool OnlyBytes = false)
        {
            GrenadeInfo grenadeInfo = new GrenadeInfo()
            {
                Unk1 = C.ReadC(),
                Unk2 = C.ReadC(),
                Unk3 = C.ReadC(),
                Unk4 = C.ReadC(),
                BoomInfo = C.ReadUH(),
                WeaponId = C.ReadD(),
                Accessory = C.ReadC(),
                Extensions = C.ReadC(),
                ObjPosX = C.ReadUH(),
                ObjPosY = C.ReadUH(),
                ObjPosZ = C.ReadUH(),
                Unk5 = C.ReadD(),
                Unk6 = C.ReadD(),
                Unk7 = C.ReadD()
            };
            if (!OnlyBytes)
                grenadeInfo.WeaponClass = (ClassType)ComDiv.GetIdStatics(grenadeInfo.WeaponId, 2);
            if (GenLog)
                CLogger.Print($"UDP_SUB_HEAD_GRENADE; Weapon Id: {grenadeInfo.WeaponId}; Acc Flag: {grenadeInfo.Accessory}; Ext: {grenadeInfo.Extensions}; Boom Info: {grenadeInfo.BoomInfo}; X: {grenadeInfo.ObjPosX}; Y: {grenadeInfo.ObjPosY}; Z: {grenadeInfo.ObjPosZ}", LoggerType.Warning);
            return grenadeInfo;
        }

        public static void WriteInfo(SyncServerPacket S, GrenadeInfo Info)
        {
            S.WriteC(Info.Unk1);
            S.WriteC(Info.Unk2);
            S.WriteC(Info.Unk3);
            S.WriteC(Info.Unk4);
            S.WriteH(Info.BoomInfo);
            S.WriteD(Info.WeaponId);
            S.WriteC(Info.Accessory);
            S.WriteC(Info.Extensions);
            S.WriteH(Info.ObjPosX);
            S.WriteH(Info.ObjPosY);
            S.WriteH(Info.ObjPosZ);
            S.WriteD(Info.Unk5);
            S.WriteD(Info.Unk6);
            S.WriteD(Info.Unk7);
        }
    }
}