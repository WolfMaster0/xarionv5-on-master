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
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Event.ACT4
{
    public static class Act4Ship
    {
        #region Methods

        public static void GenerateAct4Ship(byte faction)
        {
            EventHelper.Instance.RunEvent(new EventContainer(ServerManager.GetMapInstance(ServerManager.GetBaseMapInstanceIdByMapId(145)), EventActionType.NpcsEffectChangeState, true));
            DateTime result = Core.Extensions.TimeExtensions.RoundUp(DateTime.UtcNow, TimeSpan.FromMinutes(1));
            Observable.Timer(result - DateTime.UtcNow).Subscribe(x => Act4ShipThread.Run(faction));
        }

        #endregion
    }

    public static class Act4ShipThread
    {
        #region Methods

        public static void Run(byte faction)
        {
            MapInstance map = ServerManager.GenerateMapInstance(148, faction == 1 ? MapInstanceType.Act4ShipAngel : MapInstanceType.Act4ShipDemon, new InstanceBag());
            MapNpc mapNpc1 = new MapNpc
            {
            };
            mapNpc1.Initialize(map);
            map.AddNpc(mapNpc1);
            MapNpc mapNpc2 = new MapNpc
            {   
            };
            mapNpc2.Initialize(map);
            map.AddNpc(mapNpc2);
            Observable.Interval(TimeSpan.FromMinutes(1)).Subscribe(obs =>
            {
                OpenShip();
                Observable.Timer(TimeSpan.FromMinutes(1)).Subscribe(observer =>
                {
                    map.Broadcast(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("SHIP_MINUTE"), 0));
                    LockShip();
                });
                Observable.Timer(TimeSpan.FromSeconds(30)).Subscribe(observer =>
                {
                    map.Broadcast(UserInterfaceHelper.GenerateMsg(
                        string.Format(Language.Instance.GetMessageFromKey("SHIP_SECONDS"), 30), 0));
                });
                Observable.Timer(TimeSpan.FromSeconds(50)).Subscribe(observer =>
                {
                    map.Broadcast(UserInterfaceHelper.GenerateMsg(
                        string.Format(Language.Instance.GetMessageFromKey("SHIP_SECONDS"), 10), 0));
                });
                Observable.Timer(TimeSpan.FromMinutes(1)).Subscribe(observer =>
                {
                    map.Broadcast(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("SHIP_SETOFF"), 0));
                    List<ClientSession> sessions = map.Sessions.Where(s => s?.Character != null).ToList();
                    Observable.Timer(TimeSpan.FromSeconds(0)).Subscribe(x => TeleportPlayers(sessions));
                });
            });
        }

        private static void LockShip() => EventHelper.Instance.RunEvent(new EventContainer(ServerManager.GetMapInstance(ServerManager.GetBaseMapInstanceIdByMapId(145)), EventActionType.NpcsEffectChangeState, true));

        private static void OpenShip() => EventHelper.Instance.RunEvent(new EventContainer(ServerManager.GetMapInstance(ServerManager.GetBaseMapInstanceIdByMapId(145)), EventActionType.NpcsEffectChangeState, false));

        private static void TeleportPlayers(List<ClientSession> sessions)
        {
            foreach (ClientSession session in sessions)
            {
                switch (session.Character.Faction)
                {
                    case FactionType.None:
                        ServerManager.Instance.ChangeMap(session.Character.CharacterId, 145, 50, 41);
                        session.SendPacket(UserInterfaceHelper.GenerateInfo("You need to be part of a faction to join Act 4"));
                        return;

                    case FactionType.Angel:
                        session.Character.MapId = 130;
                        session.Character.MapX = 12;
                        session.Character.MapY = 40;
                        break;

                    case FactionType.Demon:
                        session.Character.MapId = 131;
                        session.Character.MapX = 12;
                        session.Character.MapY = 40;
                        break;
                }

                session.Character.ChangeChannel(ServerManager.Instance.Configuration.Act4IP, ServerManager.Instance.Configuration.Act4Port, 1);
            }
        }

        #endregion
    }
}