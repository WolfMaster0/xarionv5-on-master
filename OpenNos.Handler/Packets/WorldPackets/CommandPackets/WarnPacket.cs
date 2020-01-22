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
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$Warn", Authority = AuthorityType.Moderator)]
    public class WarnPacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public string CharacterName { get; set; }

        public string Reason { get; set; }

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
                WarnPacket packetDefinition = new WarnPacket();
                if (!string.IsNullOrWhiteSpace(packetSplit[2]) && !string.IsNullOrWhiteSpace(packetSplit[3]))
                {
                    packetDefinition._isParsed = true;
                    packetDefinition.CharacterName = packetSplit[2];
                    packetDefinition.Reason = packetSplit[3];
                }
                packetDefinition.ExecuteHandler(sess);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(WarnPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$Warn NAME REASON";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(),
                    $"[Warn]CharacterName: {CharacterName} Reason: {Reason}");

                CharacterDTO character = DAOFactory.CharacterDAO.LoadByName(CharacterName);
                if (character != null)
                {
                    ClientSession sess = ServerManager.Instance.GetSessionByCharacterName(CharacterName);
                    sess?.SendPacket(UserInterfaceHelper.GenerateInfo(
                        string.Format(Language.Instance.GetMessageFromKey("WARNING"), Reason)));
                    Character.InsertOrUpdatePenalty(new PenaltyLogDTO
                    {
                        AccountId = character.AccountId,
                        Reason = Reason,
                        Penalty = PenaltyType.Warning,
                        DateStart = DateTime.UtcNow,
                        DateEnd = DateTime.UtcNow,
                        AdminName = session.Character.Name
                    });
                    switch (DAOFactory.PenaltyLogDAO.LoadByAccount(character.AccountId)
                        .Count(p => p.Penalty == PenaltyType.Warning))
                    {
                        case 1:
                            break;

                        case 2:

                            //MuteMethod(characterName, "Auto-Warning mute: 2 strikes", 30);
                            break;

                        case 3:

                            //MuteMethod(characterName, "Auto-Warning mute: 3 strikes", 60);
                            break;

                        case 4:

                            //MuteMethod(characterName, "Auto-Warning mute: 4 strikes", 720);
                            break;

                        case 5:

                            //MuteMethod(characterName, "Auto-Warning mute: 5 strikes", 1440);
                            break;

                        case 6:

                            //BanMethod(characterName, 7, "LOL SIXTY NINE AMIRITE?");
                            break;
                    }
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