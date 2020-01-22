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
using OpenNos.Core.Threading;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Battle;
using OpenNos.GameObject.Event;
using OpenNos.GameObject.Event.ACT4;
using OpenNos.GameObject.EventArguments;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.Npc;
using OpenNos.PathFinder;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using static OpenNos.Domain.BCardType;

namespace OpenNos.GameObject
{
    public class MapMonster : MapMonsterDTO, IDisposable
    {
        #region Members

        private bool _disposed;
        private int _movetime;

        private bool _noAttack;

        private bool _noMove;

        private long _target;
        private int _waitCount;

        private readonly short[] _percentageBosses = {1381, 388};

        #endregion

        #region Instantiation

        public MapMonster()
        {
            Buff = new ThreadSafeSortedList<short, Buff>();
            HitQueue = new ConcurrentQueue<HitRequest>();
            OnDeathEvents = new List<EventContainer>();
            OnNoticeEvents = new List<EventContainer>();
        }

        public MapMonster(MapMonsterDTO input)
        {
            Buff = new ThreadSafeSortedList<short, Buff>();
            HitQueue = new ConcurrentQueue<HitRequest>();
            OnDeathEvents = new List<EventContainer>();
            OnNoticeEvents = new List<EventContainer>();
            IsDisabled = input.IsDisabled;
            IsMoving = input.IsMoving;
            MapId = input.MapId;
            MapMonsterId = input.MapMonsterId;
            MapX = input.MapX;
            MapY = input.MapY;
            MonsterVNum = input.MonsterVNum;
            Position = input.Position;
        }

        #endregion

        #region Properties

        public ThreadSafeSortedList<short, Buff> Buff { get; }

        public int CurrentHp { get; set; }

        public int CurrentMp { get; set; }

        public IDictionary<long, long> DamageList { get; private set; }

        public DateTime Death { private get; set; }

        public ConcurrentQueue<HitRequest> HitQueue { get; }

        public DateTime IgnoreTargetsUntil { private get; set; }

        public bool IsMateTrainer { get; set; }

        public bool IsAlive { get; set; }

        public bool IsBonus { private get; set; }

        public bool IsBoss { get; set; }

        public bool IsHostile { private get; set; }

        public bool IsTarget { private get; set; }

        public DateTime LastEffect { private get; set; }

        public DateTime LastMove { private get; set; }

        public MapInstance MapInstance { get; set; }

        public int MaxHp { get; private set; }

        public int MaxMp { get; private set; }

        public NpcMonster Monster { get; private set; }

        public ZoneEvent MoveEvent { get; set; }

        public bool NoAggresiveIcon { private get; set; }

        public List<EventContainer> OnDeathEvents { get; set; }

        public List<EventContainer> OnNoticeEvents { private get; set; }

        public List<Node> Path { private get; set; }

        public bool? ShouldRespawn { get; set; }

        public long Target
        {
            get => _target;
            set
            {
                if (value == -1 || IgnoreTargetsUntil < DateTime.UtcNow)
                {
                    _target = value;
                }
            }
        }

        private short FirstX { get; set; }

        private short FirstY { get; set; }

        private DateTime LastSkill { get; set; }

        private List<NpcMonsterSkill> Skills { get; set; }

        private UserType TargetType { get; set; }

        #endregion

        #region Methods

