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

using System;
using System.Linq;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$HelpMe", Authority = AuthorityType.User)]
    public class HelpMePacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public string Message { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            if (session is ClientSession sess)
            {
                string[] packetSplit = packet.Split(new[] {' '}, 3);
                if (packetSplit.Length < 3)
                {
                    sess.SendPacket(sess.Character.GenerateSay(ReturnHelp(), 10));
                    return;
                }

                HelpMePacket packetDefinition = new HelpMePacket();
                if (!string.IsNullOrWhiteSpace(packetSplit[2]))
                {
                    packetDefinition._isParsed = true;
                    packetDefinition.Message = packetSplit[2];
                }

                packetDefinition.ExecuteHandler(sess);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(HelpMePacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$HelpMe MESSAGE";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                if (session.Character.UsedHelpMe.AddMinutes(10) < DateTime.UtcNow)
                {
                    session.Character.UsedHelpMe = DateTime.UtcNow;
                    foreach (ClientSession team in ServerManager.Instance.Sessions.Where(s =>
                        s.Account.Authority == AuthorityType.GameMaster || s.Account.Authority == AuthorityType.Moderator))
                    {
                        if (team.HasSelectedCharacter)
                        {
                            team.SendPacket(team.Character.GenerateSay(
                                string.Format(Language.Instance.GetMessageFromKey("HELPME_NEEDSHELP"),
                                    session.Character.Name), 12));
                            team.SendPacket(team.Character.GenerateSay(
                                string.Format(Language.Instance.GetMessageFromKey("HELPME_REASON"), Message), 12));
                            team.SendPacket(session.Character.GenerateSpk("Click this message to start chatting.", 5));
                            team.SendPacket(
                                UserInterfaceHelper.GenerateMsg($"User {session.Character.Name} needs your help!", 0));
                        }
                    }

                    session.SendPacket(session.Character.GenerateSay(
                        "Our Team members were informed! You should get a message shortly.", 10));
                    session.SendPacket(session.Character.GenerateSay(
                        "In case there's noone answering, please ask for help on our Discord Server at:",
                        10));
                    session.SendPacket(session.Character.GenerateSay("https://discord.gg/Q4sDMt8", 10));
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