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
using System.Reactive.Linq;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Event.LOD
{
    public static class Lod
    {
        #region Methods

        public static void GenerateLod(int lodtime = 120)
        {
            const int hornTime = 30;
            const int hornRepawn = 4;
            const int hornStay = 1;
            EventHelper.Instance.RunEvent(new EventContainer(ServerManager.GetMapInstance(ServerManager.GetBaseMapInstanceIdByMapId(98)), EventActionType.NpcsEffectChangeState, true));
            LodThread lodThread = new LodThread();
            Observable.Timer(TimeSpan.FromMinutes(0)).Subscribe(x => lodThread.Run(lodtime * 60, hornTime * 60, hornRepawn * 60, hornStay * 60));
        }

        #endregion
    }

    public class LodThread
    {
        private int _lodTime;

        private int _hornTime;

        private int _hornRespawn;

        private int _hornStay;

        private const int Interval = 30;

        private int _dhSpawns;

        #region Methods

        public void Run(int lodTime, int hornTime, int hornRespawn, int hornStay)
        {
            _lodTime = lodTime;
            _hornTime = hornTime;
            _hornRespawn = hornRespawn;
            _hornStay = hornStay;
            for (int i = 0; i < lodTime; i += Interval)
            {
                Observable.Timer(TimeSpan.FromSeconds(i)).Subscribe(observer =>
                {
                    RefreshLod(_lodTime);

                    if (_lodTime == _hornTime || (_lodTime == _hornTime - (_hornRespawn * _dhSpawns)))
                    {
                        foreach (Family fam in ServerManager.Instance.FamilyList.GetAllItems())
                        {
                            if (fam.LandOfDeath != null)
                            {
                                EventHelper.Instance.RunEvent(new EventContainer(fam.LandOfDeath,
                                    EventActionType.ChangExpRate, 3));
                                EventHelper.Instance.RunEvent(new EventContainer(fam.LandOfDeath,
                                    EventActionType.ChangeDropRate, 3));
                                SpawnDh(fam.LandOfDeath);
                            }
                        }
                    }
                    else if (_lodTime == _hornTime - (_hornRespawn * _dhSpawns) - _hornStay)
                    {
                        foreach (Family fam in ServerManager.Instance.FamilyList.GetAllItems())
                        {
                            if (fam.LandOfDeath != null)
                            {
                                DespawnDh(fam.LandOfDeath);
                                _dhSpawns++;
                            }
                        }
                    }

                    _lodTime -= Interval;
                });
            }

            Observable.Timer(TimeSpan.FromSeconds(lodTime)).Subscribe(observer => EndLod());
        }

        private static void DespawnDh(MapInstance landOfDeath)
        {
            EventHelper.Instance.RunEvent(new EventContainer(ServerManager.GetMapInstance(ServerManager.GetBaseMapInstanceIdByMapId(98)), EventActionType.NpcsEffectChangeState, false));
            EventHelper.Instance.RunEvent(new EventContainer(landOfDeath, EventActionType.SendPacket, UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("HORN_DISAPEAR"), 0)));
            EventHelper.Instance.RunEvent(new EventContainer(landOfDeath, EventActionType.UnspawnMonsters, 443));
        }

        private static void EndLod()
        {
            foreach (Family fam in ServerManager.Instance.FamilyList.GetAllItems())
            {
                if (fam.LandOfDeath != null)
                {
                    EventHelper.Instance.RunEvent(new EventContainer(fam.LandOfDeath, EventActionType.DisposeMap, null));
                    fam.LandOfDeath = null;
                }
            }
            ServerManager.Instance.StartedEvents.Remove(EventType.LOD);
            ServerManager.Instance.StartedEvents.Remove(EventType.LODDH);
        }

        private static void RefreshLod(int remaining)
        {
            foreach (Family fam in ServerManager.Instance.FamilyList.GetAllItems())
            {
                if (fam.LandOfDeath == null)
                {
                    fam.LandOfDeath = ServerManager.GenerateMapInstance(150, MapInstanceType.LodInstance, new InstanceBag());
                }
                EventHelper.Instance.RunEvent(new EventContainer(fam.LandOfDeath, EventActionType.Clock, remaining * 10));
                EventHelper.Instance.RunEvent(new EventContainer(fam.LandOfDeath, EventActionType.StartClock, new Tuple<List<EventContainer>, List<EventContainer>>(new List<EventContainer>(), new List<EventContainer>())));
            }
        }

        private static void SpawnDh(MapInstance landOfDeath)
        {
            EventHelper.Instance.RunEvent(new EventContainer(landOfDeath, EventActionType.SpawnOnLastEntry, 443));
            EventHelper.Instance.RunEvent(new EventContainer(landOfDeath, EventActionType.SendPacket, "df 2"));
            EventHelper.Instance.RunEvent(new EventContainer(landOfDeath, EventActionType.SendPacket, UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("HORN_APPEAR"), 0)));
        }

        #endregion
    }
}