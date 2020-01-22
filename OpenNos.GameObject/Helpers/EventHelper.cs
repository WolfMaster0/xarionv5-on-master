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
using OpenNos.Domain;
using OpenNos.GameObject.Event;
using OpenNos.GameObject.Event.GAMES;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;
using OpenNos.PathFinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using OpenNos.Data;
using OpenNos.GameLog.LogHelper;
using OpenNos.GameObject.Event.ACT4;
using OpenNos.GameObject.Event.INSTANTBATTLE;
using OpenNos.GameObject.Event.LOD;
using OpenNos.GameObject.Event.MINILANDREFRESH;
using OpenNos.GameObject.Event.TALENTARENA;
using OpenNos.GameObject.EventArguments;

namespace OpenNos.GameObject.Helpers
{
    public class EventHelper
    {
        #region Members

        private static EventHelper _instance;

        #endregion

        #region Properties

        public static EventHelper Instance => _instance ?? (_instance = new EventHelper());

        #endregion

        #region Methods

        public static int CalculateComboPoint(int n)
        {
            int a = 4;
            int b = 7;
            for (int i = 0; i < n; i++)
            {
                int temp = a;
                a = b;
                b = temp + b;
            }
            return a;
        }

        public static void GenerateEvent(EventType type)
        {
            if (!ServerManager.Instance.StartedEvents.Contains(type))
            {
                Task.Factory.StartNew(() =>
                {
                    ServerManager.Instance.StartedEvents.Add(type);
                    switch (type)
                    {
                        case EventType.RankingRefresh:
                            ServerManager.Instance.RefreshRanking();
                            ServerManager.Instance.StartedEvents.Remove(EventType.RankingRefresh);
                            break;

                        case EventType.LOD:
                            Lod.GenerateLod();
                            break;

                        case EventType.MinilandRefresh:
                            MinilandRefresh.GenerateMinilandEvent();
                            break;

                        case EventType.InstantBattle:
                            InstantBattle.GenerateInstantBattle();
                            break;

                        case EventType.LODDH:
                            Lod.GenerateLod(35);
                            break;

                        case EventType.MeteoriteGame:
                            MeteoriteGame.GenerateMeteoriteGame();
                            break;

                        case EventType.Act4Ship:
                            Act4Ship.GenerateAct4Ship(1);
                            Act4Ship.GenerateAct4Ship(2);
                            break;

                        case EventType.TalentArena:
                            TalentArena.Run();
                            break;

                        case EventType.Caligor:
                            CaligorRaid.Run();
                            break;
                    }
                });
            }
        }

        public static TimeSpan GetMilisecondsBeforeTime(TimeSpan time)
        {
            TimeSpan now = TimeSpan.Parse(DateTime.UtcNow.ToString("HH:mm"));
            TimeSpan timeLeftUntilFirstRun = time - now;
            if (timeLeftUntilFirstRun.TotalHours < 0)
            {
                timeLeftUntilFirstRun += new TimeSpan(24, 0, 0);
            }
            return timeLeftUntilFirstRun;
        }

