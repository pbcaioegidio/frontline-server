// Decompiled with JetBrains decompiler
// Type: Server.Game.Data.Commands.ICommand
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Server.Game.Data.Models;


namespace Server.Game.Data.Commands
{
    public interface ICommand
    {
        string Command { get; }

        string Description { get; }

        string Permission { get; }

        string Args { get; }

        string Execute(string Command, string[] Args, Account Player);
    }
}