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
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.EventArguments;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.BasicPackets
{
    [PacketHeader("walk")]
    public class WalkPacket
    {
        #region Properties

        public short Speed { get; set; }

        public short Unknown { get; set; }

        public short XCoordinate { get; set; }

        public short YCoordinate { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 6)
            {
                return;
            }
            WalkPacket packetDefinition = new WalkPacket();
            if (short.TryParse(packetSplit[2], out short x)
                && short.TryParse(packetSplit[3], out short y)
                && short.TryParse(packetSplit[4], out short unknown)
                && short.TryParse(packetSplit[5], out short speed))
            {
                packetDefinition.XCoordinate = x;
                packetDefinition.YCoordinate = y;
                packetDefinition.Unknown = unknown;
                packetDefinition.Speed = speed;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(WalkPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (session.Character.NoMove == 0)
            {
                if (session.Character.MeditationDictionary.Count != 0)
                {
                    session.Character.MeditationDictionary.Clear();
                }

                session.Character.IsAfk = false;

                double currentRunningSeconds =
                    (DateTime.UtcNow - Process.GetCurrentProcess().StartTime.AddHours(-2).AddSeconds(-50)).TotalSeconds;
                double timeSpanSinceLastPortal = currentRunningSeconds - session.Character.LastPortal;
                int distance =
                    Map.GetDistance(new MapCell { X = session.Character.PositionX, Y = session.Character.PositionY },
                        new MapCell { X = XCoordinate, Y = YCoordinate });

                if (session.HasCurrentMapInstance
                    && !session.CurrentMapInstance.Map.IsBlockedZone(XCoordinate, YCoordinate)
                    && !session.Character.IsChangingMapInstance && !session.Character.HasShopOpened)
                {
                    if ((session.Character.Speed >= Speed
                         || session.Character.LastSpeedChange.AddSeconds(5) > DateTime.UtcNow)
                        && !(distance > 60 && timeSpanSinceLastPortal > 10))
                    {
                        #region Direction Calculation

                        if (XCoordinate < session.Character.PositionX
                            && YCoordinate < session.Character.PositionY) // NW
                        {
                            session.Character.Direction = 4;
                        }
                        else if (XCoordinate == session.Character.PositionX
                                 && YCoordinate < session.Character.PositionY) // N
                        {
                            session.Character.Direction = 0;
                        }
                        else if (XCoordinate > session.Character.PositionX
                                 && YCoordinate < session.Character.PositionY) // NE
                        {
                            session.Character.Direction = 6;
                        }
                        else if (XCoordinate < session.Character.PositionX
                                 && YCoordinate == session.Character.PositionY) // W
                        {
                            session.Character.Direction = 3;
                        }
                        // X&Y equal is impossible, skipping
                        else if (XCoordinate > session.Character.PositionX
                                 && YCoordinate == session.Character.PositionY) // E
                        {
                            session.Character.Direction = 1;
                        }
                        else if (XCoordinate < session.Character.PositionX
                                 && YCoordinate > session.Character.PositionY) // SW
                        {
                            session.Character.Direction = 7;
                        }
                        else if (XCoordinate == session.Character.PositionX
                                 && YCoordinate > session.Character.PositionY) // S
                        {
                            session.Character.Direction = 2;
                        }
                        else if (XCoordinate > session.Character.PositionX
                                 && YCoordinate > session.Character.PositionY) // SE
                        {
                            session.Character.Direction = 5;
                        }

                        #endregion

                        if (session.Character.MapInstance.MapInstanceType == MapInstanceType.BaseMapInstance)
                        {
                            session.Character.MapX = XCoordinate;
                            session.Character.MapY = YCoordinate;
                        }

                        session.Character.PositionX = XCoordinate;
                        session.Character.PositionY = YCoordinate;

                        if (session.Character.LastMonsterAggro.AddSeconds(5) > DateTime.UtcNow)
                        {
                            session.Character.UpdateBushFire();
                        }

                        if (!session.Character.InvisibleGm)
                        {
                            session.CurrentMapInstance?.Broadcast(StaticPacketHelper.Move(UserType.Player,
                                session.Character.CharacterId, session.Character.PositionX, session.Character.PositionY,
                                session.Character.Speed));
                        }

                        session.SendPacket(session.Character.GenerateCond());
                        session.Character.LastMove = DateTime.UtcNow;

                        session.CurrentMapInstance?.OnAreaEntryEvents
                            ?.Where(s => s.InZone(session.Character.PositionX, session.Character.PositionY)).ToList()
                            .ForEach(e => e.Events.ForEach(evt => EventHelper.Instance.RunEvent(evt)));
                        session.CurrentMapInstance?.OnAreaEntryEvents?.RemoveAll(s =>
                            s.InZone(session.Character.PositionX, session.Character.PositionY));

                        session.CurrentMapInstance?.OnMoveOnMapEvents?.ForEach(e => EventHelper.Instance.RunEvent(e));
                        session.CurrentMapInstance?.OnMoveOnMapEvents?.RemoveAll(s => s != null);
                        if (session.CurrentMapInstance != null)
                        {
                            session.Character.OnMove(new MoveEventArgs(session.CurrentMapInstance.Map.MapId,
                                session.Character.PositionX, session.Character.PositionY));
                        }
                    }
                    else
                    {
                        session.SendPacket(UserInterfaceHelper.GenerateModal(
                            "This is an automatique message, your movement got corrupted\n" +
                            "Please use $Unstuck\n",
                             0));
                    }
                }
            }
        }

        #endregion
    }
}