        public void RunEvent(EventContainer evt, ClientSession session = null, MapMonster monster = null)
        {
            if (evt != null)
            {
                if (session != null)
                {
                    evt.MapInstance = session.CurrentMapInstance;
                    switch (evt.EventActionType)
                    {
                        #region EventForUser

                        case EventActionType.NpcDialog:
                            session.SendPacket(session.Character.GenerateNpcDialog((int)evt.Parameter));
                            break;

                        case EventActionType.SendPacket:
                            session.SendPacket((string)evt.Parameter);
                            break;

                            #endregion
                    }
                }
                if (evt.MapInstance != null)
                {
                    switch (evt.EventActionType)
                    {
                        #region EventForUser

                        case EventActionType.NpcDialog:
                        case EventActionType.SendPacket:
                            if (session == null)
                            {
                                evt.MapInstance.Sessions.ToList().ForEach(e => RunEvent(evt, e));
                            }
                            break;

                        #endregion

                        #region MapInstanceEvent

                        case EventActionType.RegisterEvent:
                            Tuple<string, List<EventContainer>> even = (Tuple<string, List<EventContainer>>)evt.Parameter;
                            switch (even.Item1)
                            {
                                case "OnCharacterDiscoveringMap":
                                    even.Item2.ForEach(s => evt.MapInstance.OnCharacterDiscoveringMapEvents.Add(new Tuple<EventContainer, List<long>>(s, new List<long>())));
                                    break;

                                case "OnMoveOnMap":
                                    evt.MapInstance.OnMoveOnMapEvents.AddRange(even.Item2);
                                    break;

                                case "OnMapClean":
                                    evt.MapInstance.OnMapClean.AddRange(even.Item2);
                                    break;

                                case "OnLockerOpen":
                                    evt.MapInstance.UnlockEvents.AddRange(even.Item2);
                                    break;
                            }
                            break;

                        case EventActionType.RegisterWave:
                            evt.MapInstance.WaveEvents.Add((EventWave)evt.Parameter);
                            break;

                        case EventActionType.SetAreaEntry:
                            ZoneEvent even2 = (ZoneEvent)evt.Parameter;
                            evt.MapInstance.OnAreaEntryEvents.Add(even2);
                            break;

                        case EventActionType.RemoveMonsterLocker:
                            if (evt.MapInstance.InstanceBag.MonsterLocker.Current > 0)
                            {
                                evt.MapInstance.InstanceBag.MonsterLocker.Current--;
                            }
                            if (evt.MapInstance.InstanceBag.MonsterLocker.Current == 0 && evt.MapInstance.InstanceBag.ButtonLocker.Current == 0)
                            {
                                evt.MapInstance.UnlockEvents.ForEach(s => RunEvent(s));
                                evt.MapInstance.UnlockEvents.RemoveAll(s => s != null);
                            }
                            break;

                        case EventActionType.RemoveButtonLocker:
                            if (evt.MapInstance.InstanceBag.ButtonLocker.Current > 0)
                            {
                                evt.MapInstance.InstanceBag.ButtonLocker.Current--;
                            }
                            if (evt.MapInstance.InstanceBag.MonsterLocker.Current == 0 && evt.MapInstance.InstanceBag.ButtonLocker.Current == 0)
                            {
                                evt.MapInstance.UnlockEvents.ForEach(s => RunEvent(s));
                                evt.MapInstance.UnlockEvents.RemoveAll(s => s != null);
                            }
                            break;

                        case EventActionType.Effect:
                            short evt3 = (short)evt.Parameter;
                            if (monster != null)
                            {
                                monster.LastEffect = DateTime.UtcNow;
                                evt.MapInstance.Broadcast(StaticPacketHelper.GenerateEff(UserType.Monster, monster.MapMonsterId, evt3));
                            }
                            break;

                        case EventActionType.ControleMonsterInRange:
                            if (monster != null)
                            {
                                Tuple<short, byte, List<EventContainer>> evnt = (Tuple<short, byte, List<EventContainer>>)evt.Parameter;
                                List<MapMonster> mapMonsters = evt.MapInstance.GetListMonsterInRange(monster.MapX, monster.MapY, evnt.Item2);
                                if (evnt.Item1 != 0)
                                {
                                    mapMonsters.RemoveAll(s => s.MonsterVNum != evnt.Item1);
                                }
                                mapMonsters.ForEach(s => evnt.Item3.ForEach(e => RunEvent(e, monster: s)));
                            }
                            break;

                        case EventActionType.OnTarget:
                            if (monster?.MoveEvent?.InZone(monster.MapX, monster.MapY) == true)
                            {
                                monster.MoveEvent = null;
                                monster.Path = new List<Node>();
                                ((List<EventContainer>)evt.Parameter).ForEach(s => RunEvent(s, monster: monster));
                            }
                            break;

                        case EventActionType.Move:
                            ZoneEvent evt4 = (ZoneEvent)evt.Parameter;
                            if (monster != null)
                            {
                                monster.MoveEvent = evt4;
                                monster.Path = BestFirstSearch.FindPathJagged(new Node { X = monster.MapX, Y = monster.MapY }, new Node { X = evt4.X, Y = evt4.Y }, evt.MapInstance?.Map.JaggedGrid);
                            }
                            break;

                        case EventActionType.Clock:
                            evt.MapInstance.InstanceBag.Clock.TotalSecondsAmount = Convert.ToInt32(evt.Parameter);
                            evt.MapInstance.InstanceBag.Clock.SecondsRemaining = Convert.ToInt32(evt.Parameter);
                            break;

                        case EventActionType.SetMonsterLockers:
                            evt.MapInstance.InstanceBag.MonsterLocker.Current = Convert.ToByte(evt.Parameter);
                            evt.MapInstance.InstanceBag.MonsterLocker.Initial = Convert.ToByte(evt.Parameter);
                            break;

                        case EventActionType.SetButtonLockers:
                            evt.MapInstance.InstanceBag.ButtonLocker.Current = Convert.ToByte(evt.Parameter);
                            evt.MapInstance.InstanceBag.ButtonLocker.Initial = Convert.ToByte(evt.Parameter);
                            break;

                        case EventActionType.ScriptEnd:
                            switch (evt.MapInstance.MapInstanceType)
                            {
                                case MapInstanceType.TimeSpaceInstance:
                                    evt.MapInstance.InstanceBag.EndState = (byte)evt.Parameter;
                                    ClientSession client = evt.MapInstance.Sessions.FirstOrDefault();
                                    if (client != null)
                                    {
                                        Guid mapInstanceId = ServerManager.GetBaseMapInstanceIdByMapId(client.Character.MapId);
                                        MapInstance map = ServerManager.GetMapInstance(mapInstanceId);
                                        ScriptedInstance si = map.ScriptedInstances.Find(s => s.PositionX == client.Character.MapX && s.PositionY == client.Character.MapY);
                                        byte penalty = 0;
                                        if (penalty > (client.Character.Level - si.LevelMinimum) * 2)
                                        {
                                            penalty = penalty > 100 ? (byte)100 : penalty;
                                            client.SendPacket(client.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("TS_PENALTY"), penalty), 10));
                                        }
                                        int point = evt.MapInstance.InstanceBag.Point * (100 - penalty) / 100;
                                        string perfection = string.Empty;
                                        perfection += evt.MapInstance.InstanceBag.MonstersKilled >= si.MonsterAmount ? 1 : 0;
                                        perfection += evt.MapInstance.InstanceBag.NpcsKilled == 0 ? 1 : 0;
                                        perfection += evt.MapInstance.InstanceBag.RoomsVisited >= si.RoomAmount ? 1 : 0;
                                        evt.MapInstance.Broadcast($"score  {evt.MapInstance.InstanceBag.EndState} {point} 27 47 18 {si.DrawItems.Count} {evt.MapInstance.InstanceBag.MonstersKilled} {si.NpcAmount - evt.MapInstance.InstanceBag.NpcsKilled} {evt.MapInstance.InstanceBag.RoomsVisited} {perfection} 1 1");
                                        client.Character.OnFinishScriptedInstance(
                                            new FinishScriptedInstanceEventArgs(ScriptedInstanceType.TimeSpace, si.Id,
                                                point));
                                    }
                                    break;

                                case MapInstanceType.RaidInstance:
                                    evt.MapInstance.InstanceBag.EndState = (byte)evt.Parameter;
                                    client = evt.MapInstance.Sessions.FirstOrDefault();
                                    if (client != null)
                                    {
                                        Group grp = client.Character?.Group;
                                        if (grp == null)
                                        {
                                            return;
                                        }
                                        if (evt.MapInstance.InstanceBag.EndState == 1 && evt.MapInstance.Monsters.Any(s => s.IsBoss))
                                        {
                                            foreach (ClientSession sess in grp.Characters.Where(s => s.CurrentMapInstance.Monsters.Any(e => e.IsBoss)))
                                            {
                                                if (grp.Raid?.GiftItems != null)
                                                {
                                                    foreach (Gift gift in grp.Raid.GiftItems)
                                                    {
                                                        const sbyte rare = 0;
                                                        sess.Character.GiftAdd(gift.VNum, gift.Amount, rare, 0,
                                                            gift.Design, gift.IsRandomRare);
                                                    }
                                                }

                                                if (grp.Raid != null)
                                                {
                                                    sess.Character.OnFinishScriptedInstance(
                                                        new FinishScriptedInstanceEventArgs(ScriptedInstanceType.Raid,
                                                            grp.Raid.Id));
                                                }
                                            }
                                            foreach (MapMonster mon in evt.MapInstance.Monsters)
                                            {
                                                mon.CurrentHp = 0;
                                                evt.MapInstance.Broadcast(StaticPacketHelper.Out(UserType.Monster, mon.MapMonsterId));
                                                evt.MapInstance.RemoveMonster(mon);
                                            }

                                            ClientSession leader = grp.Characters.ElementAt(0);

                                            GameLogger.Instance.LogRaidSuccess(ServerManager.Instance.ChannelId,
                                                leader.Character.Name, leader.Character.CharacterId, grp.GroupId,
                                                grp.Characters.GetAllItems().Select(s => s.Character)
                                                    .Cast<CharacterDTO>().ToList());

                                            ServerManager.Instance.Broadcast(UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("RAID_SUCCEED"), grp.Raid?.Label, grp.Characters.ElementAt(0).Character.Name), 0));
                                        }

                                        Observable.Timer(TimeSpan.FromSeconds(evt.MapInstance.InstanceBag.EndState == 1 ? 30 : 0)).Subscribe(o =>
                                        {
                                            ClientSession[] grpmembers = new ClientSession[40];
                                            grp.Characters.CopyTo(grpmembers);
                                            foreach (ClientSession targetSession in grpmembers)
                                            {
                                                if (targetSession != null)
                                                {
                                                    if (targetSession.Character.Hp <= 0)
                                                    {
                                                        targetSession.Character.Hp = 1;
                                                        targetSession.Character.Mp = 1;
                                                    }
                                                    targetSession.SendPacket(Character.GenerateRaidBf(evt.MapInstance.InstanceBag.EndState));
                                                    targetSession.SendPacket(targetSession.Character.GenerateRaid(1, true));
                                                    targetSession.SendPacket(targetSession.Character.GenerateRaid(2, true));
                                                    grp.LeaveGroup(targetSession);
                                                }
                                            }
                                            ServerManager.Instance.GroupList.RemoveAll(s => s.GroupId == grp.GroupId);
                                            ServerManager.Instance.GroupsThreadSafe.Remove(grp.GroupId);
                                            evt.MapInstance.Dispose();
                                        });
                                    }
                                    break;

                                case MapInstanceType.Act4Morcos:
                                case MapInstanceType.Act4Hatus:
                                case MapInstanceType.Act4Calvina:
                                case MapInstanceType.Act4Berios:
                                    client = evt.MapInstance.Sessions.FirstOrDefault();
                                    if (client != null)
                                    {
                                        Family fam = evt.MapInstance.Sessions.FirstOrDefault(s => s?.Character?.Family != null)?.Character.Family;
                                        if (fam != null)
                                        {
                                            fam.Act4Raid.Portals.RemoveAll(s => s.DestinationMapInstanceId.Equals(fam.Act4RaidBossMap.MapInstanceId));
                                            short destX = 38;
                                            short destY = 179;
                                            short rewardVNum = 882;
                                            switch (evt.MapInstance.MapInstanceType)
                                            {
                                                //Morcos is default
                                                case MapInstanceType.Act4Hatus:
                                                    destX = 18;
                                                    destY = 10;
                                                    rewardVNum = 185;
                                                    break;

                                                case MapInstanceType.Act4Calvina:
                                                    destX = 25;
                                                    destY = 7;
                                                    rewardVNum = 942;
                                                    break;

                                                case MapInstanceType.Act4Berios:
                                                    destX = 16;
                                                    destY = 25;
                                                    rewardVNum = 999;
                                                    break;
                                            }
                                            int count = evt.MapInstance.Sessions.Count(s => s?.Character != null);
                                            foreach (ClientSession sess in evt.MapInstance.Sessions)
                                            {
                                                if (sess?.Character != null)
                                                {
                                                    sess.Character.GiftAdd(rewardVNum, 1, forceRandom: true, minRare: 1, design: 255);
                                                    sess.Character.GenerateFamilyXp(10000 / count);
                                                }
                                            }

                                            GameLogger.Instance.LogGuildRaidSuccess(ServerManager.Instance.ChannelId,
                                                fam.Name, fam.FamilyId, evt.MapInstance.MapInstanceType,
                                                evt.MapInstance.Sessions.Select(s => s.Character).Cast<CharacterDTO>()
                                                    .ToList());

                                            //TODO: Famlog
                                            CommunicationServiceClient.Instance.SendMessageToCharacter(new ScsCharacterMessage
                                            {
                                                DestinationCharacterId = fam.FamilyId,
                                                SourceCharacterId = client.Character.CharacterId,
                                                SourceWorldId = ServerManager.Instance.WorldId,
                                                Message = UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("FAMILYRAID_SUCCESS"), 0),
                                                Type = MessageType.Family
                                            });

                                            //ServerManager.Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("FAMILYRAID_SUCCESS"), grp?.Raid?.Label, grp.Characters.ElementAt(0).Character.Name), 0));

                                            Observable.Timer(TimeSpan.FromSeconds(30)).Subscribe(o =>
                                            {
                                                foreach (ClientSession targetSession in evt.MapInstance.Sessions.ToArray())
                                                {
                                                    if (targetSession != null)
                                                    {
                                                        if (targetSession.Character.Hp <= 0)
                                                        {
                                                            targetSession.Character.Hp = 1;
                                                            targetSession.Character.Mp = 1;
                                                        }

                                                        ServerManager.Instance.ChangeMapInstance(targetSession.Character.CharacterId, fam.Act4Raid.MapInstanceId, destX, destY);
                                                    }
                                                }
                                                evt.MapInstance.Dispose();
                                            });
                                        }
                                    }
                                    break;

