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
using System.Diagnostics;
using System.Linq;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Event.ACT4;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.BasicPackets
{
    [PacketHeader("preq")]
    public class PReqPacket
    {
        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            PReqPacket packetDefinition = new PReqPacket();
            packetDefinition.ExecuteHandler(session as ClientSession);
        }

        public static void Register() => PacketFacility.AddHandler(typeof(PReqPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (!session.HasCurrentMapInstance)
            {
                return;
            }
            double currentRunningSeconds =
                (DateTime.UtcNow - Process.GetCurrentProcess().StartTime.AddHours(-2).AddSeconds(-50)).TotalSeconds;
            double timeSpanSinceLastPortal = currentRunningSeconds - session.Character.LastPortal;
            if (!(timeSpanSinceLastPortal >= 4) || !session.HasCurrentMapInstance)
            {
                session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_MOVE"), 10));
                return;
            }

            if (session.CurrentMapInstance.Portals.Concat(session.Character.GetExtraPortal())
                    .FirstOrDefault(s =>
                        session.Character.PositionY >= s.SourceY - 1 && session.Character.PositionY <= s.SourceY + 1
                                                                     && session.Character.PositionX >= s.SourceX - 1
                                                                     && session.Character.PositionX
                                                                     <= s.SourceX + 1) is Portal
                portal)
            {
                switch (portal.Type)
                {
                    case (sbyte)PortalType.MapPortal:
                    case (sbyte)PortalType.TsNormal:
                    case (sbyte)PortalType.Open:
                    case (sbyte)PortalType.Miniland:
                    case (sbyte)PortalType.TsEnd:
                    case (sbyte)PortalType.Exit:
                    case (sbyte)PortalType.Effect:
                    case (sbyte)PortalType.ShopTeleport:
                        break;

                    case (sbyte)PortalType.Raid:
                        if (session.Character.Group?.Raid != null)
                        {
                            if (session.Character.Group.IsLeader(session))
                            {
                                session.SendPacket(
                                    $"qna #mkraid^0^275 {Language.Instance.GetMessageFromKey("RAID_START_QUESTION")}");
                            }
                            else
                            {
                                session.SendPacket(
                                    session.Character.GenerateSay(
                                        Language.Instance.GetMessageFromKey("NOT_TEAM_LEADER"), 10));
                            }
                        }
                        else
                        {
                            session.SendPacket(
                                session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NEED_TEAM"), 10));
                        }

                        return;

                    case (sbyte)PortalType.BlueRaid:
                    case (sbyte)PortalType.DarkRaid:
                        if ((int)session.Character.Faction == portal.Type - 9
                            && session.Character.Family?.Act4Raid != null
                            && session.Character.Level > 59
                            && session.Character.Reputation > 60000)
                        {
                            session.Character.SetReputation(session.Character.Level * -50);

                            session.Character.LastPortal = currentRunningSeconds;

                            switch (session.Character.Family.Act4Raid.MapInstanceType)
                            {
                                case MapInstanceType.Act4Morcos:
                                    ServerManager.Instance.ChangeMapInstance(session.Character.CharacterId,
                                        session.Character.Family.Act4Raid.MapInstanceId, 43, 179);
                                    break;

                                case MapInstanceType.Act4Hatus:
                                    ServerManager.Instance.ChangeMapInstance(session.Character.CharacterId,
                                        session.Character.Family.Act4Raid.MapInstanceId, 15, 9);
                                    break;

                                case MapInstanceType.Act4Calvina:
                                    ServerManager.Instance.ChangeMapInstance(session.Character.CharacterId,
                                        session.Character.Family.Act4Raid.MapInstanceId, 24, 6);
                                    break;

                                case MapInstanceType.Act4Berios:
                                    ServerManager.Instance.ChangeMapInstance(session.Character.CharacterId,
                                        session.Character.Family.Act4Raid.MapInstanceId, 20, 20);
                                    break;
                            }
                        }
                        else
                        {
                            session.SendPacket(
                                session.Character.GenerateSay(Language.Instance.GetMessageFromKey("PORTAL_BLOCKED"),
                                    10));
                        }

                        return;

                    default:
                        session.SendPacket(
                            session.Character.GenerateSay(Language.Instance.GetMessageFromKey("PORTAL_BLOCKED"), 10));
                        return;
                }

                if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.TimeSpaceInstance
                    && !session.Character.Timespace.InstanceBag.Lock)
                {
                    if (session.Character.CharacterId == session.Character.Timespace.InstanceBag.CreatorId)
                    {
                        session.SendPacket(UserInterfaceHelper.GenerateDialog(
                            $"#rstart^1 rstart {Language.Instance.GetMessageFromKey("FIRST_ROOM_START")}"));
                    }

                    return;
                }

                portal.OnTraversalEvents.ForEach(e => EventHelper.Instance.RunEvent(e));
                if (portal.DestinationMapInstanceId == default)
                {
                    return;
                }

                if (ServerManager.Instance.ChannelId == 51)
                {
                    if ((session.Character.Faction == FactionType.Angel && portal.DestinationMapId == 131)
                        || (session.Character.Faction == FactionType.Demon && portal.DestinationMapId == 130))
                    {
                        session.SendPacket(
                            session.Character.GenerateSay(Language.Instance.GetMessageFromKey("PORTAL_BLOCKED"), 10));
                        return;
                    }

                    if ((portal.DestinationMapId == 130 || portal.DestinationMapId == 131)
                        && timeSpanSinceLastPortal < 60)
                    {
                        session.SendPacket(
                            session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_MOVE"), 10));
                        return;
                    }
                }

                session.SendPacket(session.CurrentMapInstance.GenerateRsfn());

                session.Character.LastPortal = currentRunningSeconds;

                if (ServerManager.GetMapInstance(portal.SourceMapInstanceId).MapInstanceType
                    != MapInstanceType.BaseMapInstance
                    && ServerManager.GetMapInstance(portal.DestinationMapInstanceId).MapInstanceType
                    == MapInstanceType.BaseMapInstance)
                {
                    ServerManager.Instance.ChangeMap(session.Character.CharacterId, session.Character.MapId,
                        session.Character.MapX, session.Character.MapY);
                }
                else if (portal.DestinationMapInstanceId == session.Character.Miniland.MapInstanceId)
                {
                    ServerManager.Instance.JoinMiniland(session, session);
                }
                else if (portal.DestinationMapId == 20000)
                {
                    ClientSession sess = ServerManager.Instance.Sessions.FirstOrDefault(s =>
                        s.Character.Miniland.MapInstanceId == portal.DestinationMapInstanceId);
                    if (sess != null)
                    {
                        ServerManager.Instance.JoinMiniland(session, sess);
                    }
                }
                else
                {
                    if (ServerManager.Instance.ChannelId == 51)
                    {
                        short destinationX = portal.DestinationX;
                        short destinationY = portal.DestinationY;

                        if (portal.DestinationMapInstanceId == CaligorRaid.CaligorMapInstance?.MapInstanceId
                        ) /* Caligor Raid Map */
                        {
                            switch (session.Character.Faction)
                            {
                                case FactionType.Angel:
                                    destinationX = 50;
                                    destinationY = 172;
                                    break;

                                case FactionType.Demon:
                                    destinationX = 130;
                                    destinationY = 172;
                                    break;
                            }
                        }
                        else if (portal.DestinationMapId == 153) /* Unknown Land */
                        {
                            switch (session.Character.Faction)
                            {
                                case FactionType.Angel:
                                    destinationX = 50;
                                    destinationY = 172;
                                    break;

                                case FactionType.Demon:
                                    destinationX = 130;
                                    destinationY = 172;
                                    break;
                            }
                        }

                        ServerManager.Instance.ChangeMapInstance(session.Character.CharacterId,
                            portal.DestinationMapInstanceId, destinationX, destinationY);
                    }
                    else
                    {
                        ServerManager.Instance.ChangeMapInstance(session.Character.CharacterId,
                            portal.DestinationMapInstanceId, portal.DestinationX, portal.DestinationY);
                    }
                }
            }
        }

        #endregion
    }
}