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
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Handler.Packets.SharedPacketMethods;

namespace OpenNos.Handler.Packets.WorldPackets.ScriptedInstancePackets
{
    [PacketHeader("wreq")]
    public class WreqPacket
    {
        #region Properties

        public byte? Parameter { get; set; }

        public byte Value { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 3)
            {
                return;
            }
            WreqPacket packetDefinition = new WreqPacket();
            if (byte.TryParse(packetSplit[2], out byte value))
            {
                packetDefinition.Value = value;
                packetDefinition.Parameter = packetSplit.Length >= 4 && byte.TryParse(packetSplit[3], out byte parameter) ? parameter : (byte?)null;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(WreqPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            foreach (ScriptedInstance portal in session.CurrentMapInstance.ScriptedInstances)
            {
                if (session.Character.PositionY >= portal.PositionY - 1
                    && session.Character.PositionY <= portal.PositionY + 1
                    && session.Character.PositionX >= portal.PositionX - 1
                    && session.Character.PositionX <= portal.PositionX + 1)
                {
                    switch (Value)
                    {
                        case 0:
                            if (session.Character.Group?.Characters.Any(s =>
                                    s.CurrentMapInstance.InstanceBag?.Lock == false
                                    && s.CurrentMapInstance.MapInstanceType == MapInstanceType.TimeSpaceInstance
                                    && s.Character.MapId == portal.MapId
                                    && s.Character.CharacterId != session.Character.CharacterId
                                    && s.Character.MapX == portal.PositionX
                                    && s.Character.MapY == portal.PositionY) == true)
                            {
                                session.SendPacket(UserInterfaceHelper.GenerateDialog(
                                    $"#wreq^3^{session.Character.CharacterId} #wreq^0^1 {Language.Instance.GetMessageFromKey("ASK_JOIN_TEAM_TS")}"));
                            }
                            else
                            {
                                session.SendPacket(portal.GenerateRbr());
                            }

                            break;

                        case 1:
                            if (!Parameter.HasValue)
                            {
                                session.EnterInstance(portal);
                            }
                            else if (Parameter.Value == 1)
                            {
                                TreqPacket.HandlePacket(session, $"1 treq {portal.PositionX} {portal.PositionY} 1 1");
                            }
                            break;

                        case 3:
                            if (!Parameter.HasValue)
                            {
                                return;
                            }
                            ClientSession clientSession =
                                session.Character.Group?.Characters.Find(s => s.Character.CharacterId == Parameter.Value);
                            if (clientSession != null)
                            {
                                if (session.Character.Level < portal.LevelMinimum)
                                {
                                    session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                        Language.Instance.GetMessageFromKey("NOT_REQUIERED_LEVEL"), 0));
                                    return;
                                }

                                MapCell mapcell = clientSession.CurrentMapInstance.Map.GetRandomPosition();
                                session.Character.MapX = portal.PositionX;
                                session.Character.MapY = portal.PositionY;
                                ServerManager.Instance.ChangeMapInstance(session.Character.CharacterId,
                                    clientSession.CurrentMapInstance.MapInstanceId, mapcell.X, mapcell.Y);
                                session.SendPacket(portal.GenerateMainInfo());
                                session.SendPackets(portal.GenerateMinimap());
                                session.SendPacket(portal.FirstMap.InstanceBag.GenerateScore());
                                session.Character.Timespace = portal;
                            }

                            // TODO: Implement
                            break;
                    }
                }
            }
        }

        #endregion
    }
}