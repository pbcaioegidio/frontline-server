// Decompiled with JetBrains decompiler
// Type: Server.Game.Data.Commands.RoomInfoCommand
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using System.Runtime.CompilerServices;


namespace Server.Game.Data.Commands
{
    public class RoomInfoCommand : ICommand
    {
        public string Command
        {
            
            get => "roominfo";
        }

        public string Description
        {
            
            get => "Change room options";
        }

        public string Permission
        {
            
            get => "moderatorcommand";
        }

        public string Args
        {
            
            get => "%options% %value%";
        }

        
        public string Execute(string Command, string[] Args, Account Player)
        {
            RoomModel room = Player.Room;
            if (room == null)
                return "A room is required to execute the command.";
            string lower1 = Args[0].ToLower();
            string lower2 = Args[1].ToLower();
            switch (lower1)
            {
                case "time":
                    int num1 = int.Parse(lower2) * 6;
                    if (num1 < 0)
                        return $"Oops! Map 'index' Isn't Supposed To Be: {lower2}. Try an Higher Value.";
                    if (room.IsPreparing() || AllUtils.PlayerIsBattle(Player))
                        return "Oops! You Can't Change The 'time' While The Room Has Started Game Match.";
                    room.KillTime = num1;
                    room.UpdateRoomInfo();
                    return $"{ComDiv.ToTitleCase(lower1)} Changed To {room.GetTimeByMask() % 60} Minutes";
                case "flags":
                    RoomWeaponsFlag roomWeaponsFlag = (RoomWeaponsFlag)int.Parse(lower2);
                    room.WeaponsFlag = roomWeaponsFlag;
                    room.UpdateRoomInfo();
                    return $"{ComDiv.ToTitleCase(lower1)} Changed To {roomWeaponsFlag}";
                case "killcam":
                    byte num2 = byte.Parse(lower2);
                    room.KillCam = num2;
                    room.UpdateRoomInfo();
                    return $"{ComDiv.ToTitleCase(lower1)} Changed To {num2}";
                default:
                    return $"Command {ComDiv.ToTitleCase(lower1)} was not founded!";
            }
        }
    }
}