        public void AddBuff(Buff indicator)
        {
            if (indicator?.Card != null)
            {
                Buff[indicator.Card.CardId] = indicator;
                indicator.RemainingTime = indicator.Card.Duration;
                indicator.Start = DateTime.UtcNow;

                indicator.Card.BCards.ForEach(c => c.ApplyBCards(this));
                Observable.Timer(TimeSpan.FromMilliseconds(indicator.Card.Duration * 100)).Subscribe(o =>
                {
                    RemoveBuff(indicator.Card.CardId);
                    if (indicator.Card.TimeoutBuff != 0
                        && ServerManager.RandomNumber() < indicator.Card.TimeoutBuffChance)
                    {
                        AddBuff(new Buff(indicator.Card.TimeoutBuff, Monster.Level));
                    }
                });
                _noAttack |= indicator.Card.BCards.Any(s =>
                    s.Type == (byte)CardType.SpecialAttack
                    && s.SubType.Equals((byte)AdditionalTypes.SpecialAttack.NoAttack / 10));
                _noMove |= indicator.Card.BCards.Any(s =>
                    s.Type == (byte)CardType.Move
                    && s.SubType.Equals((byte)AdditionalTypes.Move.MovementImpossible / 10));
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string GenerateBoss() => $"rboss 3 {MapMonsterId} {CurrentHp} {MaxHp}";

        public string GenerateIn()
        {
            if (IsAlive && !IsDisabled)
            {
                return StaticPacketHelper.In(UserType.Monster, MonsterVNum, MapMonsterId, MapX, MapY, Position,
                    (int)(CurrentHp / (float)MaxHp * 100), (int)(CurrentMp / (float)MaxMp * 100), 0,
                    NoAggresiveIcon ? InRespawnType.NoEffect : InRespawnType.TeleportationEffect, false);
            }

            return string.Empty;
        }

        public void Initialize(MapInstance currentMapInstance)
        {
            MapInstance = currentMapInstance;
            Initialize();
        }

        /// <summary>
        /// Check if the Monster is in the given Range.
        /// </summary>
        /// <param name="mapX">The X coordinate on the Map of the object to check.</param>
        /// <param name="mapY">The Y coordinate on the Map of the object to check.</param>
        /// <param name="distance">The maximum distance of the object to check.</param>
        /// <returns>True if the Monster is in range, False if not.</returns>
        public bool IsInRange(short mapX, short mapY, byte distance)
        {
            return Map.GetDistance(new MapCell
            {
                X = mapX,
                Y = mapY
            }, new MapCell
            {
                X = MapX,
                Y = MapY
            }) <= distance + 1;
        }

        public void RunDeathEvent()
        {
            Buff.ClearAll();
            _noMove = false;
            _noAttack = false;
            if (IsBonus)
            {
                MapInstance.InstanceBag.Combo++;
                MapInstance.InstanceBag.Point += EventHelper.CalculateComboPoint(MapInstance.InstanceBag.Combo + 1);
            }
            else
            {
                MapInstance.InstanceBag.Combo = 0;
                MapInstance.InstanceBag.Point += EventHelper.CalculateComboPoint(MapInstance.InstanceBag.Combo);
            }

            MapInstance.InstanceBag.MonstersKilled++;
            OnDeathEvents.ForEach(e => EventHelper.Instance.RunEvent(e, monster: this));
        }

        public void SetDeathStatement()
        {
            IsAlive = false;
            CurrentHp = 0;
            CurrentMp = 0;
            Death = DateTime.UtcNow;
            LastMove = DateTime.UtcNow;
        }

        public void StartLife()
        {
            try
            {
                if (!MapInstance.IsSleeping)
                {
                    MonsterLife();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        /// <summary>
        /// Remove the current Target from Monster.
        /// </summary>
        internal void RemoveTarget()
        {
            if (Target != -1)
            {
                (Path ?? (Path = new List<Node>())).Clear();
                Target = -1;

                //return to origin
                Path = BestFirstSearch.FindPathJagged(new Node { X = MapX, Y = MapY }, new Node { X = FirstX, Y = FirstY },
                    MapInstance.Map.JaggedGrid);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "<Buff>k__BackingField")]
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Buff.Dispose();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Follow the Monsters target to it's position.
        /// </summary>
        /// <param name="targetSession">The TargetSession to follow</param>
        private void FollowTarget(ClientSession targetSession)
        {
            if (IsMoving && !_noMove)
            {
                const short maxDistance = 22;
                int distance = Map.GetDistance(new MapCell
                {
                    X = targetSession.Character.PositionX,
                    Y = targetSession.Character.PositionY
                },
                    new MapCell
                    {
                        X = MapX,
                        Y = MapY
                    });
                if (targetSession.Character.LastMonsterAggro.AddSeconds(5) < DateTime.UtcNow
                    || targetSession.Character.BrushFireJagged == null)
                {
                    targetSession.Character.UpdateBushFire();
                }

                targetSession.Character.LastMonsterAggro = DateTime.UtcNow;
                if (Path.Count == 0)
                {
                    short xoffset = (short)ServerManager.RandomNumber(-1, 1);
                    short yoffset = (short)ServerManager.RandomNumber(-1, 1);
                    try
                    {
                        Path = BestFirstSearch.TracePathJagged(new Node { X = MapX, Y = MapY },
                            targetSession.Character.BrushFireJagged,
                            targetSession.Character.MapInstance.Map.JaggedGrid);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(
                            $"Pathfinding using Pathfinder failed. Map: {MapId} StartX: {MapX} StartY: {MapY} TargetX: {(short)(targetSession.Character.PositionX + xoffset)} TargetY: {(short)(targetSession.Character.PositionY + yoffset)}",
                            ex);
                        RemoveTarget();
                    }
                }

                if (Monster != null && DateTime.UtcNow > LastMove && Monster.Speed > 0 && Path.Count > 0)
                {
                    int maxindex = Path.Count > Monster.Speed / 2 ? Monster.Speed / 2 : Path.Count;
                    short mapX = Path[maxindex - 1].X;
                    short mapY = Path[maxindex - 1].Y;
                    double waitingtime = WaitingTime(mapX, mapY);
                    MapInstance.Broadcast(new BroadcastPacket(null,
                        StaticPacketHelper.Move(UserType.Monster, MapMonsterId, mapX, mapY,
                            Monster.Speed), ReceiverType.All, xCoordinate: mapX, yCoordinate: mapY));

                    Observable.Timer(TimeSpan.FromMilliseconds((int)((waitingtime > 1 ? 1 : waitingtime) * 1000)))
                        .Subscribe(x =>
                        {
                            MapX = mapX;
                            MapY = mapY;
                        });
                    distance = (int)Path[0].F;
                    Path.RemoveRange(0, maxindex > Path.Count ? Path.Count : maxindex);
                }

                if (MapId != targetSession.Character.MapInstance.Map.MapId || distance > maxDistance + 3)
                {
                    if (_waitCount == 10)
                    {
                        RemoveTarget();
                        _waitCount = 0;
                    }

                    _waitCount++;
                }
                else
                {
                    _waitCount = 0;
                }
            }
        }

        private void FollowTarget(Mate mate)
        {
            if (IsMoving && !_noMove)
            {
                const short maxDistance = 22;
                int distance = Map.GetDistance(new MapCell
                {
                    X = mate.PositionX,
                    Y = mate.PositionY
                },
                    new MapCell
                    {
                        X = MapX,
                        Y = MapY
                    });

                if (mate.LastMonsterAggro.AddSeconds(5) < DateTime.UtcNow || mate.BrushFireJagged == null)
                {
                    mate.UpdateBushFire();
                }

                mate.LastMonsterAggro = DateTime.UtcNow;
                if (Path.Count == 0)
                {
                    short xoffset = (short)ServerManager.RandomNumber(-1, 1);
                    short yoffset = (short)ServerManager.RandomNumber(-1, 1);
                    try
                    {
                        Path = BestFirstSearch.TracePathJagged(new Node { X = MapX, Y = MapY }, mate.BrushFireJagged,
                            mate.Owner.MapInstance.Map.JaggedGrid);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(
                            $"Pathfinding using Pathfinder failed. Map: {MapId} StartX: {MapX} StartY: {MapY} TargetX: {(short)(mate.PositionX + xoffset)} TargetY: {(short)(mate.PositionY + yoffset)}",
                            ex);
                        RemoveTarget();
                    }
                }

                if (Monster != null && DateTime.UtcNow > LastMove && Monster.Speed > 0 && Path.Count > 0)
                {
                    int maxindex = Path.Count > Monster.Speed / 2 ? Monster.Speed / 2 : Path.Count;
                    short mapX = Path[maxindex - 1].X;
                    short mapY = Path[maxindex - 1].Y;
                    double waitingtime = WaitingTime(mapX, mapY);
                    MapInstance.Broadcast(new BroadcastPacket(null,
                        StaticPacketHelper.Move(UserType.Monster, MapMonsterId, mapX, mapY,
                            Monster.Speed), ReceiverType.All, xCoordinate: mapX, yCoordinate: mapY));

                    Observable.Timer(TimeSpan.FromMilliseconds((int)((waitingtime > 1 ? 1 : waitingtime) * 1000)))
                        .Subscribe(x =>
                        {
                            MapX = mapX;
                            MapY = mapY;
                        });
                    distance = (int)Path[0].F;
                    Path.RemoveRange(0, maxindex > Path.Count ? Path.Count : maxindex);
                }

                if (MapId != mate.Owner.MapInstance.Map.MapId || distance > maxDistance + 3)
                {
                    if (_waitCount == 10)
                    {
                        RemoveTarget();
                        _waitCount = 0;
                    }

                    _waitCount++;
                }
                else
                {
                    _waitCount = 0;
                }
            }
        }

        public void GetNearestOponent()
        {
            if (Target == -1)
            {
                const int maxDistance = 22;
                int distance = 100;
                List<ClientSession> sess = new List<ClientSession>();
                DamageList.Keys.ToList().ForEach(s => sess.Add(MapInstance.GetSessionByCharacterId(s)));
                int matedistance = distance;

                Character character = null;
                Mate mate = null;
                if (!IsMateTrainer)
                {
                    character = sess
                        .Where(s => s?.Character != null
                            && (ServerManager.Instance.ChannelId != 51
                                || (MonsterVNum - (byte)s.Character.Faction != 678
                                    && MonsterVNum - (byte)s.Character.Faction != 971)) && s.Character.Hp > 0
                            && !s.Character.InvisibleGm && !s.Character.Invisible
                            && s.Character.MapInstance == MapInstance
                            && Map.GetDistance(new MapCell { X = MapX, Y = MapY },
                                new MapCell { X = s.Character.PositionX, Y = s.Character.PositionY })
                            < Monster.NoticeRange)
                        .OrderBy(s => distance = Map.GetDistance(new MapCell { X = MapX, Y = MapY },
                            new MapCell { X = s.Character.PositionX, Y = s.Character.PositionY })).FirstOrDefault()
                        ?.Character;
                     mate = sess.SelectMany(x => x?.Character?.Mates).Where(s =>
                        s.IsTeamMember && s.Owner != null
                        && (ServerManager.Instance.ChannelId != 51
                            || (MonsterVNum - (byte)s.Owner.Faction != 678 && MonsterVNum - (byte)s.Owner.Faction != 971))
                        && s.IsAlive && !s.Owner.InvisibleGm && !s.Owner.Invisible && s.Owner.MapInstance == MapInstance
                        && Map.GetDistance(new MapCell { X = MapX, Y = MapY }, new MapCell { X = s.PositionX, Y = s.PositionY })
                        < Monster.NoticeRange).OrderBy(s => matedistance = Map.GetDistance(new MapCell { X = MapX, Y = MapY },
                        new MapCell { X = s.Owner.PositionX, Y = s.Owner.PositionY })).FirstOrDefault();
                }
                else
                {
                    mate = sess.SelectMany(x => x?.Character?.Mates).Where(s =>
                        s.IsTeamMember 
                        && s.Owner != null
                        && (ServerManager.Instance.ChannelId != 51|| (MonsterVNum - (byte)s.Owner.Faction != 678 && MonsterVNum - (byte)s.Owner.Faction != 971))
                        && s.IsAlive
                        && s.IsTeamMember
                        && s.MateType == MateType.Pet
                        && !s.Owner.InvisibleGm
                        && !s.Owner.Invisible
                        && s.Owner.MapInstance == MapInstance
                        && Map.GetDistance(new MapCell { X = MapX, Y = MapY }, new MapCell { X = s.PositionX, Y = s.PositionY })
                        < Monster.NoticeRange).OrderBy(s => matedistance = Map.GetDistance(new MapCell { X = MapX, Y = MapY },
                        new MapCell { X = s.Owner.PositionX, Y = s.Owner.PositionY })).FirstOrDefault();
                }

                if (matedistance < distance && mate != null)
                {
                    Target = mate.MateTransportId;
                    TargetType = UserType.Npc;
                }
                else if (distance < maxDistance && character != null)
                {
                    Target = character.CharacterId;
                    TargetType = UserType.Player;
                }
            }
        }

        private void HostilityTarget()
        {
            void TargetMate(Mate mate)
            {
                if (OnNoticeEvents.Count == 0 && MoveEvent == null)
                {
                    Target = mate.MateTransportId;
                    TargetType = UserType.Npc;
                    if (!NoAggresiveIcon)
                    {
                        mate.Owner.Session.SendPacket(StaticPacketHelper.GenerateEff(UserType.Monster, MapMonsterId,
                            5000));
                    }
                }

                OnNoticeEvents.ForEach(e => EventHelper.Instance.RunEvent(e, monster: this));
                OnNoticeEvents.RemoveAll(s => s != null);
            }

            void TargetCharacter(Character character)
            {
                if (OnNoticeEvents.Count == 0 && MoveEvent == null)
                {
                    Target = character.CharacterId;
                    TargetType = UserType.Player;
                    if (!NoAggresiveIcon)
                    {
                        character.Session.SendPacket(StaticPacketHelper.GenerateEff(UserType.Monster, MapMonsterId,
                            5000));
                    }
                }

                OnNoticeEvents.ForEach(e => EventHelper.Instance.RunEvent(e, monster: this));
                OnNoticeEvents.RemoveAll(s => s != null);
            }

            if (IsHostile && Target == -1)
            {
                Character character = MapInstance.Sessions.Where(s =>
                        s?.Character != null
                        && (ServerManager.Instance.ChannelId != 51
                         || (MonsterVNum - (byte)s.Character.Faction != 678
                          && MonsterVNum - (byte)s.Character.Faction != 971)) && s.Character.Hp > 0
                        && !s.Character.InvisibleGm && !s.Character.Invisible
                        && Map.GetDistance(new MapCell { X = MapX, Y = MapY },
                            new MapCell { X = s.Character.PositionX, Y = s.Character.PositionY }) < Monster.NoticeRange)
                    .OrderBy(s => ServerManager.RandomNumber(0, int.MaxValue)).FirstOrDefault()?.Character;
                Mate mate = MapInstance.Sessions.SelectMany(x => x?.Character?.Mates).Where(s =>
                        s.IsTeamMember && s.Owner != null
                        && (ServerManager.Instance.ChannelId != 51
                         || (MonsterVNum - (byte)s.Owner.Faction != 678
                          && MonsterVNum - (byte)s.Owner.Faction != 971))
                        && s.IsAlive && !s.Owner.InvisibleGm && !s.Owner.Invisible
                        && Map.GetDistance(new MapCell { X = MapX, Y = MapY },
                            new MapCell { X = s.PositionX, Y = s.PositionY }) < Monster.NoticeRange)
                    .OrderBy(s => ServerManager.RandomNumber(0, int.MaxValue)).FirstOrDefault();
                if (character != null && mate != null)
                {
                    switch (ServerManager.RandomNumber(0, 2))
                    {
                        case 0:
                            TargetCharacter(character);
                            break;

                        case 1:
                            TargetMate(mate);
                            break;
                    }
                }
                else if (character != null)
                {
                    TargetCharacter(character);
                }
                else if (mate != null)
                {
                    TargetMate(mate);
                }
            }
        }

        private void Initialize()
        {
            FirstX = MapX;
            FirstY = MapY;
            LastSkill = LastMove = LastEffect = DateTime.UtcNow;
            Target = -1;
            Path = new List<Node>();
            IsAlive = true;
            ShouldRespawn = ShouldRespawn ?? true;
            Monster = ServerManager.GetNpcMonster(MonsterVNum);
            MaxHp = Monster.MaxHP;
            MaxMp = Monster.MaxMP;
            foreach (BCard bCard in Monster.BCards.Where(s => s.Type == (byte)CardType.Summons && s.SubType != 2))
            {
                bCard.ApplyBCards(this);
            }
            if (MapInstance?.MapInstanceType == MapInstanceType.RaidInstance)
            {
                if (IsBoss)
                {
                    MaxHp *= 6;
                    MaxMp *= 6;
                }
                else
                {
                    MaxHp *= 1;
                    MaxMp *= 1;

                    if (IsTarget)
                    {
                        MaxHp *= 4;
                        MaxMp *= 4;
                    }
                }
            }

            // Irrelevant for now(Act4)
            //if (MapInstance?.MapInstanceType == MapInstanceType.Act4Morcos || MapInstance?.MapInstanceType == MapInstanceType.Act4Hatus || MapInstance?.MapInstanceType == MapInstanceType.Act4Calvina || MapInstance?.MapInstanceType == MapInstanceType.Act4Berios)
            //{
            //    if (MonsterVNum == 563 || MonsterVNum == 577 || MonsterVNum == 629 || MonsterVNum == 624)
            //    {
            //        MaxHp *= 5;
            //        MaxMp *= 5;
            //    }
            //}

            NoAggresiveIcon = Monster.NoAggresiveIcon;
            IsHostile = Monster.IsHostile;
            CurrentHp = MaxHp;
            CurrentMp = MaxMp;
            Skills = Monster.Skills.ToList();
            DamageList = new Dictionary<long, long>();
            _movetime = ServerManager.RandomNumber(400, 1600);

            //Observable.Timer(TimeSpan.FromMilliseconds(_movetime)).Subscribe(observer => RegisterMonsterLife());
        }

        /// <summary>
        /// Handle any kind of Monster interaction
        /// </summary>
        private void MonsterLife()
        {
            if (Monster == null)
            {
                return;
            }

            if ((DateTime.UtcNow - LastEffect).TotalSeconds >= 5)
            {
                LastEffect = DateTime.UtcNow;
                if (IsTarget)
                {
                    MapInstance.Broadcast(StaticPacketHelper.GenerateEff(UserType.Monster, MapMonsterId, 824));
                }

                if (IsBonus)
                {
                    MapInstance.Broadcast(StaticPacketHelper.GenerateEff(UserType.Monster, MapMonsterId, 826));
                }
            }

            if (IsBoss && IsAlive)
            {
                MapInstance.Broadcast(GenerateBoss());
            }

            // handle hit queue
            while (HitQueue.TryDequeue(out HitRequest hitRequest))
            {
                if (IsAlive && hitRequest.Session.Character.Hp > 0
                            && (hitRequest.Mate?.IsAlive != false)
                            && (ServerManager.Instance.ChannelId != 51
                                || (MonsterVNum - (byte)hitRequest.Session.Character.Faction != 678
                                    && MonsterVNum - (byte)hitRequest.Session.Character.Faction != 971)))
                {
                    int damage;
                    bool onyxWings = false;
                    bool isCaptureSkill = false;
                    int hitmode = 0;
                    if (hitRequest.TargetHitType != TargetHitType.DirectInflict)
                    {
                        isCaptureSkill =
                            hitRequest.Skill?.BCards.Any(s => s.Type.Equals((byte)CardType.Capture)) ??
                            false;

                        // calculate damage
                        BattleEntity battleEntity = hitRequest.Mate == null
                            ? new BattleEntity(hitRequest.Session.Character, hitRequest.Skill)
                            : new BattleEntity(hitRequest.Mate);
                        damage = DamageHelper.Instance.CalculateDamage(battleEntity, new BattleEntity(this),
                            hitRequest.Skill, ref hitmode, ref onyxWings, _percentageBosses.Any(s=>s == MonsterVNum));
                        if (hitRequest.Skill != null && hitRequest.Mate == null
                            && hitRequest.Session.Character.Morph != 0 && hitRequest.Session.Character.Morph != 1
                            && hitRequest.Session.Character.Morph != 8 && hitRequest.Session.Character.Morph != 16
                            && hitRequest.Skill.BCards.All(s =>
                                s.Type != (byte)CardType.AttackPower && s.Type != (byte)CardType.Element))
                        {
                            damage = 0;
                            hitmode = 0;
                        }

                        if (Monster.BCards.Find(s =>
                            s.Type == (byte)CardType.LightAndShadow
                            && s.SubType == (byte)AdditionalTypes.LightAndShadow.InflictDamageToMP) is BCard card)
                        {
                            int reduce = damage / 100 * card.FirstData;
                            if (CurrentMp < reduce)
                            {
                                CurrentMp = 0;
                            }
                            else
                            {
                                CurrentMp -= reduce;
                            }
                        }

                        // Song of Sirens
                        if (Buff.ContainsKey(548) && damage > 0)
                        {
                            RemoveBuff(548);
                        }

                        if (damage >= CurrentHp
                            && Monster.BCards.Any(s => s.Type == 39 && s.SubType == 0 && s.ThirdData == -1))
                        {
                            damage = CurrentHp - 1;
                        }
                        else if (onyxWings)
                        {
                            short onyxX = (short)(hitRequest.Session.Character.PositionX + 2);
                            short onyxY = (short)(hitRequest.Session.Character.PositionY + 2);
                            int onyxId = MapInstance.GetNextMonsterId();
                            MapMonster onyx = new MapMonster
                            {
                                MonsterVNum = 2371,
                                MapX = onyxX,
                                MapY = onyxY,
                                MapMonsterId = onyxId,
                                IsHostile = false,
                                IsMoving = false,
                                ShouldRespawn = false
                            };
                            MapInstance.Broadcast(UserInterfaceHelper.GenerateGuri(31, 1,
                                hitRequest.Session.Character.CharacterId, onyxX, onyxY));
                            onyx.Initialize(MapInstance);
                            MapInstance.AddMonster(onyx);
                            MapInstance.Broadcast(onyx.GenerateIn());
                            CurrentHp -= damage / 2;
                            HitRequest request = hitRequest;
                            int damage1 = damage;
                            Observable.Timer(TimeSpan.FromMilliseconds(350)).Subscribe(o =>
                            {
                                MapInstance.Broadcast(StaticPacketHelper.SkillUsed(UserType.Monster, onyxId, 3,
                                    MapMonsterId, -1, 0, -1, request.Skill?.Effect ?? 0, -1, -1, IsAlive,
                                    (int)(CurrentHp / (float)MaxHp * 100), damage1 / 2, 0,
                                    0));
                                MapInstance.RemoveMonster(onyx);
                                MapInstance.Broadcast(StaticPacketHelper.Out(UserType.Monster, onyx.MapMonsterId));
                            });
                        }

                        if (hitRequest.Skill.SkillVNum == 1138)
                        {
                            int rdn = ServerManager.RandomNumber(0, 100);
                            int srdn = ServerManager.RandomNumber(0, 2);
                            short[] effct1 = { 4005, 4017 };
                            short[] effect2 = { 3807, 3819 };
                            short[] effect3 = { 4405, 4421 };
                            short[] effect4 = { 3908, 3916 };
                            short[] clone12 = { 42, 13 };
                            short[] clone34 = { 42, 40 };
                            int dam = hitRequest.Session.Character.Level * 17;

                            if (rdn < 35)
                            {
                                hitRequest.Session.Character.SpawnDarkClone(2, 2, 2112, 1, 3, MapMonsterId, (short)(1141 + srdn), clone12[srdn], effct1[srdn], dam);
                                hitRequest.Session.Character.SpawnDarkClone(-2, 2, 2113, 2, 3, MapMonsterId, (short)(1143 + srdn), clone12[srdn], effect2[srdn], dam);

                                CurrentHp = CurrentHp - dam * 2 > 0 ? CurrentHp - dam * 2 : CurrentHp = 1;
                            }
                            else if (rdn < 70)
                            {
                                hitRequest.Session.Character.SpawnDarkClone(2, 2, 2112, 1, 3, MapMonsterId, (short)(1141 + srdn), clone12[srdn], effct1[srdn], dam);
                                hitRequest.Session.Character.SpawnDarkClone(-2, 2, 2113, 2, 3, MapMonsterId, (short)(1143 + srdn), clone12[srdn], effect2[srdn], dam);
                                hitRequest.Session.Character.SpawnDarkClone(-2, -2, 2114, 3, 3, MapMonsterId, (short)(1145 + srdn), clone34[srdn], effect3[srdn], dam);

                                CurrentHp = CurrentHp - dam * 3 > 0 ? CurrentHp - dam * 3 : CurrentHp = 1;
                            }
                            else
                            {

                                hitRequest.Session.Character.SpawnDarkClone(2, 2, 2112, 1, 3, MapMonsterId, (short)(1141 + srdn), clone12[srdn], effct1[srdn], dam);
                                hitRequest.Session.Character.SpawnDarkClone(-2, 2, 2113, 2, 3, MapMonsterId, (short)(1143 + srdn), clone12[srdn], effect2[srdn], dam);
                                hitRequest.Session.Character.SpawnDarkClone(-2, -2, 2114, 3, 3, MapMonsterId, (short)(1145 + srdn), clone34[srdn], effect3[srdn], dam);
                                hitRequest.Session.Character.SpawnDarkClone(2, -2, 2115, 4, 3, MapMonsterId, (short)(1147 + srdn), clone34[srdn], effect4[srdn], dam);

                                CurrentHp = CurrentHp - dam * 4 > 0 ? CurrentHp - dam * 4 : CurrentHp = 1;
                            }
                        }

                        if (hitRequest.Skill.SkillVNum == 1085)
                        {
                            hitRequest.Session.Character.MapInstance.Broadcast(
                                $"tp 1 {hitRequest.Session.Character.CharacterId} {MapX} {MapY} 0");
                            hitRequest.Session.Character.PositionX = MapX;
                            hitRequest.Session.Character.PositionY = MapY;
                        }

                        foreach (BCard bcard in hitRequest.Skill.BCards)
                        {
                            switch (bcard.Type)
                            {
                                case (byte)CardType.SpecialActions:
                                    switch (bcard.SubType)
                                    {
                                        case ((byte)AdditionalTypes.SpecialActions.PushBack / 10):
                                            {
                                                short destinationX = MapX;
                                                short destinationY = MapY;
                                                if (MapX < hitRequest.Session.Character.PositionX)
                                                {
                                                    destinationX--;
                                                }
                                                else if (MapX > hitRequest.Session.Character.PositionX)
                                                {
                                                    destinationX++;
                                                }

                                                if (MapY < hitRequest.Session.Character.PositionY)
                                                {
                                                    destinationY--;
                                                }
                                                else if (MapY > hitRequest.Session.Character.PositionY)
                                                {
                                                    destinationY++;
                                                }

                                                hitRequest.Session.Character.MapInstance.Broadcast(
                                                    $"guri 3 3 {MapMonsterId} {destinationX} {destinationY} 3 {bcard.SecondData} 2 -1");
                                                MapX = destinationX;
                                                MapY = destinationY;
                                            }
                                            break;

                                        case ((byte)AdditionalTypes.SpecialActions.FocusEnemies / 10):
                                            {
                                                short destinationX = hitRequest.Session.Character.PositionX;
                                                short destinationY = hitRequest.Session.Character.PositionY;
                                                if (MapX < hitRequest.Session.Character.PositionX)
                                                {
                                                    destinationX--;
                                                }
                                                else if (MapX > hitRequest.Session.Character.PositionX)
                                                {
                                                    destinationX++;
                                                }

                                                if (MapY < hitRequest.Session.Character.PositionY)
                                                {
                                                    destinationY--;
                                                }
                                                else if (MapY > hitRequest.Session.Character.PositionY)
                                                {
                                                    destinationY++;
                                                }

                                                hitRequest.Session.Character.MapInstance.Broadcast(
                                                    $"guri 3 3 {MapMonsterId} {destinationX} {destinationY} 3 {bcard.SecondData} 2 -1");
                                                MapX = destinationX;
                                                MapY = destinationY;
                                            }
                                            break;
                                    }

                                    break;
                            }
                        }

                        if (hitmode != 1)
                        {
                            hitRequest.Skill?.BCards.ForEach(bc =>
                            {
                                if (bc.Type == (byte)CardType.Buff)
                                {
                                    if (hitRequest.Mate != null && bc.BuffCard?.BuffType == BuffType.Good)
                                    {
                                        var bonusBuff = 0;

                                        if (hitRequest.Mate.SpInstance != null && hitRequest.Mate.IsUsingSp && bc.BuffCard?.CardId >= 2000)
                                        {
                                            if (hitRequest.Mate.SpInstance.FirstPartnerSkill == hitRequest.Skill.SkillVNum)
                                            {
                                                bonusBuff = (int)(hitRequest.Mate.SpInstance.FirstPartnerSkillRank - 1);
                                            }
                                            else if (hitRequest.Mate.SpInstance.SecondPartnerSkill == hitRequest.Skill.SkillVNum)
                                            {
                                                bonusBuff = (int)(hitRequest.Mate.SpInstance.SecondPartnerSkillRank - 1);
                                            }
                                            else if (hitRequest.Mate.SpInstance.ThirdPartnerSkill == hitRequest.Skill.SkillVNum)
                                            {
                                                bonusBuff = (int)(hitRequest.Mate.SpInstance.ThirdPartnerSkillRank - 1);
                                            }
                                        }

                                        bc.ApplyBCards(hitRequest.Mate, buffLevel: (short)bonusBuff);
                                        bc.ApplyBCards(hitRequest.Mate.Owner, buffLevel: (short)bonusBuff);
                                    }
                                    else
                                    {
                                        bc.ApplyBCards(bc.BuffCard?.BuffType != BuffType.Good
                                            ? this : (object)hitRequest.Session.Character, hitRequest.Session.Character);
                                    }
                                }
                                else
                                {
                                    bc.ApplyBCards(this, hitRequest.Session.Character);
                                }
                            });
                            hitRequest.Skill?.BCards.Where(s => s.Type.Equals((byte)CardType.Capture)).ToList()
                                .ForEach(s => s.ApplyBCards(this, hitRequest.Session));
                            if (battleEntity?.ShellWeaponEffects != null)
                            {
                                foreach (ShellEffectDTO shell in battleEntity.ShellWeaponEffects)
                                {
                                    switch (shell.Effect)
                                    {
                                        case (byte)ShellWeaponEffectType.Blackout:
                                            {
                                                Buff buff = new Buff(7, battleEntity.Level);
                                                if (ServerManager.RandomNumber() < shell.Value)
                                                {
                                                    AddBuff(buff);
                                                }

                                                break;
                                            }
                                        case (byte)ShellWeaponEffectType.DeadlyBlackout:
                                            {
                                                Buff buff = new Buff(66, battleEntity.Level);
                                                if (ServerManager.RandomNumber() < shell.Value)
                                                {
                                                    AddBuff(buff);
                                                }

                                                break;
                                            }
                                        case (byte)ShellWeaponEffectType.MinorBleeding:
                                            {
                                                Buff buff = new Buff(1, battleEntity.Level);
                                                if (ServerManager.RandomNumber() < shell.Value)
                                                {
                                                    AddBuff(buff);
                                                }

                                                break;
                                            }
                                        case (byte)ShellWeaponEffectType.Bleeding:
                                            {
                                                Buff buff = new Buff(21, battleEntity.Level);
                                                if (ServerManager.RandomNumber() < shell.Value)
                                                {
                                                    AddBuff(buff);
                                                }

                                                break;
                                            }
                                        case (byte)ShellWeaponEffectType.HeavyBleeding:
                                            {
                                                Buff buff = new Buff(42, battleEntity.Level);
                                                if (ServerManager.RandomNumber() < shell.Value)
                                                {
                                                    AddBuff(buff);
                                                }

                                                break;
                                            }
                                        case (byte)ShellWeaponEffectType.Freeze:
                                            {
                                                Buff buff = new Buff(27, battleEntity.Level);
                                                if (ServerManager.RandomNumber() < shell.Value)
                                                {
                                                    AddBuff(buff);
                                                }

                                                break;
                                            }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        damage = hitRequest.DirectDamage;
                        if (damage >= CurrentHp
                            && Monster.BCards.Any(s => s.Type == 39 && s.SubType == 0 && s.ThirdData == -1))
                        {
                            damage = CurrentHp - 1;
                        }
                    }

                    if (hitRequest.Mate != null && IsMateTrainer)
                    {
                        if (DamageList.ContainsKey(hitRequest.Mate.MateTransportId))
                        {
                            DamageList[hitRequest.Mate.MateTransportId] += damage;
                        }
                        else if (!DamageList.ContainsKey(hitRequest.Mate.MateTransportId))
                        {
                            DamageList.Add(hitRequest.Mate.MateTransportId, damage);
                        }
                    }
                    else
                    {
                        if (DamageList.ContainsKey(hitRequest.Session.Character.CharacterId))
                        {
                            DamageList[hitRequest.Session.Character.CharacterId] += damage;
                        }
                        else
                        {
                            DamageList.Add(hitRequest.Session.Character.CharacterId, damage);
                        }
                    }

                    if (IsBoss && MapInstance == CaligorRaid.CaligorMapInstance)
                    {
                        switch (hitRequest.Session.Character.Faction)
                        {
                            case FactionType.Angel:
                                CaligorRaid.AngelDamage += damage;
                                if (onyxWings)
                                {
                                    CaligorRaid.AngelDamage += damage / 2;
                                }

                                break;

                            case FactionType.Demon:
                                CaligorRaid.DemonDamage += damage;
                                if (onyxWings)
                                {
                                    CaligorRaid.DemonDamage += damage / 2;
                                }

                                break;
                        }
                    }

                    if (isCaptureSkill)
                    {
                        damage = 0;
                    }

                    if (CurrentHp <= damage && !IsMateTrainer || (CurrentMp <= (Monster.MaxMP / 5) && IsMateTrainer))
                    {
                        SetDeathStatement();
                    }
                    else
                    {
                        if (IsMateTrainer)
                        {
                            CurrentMp -= Monster.MaxMP / 5;
                        }
                        else
                        {
                            CurrentHp -= damage;
                        }
                    }

                    hitRequest.Session.Character.OnLandHit(new HitEventArgs(UserType.Monster, this, hitRequest.Skill, damage));

                    // only set the hit delay if we become the monsters target with this hit
                    if (Target == -1)
                    {
                        LastSkill = DateTime.UtcNow;
                    }

                    int nearestDistance = 100;
                    foreach (KeyValuePair<long, long> kvp in DamageList)
                    {
                        ClientSession session = MapInstance.GetSessionByCharacterId(kvp.Key);
                        if (session != null)
                        {
                            int distance = Map.GetDistance(new MapCell
                            {
                                X = MapX,
                                Y = MapY
                            }, new MapCell
                            {
                                X = session.Character.PositionX,
                                Y = session.Character.PositionY
                            });
                            if (distance < nearestDistance)
                            {
                                nearestDistance = distance;
                                Target = session.Character.CharacterId;
                                TargetType = UserType.Player;
                            }

                            foreach (Mate mate in session.Character.Mates)
                            {
                                int mateDistance = Map.GetDistance(new MapCell
                                {
                                    X = MapX,
                                    Y = MapY
                                }, new MapCell
                                {
                                    X = mate.PositionX,
                                    Y = mate.PositionY
                                });
                                if (mateDistance < nearestDistance)
                                {
                                    nearestDistance = mateDistance;
                                    Target = mate.MateTransportId;
                                    TargetType = UserType.Npc;
                                }
                            }
                        }
                    }

                    if (hitRequest.Mate == null)
                    {
                        switch (hitRequest.TargetHitType)
                        {
                            case TargetHitType.SingleTargetHit:
                                if (!isCaptureSkill)
                                {
                                    MapInstance?.Broadcast(StaticPacketHelper.SkillUsed(UserType.Player,
                                        hitRequest.Session.Character.CharacterId, 3, MapMonsterId,
                                        hitRequest.Skill.SkillVNum, hitRequest.Skill.Cooldown,
                                        hitRequest.Skill.AttackAnimation, hitRequest.SkillEffect,
                                        hitRequest.Session.Character.PositionX, hitRequest.Session.Character.PositionY,
                                        IsAlive, (int)(CurrentHp / (float)MaxHp * 100), damage, hitmode,
                                        (byte)(hitRequest.Skill.SkillType - 1)));
                                }

                                break;

                            case TargetHitType.SingleTargetHitCombo:
                                MapInstance?.Broadcast(StaticPacketHelper.SkillUsed(UserType.Player,
                                    hitRequest.Session.Character.CharacterId, 3, MapMonsterId,
                                    hitRequest.Skill.SkillVNum, hitRequest.Skill.Cooldown,
                                    hitRequest.SkillCombo.Animation, hitRequest.SkillCombo.Effect,
                                    hitRequest.Session.Character.PositionX, hitRequest.Session.Character.PositionY,
                                    IsAlive, (int)(CurrentHp / (float)MaxHp * 100), damage, hitmode,
                                    (byte)(hitRequest.Skill.SkillType - 1)));
                                break;

                            case TargetHitType.SingleAoeTargetHit:
                                switch (hitmode)
                                {
                                    case 1:
                                        hitmode = 4;
                                        break;

                                    case 3:
                                        hitmode = 6;
                                        break;

                                    default:
                                        hitmode = 5;
                                        break;
                                }

                                MapInstance?.Broadcast(StaticPacketHelper.SkillUsed(UserType.Player,
                                    hitRequest.Session.Character.CharacterId, 3, MapMonsterId,
                                    hitRequest.Skill.SkillVNum, hitRequest.Skill.Cooldown,
                                    hitRequest.Skill.AttackAnimation, hitRequest.SkillEffect,
                                    hitRequest.Session.Character.PositionX, hitRequest.Session.Character.PositionY,
                                    IsAlive, (int)(CurrentHp / (float)MaxHp * 100), damage, hitmode,
                                    (byte)(hitRequest.Skill.SkillType - 1)));

                                break;

                            case TargetHitType.AoeTargetHit:
                                switch (hitmode)
                                {
                                    case 1:
                                        hitmode = 4;
                                        break;

                                    case 3:
                                        hitmode = 6;
                                        break;

                                    default:
                                        hitmode = 5;
                                        break;
                                }

                                MapInstance?.Broadcast(StaticPacketHelper.SkillUsed(UserType.Player,
                                    hitRequest.Session.Character.CharacterId, 3, MapMonsterId,
                                    hitRequest.Skill.SkillVNum, hitRequest.Skill.Cooldown,
                                    hitRequest.Skill.AttackAnimation, hitRequest.SkillEffect,

                                    //hitRequest.Session.Character.PositionX, hitRequest.Session.Character.PositionY,
                                    0, 0,
                                    IsAlive, (int)(CurrentHp / (float)MaxHp * 100), damage, hitmode,
                                    (byte)(hitRequest.Skill.SkillType - 1)));
                                break;

                            case TargetHitType.ZoneHit:
                                MapInstance?.Broadcast(StaticPacketHelper.SkillUsed(UserType.Player,
                                    hitRequest.Session.Character.CharacterId, 3, MapMonsterId,
                                    hitRequest.Skill.SkillVNum, hitRequest.Skill.Cooldown,
                                    hitRequest.Skill.AttackAnimation, hitRequest.SkillEffect, /*hitRequest.MapX,
                                    hitRequest.MapY,*/ 0, 0, IsAlive, (int)(CurrentHp / (float)MaxHp * 100), damage, 5,
                                    (byte)(hitRequest.Skill.SkillType - 1)));
                                break;

                            case TargetHitType.SpecialZoneHit:
                                MapInstance?.Broadcast(StaticPacketHelper.SkillUsed(UserType.Player,
                                    hitRequest.Session.Character.CharacterId, 3, MapMonsterId,
                                    hitRequest.Skill.SkillVNum, hitRequest.Skill.Cooldown,
                                    hitRequest.Skill.AttackAnimation, hitRequest.SkillEffect,
                                    hitRequest.Session.Character.PositionX, hitRequest.Session.Character.PositionY,
                                    IsAlive, (int)(CurrentHp / (float)MaxHp * 100), damage, hitmode,
                                    (byte)(hitRequest.Skill.SkillType - 1)));
                                break;

                            case TargetHitType.DirectInflict:
                                MapInstance?.Broadcast(StaticPacketHelper.SkillUsed(UserType.Monster, MapMonsterId, 3,
                                    MapMonsterId, 0, 0, 0, 0, 0, 0, IsAlive, (int)(CurrentHp / (float)MaxHp * 100), 0,
                                    0, 0));
                                break;
                        }
                    }
                    else
                    {
                        MapInstance?.Broadcast(StaticPacketHelper.SkillUsed(UserType.Npc,
                            hitRequest.Mate.MateTransportId, 3, MapMonsterId, 0, 12, 11, 200, 0, 0, IsAlive,
                            CurrentHp / (MaxHp * 100), damage, hitmode, 0));
                    }

                    if (CurrentHp <= 0 && !isCaptureSkill)
                    {
                        // generate the kill bonus
                        hitRequest.Session.Character.GenerateKillBonus(this);
                    }
                }
                else
                {
                    // monster already has been killed, send cancel
                    hitRequest.Session.SendPacket(StaticPacketHelper.Cancel(2, MapMonsterId));
                }

                if (IsBoss)
                {
                    MapInstance.Broadcast(GenerateBoss());
                }
            }

            // Respawn
            if (!IsAlive && ShouldRespawn != null && !ShouldRespawn.Value)
            {
                MapInstance.RemoveMonster(this);
            }

            if (!IsAlive && ShouldRespawn != null && ShouldRespawn.Value)
            {
                double timeDeath = (DateTime.UtcNow - Death).TotalSeconds;
                if (timeDeath >= Monster.RespawnTime / 10d)
                {
                    Respawn();
                }
            }

            // normal movement
            else if (Target == -1)
            {
                Move();
            }

            // target following
            else if (MapInstance != null && Monster.MonsterType != MonsterType.Summoner)
            {
                GetNearestOponent();
                HostilityTarget();
                NpcMonsterSkill npcMonsterSkill = null;

                switch (TargetType)
                {
                    case UserType.Player:
                        ClientSession targetSession = MapInstance.GetSessionByCharacterId(Target);

                        // remove target in some situations
                        if (targetSession?.Character.Invisible != false
                            || targetSession.Character.Hp <= 0 || CurrentHp <= 0)
                        {
                            RemoveTarget();
                            return;
                        }

                        if (ServerManager.RandomNumber(0, 10) > 8 && Skills != null)
                        {
                            npcMonsterSkill = Skills
                                .Where(s => (DateTime.UtcNow - s.LastSkillUse).TotalMilliseconds >= 100 * s.Skill.Cooldown)
                                .OrderBy(rnd => ServerManager.RandomNumber()).FirstOrDefault();
                        }

                        if (npcMonsterSkill?.Skill.TargetType == 1 && npcMonsterSkill?.Skill.HitType == 0)
                        {
                            TargetHit(targetSession, npcMonsterSkill);
                        }

                        // check if target is in range
                        if (!targetSession.Character.InvisibleGm && !targetSession.Character.Invisible
                            && targetSession.Character.Hp > 0)
                        {
                            if (npcMonsterSkill != null && CurrentMp >= npcMonsterSkill.Skill.MpCost && Map.GetDistance(
                                    new MapCell
                                    {
                                        X = MapX,
                                        Y = MapY
                                    },
                                    new MapCell
                                    {
                                        X = targetSession.Character.PositionX,
                                        Y = targetSession.Character.PositionY
                                    }) < npcMonsterSkill.Skill.Range)
                            {
                                TargetHit(targetSession, npcMonsterSkill);
                            }
                            else if (Map.GetDistance(new MapCell
                            {
                                X = MapX,
                                Y = MapY
                            },
                                         new MapCell
                                         {
                                             X = targetSession.Character.PositionX,
                                             Y = targetSession.Character.PositionY
                                         }) <= Monster.BasicRange)
                            {
                                TargetHit(targetSession, null);
                            }
                            else
                            {
                                FollowTarget(targetSession);
                            }
                        }
                        else
                        {
                            FollowTarget(targetSession);
                        }

                        break;

                    case UserType.Npc:
                        Mate mate = MapInstance.Sessions.SelectMany(x => x.Character.Mates)
                            .FirstOrDefault(s => s.IsTeamMember && s.MateTransportId == Target);

                        // remove target in some situations
                        if (mate?.Owner.Invisible != false
                            || !mate.IsAlive || CurrentHp <= 0)
                        {
                            RemoveTarget();
                            return;
                        }

                        if (ServerManager.RandomNumber(0, 10) > 8 && Skills != null)
                        {
                            npcMonsterSkill = Skills
                                .Where(s => (DateTime.UtcNow - s.LastSkillUse).TotalMilliseconds >= 100 * s.Skill.Cooldown)
                                .OrderBy(rnd => ServerManager.RandomNumber()).FirstOrDefault();
                        }

                        if (npcMonsterSkill?.Skill.TargetType == 1 && npcMonsterSkill?.Skill.HitType == 0)
                        {
                            TargetHit(mate, npcMonsterSkill);
                        }

                        // check if target is in range
                        if (!mate.Owner.InvisibleGm && !mate.Owner.Invisible
                            && mate.IsAlive)
                        {
                            if (npcMonsterSkill != null && CurrentMp >= npcMonsterSkill.Skill.MpCost && Map.GetDistance(
                                    new MapCell
                                    {
                                        X = MapX,
                                        Y = MapY
                                    },
                                    new MapCell
                                    {
                                        X = mate.PositionX,
                                        Y = mate.PositionY
                                    }) < npcMonsterSkill.Skill.Range)
                            {
                                TargetHit(mate, npcMonsterSkill);
                            }
                            else if (Map.GetDistance(new MapCell
                            {
                                X = MapX,
                                Y = MapY
                            },
                                         new MapCell
                                         {
                                             X = mate.PositionX,
                                             Y = mate.PositionY
                                         }) <= Monster.BasicRange)
                            {
                                TargetHit(mate, null);
                            }
                            else
                            {
                                FollowTarget(mate);
                            }
                        }
                        else
                        {
                            FollowTarget(mate);
                        }

                        break;
                }
            }
        }

        private void Move()
        {
            // Normal Move Mode
            if (Monster == null || !IsAlive || _noMove)
            {
                return;
            }

            if (IsMoving && Monster.Speed > 0)
            {
                double time = (DateTime.UtcNow - LastMove).TotalMilliseconds;
                if (Path == null)
                {
                    Path = new List<Node>();
                }

                if (Path.Count > 0) // move back to initial position after following target
                {
                    int timetowalk = 2000 / Monster.Speed;
                    if (time > timetowalk)
                    {
                        int maxindex = Path.Count > Monster.Speed / 2 ? Monster.Speed / 2 : Path.Count;
                        if (Path[maxindex - 1] == null)
                        {
                            return;
                        }

                        short mapX = Path[maxindex - 1].X;
                        short mapY = Path[maxindex - 1].Y;
                        WaitingTime(mapX, mapY);

                        Observable.Timer(TimeSpan.FromMilliseconds(timetowalk)).Subscribe(x =>
                        {
                            MapX = mapX;
                            MapY = mapY;
                            MoveEvent?.Events.ForEach(e => EventHelper.Instance.RunEvent(e, monster: this));
                        });
                        Path.RemoveRange(0, maxindex > Path.Count ? Path.Count : maxindex);
                        MapInstance.Broadcast(new BroadcastPacket(null,
                            StaticPacketHelper.Move(UserType.Monster, MapMonsterId, MapX, MapY,
                                Monster.Speed), ReceiverType.All, xCoordinate: mapX, yCoordinate: mapY));
                        return;
                    }
                }
                else if (time > _movetime)
                {
                    _movetime = ServerManager.RandomNumber(400, 1600);
                    short mapX = FirstX, mapY = FirstY;
                    if (MapInstance.Map?.GetFreePosition(ref mapX, ref mapY, (byte)ServerManager.RandomNumber(0, 2),
                            (byte)ServerManager.RandomNumber(0, 2)) ?? false)
                    {
                        int distance = Map.GetDistance(new MapCell
                        {
                            X = mapX,
                            Y = mapY
                        }, new MapCell
                        {
                            X = MapX,
                            Y = MapY
                        });

                        double value = 1000d * distance / (2 * Monster.Speed);
                        Observable.Timer(TimeSpan.FromMilliseconds(value)).Subscribe(x =>
                        {
                            MapX = mapX;
                            MapY = mapY;
                        });

                        LastMove = DateTime.UtcNow.AddMilliseconds(value);
                        MapInstance.Broadcast(new BroadcastPacket(null,
                            StaticPacketHelper.Move(UserType.Monster, MapMonsterId, MapX, MapY,
                                Monster.Speed), ReceiverType.All));
                    }
                }
            }

            HostilityTarget();
        }

        private void RemoveBuff(short id)
        {
            Buff indicator = Buff[id];

            if (indicator != null)
            {
                Buff.Remove(id);
                _noAttack &= !indicator.Card.BCards.Any(s =>
                    s.Type == (byte)CardType.SpecialAttack
                    && s.SubType.Equals((byte)AdditionalTypes.SpecialAttack.NoAttack / 10));
                _noMove &= !indicator.Card.BCards.Any(s =>
                    s.Type == (byte)CardType.Move
                    && s.SubType.Equals((byte)AdditionalTypes.Move.MovementImpossible / 10));
            }
        }

        private void Respawn()
        {
            if (Monster != null)
            {
                DamageList = new Dictionary<long, long>();
                IsAlive = true;
                Target = -1;
                CurrentHp = MaxHp;
                CurrentMp = MaxMp;
                MapX = FirstX;
                MapY = FirstY;
                Path = new List<Node>();
                MapInstance.Broadcast(GenerateIn());
            }
        }

        /// <summary>
        /// Hit the Target Character.
        /// </summary>
        /// <param name="targetSession"></param>
        /// <param name="npcMonsterSkill"></param>
        private void TargetHit(ClientSession targetSession, NpcMonsterSkill npcMonsterSkill)
        {
            if (Monster != null && targetSession?.Character != null
                && ((DateTime.UtcNow - LastSkill).TotalMilliseconds >= 1000 + (Monster.BasicCooldown * 200)
                 || npcMonsterSkill != null) && !_noAttack)
            {
                if (npcMonsterSkill != null)
                {
                    if (CurrentMp < npcMonsterSkill.Skill.MpCost)
                    {
                        FollowTarget(targetSession);
                        return;
                    }

                    npcMonsterSkill.LastSkillUse = DateTime.UtcNow;
                    CurrentMp -= npcMonsterSkill.Skill.MpCost;
                    MapInstance.Broadcast(StaticPacketHelper.CastOnTarget(UserType.Monster, MapMonsterId, 1, Target,
                        npcMonsterSkill.Skill.CastAnimation, npcMonsterSkill.Skill.CastEffect,
                        npcMonsterSkill.Skill.SkillVNum));
                }

                LastMove = DateTime.UtcNow;

                if (targetSession.Character.IsSitting)
                {
                    targetSession.Character.IsSitting = false;
                    MapInstance.Broadcast(targetSession.Character.GenerateRest());
                }

                int castTime = 0;
                if (npcMonsterSkill != null && npcMonsterSkill.Skill.CastEffect != 0)
                {
                    MapInstance.Broadcast(
                        StaticPacketHelper.GenerateEff(UserType.Monster, MapMonsterId,
                            npcMonsterSkill.Skill.CastEffect), MapX, MapY);
                    castTime = npcMonsterSkill.Skill.CastTime * 100;
                }

                Observable.Timer(TimeSpan.FromMilliseconds(castTime)).Subscribe(o =>
                {
                    if (targetSession.Character?.Hp > 0)
                    {
                        TargetHit2(targetSession, npcMonsterSkill, RealDamage(targetSession, npcMonsterSkill).Item1, RealDamage(targetSession, npcMonsterSkill).Item2);
                    }
                });
            }
        }

        private void TargetHit(Mate mate, NpcMonsterSkill npcMonsterSkill)
        {
            if (Monster != null && mate != null
                && ((DateTime.UtcNow - LastSkill).TotalMilliseconds >= 1000 + (Monster.BasicCooldown * 200)
                 || npcMonsterSkill != null) && !_noAttack)
            {
                if (npcMonsterSkill != null)
                {
                    if (CurrentMp < npcMonsterSkill.Skill.MpCost)
                    {
                        FollowTarget(mate);
                        return;
                    }

                    npcMonsterSkill.LastSkillUse = DateTime.UtcNow;
                    CurrentMp -= npcMonsterSkill.Skill.MpCost;
                    MapInstance.Broadcast(StaticPacketHelper.CastOnTarget(UserType.Monster, MapMonsterId, 1, Target,
                        npcMonsterSkill.Skill.CastAnimation, npcMonsterSkill.Skill.CastEffect,
                        npcMonsterSkill.Skill.SkillVNum));
                }

                LastMove = DateTime.UtcNow;

                if (mate.IsSitting)
                {
                    mate.IsSitting = false;
                    MapInstance.Broadcast(mate.GenerateRest());
                }

                int castTime = 0;
                if (npcMonsterSkill != null && npcMonsterSkill.Skill.CastEffect != 0)
                {
                    MapInstance.Broadcast(
                        StaticPacketHelper.GenerateEff(UserType.Monster, MapMonsterId,
                            npcMonsterSkill.Skill.CastEffect), MapX, MapY);
                    castTime = npcMonsterSkill.Skill.CastTime * 100;
                }

                Observable.Timer(TimeSpan.FromMilliseconds(castTime)).Subscribe(o =>
                {
                    if (mate.IsAlive)
                    {
                        TargetHit2(mate, npcMonsterSkill, RealDamage(mate, npcMonsterSkill).Item1, RealDamage(mate, npcMonsterSkill).Item2);
                    }
                });
            }
        }

        private void TargetHit2(ClientSession targetSession, NpcMonsterSkill npcMonsterSkill, int damage, int hitmode)
        {
            lock (targetSession.Character.PveLockObject)
            {
                if (targetSession.Character.Hp > 0)
                {
                    if (damage >= targetSession.Character.Hp
                        && Monster.BCards.Any(s => s.Type == 39 && s.SubType == 0 && s.ThirdData == 1))
                    {
                        damage = targetSession.Character.Hp - 1;
                    }

                    targetSession.Character.GetDamage(damage);
                    targetSession.Character.OnReceiveHit(new HitEventArgs(UserType.Monster, this, npcMonsterSkill?.Skill, damage));
                    MapInstance.Broadcast(null, targetSession.Character.GenerateStat(), ReceiverType.OnlySomeone,
                        string.Empty, Target);
                    MapInstance.Broadcast(npcMonsterSkill != null
                        ? StaticPacketHelper.SkillUsed(UserType.Monster, MapMonsterId, 1, Target,
                            npcMonsterSkill.SkillVNum, npcMonsterSkill.Skill.Cooldown,
                            npcMonsterSkill.Skill.AttackAnimation, npcMonsterSkill.Skill.Effect, MapX, MapY,
                            targetSession.Character.Hp > 0,
                            (int)(targetSession.Character.Hp / (float)targetSession.Character.HPLoad() * 100), damage,
                            hitmode, 0)
                        : StaticPacketHelper.SkillUsed(UserType.Monster, MapMonsterId, 1, Target, 0,
                            Monster.BasicCooldown, 11, Monster.BasicSkill, 0, 0, targetSession.Character.Hp > 0,
                            (int)(targetSession.Character.Hp / (float)targetSession.Character.HPLoad() * 100), damage,
                            hitmode, 0));
                    npcMonsterSkill?.Skill.BCards.ForEach(s => s.ApplyBCards(this));
                    LastSkill = DateTime.UtcNow;
                    if (targetSession.Character.Hp <= 0)
                    {
                        RemoveTarget();
                        Observable.Timer(TimeSpan.FromMilliseconds(1000)).Subscribe(o =>
                            ServerManager.Instance.AskRevive((long)targetSession.Character?.CharacterId));
                    }
                }
            }

            if (npcMonsterSkill != null && (npcMonsterSkill.Skill.Range > 0 || npcMonsterSkill.Skill.TargetRange > 0))
            {
                foreach (Character characterInRange in MapInstance
                    .GetCharactersInRange(
                        npcMonsterSkill.Skill.TargetRange == 0 ? MapX : targetSession.Character.PositionX,
                        npcMonsterSkill.Skill.TargetRange == 0 ? MapY : targetSession.Character.PositionY,
                        npcMonsterSkill.Skill.TargetRange).Where(s =>
                        s.CharacterId != Target
                        && (ServerManager.Instance.ChannelId != 51
                         || (MonsterVNum - (byte)s.Faction != 678 && MonsterVNum - (byte)s.Faction != 971))
                        && s.Hp > 0 && !s.InvisibleGm))
                {
                    int Individualdamage = RealDamage(characterInRange.Session, npcMonsterSkill).Item1;
                    int Individualhitmode = RealDamage(characterInRange.Session, npcMonsterSkill).Item2;

                    if (characterInRange.IsSitting)
                    {
                        characterInRange.IsSitting = false;
                        MapInstance.Broadcast(characterInRange.GenerateRest());
                    }

                    if (characterInRange.HasGodMode)
                    {
                        Individualdamage = 0;
                        Individualhitmode = 1;
                    }

                    if (characterInRange.Hp > 0)
                    {
                        characterInRange.GetDamage(Individualdamage);
                        MapInstance.Broadcast(null, characterInRange.GenerateStat(), ReceiverType.OnlySomeone,
                            string.Empty, characterInRange.CharacterId);
                        MapInstance.Broadcast(StaticPacketHelper.SkillUsed(UserType.Monster, MapMonsterId, 1,
                            characterInRange.CharacterId, 0, Monster.BasicCooldown, 11, Monster.BasicSkill, 0, 0,
                            characterInRange.Hp > 0, (int)(characterInRange.Hp / (float)characterInRange.HPLoad() * 100),
                            Individualdamage, Individualhitmode, 0));
                        if (characterInRange.Hp <= 0)
                        {
                            Observable.Timer(TimeSpan.FromMilliseconds(1000)).Subscribe(o =>
                                ServerManager.Instance.AskRevive(characterInRange.CharacterId));
                        }
                    }
                }

                foreach (Mate mateInRange in MapInstance.Sessions.SelectMany(x => x.Character.Mates).Where(s =>
                    s.IsTeamMember && s.MateTransportId != Target
                    && (ServerManager.Instance.ChannelId != 51
                     || (MonsterVNum - (byte)s.Owner.Faction != 678 && MonsterVNum - (byte)s.Owner.Faction != 971))
                    && s.IsAlive && !s.Owner.InvisibleGm && Map.GetDistance(
                        new MapCell()
                        {
                            X = npcMonsterSkill.Skill.TargetRange == 0 ? MapX : targetSession.Character.PositionX,
                            Y = npcMonsterSkill.Skill.TargetRange == 0 ? MapY : targetSession.Character.PositionY
                        }, new MapCell() { X = s.PositionX, Y = s.PositionY }) <= npcMonsterSkill.Skill.TargetRange))
                {
                    if (mateInRange.IsSitting)
                    {
                        mateInRange.IsSitting = false;
                        MapInstance.Broadcast(mateInRange.GenerateRest());
                    }

                    int Individualdamage = RealDamage(mateInRange, npcMonsterSkill).Item1;
                    int Individualhitmode = RealDamage(mateInRange, npcMonsterSkill).Item2;

                    if (mateInRange.Owner.HasGodMode)
                    {
                        Individualdamage = 0;
                        Individualhitmode = 1;
                    }

                    if (mateInRange.IsAlive)
                    {
                        mateInRange.GetDamage(Individualdamage);
                        mateInRange.LastDefense = DateTime.UtcNow;
                        MapInstance.Broadcast(StaticPacketHelper.SkillUsed(UserType.Monster, MapMonsterId, 2,
                            mateInRange.MateTransportId, 0, Monster.BasicCooldown, 11, Monster.BasicSkill, 0, 0,
                            mateInRange.Hp > 0, (int)(mateInRange.Hp / (float)mateInRange.MaxHp * 100), Individualdamage, Individualhitmode, 0));
                        mateInRange.Owner.Session.SendPackets(mateInRange.Owner.GeneratePst());
                        mateInRange.Owner.Session.SendPacket(mateInRange.GenerateScPacket());
                        //if (mateInRange.Hp <= 0)
                        //{
                        //    mateInRange.RevivalMate();
                        //}
                    }
                }
            }
        }

        private void TargetHit2(Mate mate, NpcMonsterSkill npcMonsterSkill, int damage, int hitmode)
        {
            lock (mate.PveLockObject)
            {
                if (mate.IsAlive)
                {
                    if (damage >= mate.Hp
                        && Monster.BCards.Any(s => s.Type == 39 && s.SubType == 0 && s.ThirdData == 1))
                    {
                        damage = mate.Hp - 1;
                    }

                    mate.GetDamage(damage);
                    mate.LastDefense = DateTime.UtcNow;
                    MapInstance.Broadcast(npcMonsterSkill != null
                        ? StaticPacketHelper.SkillUsed(UserType.Monster, MapMonsterId, 2, Target,
                            npcMonsterSkill.SkillVNum, npcMonsterSkill.Skill.Cooldown,
                            npcMonsterSkill.Skill.AttackAnimation, npcMonsterSkill.Skill.Effect, MapX, MapY,
                            mate.Hp > 0, (int)(mate.Hp / (float)mate.MaxHp * 100), damage, hitmode, 0)
                        : StaticPacketHelper.SkillUsed(UserType.Monster, MapMonsterId, 2, Target, 0,
                            Monster.BasicCooldown, 11, Monster.BasicSkill, 0, 0, mate.Hp > 0,
                            (int)(mate.Hp / (float)mate.MaxHp * 100), damage, hitmode, 0));
                    npcMonsterSkill?.Skill.BCards.ForEach(s => s.ApplyBCards(this));
                    LastSkill = DateTime.UtcNow;
                    mate.Owner.Session.SendPackets(mate.Owner.GeneratePst());
                    mate.Owner.Session.SendPacket(mate.GenerateScPacket());
                    if (mate.Hp <= 0)
                    {
                        RemoveTarget();
                        mate.GenerateDeath();
                        mate.Owner.Session.CurrentMapInstance.Broadcast(mate.GenerateOut());
                    }
                }
            }

            if (npcMonsterSkill != null && (npcMonsterSkill.Skill.Range > 0 || npcMonsterSkill.Skill.TargetRange > 0))
            {
                foreach (Character characterInRange in MapInstance
                    .GetCharactersInRange(npcMonsterSkill.Skill.TargetRange == 0 ? MapX : mate.PositionX,
                        npcMonsterSkill.Skill.TargetRange == 0 ? MapY : mate.PositionY,
                        npcMonsterSkill.Skill.TargetRange).Where(s =>
                        s.CharacterId != Target
                        && (ServerManager.Instance.ChannelId != 51
                         || (MonsterVNum - (byte)s.Faction != 678 && MonsterVNum - (byte)s.Faction != 971))
                        && s.Hp > 0 && !s.InvisibleGm))
                {
                    if (characterInRange.IsSitting)
                    {
                        characterInRange.IsSitting = false;
                        MapInstance.Broadcast(characterInRange.GenerateRest());
                    }

                    int Individualdamage = RealDamage(characterInRange.Session, npcMonsterSkill).Item1;
                    int Individualhitmode = RealDamage(characterInRange.Session, npcMonsterSkill).Item2;

                    if (characterInRange.HasGodMode)
                    {
                        Individualdamage = 0;
                        Individualhitmode = 1;
                    }

                    if (characterInRange.Hp > 0)
                    {
                        characterInRange.GetDamage(Individualdamage);
                        MapInstance.Broadcast(null, characterInRange.GenerateStat(), ReceiverType.OnlySomeone,
                            string.Empty, characterInRange.CharacterId);
                        MapInstance.Broadcast(StaticPacketHelper.SkillUsed(UserType.Monster, MapMonsterId, 1,
                            characterInRange.CharacterId, 0, Monster.BasicCooldown, 11, Monster.BasicSkill, 0, 0,
                            characterInRange.Hp > 0, (int)(characterInRange.Hp / (float)characterInRange.HPLoad() * 100),
                            Individualdamage, Individualhitmode, 0));
                        if (characterInRange.Hp <= 0)
                        {
                            Observable.Timer(TimeSpan.FromMilliseconds(1000)).Subscribe(o =>
                                ServerManager.Instance.AskRevive(characterInRange.CharacterId));
                        }
                    }
                }

                foreach (Mate mateInRange in MapInstance.Sessions.SelectMany(x => x.Character.Mates).Where(s =>
                    s.IsTeamMember && s.MateTransportId != Target
                    && (ServerManager.Instance.ChannelId != 51
                     || (MonsterVNum - (byte)s.Owner.Faction != 678 && MonsterVNum - (byte)s.Owner.Faction != 971))
                    && s.Hp > 0 && !s.Owner.InvisibleGm && Map.GetDistance(
                        new MapCell()
                        {
                            X = npcMonsterSkill.Skill.TargetRange == 0 ? MapX : mate.PositionX,
                            Y = npcMonsterSkill.Skill.TargetRange == 0 ? MapY : mate.PositionY
                        }, new MapCell() { X = s.PositionX, Y = s.PositionY }) <= npcMonsterSkill.Skill.TargetRange))
                {
                    if (mateInRange.IsSitting)
                    {
                        mateInRange.IsSitting = false;
                        MapInstance.Broadcast(mateInRange.GenerateRest());
                    }

                    int Individualdamage = RealDamage(mateInRange, npcMonsterSkill).Item1;
                    int Individualhitmode = RealDamage(mateInRange, npcMonsterSkill).Item2;

                    if (mateInRange.Owner.HasGodMode)
                    {
                        Individualdamage = 0;
                        Individualhitmode = 1;
                    }

                    if (mateInRange.IsAlive)
                    {
                        mateInRange.GetDamage(Individualdamage);
                        mateInRange.LastDefense = DateTime.UtcNow;
                        MapInstance.Broadcast(StaticPacketHelper.SkillUsed(UserType.Monster, MapMonsterId, 2,
                            mateInRange.MateTransportId, 0, Monster.BasicCooldown, 11, Monster.BasicSkill, 0, 0,
                            mateInRange.Hp > 0, (int)(mateInRange.Hp / (float)mateInRange.MaxHp * 100), Individualdamage, Individualhitmode, 0));
                        mateInRange.Owner.Session.SendPackets(mateInRange.Owner.GeneratePst());
                        mateInRange.Owner.Session.SendPacket(mateInRange.GenerateScPacket());
                        if (mateInRange.Hp <= 0)
                        {
                            mate.GenerateDeath();
                            mateInRange.Owner.Session.CurrentMapInstance.Broadcast(mateInRange.GenerateOut());
                        }
                    }
                }
            }
        }

        private double WaitingTime(short mapX, short mapY)
        {
            double waitingtime = Map.GetDistance(new MapCell
            {
                X = mapX,
                Y = mapY
            },
                                     new MapCell
                                     {
                                         X = MapX,
                                         Y = MapY
                                     }) / (double)Monster.Speed;
            LastMove = DateTime.UtcNow.AddSeconds(waitingtime > 1 ? 1 : waitingtime);
            return waitingtime;
        }

        private Tuple<int, int> RealDamage(ClientSession targetSession, NpcMonsterSkill npcMonsterSkill)
        {
            int hitmode = 0;
            bool onyxWings = false;
            int damage = DamageHelper.Instance.CalculateDamage(new BattleEntity(this),
                new BattleEntity(targetSession.Character, null), npcMonsterSkill?.Skill, ref hitmode,
                ref onyxWings, _percentageBosses.Any(s=>s == MonsterVNum));
            if (targetSession.Character.Buff.ContainsKey(144))
            {
                if (ServerManager.RandomNumber() < 50)
                {
                    AddBuff(new Buff(372, 1));
                }
                damage = 1;
            }

            // deal 0 damage to GM with GodMode
            if (targetSession.Character.HasGodMode)
            {
                damage = 0;
            }

            int mindSink = targetSession.Character.GetBuff(CardType.DarkCloneSummon,
                (byte)AdditionalTypes.DarkCloneSummon.ConvertDamageToHPChance)[0];
            if (mindSink != 0 && hitmode != 1)
            {
                targetSession.Character.Hp = targetSession.Character.Hp + damage < targetSession.Character.HPMax ? targetSession.Character.Hp + damage : targetSession.Character.Hp = targetSession.Character.HPMax;
                targetSession.Character.MapInstance.Broadcast(targetSession.Character.GenerateRc(damage));
                targetSession.Character.Collect += damage;
                damage = 0;
            }

            int[] manaShield = targetSession.Character.GetBuff(CardType.LightAndShadow,
                (byte)AdditionalTypes.LightAndShadow.InflictDamageToMP);
            if (manaShield[0] != 0 && hitmode != 1)
            {
                int reduce = damage / 100 * manaShield[0];
                if (targetSession.Character.Mp < reduce)
                {
                    targetSession.Character.Mp = 0;
                }
                else
                {
                    targetSession.Character.Mp -= reduce;
                }
            }

            int[] reflect = targetSession.Character.GetBuff(CardType.TauntSkill,
                (byte)AdditionalTypes.TauntSkill.ReflectsMaximumDamageFrom);

            if (reflect[0] != 0 && hitmode != 1)
            {
                if (damage > reflect[0])
                {
                    targetSession.CurrentMapInstance.Broadcast(StaticPacketHelper.SkillUsed(UserType.Player, targetSession.Character.CharacterId, 3,
                        MapMonsterId, -1, 0, -1, 4500, -1, -1, true, 100, reflect[0], 0, 1));
                }
                else
                {
                    targetSession.CurrentMapInstance.Broadcast(StaticPacketHelper.SkillUsed(UserType.Player, targetSession.Character.CharacterId, 3,
                        MapMonsterId, -1, 0, -1, 4500, -1, -1, true, 100, reflect[0] = damage, 0, 1));
                }
                if (CurrentHp - reflect[0] < 1)
                {
                    CurrentHp = 1;
                }
                else
                {
                    CurrentHp -= reflect[0];
                }
                damage = 0;
            }

            int[] criticalDefence = targetSession.Character.GetBuff(CardType.VulcanoElementBuff, (byte)AdditionalTypes.VulcanoElementBuff.CriticalDefence);

            if (criticalDefence[0] != 0 && hitmode == 3)
            {
                if (damage > criticalDefence[0])
                {
                    damage = criticalDefence[0];
                }
            }

            return Tuple.Create(damage, hitmode);
        }

        private Tuple<int, int> RealDamage(Mate mate, NpcMonsterSkill npcMonsterSkill)
        {
            int hitmode = 0;
            bool onyxWings = false;
            int damage = DamageHelper.Instance.CalculateDamage(new BattleEntity(this), new BattleEntity(mate),
                npcMonsterSkill?.Skill, ref hitmode, ref onyxWings, _percentageBosses.Any(s=>s == MonsterVNum));

            if (mate.Buff.ContainsKey(144))
            {
                if (ServerManager.RandomNumber() < 50)
                {
                    AddBuff(new Buff(372, 1));
                }
                damage = 1;
            }

            // deal 0 damage to GM with GodMode
            if (mate.Owner.HasGodMode)
            {
                damage = 0;
            }

            int[] manaShield = mate.GetBuff(CardType.LightAndShadow,
                (byte)AdditionalTypes.LightAndShadow.InflictDamageToMP);
            if (manaShield[0] != 0 && hitmode != 1)
            {
                int reduce = damage / 100 * manaShield[0];
                if (mate.Mp < reduce)
                {
                    mate.Mp = 0;
                }
                else
                {
                    mate.Mp -= reduce;
                }
            }

            return Tuple.Create(damage,hitmode);
        }

#pragma warning disable RCS1213 // Remove unused member declaration.
        // ReSharper disable once UnusedMember.Local
        private void RegisterMonsterLife() => Observable.Interval(TimeSpan.FromMilliseconds(400)).Subscribe(observer => StartLife());
#pragma warning restore RCS1213 // Remove unused member declaration.

        #endregion
    }
}