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
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Event.INSTANTBATTLE
{
    public static class InstantBattle
    {
        #region Methods

        public static void GenerateInstantBattle()
        {
            ServerManager.Instance.Broadcast(
                UserInterfaceHelper.GenerateMsg(
                    string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES"), 5), 0));
            ServerManager.Instance.Broadcast(
                UserInterfaceHelper.GenerateMsg(
                    string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES"), 5), 1));

            Observable.Timer(TimeSpan.FromMinutes(4)).Subscribe(observer =>
            {
                ServerManager.Instance.Broadcast(
                    UserInterfaceHelper.GenerateMsg(
                        string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES"), 1), 0));
                ServerManager.Instance.Broadcast(
                    UserInterfaceHelper.GenerateMsg(
                        string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES"), 1), 1));
            });
            Observable.Timer(TimeSpan.FromSeconds(270)).Subscribe(observer =>
            {
                ServerManager.Instance.Broadcast(UserInterfaceHelper.GenerateMsg(
                    string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_SECONDS"), 30), 0));
                ServerManager.Instance.Broadcast(UserInterfaceHelper.GenerateMsg(
                    string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_SECONDS"), 30), 1));
            });
            Observable.Timer(TimeSpan.FromSeconds(290)).Subscribe(observer =>
            {
                ServerManager.Instance.Broadcast(UserInterfaceHelper.GenerateMsg(
                    string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_SECONDS"), 10), 0));
                ServerManager.Instance.Broadcast(UserInterfaceHelper.GenerateMsg(
                    string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_SECONDS"), 10), 1));
            });
            Observable.Timer(TimeSpan.FromMinutes(5)).Subscribe(observer =>
            {
                ServerManager.Instance.Broadcast(
                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_STARTED"), 1));
                ServerManager.Instance.Broadcast(
                    $"qnaml 1 #guri^506 {Language.Instance.GetMessageFromKey("INSTANTBATTLE_QUESTION")}");
                ServerManager.Instance.EventInWaiting = true;
            });
            Observable.Timer(TimeSpan.FromSeconds(330)).Subscribe(observer =>
            {
                ServerManager.Instance.Broadcast(
                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_STARTED"), 1));
                ServerManager.Instance.Sessions.Where(s => s.Character?.IsWaitingForEvent == false).ToList()
                    .ForEach(s => s.SendPacket("esf"));
                ServerManager.Instance.EventInWaiting = false;
                IEnumerable<ClientSession> sessions = ServerManager.Instance.Sessions.Where(s =>
                    s.Character?.IsWaitingForEvent == true
                    && s.Character.MapInstance.MapInstanceType == MapInstanceType.BaseMapInstance);
                List<Tuple<MapInstance, byte>> maps = new List<Tuple<MapInstance, byte>>();
                MapInstance map = null;
                int i = -1;
                int level = 0;
                byte instancelevel = 1;
                foreach (ClientSession s in sessions.OrderBy(s => s.Character?.Level))
                {
                    i++;
                    if (s.Character.Level > 79 && level <= 79)
                    {
                        i = 0;
                        instancelevel = 80;
                    }
                    else if (s.Character.Level > 69 && level <= 69)
                    {
                        i = 0;
                        instancelevel = 70;
                    }
                    else if (s.Character.Level > 59 && level <= 59)
                    {
                        i = 0;
                        instancelevel = 60;
                    }
                    else if (s.Character.Level > 49 && level <= 49)
                    {
                        i = 0;
                        instancelevel = 50;
                    }
                    else if (s.Character.Level > 39 && level <= 39)
                    {
                        i = 0;
                        instancelevel = 30;
                    }

                    if (i % 50 == 0)
                    {
                        map = ServerManager.GenerateMapInstance(2004, MapInstanceType.NormalInstance,
                            new InstanceBag());
                        maps.Add(new Tuple<MapInstance, byte>(map, instancelevel));
                    }

                    if (map != null)
                    {
                        ServerManager.Instance.TeleportOnRandomPlaceInMap(s, map.MapInstanceId);
                    }

                    level = s.Character.Level;
                }

                ServerManager.Instance.Sessions.Where(s => s.Character != null).ToList()
                    .ForEach(s => s.Character.IsWaitingForEvent = false);
                ServerManager.Instance.StartedEvents.Remove(EventType.InstantBattle);
                foreach (Tuple<MapInstance, byte> mapinstance in maps)
                {
                    Observable.Timer(TimeSpan.FromSeconds(10)).Subscribe(x => InstantBattleTask.Run(mapinstance));
                }
            });
        }

        #endregion

        #region Classes

        private static class InstantBattleTask
        {
            #region Methods

            public static void Run(Tuple<MapInstance, byte> mapinstance)
            {
                long maxGold = ServerManager.Instance.Configuration.MaxGold;
                bool done = false;
                if (!mapinstance.Item1.Sessions.Skip(3 - 1).Any())
                {
                    mapinstance.Item1.Sessions.Where(s => s.Character != null).ToList().ForEach(s => ServerManager.Instance.ChangeMap(s.Character.CharacterId, s.Character.MapId, s.Character.MapX, s.Character.MapY));
                }
                Observable.Timer(TimeSpan.FromMinutes(12)).Subscribe(x =>
                {
                    for (int d = 0; d < 180; d++)
                    {
                        Observable.Timer(TimeSpan.FromSeconds(d)).Subscribe(observer =>
                        {
                            if (!done && !mapinstance.Item1.Monsters.Any(s => s.CurrentHp > 0))
                            {
                                done = true;
                                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(0),
                                    new EventContainer(mapinstance.Item1, EventActionType.SpawnPortal,
                                        new Portal {SourceX = 47, SourceY = 33, DestinationMapId = 1}));
                                mapinstance.Item1.Broadcast(
                                    UserInterfaceHelper.GenerateMsg(
                                        Language.Instance.GetMessageFromKey("INSTANTBATTLE_SUCCEEDED"), 0));
                                foreach (ClientSession cli in mapinstance.Item1.Sessions.Where(s => s.Character != null)
                                    .ToList())
                                {
                                    cli.Character.GenerateFamilyXp(cli.Character.Level * 25);
                                    cli.Character.SetReputation(cli.Character.Level * 200);
                                    cli.Character.Gold += cli.Character.Level * 5000;
                                    cli.Character.Gold = cli.Character.Gold > maxGold ? maxGold : cli.Character.Gold;
                                    cli.Character.SpAdditionPoint += cli.Character.Level * 500;
                                    cli.Character.SpAdditionPoint = cli.Character.SpAdditionPoint > 1000000
                                        ? 1000000
                                        : cli.Character.SpAdditionPoint;
                                    cli.SendPacket(cli.Character.GenerateSpPoint());
                                    cli.SendPacket(cli.Character.GenerateGold());
                                    cli.SendPacket(cli.Character.GenerateSay(
                                        string.Format(Language.Instance.GetMessageFromKey("WIN_MONEY"),
                                            cli.Character.Level * 1000), 10));
                                    cli.SendPacket(cli.Character.GenerateSay(
                                        string.Format(Language.Instance.GetMessageFromKey("WIN_REPUT"),
                                            cli.Character.Level * 50), 10));
                                    cli.SendPacket(cli.Character.GenerateSay(
                                        string.Format(Language.Instance.GetMessageFromKey("WIN_FXP"),
                                            cli.Character.Level * 10), 10));
                                    cli.SendPacket(cli.Character.GenerateSay(
                                        string.Format(Language.Instance.GetMessageFromKey("WIN_SP_POINT"),
                                            cli.Character.Level * 100), 10));
                                }
                            }
                        });
                    }
                });

                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(15), new EventContainer(mapinstance.Item1, EventActionType.DisposeMap, null));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(3), new EventContainer(mapinstance.Item1, EventActionType.SendPacket, UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 12), 0)));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(5), new EventContainer(mapinstance.Item1, EventActionType.SendPacket, UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 10), 0)));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(10), new EventContainer(mapinstance.Item1, EventActionType.SendPacket, UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 5), 0)));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(11), new EventContainer(mapinstance.Item1, EventActionType.SendPacket, UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 4), 0)));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(12), new EventContainer(mapinstance.Item1, EventActionType.SendPacket, UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 3), 0)));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(13), new EventContainer(mapinstance.Item1, EventActionType.SendPacket, UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 2), 0)));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(14), new EventContainer(mapinstance.Item1, EventActionType.SendPacket, UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 1), 0)));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(14.5), new EventContainer(mapinstance.Item1, EventActionType.SendPacket, UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_SECONDS_REMAINING"), 30), 0)));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(14.5), new EventContainer(mapinstance.Item1, EventActionType.SendPacket, UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_SECONDS_REMAINING"), 30), 0)));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(0), new EventContainer(mapinstance.Item1, EventActionType.SendPacket, UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MONSTERS_INCOMING"), 0)));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromSeconds(10), new EventContainer(mapinstance.Item1, EventActionType.SendPacket, UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MONSTERS_HERE"), 0)));

                for (int wave = 0; wave < 4; wave++)
                {
                    EventHelper.Instance.ScheduleEvent(TimeSpan.FromSeconds(130 + (wave * 160)), new EventContainer(mapinstance.Item1, EventActionType.SendPacket, UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MONSTERS_WAVE"), 0)));
                    EventHelper.Instance.ScheduleEvent(TimeSpan.FromSeconds(160 + (wave * 160)), new EventContainer(mapinstance.Item1, EventActionType.SendPacket, UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MONSTERS_INCOMING"), 0)));
                    EventHelper.Instance.ScheduleEvent(TimeSpan.FromSeconds(170 + (wave * 160)), new EventContainer(mapinstance.Item1, EventActionType.SendPacket, UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MONSTERS_HERE"), 0)));
                    EventHelper.Instance.ScheduleEvent(TimeSpan.FromSeconds(10 + (wave * 160)), new EventContainer(mapinstance.Item1, EventActionType.SpawnMonsters, GetInstantBattleMonster(mapinstance.Item1.Map, mapinstance.Item2, wave)));
                    EventHelper.Instance.ScheduleEvent(TimeSpan.FromSeconds(140 + (wave * 160)), new EventContainer(mapinstance.Item1, EventActionType.DropItems, GetInstantBattleDrop(mapinstance.Item1.Map, mapinstance.Item2, wave)));
                }
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromSeconds(650), new EventContainer(mapinstance.Item1, EventActionType.SpawnMonsters, GetInstantBattleMonster(mapinstance.Item1.Map, mapinstance.Item2, 4)));
            }

            private static IEnumerable<Tuple<short, int, short, short>> GenerateDrop(Map map, short vnum, int amountofdrop, int amount)
            {
                List<Tuple<short, int, short, short>> dropParameters = new List<Tuple<short, int, short, short>>();
                for (int i = 0; i < amountofdrop; i++)
                {
                    MapCell cell = map.GetRandomPosition();
                    dropParameters.Add(new Tuple<short, int, short, short>(vnum, amount, cell.X, cell.Y));
                }
                return dropParameters;
            }

            private static List<Tuple<short, int, short, short>> GetInstantBattleDrop(Map map, short instantbattletype, int wave)
            {
                List<Tuple<short, int, short, short>> dropParameters = new List<Tuple<short, int, short, short>>();
                switch (instantbattletype)
                {
                    case 1:
                        switch (wave)
                        {
                            case 0:
                                dropParameters.AddRange(GenerateDrop(map, 1046, 15, 5000));
                                dropParameters.AddRange(GenerateDrop(map, 1030, 8, 5));
                                dropParameters.AddRange(GenerateDrop(map, 2018, 5, 5));
                                dropParameters.AddRange(GenerateDrop(map, 1242, 5, 1));
                                break;

                            case 1:
                                dropParameters.AddRange(GenerateDrop(map, 1046, 15, 1000));
                                dropParameters.AddRange(GenerateDrop(map, 1030, 8, 3));
                                dropParameters.AddRange(GenerateDrop(map, 1242, 10, 1));
                                dropParameters.AddRange(GenerateDrop(map, 2282, 5, 1));
                                break;

                            case 2:
                                dropParameters.AddRange(GenerateDrop(map, 1046, 15, 1000));
                                dropParameters.AddRange(GenerateDrop(map, 1030, 10, 5));
                                dropParameters.AddRange(GenerateDrop(map, 2282, 10, 5));
                                break;

                            case 3:
                                dropParameters.AddRange(GenerateDrop(map, 1046, 15, 1000));
                                dropParameters.AddRange(GenerateDrop(map, 1030, 10, 5));
                                dropParameters.AddRange(GenerateDrop(map, 2282, 10, 5));
                                dropParameters.AddRange(GenerateDrop(map, 1134, 1, 5));
                                break;
                        }
                        break;

                    case 40:
                        switch (wave)
                        {
                            case 0:
                                dropParameters.AddRange(GenerateDrop(map, 1046, 15, 5000));
                                dropParameters.AddRange(GenerateDrop(map, 1030, 8, 5));
                                dropParameters.AddRange(GenerateDrop(map, 2018, 5, 5));
                                dropParameters.AddRange(GenerateDrop(map, 1242, 5, 1));
                                break;

                            case 1:
                                dropParameters.AddRange(GenerateDrop(map, 1046, 15, 10000));
                                dropParameters.AddRange(GenerateDrop(map, 1030, 8, 3));
                                dropParameters.AddRange(GenerateDrop(map, 1242, 10, 1));
                                dropParameters.AddRange(GenerateDrop(map, 2282, 5, 1));
                                break;

                            case 2:
                                dropParameters.AddRange(GenerateDrop(map, 1046, 15, 10000));
                                dropParameters.AddRange(GenerateDrop(map, 1030, 10, 5));
                                dropParameters.AddRange(GenerateDrop(map, 2282, 10, 5));
                                break;

                            case 3:
                                dropParameters.AddRange(GenerateDrop(map, 1046, 15, 10000));
                                dropParameters.AddRange(GenerateDrop(map, 1030, 10, 5));
                                dropParameters.AddRange(GenerateDrop(map, 2282, 10, 5));
                                dropParameters.AddRange(GenerateDrop(map, 1134, 1, 5));
                                break;
                        }
                        break;

                    case 50:
                        switch (wave)
                        {
                            case 0:
                                dropParameters.AddRange(GenerateDrop(map, 1046, 15, 5000));
                                dropParameters.AddRange(GenerateDrop(map, 1030, 8, 5));
                                dropParameters.AddRange(GenerateDrop(map, 2018, 5, 5));
                                dropParameters.AddRange(GenerateDrop(map, 1242, 5, 1));
                                break;

                            case 1:
                                dropParameters.AddRange(GenerateDrop(map, 1046, 15, 10000));
                                dropParameters.AddRange(GenerateDrop(map, 1030, 8, 3));
                                dropParameters.AddRange(GenerateDrop(map, 1242, 10, 1));
                                dropParameters.AddRange(GenerateDrop(map, 2282, 5, 1));
                                break;

                            case 2:
                                dropParameters.AddRange(GenerateDrop(map, 1046, 15, 10000));
                                dropParameters.AddRange(GenerateDrop(map, 1030, 10, 5));
                                dropParameters.AddRange(GenerateDrop(map, 2282, 10, 5));
                                break;

                            case 3:
                                dropParameters.AddRange(GenerateDrop(map, 1046, 15, 10000));
                                dropParameters.AddRange(GenerateDrop(map, 1030, 10, 5));
                                dropParameters.AddRange(GenerateDrop(map, 2282, 10, 5));
                                dropParameters.AddRange(GenerateDrop(map, 1134, 1, 7));
                                break;
                        }
                        break;

                    case 60:
                        switch (wave)
                        {
                            case 0:
                                dropParameters.AddRange(GenerateDrop(map, 1046, 15, 10000));
                                dropParameters.AddRange(GenerateDrop(map, 1030, 20, 5));
                                dropParameters.AddRange(GenerateDrop(map, 2282, 10, 5));
                                dropParameters.AddRange(GenerateDrop(map, 1242, 5, 1));
                                break;

                            case 1:
                                dropParameters.AddRange(GenerateDrop(map, 1046, 15, 30000));
                                dropParameters.AddRange(GenerateDrop(map, 1030, 20, 5));
                                dropParameters.AddRange(GenerateDrop(map, 1242, 20, 1));
                                dropParameters.AddRange(GenerateDrop(map, 2282, 20, 3));
                                break;

                            case 2:
                                dropParameters.AddRange(GenerateDrop(map, 1046, 15, 50000));
                                dropParameters.AddRange(GenerateDrop(map, 1030, 20, 5));
                                dropParameters.AddRange(GenerateDrop(map, 2282, 20, 3));
                                break;

                            case 3:
                                dropParameters.AddRange(GenerateDrop(map, 1046, 15, 100000));
                                dropParameters.AddRange(GenerateDrop(map, 1030, 20, 5));
                                dropParameters.AddRange(GenerateDrop(map, 2282, 20, 2));
                                dropParameters.AddRange(GenerateDrop(map, 1134, 1, 7));
                                break;
                        }
                        break;

                    case 70:
                        switch (wave)
                        {
                            case 0:
                                dropParameters.AddRange(GenerateDrop(map, 1046, 15, 10000));
                                dropParameters.AddRange(GenerateDrop(map, 1030, 20, 5));
                                dropParameters.AddRange(GenerateDrop(map, 2282, 10, 5));
                                dropParameters.AddRange(GenerateDrop(map, 1242, 5, 1));
                                break;

                            case 1:
                                dropParameters.AddRange(GenerateDrop(map, 1046, 15, 30000));
                                dropParameters.AddRange(GenerateDrop(map, 1030, 20, 5));
                                dropParameters.AddRange(GenerateDrop(map, 1242, 20, 1));
                                dropParameters.AddRange(GenerateDrop(map, 2282, 20, 3));
                                break;

                            case 2:
                                dropParameters.AddRange(GenerateDrop(map, 1046, 15, 50000));
                                dropParameters.AddRange(GenerateDrop(map, 1030, 20, 5));
                                dropParameters.AddRange(GenerateDrop(map, 2282, 20, 3));
                                break;

                            case 3:
                                dropParameters.AddRange(GenerateDrop(map, 1046, 15, 100000));
                                dropParameters.AddRange(GenerateDrop(map, 1030, 20, 5));
                                dropParameters.AddRange(GenerateDrop(map, 2282, 20, 2));
                                dropParameters.AddRange(GenerateDrop(map, 1134, 1, 8));
                                break;
                        }
                        break;

                    case 80:
                        switch (wave)
                        {
                            case 0:
                                dropParameters.AddRange(GenerateDrop(map, 1046, 15, 100000));
                                dropParameters.AddRange(GenerateDrop(map, 1030, 40, 5));
                                dropParameters.AddRange(GenerateDrop(map, 2282, 40, 5));
                                dropParameters.AddRange(GenerateDrop(map, 1242, 25, 1));
                                break;

                            case 1:
                                dropParameters.AddRange(GenerateDrop(map, 1046, 15, 300000));
                                dropParameters.AddRange(GenerateDrop(map, 1030, 40, 5));
                                dropParameters.AddRange(GenerateDrop(map, 1242, 40, 1));
                                dropParameters.AddRange(GenerateDrop(map, 2282, 40, 3));
                                break;

                            case 2:
                                dropParameters.AddRange(GenerateDrop(map, 1046, 15, 500000));
                                dropParameters.AddRange(GenerateDrop(map, 1030, 40, 5));
                                dropParameters.AddRange(GenerateDrop(map, 2282, 40, 3));
                                break;

                            case 3:
                                dropParameters.AddRange(GenerateDrop(map, 1046, 15, 500000));
                                dropParameters.AddRange(GenerateDrop(map, 1030, 50, 10));
                                dropParameters.AddRange(GenerateDrop(map, 2282, 50, 10));
                                dropParameters.AddRange(GenerateDrop(map, 1134, 1, 25));
                                break;
                        }
                        break;
                }
                return dropParameters;
            }

            private static List<MonsterToSummon> GetInstantBattleMonster(Map map, short instantbattletype, int wave)
            {
                List<MonsterToSummon> summonParameters = new List<MonsterToSummon>();

                switch (instantbattletype)
                {
                    case 1:
                        switch (wave)
                        {
                            case 0:
                                summonParameters.AddRange(map.GenerateMonsters(1, 16, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(58, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(105, 16, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(107, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(108, 8, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(111, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(136, 15, true, new List<EventContainer>()));
                                break;

                            case 1:
                                summonParameters.AddRange(map.GenerateMonsters(194, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(114, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(99, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(39, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(2, 16, true, new List<EventContainer>()));
                                break;

                            case 2:
                                summonParameters.AddRange(map.GenerateMonsters(140, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(100, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(81, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(12, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(4, 16, true, new List<EventContainer>()));
                                break;

                            case 3:
                                summonParameters.AddRange(map.GenerateMonsters(115, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(112, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(110, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(14, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(5, 16, true, new List<EventContainer>()));
                                break;

                            case 4:
                                summonParameters.AddRange(map.GenerateMonsters(979, 1, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(167, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(137, 10, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(22, 15, false, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(17, 8, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(16, 16, true, new List<EventContainer>()));
                                break;
                        }
                        break;

                    case 40:
                        switch (wave)
                        {
                            case 0:
                                summonParameters.AddRange(map.GenerateMonsters(120, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(151, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(149, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(139, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(73, 16, true, new List<EventContainer>()));
                                break;

                            case 1:
                                summonParameters.AddRange(map.GenerateMonsters(152, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(147, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(104, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(62, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(8, 16, true, new List<EventContainer>()));
                                break;

                            case 2:
                                summonParameters.AddRange(map.GenerateMonsters(153, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(132, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(86, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(76, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(68, 16, true, new List<EventContainer>()));
                                break;

                            case 3:
                                summonParameters.AddRange(map.GenerateMonsters(134, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(91, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(133, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(70, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(89, 16, true, new List<EventContainer>()));
                                break;

                            case 4:
                                summonParameters.AddRange(map.GenerateMonsters(154, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(200, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(77, 8, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(217, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(724, 1, true, new List<EventContainer>()));
                                break;
                        }
                        break;

                    case 50:
                        switch (wave)
                        {
                            case 0:
                                summonParameters.AddRange(map.GenerateMonsters(134, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(91, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(89, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(77, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(71, 16, true, new List<EventContainer>()));
                                break;

                            case 1:
                                summonParameters.AddRange(map.GenerateMonsters(217, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(200, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(154, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(92, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(79, 16, true, new List<EventContainer>()));
                                break;

                            case 2:
                                summonParameters.AddRange(map.GenerateMonsters(235, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(226, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(214, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(204, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(201, 15, true, new List<EventContainer>()));
                                break;

                            case 3:
                                summonParameters.AddRange(map.GenerateMonsters(249, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(236, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(227, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(218, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(202, 15, true, new List<EventContainer>()));
                                break;

                            case 4:
                                summonParameters.AddRange(map.GenerateMonsters(583, 1, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(400, 13, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(255, 8, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(253, 13, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(251, 10, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(205, 14, true, new List<EventContainer>()));
                                break;
                        }
                        break;

                    case 60:
                        switch (wave)
                        {
                            case 0:
                                summonParameters.AddRange(map.GenerateMonsters(242, 12, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(234, 12, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(215, 12, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(207, 12, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(202, 13, true, new List<EventContainer>()));
                                break;

                            case 1:
                                summonParameters.AddRange(map.GenerateMonsters(402, 12, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(253, 12, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(237, 12, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(216, 12, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(205, 13, true, new List<EventContainer>()));
                                break;

                            case 2:
                                summonParameters.AddRange(map.GenerateMonsters(402, 12, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(243, 12, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(228, 12, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(255, 12, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(205, 13, true, new List<EventContainer>()));
                                break;

                            case 3:
                                summonParameters.AddRange(map.GenerateMonsters(268, 12, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(255, 12, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(254, 12, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(174, 12, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(172, 13, true, new List<EventContainer>()));
                                break;

                            case 4:
                                summonParameters.AddRange(map.GenerateMonsters(725, 1, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(407, 12, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(272, 12, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(261, 12, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(256, 12, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(275, 13, true, new List<EventContainer>()));
                                break;
                        }
                        break;

                    case 70:
                        switch (wave)
                        {
                            case 0:
                                summonParameters.AddRange(map.GenerateMonsters(402, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(253, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(237, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(216, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(205, 15, true, new List<EventContainer>()));
                                break;

                            case 1:
                                summonParameters.AddRange(map.GenerateMonsters(402, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(243, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(228, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(225, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(205, 15, true, new List<EventContainer>()));
                                break;

                            case 2:
                                summonParameters.AddRange(map.GenerateMonsters(255, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(254, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(251, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(174, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(172, 15, true, new List<EventContainer>()));
                                break;

                            case 3:
                                summonParameters.AddRange(map.GenerateMonsters(407, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(272, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(261, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(257, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(256, 15, true, new List<EventContainer>()));
                                break;

                            case 4:
                                summonParameters.AddRange(map.GenerateMonsters(748, 1, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(444, 13, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(439, 13, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(275, 13, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(274, 13, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(273, 13, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(163, 13, true, new List<EventContainer>()));
                                break;
                        }
                        break;

                    case 80:
                        switch (wave)
                        {
                            case 0:
                                summonParameters.AddRange(map.GenerateMonsters(1007, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1003, 15, false, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1002, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1001, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1000, 16, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1007, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1003, 15, false, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1002, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1001, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1000, 16, true, new List<EventContainer>()));
                                break;

                            case 1:
                                summonParameters.AddRange(map.GenerateMonsters(1199, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1198, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1197, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1196, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1123, 16, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1199, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1198, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1197, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1196, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1123, 16, true, new List<EventContainer>()));
                                break;

                            case 2:
                                summonParameters.AddRange(map.GenerateMonsters(1305, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1304, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1303, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1302, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1194, 16, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1305, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1304, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1303, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1302, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1194, 16, true, new List<EventContainer>()));
                                break;

                            case 3:
                                summonParameters.AddRange(map.GenerateMonsters(1902, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1901, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1900, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1045, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1043, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1042, 16, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1902, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1901, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1900, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1045, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1043, 15, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1042, 16, true, new List<EventContainer>()));
                                break;

                            case 4:
                                summonParameters.AddRange(map.GenerateMonsters(637, 1, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(637, 1, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1903, 13, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1053, 13, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1051, 13, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1049, 13, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1048, 13, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1047, 13, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(637, 1, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1903, 13, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1053, 13, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1051, 13, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1049, 13, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1048, 13, true, new List<EventContainer>()));
                                summonParameters.AddRange(map.GenerateMonsters(1047, 13, true, new List<EventContainer>()));
                                break;
                        }
                        break;
                }
                return summonParameters;
            }

            #endregion
        }

        #endregion
    }
}