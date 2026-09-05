using Fleck;
using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Managers;
using Plugin.Core.Utility;
using Server.Game.Rcon.Admin;
using Server.Game.Rcon.UserArea;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Server.Game.Rcon
{
    public class RconCommand
    {
        private static RconCommand Manager;

        public static RconCommand Instance()
            => Manager != null ? Manager : (Manager = new RconCommand());

        private static IWebSocketConnection currentClient;
        private WebSocketServer ServerRcon;

        public RconCommand()
        {
            try
            {
                FleckLog.Level = Fleck.LogLevel.Error;

                ServerRcon = new WebSocketServer($"ws://{ConfigLoader.RconIp}:{ConfigLoader.RconPort}");
                ServerRcon.Start(Client =>
                {
                    Client.OnMessage = Message =>
                    {
                        currentClient = Client;
                        Receive(Decode(Message));
                    };
                });
                CLogger.Print($"RconManager Address ws://{ConfigLoader.RconIp}:{ConfigLoader.RconPort}", LoggerType.Info);
            }
            catch (Exception Exc)
            {
                CLogger.Print("RconManager start: " + Exc.ToString(), LoggerType.Warning, Exc);
            }
        }

        private void Receive(string Message)
        {
            try
            {
                if (!ConfigLoader.RconValidIps.Contains(currentClient.ConnectionInfo.ClientIpAddress) && ConfigLoader.RconNotValidIpEnable)
                {
                    if (ConfigLoader.RconPrintNotValidIp)
                    {
                        RconLogger.Logs("Received message from unlisted ip: " + currentClient.ConnectionInfo.ClientIpAddress);
                        RconLogger.LogsPanel("Received message from unlisted ip: " + currentClient.ConnectionInfo.ClientIpAddress, 1);
                    }
                    return;
                }

                var jsonCommand = JsonSerializer.Deserialize<Dictionary<string, object>>(Message);

                if (jsonCommand.ContainsKey("command") && jsonCommand.ContainsKey("password"))
                {
                    string Opcode = jsonCommand["command"].ToString();
                    string Password = jsonCommand["password"].ToString();

                    if (ConfigLoader.RconInfoCommand)
                    {
                        CLogger.Print($"[{currentClient.ConnectionInfo.ClientIpAddress}] JSON Command : {Opcode}", LoggerType.Info);
                    }

                    if (Password != ConfigLoader.RconPassword)
                    {
                        RconLogger.Logs("RconManager received request with wrong password: " + Password);
                        RconLogger.LogsPanel("RconManager received request with wrong password: " + Password, 1);
                        return;
                    }

                    if (ProcessReadOnly(Opcode))
                        return;

                    ProcessCommand(Opcode, jsonCommand);
                    return;
                }
            }
            catch (Exception Exc)
            {
                RconLogger.Logs("RconManager error receiving: " + Exc.Message);
                RconLogger.LogsPanel("RconManager error receiving", 1);
            }
        }

        /// <summary>
        /// Read-only opcodes answered before the dispatch table: no token check and no
        /// token rotation, so the panel can poll them without churning web_rcon_token.
        /// </summary>
        private bool ProcessReadOnly(string opcode)
        {
            if (opcode != "SNAPSHOT")
                return false;

            try
            {
                SendResponse(Encode("Json_" + RconSnapshot.Build()));
            }
            catch (Exception Exc)
            {
                RconLogger.Logs("RconManager snapshot failed: " + Exc.Message);
                RconLogger.LogsPanel("Snapshot failed", 1);
            }
            return true;
        }

        private void ProcessCommand(string opcode, Dictionary<string, object> jsonData)
        {
            RconReceive Receive = null;

            switch (opcode)
            {
                //ADMIN PANEL
                case "102": Receive = new RconPCCafeAdmin(); break; //ready
                case "103": Receive = new RconTopupAdmin(); break; //ready

                case "104": Receive = new RconGiftItemAdmin(); break; //ready

                case "105": Receive = new RconBannedAdmin(); break; //ready
                case "106": Receive = new RconAnnounceAdmin(); break; //ready
                case "107": Receive = new RconRankAdmin(); break; //ready

                //PLAYER AREA
                case "WEBSHOP": Receive = new RconWebshopUser(); break; //ready
                case "REDEEMCODE": Receive = new RconRedeemUser(); break; //ready
                case "CNICKNAME": Receive = new RconChangenickUser(); break;// ready

                //DEVELOPER AREA
                case "KICKPLAYER": Receive = new RconKickPlayerAdmin(); break; // ready
                case "SCREENSHOT": Receive = new RconCaptureAdmin("screenshot"); break;
                case "CLIP": Receive = new RconCaptureAdmin("clip"); break;
                case "EVENTS": Receive = new RconEventsAdmin(); break;
                case "WHY": Receive = new RconWhyAdmin(); break;
                case "BLOCKIP": Receive = new RconBlockIPAdmin(); break; //ready
                case "BALANCE": Receive = new RconBalanceAdmin(); break; // ready
                case "SIMRANKUP": Receive = new RconSimRankUpAdmin(); break; // dev: forca promocao sem partida
                case "SIMINVEXTEND": Receive = new RconSimInvExtendAdmin(); break; // dev: 3337 isolado, sem cobranca
                case "SIMUSERINFOBLOCK": Receive = new RconSimUserInfoCarriersAdmin(); break; // dev: 3597+3682 isolados, sem estado

                //ROOM CONTROL (admin panel)
                case "ENDBATTLE": Receive = new RconEndBattleAdmin(); break;
                case "BALANCETEAMS": Receive = new RconBalanceTeamsAdmin(); break;
                case "KICKHOST": Receive = new RconKickHostAdmin(); break;
                case "FLASHSALE": Receive = new RconFlashSaleAdmin(); break;
                default:
                    RconLogger.Logs("RconManager received request with unknown opcode: " + opcode);
                    RconLogger.LogsPanel("RconManager received request with unknown opcode: " + opcode, 1);
                    return;
            }

            try
            {
                Receive.InitJson(jsonData); // Nuevo método para inicializar con JSON
                Receive.Run();
                // Reset Token
                Randomwinload();
                ComDiv.UpdateDB("web_rcon_token", new string[] { "token" }, finalString);
            }
            catch (Exception ex)
            {
                RconLogger.Logs("Error executing JSON command: " + ex.Message);
                RconLogger.LogsPanel("Error executing JSON command", 1);
            }
            finally
            {
                Receive = null;
            }
        }

        public static void SendResponse(string pesan)
        {
            try
            {
                if (currentClient != null && currentClient.ConnectionInfo != null)
                {
                    currentClient.Send(pesan);
                }
            }
            catch (Exception ex)
            {
                RconLogger.LogsPanel("Error in SendResponse", 1);
                RconLogger.Logs("Error in SendResponse: " + ex.Message);
            }
        }

        public static bool CheckToken(string CheckToken)
        {
            foreach (string Token in RconManager.GetTokenRconList())
            {
                if (CheckToken.Length != 0 || Token.Length != 0 || Token != null || Token == "")
                {
                    if (CheckToken == Token)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static string finalString;

        public static void Randomwinload()
        {
            var chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[32];
            var random = new Random();
            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }
            finalString = new string(stringChars);
        }

        public static string Encode(string input)
        {
            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            string encoded = Convert.ToBase64String(inputBytes);
            return encoded;
        }

        public static string Decode(string encodedInput)
        {
            byte[] decodedBytes = Convert.FromBase64String(encodedInput);
            string decoded = System.Text.Encoding.UTF8.GetString(decodedBytes);
            return decoded;
        }
    }
}