// This file is part of the OpenNos NosTale Emulator Project.
// 
// This program is licensed under a deviated version of the Fair Source License,
// granting you a non-exclusive, non-transferable, royalty-free and fully-paid-up
// license, under all of the Licensor's copyright and patent rights, to use, copy, prepare
// derivative works of, publicly perform and display the Software, subject to the
// conditions found in the LICENSE file.
// 
// THIS FILE IS PROVIDED "AS IS", WITHOUT WARRANTY OR
// CONDITION, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. THE AUTHORS HEREBY DISCLAIM ALL LIABILITY, WHETHER IN
// AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE.
using OpenNos.Core;
using OpenNos.Core.Cryptography;
using OpenNos.Core.Serializing;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using System;
using System.Configuration;
using System.Linq;

namespace OpenNos.Handler.Packets.LoginPackets
{
    [PacketHeader("NoS0575", CharacterRequired = false)]
    public class LoginPacket
    {
        #region Properties

        public string ClientData { get; set; }

        public string Name { get; set; }

        public int Number { get; set; }

        public string Password { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 6)
            {
                return;
            }
            LoginPacket loginPacket = new LoginPacket();
            if (int.TryParse(packetSplit[1], out int number))
            {
                loginPacket.Number = number;
                loginPacket.Name = packetSplit[2];
                loginPacket.Password = packetSplit[3];
                loginPacket.ClientData = packetSplit[4];
                loginPacket.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(LoginPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            string BuildServersPacket(string username, int sessionId, bool ignoreUserName)
            {
                string channelPacket = CommunicationServiceClient.Instance.RetrieveRegisteredWorldServers(username, sessionId, ignoreUserName);

                if (channelPacket?.Contains(':') != true)
                {
                    // no need for this as in release the debug is ignored eitherway
                    //if (ServerManager.Instance.IsDebugMode)
                    Logger.Debug("Could not retrieve Worldserver groups. Please make sure they've already been registered.");

                    // find a new way to display this message
                    //Session.SendPacket($"fail {Language.Instance.GetMessageFromKey("NO_WORLDSERVERS")}");
                    session.SendPacket($"fail {Language.Instance.GetMessageFromKey("IDERROR")}");
                }

                return channelPacket;
            }

            UserDTO user = new UserDTO
            {
                Name = Name,
                Password = ConfigurationManager.AppSettings["UseOldCrypto"] == "true" ? CryptographyBase.Sha512(LoginCryptography.GetPassword(Password)).ToUpper() : Password
            };
            AccountDTO loadedAccount = DAOFactory.AccountDAO.LoadByName(user.Name);
            if (loadedAccount?.Password.ToUpper().Equals(user.Password) == true)
            {
                string ipAddress = session.IpAddress;
                DAOFactory.AccountDAO.WriteGeneralLog(loadedAccount.AccountId, ipAddress, null, GeneralLogType.Connection, "LoginServer");

                //check if the account is connected
                if (!CommunicationServiceClient.Instance.IsAccountConnected(loadedAccount.AccountId))
                {
                    AuthorityType type = loadedAccount.Authority;
                    PenaltyLogDTO penalty = DAOFactory.PenaltyLogDAO.LoadByAccount(loadedAccount.AccountId).FirstOrDefault(s => s.DateEnd > DateTime.UtcNow && s.Penalty == PenaltyType.Banned);
                    if (penalty != null)
                    {
                        // find a new way to display date of ban
                        session.SendPacket($"fail {string.Format(Language.Instance.GetMessageFromKey("BANNED"), penalty.Reason, penalty.DateEnd.ToString("yyyy-MM-dd-HH:mm"))}"); ;
                    }
                    else
                    {
                        switch (type)
                        {
                            case AuthorityType.Unconfirmed:
                                {
                                    session.SendPacket($"fail {Language.Instance.GetMessageFromKey("NOTVALIDATE")}");
                                }
                                break;

                            case AuthorityType.Banned:
                                {
                                    session.SendPacket($"fail {string.Format(Language.Instance.GetMessageFromKey("BANNED"), penalty.Reason, penalty.DateEnd.ToString("yyyy-MM-dd-HH:mm"))}"); ;
                                }
                                break;

                            case AuthorityType.Closed:
                                {
                                    session.SendPacket($"fail {Language.Instance.GetMessageFromKey("IDERROR")}");
                                }
                                break;

                            default:
                                {
                                    if (loadedAccount.Authority == AuthorityType.User || loadedAccount.Authority == AuthorityType.BitchNiggerFaggot)
                                    {
                                        MaintenanceLogDTO maintenanceLog = DAOFactory.MaintenanceLogDAO.LoadFirst();
                                        if (maintenanceLog != null)
                                        {
                                            // find a new way to display date and reason of maintenance
                                            session.SendPacket($"fail {string.Format(Language.Instance.GetMessageFromKey("MAINTENANCE"), maintenanceLog.DateEnd, maintenanceLog.Reason)}");
                                            return;
                                        }
                                    }

                                    int newSessionId = SessionFactory.Instance.GenerateSessionId();
                                    Logger.Debug(string.Format(Language.Instance.GetMessageFromKey("CONNECTION"), user.Name, newSessionId));
                                    try
                                    {
                                        ipAddress = ipAddress.Substring(6, ipAddress.LastIndexOf(':') - 6);
                                        CommunicationServiceClient.Instance.RegisterAccountLogin(loadedAccount.AccountId, newSessionId, ipAddress);
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Error("General Error SessionId: " + newSessionId, ex);
                                    }
                                    string[] clientData = ClientData.Split('.');
                                    bool ignoreUserName = clientData.Length < 3 ? false : short.TryParse(clientData[3], out short clientVersion) && (clientVersion < 3075 || ConfigurationManager.AppSettings["UseOldCrypto"] == "true");
                                    session.SendPacket(BuildServersPacket(user.Name, newSessionId, ignoreUserName));
                                }
                                break;
                        }
                    }
                }
                else
                {
                    session.SendPacket($"fail {Language.Instance.GetMessageFromKey("ALREADY_CONNECTED")}");
                }
            }
            else
            {
                session.SendPacket($"fail {Language.Instance.GetMessageFromKey("IDERROR")}");
            }
        }

        #endregion
    }
}