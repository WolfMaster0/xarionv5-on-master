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

using System;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.BasicPackets
{
    [PacketHeader("compl")]
    public class ComplimentPacket
    {
        #region Properties

        public long CharacterId { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 3)
            {
                return;
            }
            ComplimentPacket packetDefinition = new ComplimentPacket();
            if (long.TryParse(packetSplit[3], out long charId))
            {
                packetDefinition.CharacterId = charId;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(ComplimentPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            ClientSession sess = ServerManager.Instance.GetSessionByCharacterId(CharacterId);
            if (sess != null)
            {
                if (session.Character.Level >= 30)
                {
                    GeneralLogDTO dto =
                        session.Character.GeneralLogs.LastOrDefault(s =>
                            s.LogData == "World" && s.LogType == "Connection");
                    GeneralLogDTO lastcompliment =
                        session.Character.GeneralLogs.LastOrDefault(s =>
                            s.LogData == "World" && s.LogType == "Compliment");
                    if (dto?.Timestamp.AddMinutes(60) <= DateTime.UtcNow)
                    {
                        if (lastcompliment == null || lastcompliment.Timestamp.AddDays(1) <= DateTime.UtcNow.Date)
                        {
                            sess.Character.Compliment++;
                            session.SendPacket(session.Character.GenerateSay(
                                string.Format(Language.Instance.GetMessageFromKey("COMPLIMENT_GIVEN"),
                                    sess.Character.Name), 12));
                            session.Character.GeneralLogs.Add(new GeneralLogDTO
                            {
                                AccountId = session.Account.AccountId,
                                CharacterId = session.Character.CharacterId,
                                IpAddress = session.IpAddress,
                                LogData = "World",
                                LogType = "Compliment",
                                Timestamp = DateTime.UtcNow
                            });

                            session.CurrentMapInstance?.Broadcast(session,
                                session.Character.GenerateSay(
                                    string.Format(Language.Instance.GetMessageFromKey("COMPLIMENT_RECEIVED"),
                                        session.Character.Name), 12), ReceiverType.OnlySomeone,
                                characterId: CharacterId);
                        }
                        else
                        {
                            session.SendPacket(
                                session.Character.GenerateSay(
                                    Language.Instance.GetMessageFromKey("COMPLIMENT_COOLDOWN"), 11));
                        }
                    }
                    else if (dto != null)
                    {
                        session.SendPacket(session.Character.GenerateSay(
                            string.Format(Language.Instance.GetMessageFromKey("COMPLIMENT_LOGIN_COOLDOWN"),
                                (dto.Timestamp.AddMinutes(60) - DateTime.UtcNow).Minutes), 11));
                    }
                }
                else
                {
                    session.SendPacket(
                        session.Character.GenerateSay(Language.Instance.GetMessageFromKey("COMPLIMENT_NOT_MINLVL"),
                            11));
                }
            }
        }

        #endregion
    }
}