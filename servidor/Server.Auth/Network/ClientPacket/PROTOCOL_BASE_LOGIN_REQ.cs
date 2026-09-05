////lOGIN_REQ WITH USERNAME AND PASSWORD FOR RU:
using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Logging;
using Plugin.Core.JSON;
using Plugin.Core.Models;
using Plugin.Core.Security;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using Server.Auth.Data.Managers;
using Server.Auth.Data.Models;
using Server.Auth.Data.Sync.Server;
using Server.Auth.Data.Utils;
using Server.Auth.Network.ServerPacket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace Server.Auth.Network.ClientPacket
{
    public class PROTOCOL_BASE_LOGIN_REQ : AuthClientPacket
    {
        #region Private Fields - Login Data

        private byte _verificationByte1;
        private byte _verificationByte2;
        private byte _verificationByte3;
        private byte _verificationByte4;
        private uint _hwSpec0;
        private string _username;
        private string _password;
        private string _token;
        private string _clientVersion;
        private string _ipAddress;
        private string _screenResolution;
        private string _userFileList;
        private ClientLocale _clientLocale;
        private PhysicalAddress _macAddress;

        #endregion Private Fields - Login Data

        #region Read Method

        //LOGIN REQ 121 BR (token base64) - conta fixa de teste
        public override void Read()
        {
            try
            {
                byte[] __rawReq = MStream.ToArray();
                _hwSpec0 = (__rawReq != null && __rawReq.Length >= 5) ? BitConverter.ToUInt32(__rawReq, 1) : 0u;
                _verificationByte1 = ReadC();
                _verificationByte2 = ReadC();
                _verificationByte3 = ReadC();
                _verificationByte4 = ReadC();
                byte[] macBytes = ReadB(6);
                _macAddress = new PhysicalAddress(macBytes);
                ReadB(21);
                _screenResolution = $"{ReadH()}x{ReadH()}";
                ReadB(10);
                _userFileList = ReadS(ReadC());
                ReadB(16);
                _clientLocale = (ClientLocale)ReadC();
                _clientVersion = $"{ReadC()}.{ReadC()}";
                ReadB(1);
                int tokenLen = ReadH();
                byte[] tokenBytes = ReadB(tokenLen);
                _token = Convert.ToBase64String(tokenBytes);
                _ipAddress = Client.GetIPAddress();

                CLogger.Event(LogCat.Login, new { res = _screenResolution, locale = _clientLocale, ver = _clientVersion, hwid = _userFileList, token = _token });
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error in Read(): {ex.Message} (Pos: {MStream.Position}/{MStream.Length})", LoggerType.Error);
                throw;
            }
        }

        //LOGIN REQ Version 3.108 RU (username/password)
        //public override void Read()
        //{
        //    try
        //    {
        //        _verificationByte1 = ReadC();
        //        _verificationByte2 = ReadC();
        //        _verificationByte3 = ReadC();
        //        _verificationByte4 = ReadC();
        //        byte[] macBytes = ReadB(6);
        //        _macAddress = new PhysicalAddress(macBytes);
        //        ReadB(21);
        //        _screenResolution = $"{ReadH()}x{ReadH()}";
        //        ReadB(10);
        //        _userFileList = ReadS(ReadC());
        //        ReadB(16);
        //        _clientLocale = (ClientLocale)ReadC();
        //        _clientVersion = $"{ReadC()}.{ReadC()}";
        //        ReadB(3);
        //        _password = ReadS(ReadC());
        //        _username = ReadS(ReadC());
        //        _ipAddress = Client.GetIPAddress();
        //    }
        //    catch (Exception ex)
        //    {
        //        CLogger.Print($"Error in Read(): {ex.Message} (Pos: {MStream.Position}/{MStream.Length})", LoggerType.Error);
        //        throw;
        //    }
        //}

        //Login req Version 88 RU
        //public override void Read()
        //{
        //    try
        //    {
        //      
        //           
        //        _verificationByte1 = ReadC();
        //        _verificationByte2 = ReadC();
        //        _verificationByte3 = ReadC();
        //        _verificationByte4 = ReadC();
        //        byte[] macBytes = ReadB(6);
        //        _macAddress = new PhysicalAddress(macBytes);
        //        ReadB(20);
        //        _screenResolution = $"{ReadH()}x{ReadH()}";
        //        ReadB(9);
        //        _userFileList = ReadS(ReadC());
        //        ReadB(16);
        //        _clientLocale = (ClientLocale)ReadC();
        //        _clientVersion = $"{ReadC()}.{ReadC()}";
        //        ReadB(3);
        //        _password = ReadS(ReadC());
        //        _username = ReadS(ReadC());
        //        ReadC();
        //        _ipAddress = Client.GetIPAddress();

        //    }
        //    catch (Exception ex)
        //    {
        //        CLogger.Print($"Error in Read(): {ex.Message} (Pos: {MStream.Position}/{MStream.Length})", LoggerType.Error);
        //        throw;
        //    }
        //}

        //LOGIN REQ Version 3.112 RU
        //public override void Read()
        //{
        //    try
        //    {
        //        int pos = 4;

        //        _verificationByte1 = _raw[pos++];
        //        _verificationByte2 = _raw[pos++];
        //        _verificationByte3 = _raw[pos++];
        //        _verificationByte4 = _raw[pos++];

        //        _macAddress = new PhysicalAddress(_raw.Skip(pos).Take(6).ToArray()); pos += 6;

        //        pos += 20;

        //        ushort w = (ushort)((_raw[pos] << 8) | _raw[pos + 1]); pos += 2;
        //        ushort h = (ushort)((_raw[pos] << 8) | _raw[pos + 1]); pos += 2;
        //        _screenResolution = $"{w}x{h}";

        //        pos += 11;                              // [38-48] skip 11 bytes

        //        byte hashLen = _raw[pos++];             // [49] = 0x20 = 32
        //        CLogger.Print($"hashLen={hashLen} pos={pos}", LoggerType.Debug);

        //        _userFileList = System.Text.Encoding.ASCII.GetString(_raw, pos, hashLen).TrimEnd('\0');
        //        pos += hashLen;                         // [50-81]

        //        CLogger.Print($"pos after hash={pos}, remaining: {BitConverter.ToString(_raw.Skip(pos).ToArray())}", LoggerType.Debug);

        //        pos += 16;                              // [82-97]
        //        _clientLocale = (ClientLocale)_raw[pos++]; // [98]
        //        _clientVersion = $"{_raw[pos++]}.{_raw[pos++]}"; // [99-100]
        //        pos += 3;                               // [101-103]

        //        byte passLen = _raw[pos++];             // [104]
        //        _password = System.Text.Encoding.ASCII.GetString(_raw, pos, passLen).TrimEnd('\0');
        //        pos += passLen;

        //        byte userLen = _raw[pos++];
        //        _username = System.Text.Encoding.ASCII.GetString(_raw, pos, userLen).TrimEnd('\0');
        //        pos += userLen;

        //        _ipAddress = Client.GetIPAddress();

        //        CLogger.Print($"Parsed → user={_username} pass={_password} ver={_clientVersion} locale={_clientLocale} res={_screenResolution}", LoggerType.Debug);
        //    }
        //    catch (Exception ex)
        //    {
        //        CLogger.Print($"Read() error: {ex.Message}", LoggerType.Error);
        //        throw;
        //    }
        //}

        #endregion Read Method

        #region Run Method - Main Login Logic

        private static readonly Dictionary<string, DateTime> _lastLoginAttempts = new Dictionary<string, DateTime>();
        private static readonly object _lockObject = new object();

        public override void Run()
        {
            try
            {
                if (Client == null)
                {
                    CLogger.Print("PROTOCOL_BASE_LOGIN_REQ.Run(): Client is null", LoggerType.Error);
                    return;
                }

                string clientKey = Client.GetIPAddress();

                lock (_lockObject)
                {
                    if (_lastLoginAttempts.ContainsKey(clientKey))
                    {
                        var timeDiff = DateTime.Now - _lastLoginAttempts[clientKey];
                        if (timeDiff.TotalMilliseconds < 1000)
                        {
                            Console.WriteLine($"Duplicate login attempt blocked for {clientKey}");
                            Client.Close(1000, false);
                            return;
                        }
                    }

                    _lastLoginAttempts[clientKey] = DateTime.Now;
                }

                uint verificationKey = _hwSpec0;

                ServerConfig config = AuthXender.Client.Config;
                Account player = Client.Player = AccountManager.GetAccountDB(_token, null, 0, 95);
                if (player == null)
                {
                    SecurityDao.LogLogin(SecurityDao.SourceAuth, "", 0, "bad_token", _ipAddress, _userFileList, "token desconhecido");
                    HandleInvalidCredentials(null);
                    return;
                }

                _username = player.Username;
                _password = player.Password;

                // FL GUARD: token precisa ter sido emitido pelo launcher e estar dentro da validade
                if (ConfigLoader.RequireOtpToken)
                {
                    DateTime? expires = SecurityDao.GetTokenExpires(player.PlayerId);
                    if (expires == null || expires.Value <= DateTimeUtil.Now())
                    {
                        SecurityDao.LogLogin(SecurityDao.SourceAuth, _username, player.PlayerId, "token_expired", _ipAddress, _userFileList,
                            expires == null ? "token sem validade (exe direto?)" : "expirado em " + expires.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                        SecurityDao.LogEvent(SecurityDao.SourceAuth, player.PlayerId, _username, player.Nickname, "login_denied", "TOKEN_EXPIRED",
                            expires == null ? "Login sem passar pelo launcher" : "Token expirado",
                            "{\"ip\":\"" + _ipAddress + "\",\"hwid_exe\":\"" + (_userFileList ?? "") + "\",\"expires\":\"" + (expires?.ToString("s") ?? "") + "\"}",
                            expires == null ? 3 : 1, "FG-100");
                        Client.SendPacket(new PROTOCOL_BASE_LOGIN_ACK(EventErrorEnum.EVENT_LOG_IN_TIME_OUT_2, player, 0U));
                        CLogger.Print($"[FL GUARD] Token inválido/expirado [{_username}] ip={_ipAddress}", LoggerType.Warning);
                        Client.Close(1000, false);
                        return;
                    }
                }
                if (!ValidateInitialRequirements(config))
                {
                    HandleInitialValidationFailure(config);
                    return;
                }

                CLogger.Event(LogCat.Nick, new { user = _username, nick = player.Nickname, nickLen = player.Nickname == null ? -2 : player.Nickname.Length, statusId = player.StatusId(), srvId = player.Status.ServerId });
                ProcessLogin(player, config, verificationKey);
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }

        #endregion Run Method - Main Login Logic

        #region Validation Methods
        private bool ValidateInitialRequirements(ServerConfig config)
        {
            if (config == null)
                return false;

            if (!ConfigLoader.IsTestMode && !ConfigLoader.GameLocales.Contains(_clientLocale))
                return false;

            if (_username.Length < ConfigLoader.MinUserSize || _username.Length > ConfigLoader.MaxUserSize)
                return false;

            if (!ConfigLoader.IsTestMode && _password.Length < ConfigLoader.MinPassSize)
                return false;

            if (_macAddress.GetAddressBytes().SequenceEqual(new byte[6]))
                return false;

            if (_clientVersion != config.ClientVersion)
                return false;

            if (config.AccessUFL && _userFileList != config.UserFileList)
                return false;

            if (_screenResolution.Equals("0x0"))
                return false;

            return true;
        }

        private void HandleInitialValidationFailure(ServerConfig config)
        {
            string errorMessage = GetValidationErrorMessage(config);

            Client.SendPacket(new PROTOCOL_SERVER_MESSAGE_DISCONNECTIONSUCCESS_ACK(2147483904U /*0x80000100*/));
            CLogger.Print(errorMessage, LoggerType.Warning);
            Client.Close(1000, true);
        }
        private string GetValidationErrorMessage(ServerConfig config)
        {
            if (config == null)
                return $"Invalid server config [{_username}]";

            if (!ConfigLoader.IsTestMode && !ConfigLoader.GameLocales.Contains(_clientLocale))
                return $"Country: {_clientLocale} of blocked client [{_username}]";

            if (_username.Length < ConfigLoader.MinUserSize)
                return $"Username too short [{_username}]";

            if (_username.Length > ConfigLoader.MaxUserSize)
                return $"Username too long [{_username}]";

            if (!ConfigLoader.IsTestMode && _password.Length < ConfigLoader.MinPassSize)
                return $"Password too short [{_username}]";

            if (!ConfigLoader.IsTestMode && _password.Length > ConfigLoader.MaxPassSize)
                return $"Password too long [{_username}]";

            if (_macAddress.GetAddressBytes().SequenceEqual(new byte[6]))
                return $"Invalid MAC Address [{_username}]";

            if (_clientVersion != config.ClientVersion)
                return $"Version: {_clientVersion} not supported [{_username}]";

            if (config.AccessUFL && _userFileList != config.UserFileList)
                return $"UserFileList: {_userFileList} not supported [{_username}]";

            if (_screenResolution.Equals("0x0"))
                return $"Invalid {_screenResolution} resolution [{_username}]";

            return "There is something wrong happened when trying to login " + _username;
        }

        #endregion Validation Methods

        #region Login Processing Methods
        private void ProcessLogin(Account player, ServerConfig config, uint verificationKey)
        {
            BanHistory activeBan = DaoManagerSQL.GetActiveBanForPlayer(player.PlayerId);

            if (activeBan != null && activeBan.EndDate > DateTimeUtil.Now())
            {
                if (player.BanObjectId != activeBan.ObjectId)
                {
                    player.BanObjectId = activeBan.ObjectId;
                    ComDiv.UpdateDB("accounts", "ban_object_id", activeBan.ObjectId, "player_id", player.PlayerId);
                }

                SecurityDao.LogLogin(SecurityDao.SourceAuth, _username, player.PlayerId, "account_banned", _ipAddress, _userFileList,
                    "ban #" + activeBan.ObjectId + " até " + activeBan.EndDate.ToString("yyyy-MM-dd HH:mm"));
                Client.SendPacket(new PROTOCOL_BASE_LOGIN_ACK(EventErrorEnum.EVENT_LOG_IN_BLOCK_ACCOUNT, player, 0U));
                CLogger.Print($"Account is banned until {activeBan.EndDate:yyyy-MM-dd HH:mm:ss} [{player.Username}]", LoggerType.Warning);
                Client.Close(1000, false);
                return;
            }

            if (player.IsBanned())
            {
                SecurityDao.LogLogin(SecurityDao.SourceAuth, _username, player.PlayerId, "account_banned", _ipAddress, _userFileList, "permanente");
                Client.SendPacket(new PROTOCOL_BASE_LOGIN_ACK(EventErrorEnum.EVENT_LOG_IN_BLOCK_ACCOUNT, player, 0U));
                CLogger.Print($"Permanently banned [{player.Username}]", LoggerType.Warning);
                Client.Close(1000, false);
                return;
            }

            if (player.MacAddress != _macAddress)
                ComDiv.UpdateDB("accounts", "mac_address", _macAddress, "player_id", player.PlayerId);
            bool isMacBanned;
            bool isIpBanned;
            DaoManagerSQL.GetBanStatus($"{_macAddress}", _ipAddress, out isMacBanned, out isIpBanned);

            if (isMacBanned || isIpBanned)
            {
                string cat = isIpBanned ? "IP_BANNED" : "MAC_BANNED";
                SecurityDao.LogLogin(SecurityDao.SourceAuth, _username, player.PlayerId, isIpBanned ? "ip_banned" : "mac_banned", _ipAddress, _userFileList, "");
                SecurityDao.LogEvent(SecurityDao.SourceAuth, player.PlayerId, _username, player.Nickname, "login_denied", cat,
                    (isIpBanned ? "IP" : "MAC") + " banido tentou logar",
                    "{\"ip\":\"" + _ipAddress + "\",\"mac\":\"" + _macAddress + "\",\"hwid_exe\":\"" + (_userFileList ?? "") + "\"}",
                    3, isIpBanned ? "FG-102" : "FG-101");
                CLogger.Print($"{(isMacBanned ? "MAC Address blocked" : "IP4 Address blocked")} [{player.Username}]", LoggerType.Warning);
                Client.SendPacket(new PROTOCOL_BASE_LOGIN_ACK(
                    isIpBanned ? EventErrorEnum.LOGIN_BLOCK_IP : EventErrorEnum.EVENT_LOG_IN_BLOCK_ACCOUNT,
                    player,
                    0U
                ));
                Client.Close(1000, false);
                return;
            }

            if (!string.IsNullOrWhiteSpace(_userFileList) && HwIdBanCache.IsBanned(_userFileList))
            {
                SecurityDao.LogLogin(SecurityDao.SourceAuth, _username, player.PlayerId, "device_banned", _ipAddress, _userFileList, "hwid_exe em base_ban_hwid");
                SecurityDao.LogEvent(SecurityDao.SourceAuth, player.PlayerId, _username, player.Nickname, "login_denied", "DEVICE_BANNED",
                    "HWID do cliente banido tentou logar",
                    "{\"ip\":\"" + _ipAddress + "\",\"mac\":\"" + _macAddress + "\",\"hwid_exe\":\"" + _userFileList + "\"}",
                    4, "FG-101");
                Client.SendPacket(new PROTOCOL_BASE_LOGIN_ACK(EventErrorEnum.EVENT_LOG_IN_BLOCK_ACCOUNT, player, 0U));
                CLogger.Print($"HWID bloqueado [{player.Username}] hwid={_userFileList}", LoggerType.Warning);
                Client.Close(1000, false);
                return;
            }

            bool hasAccess = (player.IsGM() && config.OnlyGM) || (player.AuthLevel() >= AccessLevel.NORMAL && !config.OnlyGM);

            if (!hasAccess)
            {
                Client.SendPacket(new PROTOCOL_BASE_LOGIN_ACK(EventErrorEnum.EVENT_LOG_IN_TIME_OUT_2, player, 0U));
                CLogger.Print($"Invalid access level [{player.Username}]", LoggerType.Warning);
                Client.Close(1000, false);
                return;
            }

            Account cachedAccount = AccountManager.GetAccount(player.PlayerId, true);
            if (player.IsOnline)
            {
                HandleAlreadyOnline(player, cachedAccount);
                return;
            }
            if (player.BanObjectId > 0)
            {
                BanHistory banInfo = DaoManagerSQL.GetAccountBan(player.BanObjectId);

                if (banInfo != null && banInfo.EndDate > DateTimeUtil.Now())
                {
                    Client.SendPacket(new PROTOCOL_BASE_LOGIN_ACK(EventErrorEnum.EVENT_LOG_IN_BLOCK_ACCOUNT, player, 0U));
                    CLogger.Print($"Account with ban is Active [{player.Username}]", LoggerType.Warning);
                    Client.Close(1000, false);
                    return;
                }
            }

            CompleteSuccessfulLogin(player, cachedAccount, verificationKey);
        }
        private void HandleAlreadyOnline(Account player, Account cachedAccount)
        {
            Client.SendPacket(new PROTOCOL_BASE_LOGIN_ACK(EventErrorEnum.EVENT_LOG_IN_ALREADY_LOGIN, player, 0U));
            CLogger.Print($"Account online [{player.Username}]", LoggerType.Warning);

            if (cachedAccount != null && cachedAccount.Connection != null)
            {
                cachedAccount.SendPacket(new PROTOCOL_AUTH_ACCOUNT_KICK_ACK(1));
                cachedAccount.SendPacket(new PROTOCOL_SERVER_MESSAGE_ERROR_ACK(2147487744U));
                cachedAccount.Close(1000);
            }
            else
            {
                AuthLogin.SendLoginKickInfo(player);
            }

            Client.Close(1000, false);
        }
        private void CompleteSuccessfulLogin(Account player, Account cachedAccount, uint verificationKey)
        {
            FlagCountryLATAM();
            player.SetPlayerId(player.PlayerId, 8159 | 16);
            uint sessionKey = AllUtils.ValidateKey(player.PlayerId, Client.SessionId, verificationKey);
            Client.SendPacket(new PROTOCOL_BASE_LOGIN_ACK(
                EventErrorEnum.SUCCESS,
                player,
                sessionKey
            ));
            Client.SendPacket(new PROTOCOL_AUTH_GET_POINT_CASH_ACK(0U, player));
            if (player.ClanId > 0)
            {
                player.ClanPlayers = ClanManager.GetClanPlayers(player.ClanId, player.PlayerId);
                Client.SendPacket(new PROTOCOL_CS_MEMBER_INFO_ACK(player.ClanPlayers));
            }
            player.Status.SetData(uint.MaxValue, player.PlayerId);
            player.Status.UpdateServer(0);
            player.SetOnlineStatus(true);
            if (cachedAccount != null)
                cachedAccount.Connection = Client;
            Client.HeartBeatCounter();
            SendRefresh.RefreshAccount(player, true);

            // FL GUARD: registra IP/HWID do exe e, se configurado, queima o token OTP
            SecurityDao.TouchLoginSuccess(player.PlayerId, _ipAddress, _userFileList);
            SecurityDao.LogLogin(SecurityDao.SourceAuth, _username, player.PlayerId, "ok", _ipAddress, _userFileList, "");
            if (ConfigLoader.RequireOtpToken && ConfigLoader.OtpOneShot)
                SecurityDao.InvalidateToken(player.PlayerId);

            // Binding Auth→Game: ticket one-shot sincronizado para o Game validar USER_ENTER
            LoginSessionSync.BroadcastTicket(
                player.PlayerId,
                player.Username,
                Client.GetIPAddress(),
                sessionKey);
        }

        private void HandleInvalidCredentials(Account player)
        {
            string errorMessage = "";
            EventErrorEnum errorCode = EventErrorEnum.FAIL;

            if (player == null)
            {
                errorMessage = "Invalid username or password";
                errorCode = EventErrorEnum.LOGIN_ID_PASS_INCORRECT;
            }
            else
            {
                errorMessage = "Invalid password";
                errorCode = EventErrorEnum.LOGIN_ID_PASS_INCORRECT;
            }

            Client.SendPacket(new PROTOCOL_BASE_LOGIN_ACK(errorCode, player, 0U));
            CLogger.Print($"{errorMessage} [{_username}]", LoggerType.Warning);
            Client.Close(1000, false);
        }

        #endregion Login Processing Methods

        #region Country Flag Update
        public bool FlagCountryLATAM()
        {
            try
            {
                Account p = Client.Player;

                if (p == null)
                {
                    CLogger.Print("FlagCountryLATAM: Player is null", LoggerType.Warning);
                    return false;
                }
                if (p.CountryFlags > 0)
                {
                    return true;
                }

                string country = ResolveCountryName(Client.GetIPAddress());
                if (string.IsNullOrEmpty(country))
                {
                    CLogger.Print($"Failed to get country info for IP: {Client.GetIPAddress()}", LoggerType.Warning);
                    return false;
                }

                int countryFlag = MapLatamCountryFlag(country);
                if (ComDiv.UpdateDB("accounts", "country_flags", countryFlag, "player_id", p.PlayerId))
                {
                    p.CountryFlags = countryFlag;
                    CLogger.Print($"Country flag updated to {countryFlag} ({country}) for player {p.PlayerId}", LoggerType.Info);
                }

                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error updating country flag: {ex.Message}", LoggerType.Warning);
                return false;
            }
        }

        private static string ResolveCountryName(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return "Brazil";

            if (IPAddress.TryParse(ip, out IPAddress addr) && IsLocalOrPrivate(addr))
                return "Brazil";

            try
            {
                using (var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) })
                {
                    string json = http.GetStringAsync($"http://ip-api.com/json/{ip}?fields=status,country").GetAwaiter().GetResult();
                    using (JsonDocument doc = JsonDocument.Parse(json))
                    {
                        JsonElement root = doc.RootElement;
                        if (root.TryGetProperty("status", out JsonElement status) &&
                            status.GetString() == "success" &&
                            root.TryGetProperty("country", out JsonElement country))
                        {
                            string name = country.GetString();
                            if (!string.IsNullOrWhiteSpace(name))
                                return name;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"GeoIP lookup failed for {ip}: {ex.Message}", LoggerType.Debug);
            }

            return "Brazil";
        }

        private static bool IsLocalOrPrivate(IPAddress addr)
        {
            if (IPAddress.IsLoopback(addr))
                return true;
            if (addr.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                return false;
            byte[] b = addr.GetAddressBytes();
            return b[0] == 10
                || (b[0] == 172 && b[1] >= 16 && b[1] <= 31)
                || (b[0] == 192 && b[1] == 168)
                || (b[0] == 169 && b[1] == 254);
        }

        private static int MapLatamCountryFlag(string country)
        {
            switch (country)
            {
                case "Venezuela": return 1;
                case "Peru": return 2;
                case "Chile": return 3;
                case "Ecuador": return 4;
                case "Bolivia": return 5;
                case "Argentina": return 6;
                case "Mexico": return 7;
                case "United States": return 8;
                case "Spain": return 9;
                case "France": return 10;
                case "Colombia": return 11;
                case "Paraguay": return 12;
                case "Dominican Republic": return 13;
                case "Puerto Rico": return 14;
                case "Uruguay": return 15;
                case "Trinidad and Tobago": return 16;
                case "Portugal": return 17;
                case "Italy": return 18;
                case "Canada": return 19;
                case "Honduras": return 20;
                case "Brazil": return 0; // FrontLine BR — sem slot LATAM dedicado
                default: return 0;
            }
        }

        #endregion Country Flag Update
    }
}





////LOGIN_REQ WITH TOKEN FOR SEA//BR/ID:
//﻿using IpPublicKnowledge;
//using Plugin.Core;
//using Plugin.Core.Enums;
//using Plugin.Core.JSON;
//using Plugin.Core.Models;
//using Plugin.Core.SQL;
//using Plugin.Core.Utility;
//using Server.Auth.Data.Managers;
//using Server.Auth.Data.Models;
//using Server.Auth.Data.Sync.Server;
//using Server.Auth.Data.Utils;
//using Server.Auth.Network.ServerPacket;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.NetworkInformation;
//using System.Runtime.CompilerServices;
//using System.Security.Cryptography;

////LOGIN_REQ WITH TOKEN FOR SEA/BR/ID :
//namespace Server.Auth.Network.ClientPacket
//{

//    public class PROTOCOL_BASE_LOGIN_REQ : AuthClientPacket
//    {
//        #region Private Fields - Login Data
////LOGIN_REQ WITH TOKEN FOR SEA/BR/ID :
//        private byte _verificationByte1;
//        private byte _verificationByte2;
//        private byte _verificationByte3;
//        private byte _verificationByte4;
//        private string _username;
//        private string _password;
//        private string _token;
//        private string _clientVersion;
//        private string _ipAddress;
//        private string _screenResolution;
//        private string _userFileList;
//        private ClientLocale _clientLocale;
//        private PhysicalAddress _macAddress;
//        private IPI acs;
//        #endregion Private Fields - Login Data

//        #region Read Method

//        //Login REQ 118 ID
//        public override void Read()
//        {

//            try
//            {
//                _verificationByte1 = ReadC();
//                _verificationByte2 = ReadC();
//                _verificationByte3 = ReadC();
//                _verificationByte4 = ReadC();
//                ReadC();
//                byte[] macBytes = ReadB(6);
//                _macAddress = new PhysicalAddress(macBytes);
//                ReadB(20);
//                _screenResolution = $"{ReadH()}x{ReadH()}";
//                ReadB(10);
//                _userFileList = ReadS(ReadC());
//                ReadB(16);
//                _clientLocale = (ClientLocale)ReadC();
//                ushort versionMinor = (ushort)ReadH();
//                ReadC();
//                _clientVersion = $"{versionMinor}.0";
//                _token = ReadS(ReadH());
//                _ipAddress = Client.GetIPAddress();
//                ReadB(4);

//                string rawToken = _token ?? string.Empty;
//                string decoded = rawToken;
//                try
//                {
//                    byte[] tokenBytes = Convert.FromBase64String(rawToken);
//                    decoded = System.Text.Encoding.UTF8.GetString(tokenBytes);
//                }
//                catch { }

//                int sep = decoded.IndexOf(':');
//                if (sep > 0)
//                {
//                    _username = decoded.Substring(0, sep);
//                    _password = decoded.Substring(sep + 1);
//                }
//                else
//                {

//                    _username = decoded;
//                    _password = rawToken;
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error in Read(): {ex.Message}");
//                throw;
//            }
//        }

//        #endregion Read Method

//        #region Run Method - Main Login Logic

//        private static readonly Dictionary<string, DateTime> _lastLoginAttempts = new Dictionary<string, DateTime>();
//        private static readonly object _lockObject = new object();

//        public override void Run()
//        {
//            try
//            {
//                if (Client == null)
//                {
//                    CLogger.Print("PROTOCOL_BASE_LOGIN_REQ.Run(): Client is null", LoggerType.Error);
//                    return;
//                }

//                string clientKey = Client.GetIPAddress();

//                lock (_lockObject)
//                {
//                    if (_lastLoginAttempts.ContainsKey(clientKey))
//                    {
//                        var timeDiff = DateTime.Now - _lastLoginAttempts[clientKey];
//                        if (timeDiff.TotalMilliseconds < 1000)
//                        {
//                            CLogger.Print($"Duplicate login attempt blocked for {clientKey}", LoggerType.Warning);
//                            Client.Close(1000, false);
//                            return;
//                        }
//                    }
//                    _lastLoginAttempts[clientKey] = DateTime.Now;
//                }
//                uint verificationKey = ComDiv.Verificate(
//                    _verificationByte1,
//                    _verificationByte2,
//                    _verificationByte3,
//                    _verificationByte4
//                );

//                if (verificationKey == 0U)
//                {
//                    CLogger.Print($"Invalid verification bytes for IP: {clientKey}", LoggerType.Warning);
//                    Client.Close(1000, false);
//                    return;
//                }

//                if (ConfigLoader.UseCryptedPassword)
//                    _password = Bitwise.HashString(_password, ConfigLoader.CryptedPasswordSalt);

//                if (ConfigLoader.DebugMode)
//                    CLogger.Print($"[LOGIN] username='{_username}', password='{_password}', token='{_token}', crypted={ConfigLoader.UseCryptedPassword}", LoggerType.Debug);
//                ServerConfig config = AuthXender.Client.Config;

//                if (!ValidateInitialRequirements(config))
//                {
//                    HandleInitialValidationFailure(config);
//                    return;
//                }
//                Account player = Client.Player = AccountManager.GetAccountDB(_username, _password, 2, 95);

//                if (player == null && ConfigLoader.AutoAccount)
//                {
//                    if (!AccountManager.CreateAccount(out player, _username, _password))
//                    {
//                        Client.SendPacket(new PROTOCOL_BASE_LOGIN_ACK(EventErrorEnum.LOGIN_DELETE_ACCOUNT, null, 0U));
//                        CLogger.Print($"Failed to create account automatically [{_username}]", LoggerType.Warning);
//                        Client.Close(1000, false);
//                        return;
//                    }

//                    Client.Player = player;
//                    CLogger.Print($"Account created automatically [{_username}]", LoggerType.Info);
//                }

//                if (player != null)
//                {
//                    ProcessLogin(player, config, verificationKey);
//                }
//                else
//                {
//                    HandleInvalidCredentials(null);
//                }
//            }
//            catch (Exception ex)
//            {
//                CLogger.Print(ex.Message, LoggerType.Error, ex);
//            }
//        }

//        #endregion Run Method - Main Login Logic

//        #region Validation Methods

//        private bool ValidateInitialRequirements(ServerConfig config)
//        {
//            if (config == null)
//                return false;

//            if (!ConfigLoader.IsTestMode && !ConfigLoader.GameLocales.Contains(_clientLocale))
//                return false;

//            if (_username.Length < ConfigLoader.MinUserSize || _username.Length > ConfigLoader.MaxUserSize)
//                return false;

//            if (!ConfigLoader.IsTestMode && _password.Length < ConfigLoader.MinPassSize)
//                return false;

//            if (_macAddress.GetAddressBytes().SequenceEqual(new byte[6]))
//                return false;

//            if (_clientVersion != config.ClientVersion)
//                return false;

//            if (config.AccessUFL && _userFileList != config.UserFileList)
//                return false;

//            if (_screenResolution.Equals("0x0") || ResolutionJSON.GetDisplay(_screenResolution).Equals("Invalid"))
//                return false;

//            return true;
//        }

//        private void HandleInitialValidationFailure(ServerConfig config)
//        {
//            string errorMessage = GetValidationErrorMessage(config);
//            Client.SendPacket(new PROTOCOL_SERVER_MESSAGE_DISCONNECTIONSUCCESS_ACK(2147483904U, false));
//            CLogger.Print(errorMessage, LoggerType.Warning);
//            Client.Close(1000, true);
//        }


//        private string GetValidationErrorMessage(ServerConfig config)
//        {
//            if (config == null)
//                return $"Invalid server config [{_username}]";

//            if (!ConfigLoader.IsTestMode && !ConfigLoader.GameLocales.Contains(_clientLocale))
//                return $"Country: {_clientLocale} of blocked client [{_username}]";

//            if (_username.Length < ConfigLoader.MinUserSize)
//                return $"Username too short [{_username}]";

//            if (_username.Length > ConfigLoader.MaxUserSize)
//                return $"Username too long [{_username}]";

//            if (!ConfigLoader.IsTestMode && _password.Length < ConfigLoader.MinPassSize)
//                return $"Password too short [{_username}]";

//            if (!ConfigLoader.IsTestMode && _password.Length > ConfigLoader.MaxPassSize)
//                return $"Password too long [{_username}]";

//            if (_macAddress.GetAddressBytes().SequenceEqual(new byte[6]))
//                return $"Invalid MAC Address [{_username}]";

//            if (_clientVersion != config.ClientVersion)
//                return $"Version: {_clientVersion} not supported (expected: {config.ClientVersion}) [{_username}]";

//            if (config.AccessUFL && _userFileList != config.UserFileList)
//                return $"UserFileList: {_userFileList} not supported [{_username}]";

//            if (_screenResolution.Equals("0x0") || ResolutionJSON.GetDisplay(_screenResolution).Equals("Invalid"))
//                return $"Invalid {_screenResolution} resolution [{_username}]";

//            return $"Unknown validation error [{_username}]";
//        }

//        #endregion Validation Methods

//        #region Login Processing Methods


//        private void ProcessLogin(Account player, ServerConfig config, uint verificationKey)
//        {

//            BanHistory activeBan = DaoManagerSQL.GetActiveBanForPlayer(player.PlayerId);

//            if (activeBan != null && activeBan.EndDate > DateTimeUtil.Now())
//            {
//                if (player.BanObjectId != activeBan.ObjectId)
//                {
//                    player.BanObjectId = activeBan.ObjectId;
//                    ComDiv.UpdateDB("accounts", "ban_object_id", activeBan.ObjectId, "player_id", player.PlayerId);
//                }

//                Client.SendPacket(new PROTOCOL_BASE_LOGIN_ACK(EventErrorEnum.EVENT_LOG_IN_BLOCK_ACCOUNT, player, 0U));
//                CLogger.Print($"Account is banned until {activeBan.EndDate:yyyy-MM-dd HH:mm:ss} [{player.Username}]", LoggerType.Warning);
//                Client.Close(1000, false);
//                return;
//            }


//            if (player.IsBanned())
//            {
//                Client.SendPacket(new PROTOCOL_BASE_LOGIN_ACK(EventErrorEnum.EVENT_LOG_IN_BLOCK_ACCOUNT, player, 0U));
//                CLogger.Print($"Permanently banned [{player.Username}]", LoggerType.Warning);
//                Client.Close(1000, false);
//                return;
//            }


//            if (player.MacAddress != _macAddress)
//                ComDiv.UpdateDB("accounts", "mac_address", _macAddress, "player_id", player.PlayerId);


//            bool isMacBanned;
//            bool isIpBanned;
//            DaoManagerSQL.GetBanStatus($"{_macAddress}", _ipAddress, out isMacBanned, out isIpBanned);

//            if (isMacBanned || isIpBanned)
//            {
//                CLogger.Print(
//                    $"{(isMacBanned ? "MAC Address blocked" : "IP4 Address blocked")} [{player.Username}]",
//                    LoggerType.Warning
//                );
//                Client.SendPacket(new PROTOCOL_BASE_LOGIN_ACK(
//                    isIpBanned ? EventErrorEnum.LOGIN_BLOCK_IP : EventErrorEnum.EVENT_LOG_IN_BLOCK_ACCOUNT,
//                    player,
//                    0U
//                ));
//                Client.Close(1000, false);
//                return;
//            }


//            bool hasAccess = (player.IsGM() && config.OnlyGM) ||
//                             (player.AuthLevel() >= AccessLevel.NORMAL && !config.OnlyGM);

//            if (!hasAccess)
//            {
//                Client.SendPacket(new PROTOCOL_BASE_LOGIN_ACK(EventErrorEnum.EVENT_LOG_IN_TIME_OUT_2, player, 0U));
//                CLogger.Print($"Invalid access level [{player.Username}]", LoggerType.Warning);
//                Client.Close(1000, false);
//                return;
//            }

//            Account cachedAccount = AccountManager.GetAccount(player.PlayerId, true);

//            if (player.IsOnline)
//            {
//                HandleAlreadyOnline(player, cachedAccount);
//                return;
//            }

//            if (player.BanObjectId > 0)
//            {
//                BanHistory banInfo = DaoManagerSQL.GetAccountBan(player.BanObjectId);

//                if (banInfo != null && banInfo.EndDate > DateTimeUtil.Now())
//                {
//                    Client.SendPacket(new PROTOCOL_BASE_LOGIN_ACK(EventErrorEnum.EVENT_LOG_IN_BLOCK_ACCOUNT, player, 0U));
//                    CLogger.Print($"Account with ban is Active [{player.Username}]", LoggerType.Warning);
//                    Client.Close(1000, false);
//                    return;
//                }
//            }

//            CompleteSuccessfulLogin(player, cachedAccount, verificationKey);
//        }
//        private void HandleAlreadyOnline(Account player, Account cachedAccount)
//        {
//            Client.SendPacket(new PROTOCOL_BASE_LOGIN_ACK(EventErrorEnum.EVENT_LOG_IN_ALREADY_LOGIN, player, 0U));
//            CLogger.Print($"Account online [{player.Username}]", LoggerType.Warning);

//            if (cachedAccount != null && cachedAccount.Connection != null)
//            {
//                cachedAccount.SendPacket(new PROTOCOL_AUTH_ACCOUNT_KICK_ACK(1));
//                cachedAccount.SendPacket(new PROTOCOL_SERVER_MESSAGE_ERROR_ACK(2147487744U));
//                cachedAccount.Close(1000);
//            }
//            else
//            {
//                AuthLogin.SendLoginKickInfo(player);
//            }

//            Client.Close(1000, false);
//        }
//        private void CompleteSuccessfulLogin(Account player, Account cachedAccount, uint verificationKey)
//        {
//            FlagCountryLATAM();
//            player.SetPlayerId(player.PlayerId, 8159);
//            Client.SendPacket(new PROTOCOL_BASE_LOGIN_ACK(
//                EventErrorEnum.SUCCESS,
//                player,
//                AllUtils.ValidateKey(player.PlayerId, Client.SessionId, verificationKey)
//            ));

//            Client.SendPacket(new PROTOCOL_AUTH_GET_POINT_CASH_ACK(0U, player));

//            if (player.ClanId > 0)
//            {
//                player.ClanPlayers = ClanManager.GetClanPlayers(player.ClanId, player.PlayerId);
//                Client.SendPacket(new PROTOCOL_CS_MEMBER_INFO_ACK(player.ClanPlayers));
//            }

//            player.Status.SetData(uint.MaxValue, player.PlayerId);
//            player.Status.UpdateServer(0);
//            player.SetOnlineStatus(true);

//            if (cachedAccount != null)
//                cachedAccount.Connection = Client;

//            Client.HeartBeatCounter();
//            SendRefresh.RefreshAccount(player, true);

//            CLogger.Print($"Login successful [{player.Username}] IP: {_ipAddress}", LoggerType.Info);
//        }

//        private void HandleInvalidCredentials(Account player)
//        {
//            EventErrorEnum errorCode = EventErrorEnum.LOGIN_ID_PASS_INCORRECT;

//            Client.SendPacket(new PROTOCOL_BASE_LOGIN_ACK(errorCode, player, 0U));
//            CLogger.Print(
//                player == null
//                    ? $"Invalid username or password [{_username}]"
//                    : $"Invalid password [{_username}]",
//                LoggerType.Warning
//            );
//            Client.Close(1000, false);
//        }

//        #endregion Login Processing Methods

//        #region Country Flag Update

//        public bool FlagCountryLATAM()
//        {
//            try
//            {
//                Account p = Client.Player;

//                if (p == null)
//                {
//                    CLogger.Print("FlagCountryLATAM: Player is null", LoggerType.Warning);
//                    return false;
//                }

//                if (p.CountryFlags > 0)
//                    return true;

//                acs = IPK.GetIpInfo(Client.GetAddress());

//                if (acs == null || string.IsNullOrEmpty(acs.country))
//                {
//                    CLogger.Print($"Failed to get country info for IP: {Client.GetAddress()}", LoggerType.Warning);
//                    return false;
//                }

//                int countryFlag;
//                switch (acs.country)
//                {
//                    case "Venezuela": countryFlag = 1; break;
//                    case "Peru": countryFlag = 2; break;
//                    case "Chile": countryFlag = 3; break;
//                    case "Ecuador": countryFlag = 4; break;
//                    case "Bolivia": countryFlag = 5; break;
//                    case "Argentina": countryFlag = 6; break;
//                    case "Mexico": countryFlag = 7; break;
//                    case "United States": countryFlag = 8; break;
//                    case "Spain": countryFlag = 9; break;
//                    case "France": countryFlag = 10; break;
//                    case "Colombia": countryFlag = 11; break;
//                    case "Paraguay": countryFlag = 12; break;
//                    case "Dominican Republic": countryFlag = 13; break;
//                    case "Puerto Rico": countryFlag = 14; break;
//                    case "Uruguay": countryFlag = 15; break;
//                    case "Trinidad and Tobago": countryFlag = 16; break;
//                    case "Portugal": countryFlag = 17; break;
//                    case "Italy": countryFlag = 18; break;
//                    case "Canada": countryFlag = 19; break;
//                    case "Honduras": countryFlag = 20; break;
//                    default: countryFlag = 0; break;
//                }

//                if (ComDiv.UpdateDB("accounts", "country_flags", countryFlag, "player_id", p.PlayerId))
//                {
//                    p.CountryFlags = countryFlag;
//                    CLogger.Print(
//                        $"Country flag updated to {countryFlag} ({acs.country}) for player {p.PlayerId}",
//                        LoggerType.Info
//                    );
//                }

//                return true;
//            }
//            catch (Exception ex)
//            {
//                CLogger.Print($"Error updating country flag: {ex.Message}", LoggerType.Error, ex);
//                return false;
//            }
//        }

//        #endregion Country Flag Update
//    }
//}
