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
    [PacketHeader("$BlockExp", Authority = AuthorityType.GameMaster)]
    public class BlockExpPacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public string CharacterName { get; set; }

        public int Duration { get; set; }

        public string Reason { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(new[] { ' ' }, 5);
            if (!(session is ClientSession sess))
            {
                return;
            }
            if (packetSplit.Length < 4)
            {
                sess.SendPacket(sess.Character.GenerateSay(ReturnHelp(), 10));
                return;
            }
            BlockExpPacket packetDefinition = new BlockExpPacket();
            if (!string.IsNullOrEmpty(packetSplit[2])
                && int.TryParse(packetSplit[3], out int duration))
            {
                packetDefinition._isParsed = true;
                packetDefinition.CharacterName = packetSplit[2];
                packetDefinition.Duration = duration;
                packetDefinition.Reason = string.IsNullOrEmpty(packetSplit[4]) ? string.Empty : packetSplit[4].Trim();
            }
            packetDefinition.ExecuteHandler(sess);
        }

        public static void Register() => PacketFacility.AddHandler(typeof(BlockExpPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$BlockExp CHARACTERNAME DURATION REASON";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(),
                    $"[BlockExp]CharacterName: {CharacterName} Reason: {Reason} Until: {DateTime.UtcNow.AddMinutes(Duration)}");

                if (Duration == 0)
                {
                    Duration = 60;
                }
                CharacterDTO character = DAOFactory.CharacterDAO.LoadByName(CharacterName);
                if (character != null)
                {
                    ClientSession targetSession =
                        ServerManager.Instance.Sessions.FirstOrDefault(s =>
                            s.Character?.Name == CharacterName);
                    targetSession?.SendPacket(Duration == 1
                        ? UserInterfaceHelper.GenerateInfo(
                            string.Format(Language.Instance.GetMessageFromKey("MUTED_SINGULAR"), Reason))
                        : UserInterfaceHelper.GenerateInfo(string.Format(
                            Language.Instance.GetMessageFromKey("MUTED_PLURAL"), Reason,
                            Duration)));
                    PenaltyLogDTO log = new PenaltyLogDTO
                    {
                        AccountId = character.AccountId,
                        Reason = Reason,
                        Penalty = PenaltyType.BlockExp,
                        DateStart = DateTime.UtcNow,
                        DateEnd = DateTime.UtcNow.AddMinutes(Duration),
                        AdminName = targetSession?.Character.Name
                    };
                    Character.InsertOrUpdatePenalty(log);
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