                                case MapInstanceType.CaligorInstance:
                                    FactionType winningFaction = CaligorRaid.AngelDamage > CaligorRaid.DemonDamage ? FactionType.Angel : FactionType.Demon;
                                    foreach (ClientSession sess in evt.MapInstance.Sessions)
                                    {
                                        if (sess?.Character == null)
                                        {
                                            continue;
                                        }

                                        if (CaligorRaid.RemainingTime > 2400)
                                        {
                                            sess.Character.GiftAdd(
                                                sess.Character.Faction == winningFaction
                                                    ? (short) 5960
                                                    : (short) 5961, 1);
                                        }
                                        else
                                        {
                                            sess.Character.GiftAdd(
                                                sess.Character.Faction == winningFaction
                                                    ? (short) 5961
                                                    : (short) 5958, 1);
                                        }
                                        sess.Character.GiftAdd(5959, 1);
                                        sess.Character.GenerateFamilyXp(500);
                                    }
                                    evt.MapInstance.Broadcast(UserInterfaceHelper.GenerateChdm(ServerManager.GetNpcMonster(2305).MaxHP, CaligorRaid.AngelDamage, CaligorRaid.DemonDamage, CaligorRaid.RemainingTime));
                                    break;
                            }
                            break;

                        case EventActionType.MapClock:
                            evt.MapInstance.Clock.TotalSecondsAmount = Convert.ToInt32(evt.Parameter);
                            evt.MapInstance.Clock.SecondsRemaining = Convert.ToInt32(evt.Parameter);
                            break;

