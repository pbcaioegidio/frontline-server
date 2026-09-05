// Decompiled with JetBrains decompiler
// Type: Server.Match.Data.Models.ObjectModel
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using Plugin.Core.Utility;
using System;
using System.Collections.Generic;


namespace Server.Match.Data.Models
{
    public class ObjectModel
    {
        public int Id { get; set; }

        public int Life { get; set; }

        public int Animation { get; set; }

        public int UltraSync { get; set; }

        public int UpdateId { get; set; }

        public bool NeedSync { get; set; }

        public bool Destroyable { get; set; }

        public bool NoInstaSync { get; set; }

        public List<AnimModel> Animations { get; set; }

        public List<DeffectModel> Effects { get; set; }

        public ObjectModel(bool A_1)
        {
            this.NeedSync = A_1;
            if (A_1)
                this.Animations = new List<AnimModel>();
            this.UpdateId = 1;
            this.Effects = new List<DeffectModel>();
        }

        public int CheckDestroyState(int Life)
        {
            for (int index = this.Effects.Count - 1; index > -1; --index)
            {
                DeffectModel effect = this.Effects[index];
                if (effect.Life >= Life)
                    return effect.Id;
            }
            return 0;
        }

        public int GetRandomAnimation(RoomModel Room, ObjectInfo Obj)
        {
            if (this.Animations != null && this.Animations.Count > 0)
            {
                AnimModel animation = this.Animations[new Random().Next(this.Animations.Count)];
                Obj.Animation = animation;
                Obj.UseDate = DateTimeUtil.Now();
                if (animation.OtherObj > 0)
                {
                    ObjectInfo objectInfo = Room.Objects[animation.OtherObj];
                    this.GetAnim(animation.OtherAnim, 0.0f, 0.0f, objectInfo);
                }
                return animation.Id;
            }
            Obj.Animation = (AnimModel)null;
            return (int)byte.MaxValue;
        }

        public void GetAnim(int AnimId, float Time, float Duration, ObjectInfo Obj)
        {
            if (AnimId == (int)byte.MaxValue || Obj == null || Obj.Model == null || Obj.Model.Animations == null || Obj.Model.Animations.Count == 0)
                return;
            foreach (AnimModel animation in Obj.Model.Animations)
            {
                if (animation.Id == AnimId)
                {
                    Obj.Animation = animation;
                    Time -= Duration;
                    Obj.UseDate = DateTimeUtil.Now().AddSeconds((double)Time * -1.0);
                    break;
                }
            }
        }
    }
}