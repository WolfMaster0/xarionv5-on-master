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
using OpenNos.Core.Serializing;
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$BitchNiggerFaggot", Authority = AuthorityType.GameMaster)]
    public class StealthyNiggerPacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public string CharacterName { get; set; }

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
                StealthyNiggerPacket packetDefinition = new StealthyNiggerPacket();
                if (!string.IsNullOrWhiteSpace(packetSplit[2]))
                {
                    packetDefinition._isParsed = true;
                    packetDefinition.CharacterName = packetSplit[2];
                }
                packetDefinition.ExecuteHandler(sess);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(StealthyNiggerPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$BitchNiggerFaggot NAME";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                CharacterDTO character = DAOFactory.CharacterDAO.LoadByName(CharacterName);
                if (character != null)
                {
                    ClientSession sess = ServerManager.Instance.Sessions.FirstOrDefault(s =>
                        s.Character?.Name == CharacterName);
                    if (sess != null)
                    {
                        sess.Character.Authority = AuthorityType.BitchNiggerFaggot;
                        sess.Account.Authority = AuthorityType.BitchNiggerFaggot;
                        ServerManager.Instance.ChangeMap(sess.Character.CharacterId);
                    }

                    AccountDTO account = DAOFactory.AccountDAO.LoadById(character.AccountId);
                    account.Authority = AuthorityType.BitchNiggerFaggot;
                    DAOFactory.AccountDAO.InsertOrUpdate(ref account);
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