                        case EventActionType.StartClock:
                            Tuple<List<EventContainer>, List<EventContainer>> eve = (Tuple<List<EventContainer>, List<EventContainer>>)evt.Parameter;
                            evt.MapInstance.InstanceBag.Clock.StopEvents = eve.Item1;
                            evt.MapInstance.InstanceBag.Clock.TimeoutEvents = eve.Item2;
                            evt.MapInstance.InstanceBag.Clock.StartClock();
                            evt.MapInstance.Broadcast(evt.MapInstance.InstanceBag.Clock.GetClock());
                            break;

                        case EventActionType.Teleport:
                            Tuple<short, short, short, short> tp = (Tuple<short, short, short, short>)evt.Parameter;
                            List<Character> characters = evt.MapInstance.GetCharactersInRange(tp.Item1, tp.Item2, 5).ToList();
                            characters.ForEach(s =>
                            {
                                s.PositionX = tp.Item3;
                                s.PositionY = tp.Item4;
                                evt.MapInstance?.Broadcast(s.Session, s.GenerateTp(), ReceiverType.Group);
                            });
                            break;

                        case EventActionType.StopClock:
                            evt.MapInstance.InstanceBag.Clock.StopClock();
                            evt.MapInstance.Broadcast(evt.MapInstance.InstanceBag.Clock.GetClock());
                            break;

