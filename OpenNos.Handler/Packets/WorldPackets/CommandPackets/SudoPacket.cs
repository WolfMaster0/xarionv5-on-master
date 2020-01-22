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
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$Sudo", Authority = AuthorityType.GameMaster)]
    public class SudoPacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public string CharacterName { get; set; }

        public string CommandContents { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            if (session is ClientSession sess)
            {
                string[] packetSplit = packet.Split(new[] { ' ' }, 4);
                if (packetSplit.Length < 4)
                {
                    sess.SendPacket(sess.Character.GenerateSay(ReturnHelp(), 10));
                    return;
                }
                SudoPacket packetDefinition = new SudoPacket();
                if (!string.IsNullOrWhiteSpace(packetSplit[2]) && !string.IsNullOrWhiteSpace(packetSplit[3]))
                {
                    packetDefinition._isParsed = true;
                    packetDefinition.CharacterName = packetSplit[2];
                    packetDefinition.CommandContents = packetSplit[3];
                }
                packetDefinition.ExecuteHandler(sess);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(SudoPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$Sudo NAME COMMAND";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(),
                    $"[Sudo]CharacterName: {CharacterName} CommandContents:{CommandContents}");
                string message =
                    $"INFO: {session.Character.Name} used Sudo on {(CharacterName == "*" ? "*Everyone*" : $"\"{CharacterName}\"")} with Content \"{CommandContents}\"!";
                foreach (ClientSession sess in ServerManager.Instance.Sessions.Where(s=>s.Account.Authority == AuthorityType.GameMaster))
                {
                    sess.SendPacket(UserInterfaceHelper.GenerateSay(message, 10));
                    sess.SendPacket(UserInterfaceHelper.GenerateMsg(message, 2));
                }
                if (CharacterName == "*")
                {
                    foreach (ClientSession sess in session.CurrentMapInstance.Sessions)
                    {
                        sess.ReceivePacket(CommandContents, true);
                    }
                }
                else
                {
                    ClientSession sess = ServerManager.Instance.GetSessionByCharacterName(CharacterName);

                    if (sess != null && !string.IsNullOrWhiteSpace(CommandContents))
                    {
                        sess.ReceivePacket(CommandContents, true);
                    }
                    else
                    {
                        session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED"), 0));
                    }
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