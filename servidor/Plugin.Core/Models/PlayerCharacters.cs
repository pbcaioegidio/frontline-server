// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.PlayerCharacters
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System.Collections.Generic;

namespace Plugin.Core.Models
{
    public class PlayerCharacters
    {
        public List<CharacterModel> Characters { get; set; }

        public PlayerCharacters() => this.Characters = new List<CharacterModel>();

        public int GetCharacterIdx(int Slot)
        {
            lock (this.Characters)
            {
                for (int index = 0; index < this.Characters.Count; ++index)
                {
                    if (this.Characters[index].Slot == Slot)
                        return index;
                }
            }
            return -1;
        }

        public CharacterModel GetCharacter(int Id)
        {
            lock (this.Characters)
            {
                foreach (CharacterModel character in this.Characters)
                {
                    if (character.Id == Id)
                        return character;
                }
            }
            return (CharacterModel)null;
        }

        public CharacterModel GetCharacterSlot(int Slot)
        {
            lock (this.Characters)
            {
                foreach (CharacterModel character in this.Characters)
                {
                    if (character.Slot == Slot)
                        return character;
                }
            }
            return (CharacterModel)null;
        }

        public CharacterModel GetCharacter(int Slot, out int Index)
        {
            lock (this.Characters)
            {
                for (int index = 0; index < this.Characters.Count; ++index)
                {
                    CharacterModel character = this.Characters[index];
                    if (character.Slot == Slot)
                    {
                        Index = index;
                        return character;
                    }
                }
            }
            Index = -1;
            return (CharacterModel)null;
        }

        public void AddCharacter(CharacterModel Character)
        {
            lock (this.Characters)
                this.Characters.Add(Character);
        }

        public bool RemoveCharacter(CharacterModel Character)
        {
            lock (this.Characters)
                return this.Characters.Remove(Character);
        }
    }
}