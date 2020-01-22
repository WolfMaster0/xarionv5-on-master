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
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace OpenNos.GameObject.Event.ACT4
{
    public static class CaligorRaid
    {
        #region Properties

        public static int AngelDamage { get; set; }

        public static MapInstance CaligorMapInstance { get; set; }

        public static int DemonDamage { get; set; }

        public static bool IsLocked { get; set; }

        public static bool IsRunning { get; set; }

        public static int RemainingTime { get; set; }

        public static MapInstance UnknownLandMapInstance { get; set; }

        #endregion

        #region Methods

        public static void Run()
        {
            CaligorRaidThread raidThread = new CaligorRaidThread();
            Observable.Timer(TimeSpan.FromMinutes(0)).Subscribe(x => raidThread.Run());
        }

        #endregion
    }

    public class CaligorRaidThread
    {
        #region Methods

        public void Run()
        {
            CaligorRaid.RemainingTime = 3600;
            const int interval = 3;

            CaligorRaid.CaligorMapInstance = ServerManager.GenerateMapInstance(154, MapInstanceType.CaligorInstance, new InstanceBag());

            CaligorRaid.UnknownLandMapInstance = ServerManager.GetMapInstance(ServerManager.GetBaseMapInstanceIdByMapId(153));
            CaligorRaid.UnknownLandMapInstance.CreatePortal(new Portal
            {
                SourceMapId = 153,
                SourceX = 70,
                SourceY = 159,
                DestinationMapId = 0,
                DestinationX = 70,
                DestinationY = 159,
                DestinationMapInstanceId = CaligorRaid.CaligorMapInstance.MapInstanceId,
                Type = -1
            });
            CaligorRaid.UnknownLandMapInstance.CreatePortal(new Portal
            {
                SourceMapId = 153,
                SourceX = 110,
                SourceY = 159,
                DestinationMapId = 0,
                DestinationX = 110,
                DestinationY = 159,
                DestinationMapInstanceId = CaligorRaid.CaligorMapInstance.MapInstanceId,
                Type = -1
            });

            List<EventContainer> onDeathEvents = new List<EventContainer>
            {
                new EventContainer(CaligorRaid.CaligorMapInstance, EventActionType.ScriptEnd, (byte)1)
            };

            MapMonster caligor = CaligorRaid.CaligorMapInstance.Monsters.Find(s => s.Monster.NpcMonsterVNum == 2305);

            if (caligor != null)
            {
                caligor.OnDeathEvents = onDeathEvents;
                caligor.IsBoss = true;
            }

            ServerManager.Shout(Language.Instance.GetMessageFromKey("CALIGOR_OPEN"), true);

            RefreshRaid();

            ServerManager.Instance.Act4RaidStart = DateTime.UtcNow;

            for (int i = 0; i < CaligorRaid.RemainingTime; i += interval)
            {
                Observable.Timer(TimeSpan.FromSeconds(i)).Subscribe(observer =>
                {
                    CaligorRaid.RemainingTime -= interval;
                    RefreshRaid();
                });
            }

            Observable.Timer(TimeSpan.FromSeconds(CaligorRaid.RemainingTime)).Subscribe(observer => EndRaid());
        }

        private void TeleportPlayer(ClientSession sess, int delay)
        {
            Observable.Timer(TimeSpan.FromMilliseconds(delay)).Subscribe(observer =>
            {
                ServerManager.Instance.ChangeMapInstance(sess.Character.CharacterId,
                    CaligorRaid.UnknownLandMapInstance.MapInstanceId, sess.Character.MapX, sess.Character.MapY);
            });
        }

        private void EndRaid()
        {
            ServerManager.Shout(Language.Instance.GetMessageFromKey("CALIGOR_END"), true);

            int delay = 100;
            foreach (ClientSession sess in CaligorRaid.CaligorMapInstance.Sessions.ToList())
            {
                TeleportPlayer(sess, delay);
                delay += 100;
            }
            EventHelper.Instance.RunEvent(new EventContainer(CaligorRaid.CaligorMapInstance, EventActionType.DisposeMap, null));
            CaligorRaid.IsRunning = false;
            CaligorRaid.AngelDamage = 0;
            CaligorRaid.DemonDamage = 0;
            ServerManager.Instance.StartedEvents.Remove(EventType.Caligor);
        }

        private void LockRaid()
        {
            foreach (Portal p in CaligorRaid.UnknownLandMapInstance.Portals.Where(s => s.DestinationMapInstanceId == CaligorRaid.CaligorMapInstance.MapInstanceId).ToList())
            {
                CaligorRaid.UnknownLandMapInstance.Portals.Remove(p);
                CaligorRaid.UnknownLandMapInstance.Broadcast(p.GenerateGp());
            }
            ServerManager.Shout(Language.Instance.GetMessageFromKey("CALIGOR_LOCKED"), true);
            CaligorRaid.IsLocked = true;
        }

        private void RefreshRaid()
        {
            int maxHP = ServerManager.GetNpcMonster(2305).MaxHP;
            CaligorRaid.CaligorMapInstance.Broadcast(UserInterfaceHelper.GenerateChdm(maxHP, CaligorRaid.AngelDamage, CaligorRaid.DemonDamage, CaligorRaid.RemainingTime));

            if (((maxHP / 10) * 8 < CaligorRaid.AngelDamage + CaligorRaid.DemonDamage) && !CaligorRaid.IsLocked)
            {
                LockRaid();
            }
        }

        #endregion
    }
}