                        case EventActionType.StartMapClock:
                            eve = (Tuple<List<EventContainer>, List<EventContainer>>)evt.Parameter;
                            evt.MapInstance.Clock.StopEvents = eve.Item1;
                            evt.MapInstance.Clock.TimeoutEvents = eve.Item2;
                            evt.MapInstance.Clock.StartClock();
                            evt.MapInstance.Broadcast(evt.MapInstance.Clock.GetClock());
                            break;

                        case EventActionType.StopMapClock:
                            evt.MapInstance.Clock.StopClock();
                            evt.MapInstance.Broadcast(evt.MapInstance.Clock.GetClock());
                            break;

                        case EventActionType.SpawnPortal:
                            evt.MapInstance.CreatePortal((Portal)evt.Parameter);
                            break;

                        case EventActionType.RefreshMapItems:
                            evt.MapInstance.MapClear();
                            break;

                        case EventActionType.NpcsEffectChangeState:
                            evt.MapInstance.Npcs.ForEach(s => s.EffectActivated = (bool)evt.Parameter);
                            break;

                        case EventActionType.ChangePortalType:
                            Tuple<int, PortalType> param = (Tuple<int, PortalType>)evt.Parameter;
                            Portal portal = evt.MapInstance.Portals.Find(s => s.PortalId == param.Item1);
                            if (portal != null)
                            {
                                portal.Type = (short)param.Item2;
                            }
                            break;

