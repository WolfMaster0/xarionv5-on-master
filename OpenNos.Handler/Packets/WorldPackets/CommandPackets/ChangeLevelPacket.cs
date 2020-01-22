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

using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$Lvl", Authority = AuthorityType.GameMaster)]
    public class ChangeLevelPacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public byte Level { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (!(session is ClientSession sess))
            {
                return;
            }

            if (packetSplit.Length < 3)
            {
                sess.SendPacket(sess.Character.GenerateSay(ReturnHelp(), 10));
                return;
            }

            ChangeLevelPacket packetDefinition = new ChangeLevelPacket();
            if (byte.TryParse(packetSplit[2], out byte level))
            {
                packetDefinition._isParsed = true;
                packetDefinition.Level = level;
            }

            packetDefinition.ExecuteHandler(sess);
        }

        public static void Register() => PacketFacility.AddHandler(typeof(ChangeLevelPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$Lvl LEVEL";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(), $"[Lvl]Level: {Level}");

                if (Level > 0)
                {
                    session.Character.Level = Level;
                    session.Character.LevelXp = 0;
                    session.Character.Hp = (int)session.Character.HPLoad();
                    session.Character.Mp = (int)session.Character.MPLoad();
                    session.SendPacket(session.Character.GenerateStat());
                    session.SendPacket(session.Character.GenerateStatChar());
                    session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("LEVEL_CHANGED"), 0));
                    session.SendPacket(session.Character.GenerateLev());
                    session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateIn(),
                        ReceiverType.AllExceptMe);
                    session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateGidx(),
                        ReceiverType.AllExceptMe);
                    session.CurrentMapInstance?.Broadcast(
                        StaticPacketHelper.GenerateEff(UserType.Player, session.Character.CharacterId, 6),
                        session.Character.PositionX, session.Character.PositionY);
                    session.CurrentMapInstance?.Broadcast(
                        StaticPacketHelper.GenerateEff(UserType.Player, session.Character.CharacterId, 198),
                        session.Character.PositionX, session.Character.PositionY);
                    ServerManager.Instance.UpdateGroup(session.Character.CharacterId);
                    if (session.Character.Family != null)
                    {
                        ServerManager.Instance.FamilyRefresh(session.Character.Family.FamilyId);
                        CommunicationServiceClient.Instance.SendMessageToCharacter(new ScsCharacterMessage
                        {
                            DestinationCharacterId = session.Character.Family.FamilyId,
                            SourceCharacterId = session.Character.CharacterId,
                            SourceWorldId = ServerManager.Instance.WorldId,
                            Message = "fhis_stc",
                            Type = MessageType.Family
                        });
                    }
                }
                else
                {
                    session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
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