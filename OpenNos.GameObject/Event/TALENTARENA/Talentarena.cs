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
using System.Threading;
using OpenNos.Core;
using OpenNos.Core.Extensions;
using OpenNos.Core.Threading;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Event.TALENTARENA
{
    public static class TalentArena
    {
        public static bool IsRunning { get; private set; }

        public static ThreadSafeSortedList<long, ClientSession> RegisteredParticipants { get; private set; }

        public static ThreadSafeSortedList<long, Group> RegisteredGroups { get; set; }

        public static ThreadSafeSortedList<long, List<Group>> PlayingGroups { get; set; }

        #region Methods

        public static void Run()
        {
            RegisteredParticipants = new ThreadSafeSortedList<long, ClientSession>();
            RegisteredGroups = new ThreadSafeSortedList<long, Group>();
            PlayingGroups = new ThreadSafeSortedList<long, List<Group>>();

            ServerManager.Shout(Language.Instance.GetMessageFromKey("TALENTARENA_OPEN"), true);

            GroupingThread groupingThread = new GroupingThread();
            Observable.Timer(TimeSpan.FromSeconds(0)).Subscribe(observer => groupingThread.RunThread());

            MatchmakingThread matchmakingThread = new MatchmakingThread();
            Observable.Timer(TimeSpan.FromSeconds(3)).Subscribe(observer => matchmakingThread.RunThread());

            IsRunning = true;

            Observable.Timer(TimeSpan.FromMinutes(30)).Subscribe(observer =>
            {
                groupingThread.RequestStop();
                matchmakingThread.RequestStop();
                RegisteredParticipants.ClearAll();
                RegisteredGroups.ClearAll();
                IsRunning = false;
                ServerManager.Instance.StartedEvents.Remove(EventType.TalentArena);
            });
        }

        private class GroupingThread
        {
            private bool _shouldStop;

            public void RunThread()
            {
                byte[] levelCaps = { 40, 50, 60, 70, 80, 85, 90, 95, 100, 120, 150, 180, 255 };
                while (!_shouldStop)
                {
                    IEnumerable<IGrouping<byte, ClientSession>> groups = from sess in RegisteredParticipants.GetAllItems()
                                                                         group sess by Array.Find(levelCaps, s => s > sess.Character.Level) into grouping
                                                                         select grouping;
                    foreach (IGrouping<byte, ClientSession> group in groups)
                    {
                        foreach (List<ClientSession> grp in group.ToList().Split(3).Where(s => s.Count == 3))
                        {
                            Group g = new Group
                            {
                                GroupType = GroupType.TalentArena,
                                TalentArenaBattle = new TalentArenaBattle
                                {
                                    GroupLevel = group.Key
                                }
                            };

                            foreach (ClientSession sess in grp)
                            {
                                RegisteredParticipants.Remove(sess);
                                g.JoinGroup(sess);
                                sess.SendPacket(UserInterfaceHelper.GenerateBsInfo(1, 3, -1, 6));
                                Observable.Timer(TimeSpan.FromSeconds(1)).Subscribe(observer => sess.SendPacket(UserInterfaceHelper.GenerateBsInfo(1, 3, 300, 1)));
                            }
                            RegisteredGroups[g.GroupId] = g;
                        }
                    }

                    Thread.Sleep(5000);
                }
            }

            public void RequestStop() => _shouldStop = true;
        }

        private class MatchmakingThread
        {
            private bool _shouldStop;

            public void RunThread()
            {
                while (!_shouldStop)
                {
                    IEnumerable<IGrouping<byte, Group>> groups = from grp in RegisteredGroups.GetAllItems()
                                                                 where grp.TalentArenaBattle != null
                                                                 group grp by grp.TalentArenaBattle.GroupLevel into grouping
                                                                 select grouping;

                    foreach (IGrouping<byte, Group> group in groups)
                    {
                        Group prevGroup = null;

                        foreach (Group g in group)
                        {
                            if (prevGroup == null)
                            {
                                prevGroup = g;
                            }
                            else
                            {
                                RegisteredGroups.Remove(g);
                                RegisteredGroups.Remove(prevGroup);

                                MapInstance mapInstance = ServerManager.GenerateMapInstance(2015, MapInstanceType.NormalInstance, new InstanceBag());
                                mapInstance.IsPvp = true;

                                g.TalentArenaBattle.MapInstance = mapInstance;
                                prevGroup.TalentArenaBattle.MapInstance = mapInstance;

                                g.TalentArenaBattle.Side = 0;
                                prevGroup.TalentArenaBattle.Side = 1;

                                g.TalentArenaBattle.Calls = 5;
                                prevGroup.TalentArenaBattle.Calls = 5;

                                List<ClientSession> gs = g.Characters.GetAllItems().Concat(prevGroup.Characters.GetAllItems()).ToList();
                                foreach (ClientSession sess in gs)
                                {
                                    sess.SendPacket(UserInterfaceHelper.GenerateBsInfo(1, 3, -1, 2));
                                }
                                Thread.Sleep(1000);
                                foreach (ClientSession sess in gs)
                                {
                                    sess.SendPacket(UserInterfaceHelper.GenerateBsInfo(2, 3, 0, 0));
                                    sess.SendPacket(UserInterfaceHelper.GenerateTeamArenaClose());
                                }
                                Thread.Sleep(5000);
                                foreach (ClientSession sess in gs)
                                {
                                    sess.SendPacket(UserInterfaceHelper.GenerateTeamArenaMenu(0, 0, 0, 0, 0));
                                    short x = 125;
                                    if (sess.Character.Group.TalentArenaBattle.Side == 0)
                                    {
                                        x = 15;
                                    }
                                    ServerManager.Instance.ChangeMapInstance(sess.Character.CharacterId, mapInstance.MapInstanceId, x, 39);
                                    sess.SendPacketAfter(UserInterfaceHelper.GenerateTeamArenaMenu(3, 0, 0, 60, 0), 5000);
                                }

                                // TODO: Other Setup stuff

                                PlayingGroups[g.GroupId] = new List<Group> { g, prevGroup };

                                BattleThread battleThread = new BattleThread();
                                Observable.Timer(TimeSpan.FromSeconds(0)).Subscribe(observer => battleThread.Run(PlayingGroups[g.GroupId]));

                                prevGroup = null;
                            }
                        }
                    }

                    Thread.Sleep(5000);
                }
            }

            public void RequestStop() => _shouldStop = true;
        }

        private class BattleThread
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            private List<ClientSession> Characters { get; set; }

            public void Run(List<Group> groups) => Characters = groups[0].Characters.GetAllItems().Concat(groups[1].Characters.GetAllItems()).ToList();

            // TODO: Battle Thread System main loop

        }

        #endregion
    }
}