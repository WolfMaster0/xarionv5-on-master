// This file is part of the OpenNos NosTale Emulator Project.
//
// This program is licensed under a deviated version of the Fair Source License, granting you a
// non-exclusive, non-transferable, royalty-free and fully-paid-up license, under all of the
// Licensor's copyright and patent rights, to use, copy, prepare derivative works of, publicly
// perform and display the Software, subject to the conditions found in the LICENSE file.
//
// THIS FILE IS PROVIDED "AS IS", WITHOUT WARRANTY OR CONDITION, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. THE AUTHORS HEREBY DISCLAIM ALL LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT
// OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE.

using System.Linq;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$Demote", Authority = AuthorityType.GameMaster)]
    public class DemotePacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public string Name { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            if (session is ClientSession sess)
            {
                string[] packetSplit = packet.Split(' ');
                if (packetSplit.Length < 3)
                {
                    sess.SendPacket(sess.Character.GenerateSay(ReturnHelp(), 10));
                    return;
                }
                DemotePacket packetDefinition = new DemotePacket();
                if (!string.IsNullOrWhiteSpace(packetSplit[2]))
                {
                    packetDefinition._isParsed = true;
                    packetDefinition.Name = packetSplit[2];
                }
                packetDefinition.ExecuteHandler(sess);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(DemotePacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$Demote NAME";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(),
                    $"[Demote]CharacterName: {Name}");

                AccountDTO account = DAOFactory.AccountDAO.LoadById(DAOFactory.CharacterDAO.LoadByName(Name).AccountId);
                if (account?.Authority > AuthorityType.User)
                {
                    account.Authority--;
                    DAOFactory.AccountDAO.InsertOrUpdate(ref account);
                    ClientSession sess =
                        ServerManager.Instance.Sessions.FirstOrDefault(s => s.Character?.Name == Name);
                    if (sess != null)
                    {
                        sess.Account.Authority--;
                        sess.Character.Authority--;
                        ServerManager.Instance.ChangeMap(sess.Character.CharacterId);
                        DAOFactory.AccountDAO.WriteGeneralLog(sess.Account.AccountId, sess.IpAddress,
                            sess.Character.CharacterId, GeneralLogType.Demotion, $"by: {session.Character.Name}");
                    }
                    else
                    {
                        DAOFactory.AccountDAO.WriteGeneralLog(account.AccountId, "127.0.0.1", null,
                            GeneralLogType.Demotion, $"by: {session.Character.Name}");
                    }

                    session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
                }
                else
                {
                    session.SendPacket(
                        session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                }
            }
            else
            {
                session.SendPacket(session.Character.GenerateSay(ReturnHelp(), 10));
            }
        }

        #endregion
    }
}