                        case EventActionType.ChangeDropRate:
                            evt.MapInstance.DropRate = (int)evt.Parameter;
                            break;

                        case EventActionType.ChangExpRate:
                            evt.MapInstance.XpRate = (int)evt.Parameter;
                            break;

                        case EventActionType.DisposeMap:
                            evt.MapInstance.Dispose();
                            break;

                        case EventActionType.SpawnButton:
                            evt.MapInstance.SpawnButton((MapButton)evt.Parameter);
                            break;

                        case EventActionType.UnspawnMonsters:
                            evt.MapInstance.DespawnMonster((int)evt.Parameter);
                            break;

                        case EventActionType.SpawnMonster:
                            evt.MapInstance.SummonMonster((MonsterToSummon)evt.Parameter);
                            break;

                        case EventActionType.SpawnMonsters:
                            evt.MapInstance.SummonMonsters((List<MonsterToSummon>)evt.Parameter);
                            break;

                        case EventActionType.RefreshRaidGoal:
                            ClientSession cl = evt.MapInstance.Sessions.FirstOrDefault();
                            if (cl?.Character != null)
                            {
                                ServerManager.Instance.Broadcast(cl, cl.Character?.Group?.GeneraterRaidmbf(cl), ReceiverType.Group);
                                ServerManager.Instance.Broadcast(cl, UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NEW_MISSION"), 0), ReceiverType.Group);
                            }
                            break;

                        case EventActionType.SpawnNpc:
                            evt.MapInstance.SummonNpc((NpcToSummon)evt.Parameter);
                            break;

                        case EventActionType.SpawnNpcs:
                            evt.MapInstance.SummonNpcs((List<NpcToSummon>)evt.Parameter);
                            break;

                        case EventActionType.DropItems:
                            evt.MapInstance.DropItems((List<Tuple<short, int, short, short>>)evt.Parameter);
                            break;

                        case EventActionType.ThrowItems:
                            Tuple<int, short, byte, int, int> parameters = (Tuple<int, short, byte, int, int>)evt.Parameter;
                            if (monster != null)
                            {
                                parameters = new Tuple<int, short, byte, int, int>(monster.MapMonsterId, parameters.Item2, parameters.Item3, parameters.Item4, parameters.Item5);
                            }
                            evt.MapInstance.ThrowItems(parameters);
                            break;

                        case EventActionType.SpawnOnLastEntry:
                            Character lastincharacter = evt.MapInstance.Sessions.OrderByDescending(s => s.RegisterTime).FirstOrDefault()?.Character;
                            List<MonsterToSummon> summonParameters = new List<MonsterToSummon>();
                            MapCell hornSpawn = new MapCell
                            {
                                X = lastincharacter?.PositionX ?? 154,
                                Y = lastincharacter?.PositionY ?? 140
                            };
                            long hornTarget = lastincharacter?.CharacterId ?? -1;
                            summonParameters.Add(new MonsterToSummon(Convert.ToInt16(evt.Parameter), hornSpawn, hornTarget, true));
                            evt.MapInstance.SummonMonsters(summonParameters);
                            break;

                            #endregion
                    }
                }
            }
        }

        public void ScheduleEvent(TimeSpan timeSpan, EventContainer evt) => Observable.Timer(timeSpan).Subscribe(x => RunEvent(evt));

        #endregion
    }
}