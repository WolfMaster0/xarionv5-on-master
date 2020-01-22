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

using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OpenNos.Core.Threading;
using OpenNos.GameObject.Event;
using OpenNos.GameObject.Networking;
using OpenNos.XMLModel.ScriptedInstance.Model;

namespace OpenNos.GameObject
{
    public class ScriptedInstance : ScriptedInstanceDTO
    {
        #region Members

        private readonly Dictionary<int, MapInstance> _mapInstanceDictionary = new Dictionary<int, MapInstance>();

        private IDisposable _disposable;

        #endregion

        #region Instantiation

        public ScriptedInstance()
        {
        }

        public ScriptedInstance(ScriptedInstanceDTO input)
        {
            MapId = input.MapId;
            PositionX = input.PositionX;
            PositionY = input.PositionY;
            Script = input.Script;
            ScriptedInstanceId = input.ScriptedInstanceId;
            Type = input.Type;
        }

        #endregion

        #region Properties

        public List<Gift> DrawItems { get; set; }

        public MapInstance FirstMap { get; set; }

        public List<Gift> GiftItems { get; set; }

        public long Gold { get; set; }

        public byte Id { get; set; }

        public InstanceBag InstanceBag { get; } = new InstanceBag();

        public string Label { get; set; }

        public byte LevelMaximum { get; set; }

        public byte LevelMinimum { get; set; }

        public byte Lives { get; set; }

        public ScriptedInstanceModel Model { get; set; }

        public int MonsterAmount { get; internal set; }

        public string Name { get; set; }

        public int NpcAmount { get; internal set; }

        public int Reputation { get; set; }

        public List<Gift> RequiredItems { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int RoomAmount { get; internal set; }

        public List<Gift> SpecialItems { get; set; }

        public short StartX { get; set; }

        public short StartY { get; set; }

        #endregion

        #region Methods

        public void Dispose()
        {
            Observable.Timer(TimeSpan.FromSeconds(10)).Subscribe(observer =>
                _mapInstanceDictionary.Values.ToList().ForEach(m => m.Dispose()));
        }

        public string GenerateMainInfo() => $"minfo 0 1 -1.0/0 -1.0/0 -1/0 -1.0/0 1 {InstanceBag.Lives + 1} 0";

        public List<string> GenerateMinimap()
        {
            List<string> lst = new List<string> { "rsfm 0 0 4 12" };
            _mapInstanceDictionary.Values.ToList().ForEach(s => lst.Add(s.GenerateRsfn(true)));
            return lst;
        }

        public string GenerateRbr()
        {
            string drawgift = string.Empty;
            string bonusitems = string.Empty;
            string specialitems = string.Empty;

            for (int i = 0; i < 5; i++)
            {
                Gift gift = DrawItems?.ElementAtOrDefault(i);
                drawgift += $" {(gift == null ? "-1.0" : $"{gift.VNum}.{gift.Amount}")}";
            }
            for (int i = 0; i < 2; i++)
            {
                Gift gift = SpecialItems?.ElementAtOrDefault(i);
                specialitems += $" {(gift == null ? "-1.0" : $"{gift.VNum}.{gift.Amount}")}";
            }

            for (int i = 0; i < 3; i++)
            {
                Gift gift = GiftItems?.ElementAtOrDefault(i);
                bonusitems += (i == 0 ? string.Empty : " ") + (gift == null ? "-1.0" : $"{gift.VNum}.{gift.Amount}");
            }
            const int winnerScore = 0;
            const string winner = "";
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            // ReSharper disable once UnreachableCode
#pragma warning disable 162
            return $"rbr 0.0.0 4 15 {LevelMinimum}.{LevelMaximum} {RequiredItems?.Sum(s => s.Amount)} {drawgift} {specialitems} {bonusitems} {winnerScore}.{(winnerScore > 0 ? winner : string.Empty)} 0 0 {Name}\n{Label}";
#pragma warning restore 162
        }

        public string GenerateWp() => $"wp {PositionX} {PositionY} {ScriptedInstanceId} 0 {LevelMinimum} {LevelMaximum}";

        public void LoadGlobals()
        {
            if (Script != null)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ScriptedInstanceModel));
                using (StringReader textReader = new StringReader(Script))
                {
                    Model = (ScriptedInstanceModel)serializer.Deserialize(textReader);
                }

                if (Model?.Globals != null)
                {
                    RequiredItems = new List<Gift>();
                    DrawItems = new List<Gift>();
                    SpecialItems = new List<Gift>();
                    GiftItems = new List<Gift>();

                    // set the values
                    Id = Model.Globals.Id?.Value ?? 0;
                    Gold = Model.Globals.Gold?.Value ?? 0;
                    Reputation = Model.Globals.Reputation?.Value ?? 0;
                    StartX = Model.Globals.StartX?.Value ?? 0;
                    StartY = Model.Globals.StartY?.Value ?? 0;
                    Lives = Model.Globals.Lives?.Value ?? 0;
                    LevelMinimum = Model.Globals.LevelMinimum?.Value ?? 1;
                    LevelMaximum = Model.Globals.LevelMaximum?.Value ?? 99;
                    Name = Model.Globals.Name?.Value ?? "No Name";
                    Label = Model.Globals.Label?.Value ?? "No Description";
                    if (Model.Globals.RequiredItems != null)
                    {
                        foreach (XMLModel.Shared.Item item in Model.Globals.RequiredItems)
                        {
                            RequiredItems.Add(new Gift(item.VNum, item.Amount, item.Design, item.IsRandomRare));
                        }
                    }
                    if (Model.Globals.DrawItems != null)
                    {
                        foreach (XMLModel.Shared.Item item in Model.Globals.DrawItems)
                        {
                            DrawItems.Add(new Gift(item.VNum, item.Amount, item.Design, item.IsRandomRare));
                        }
                    }
                    if (Model.Globals.SpecialItems != null)
                    {
                        foreach (XMLModel.Shared.Item item in Model.Globals.SpecialItems)
                        {
                            SpecialItems.Add(new Gift(item.VNum, item.Amount, item.Design, item.IsRandomRare));
                        }
                    }
                    if (Model.Globals.GiftItems != null)
                    {
                        foreach (XMLModel.Shared.Item item in Model.Globals.GiftItems)
                        {
                            GiftItems.Add(new Gift(item.VNum, item.Amount, item.Design, item.IsRandomRare));
                        }
                    }
                }
            }
        }

        public void LoadScript(MapInstanceType mapinstancetype)
        {
            if (Model != null)
            {
                InstanceBag.Lives = Lives;
                if (Model.InstanceEvents?.CreateMap != null)
                {
                    foreach (XMLModel.ScriptedInstance.Objects.CreateMap createMap in Model.InstanceEvents.CreateMap)
                    {
                        MapInstance mapInstance = ServerManager.GenerateMapInstance(createMap.VNum, mapinstancetype, InstanceBag);
                        mapInstance.Portals?.Clear();
                        mapInstance.MapIndexX = createMap.IndexX;
                        mapInstance.MapIndexY = createMap.IndexY;
                        if (!_mapInstanceDictionary.ContainsKey(createMap.Map))
                        {
                            _mapInstanceDictionary.Add(createMap.Map, mapInstance);
                        }
                    }
                }

                FirstMap = _mapInstanceDictionary.Values.FirstOrDefault();
                Observable.Timer(TimeSpan.FromMinutes(3)).Subscribe(x =>
                {
                    if (!InstanceBag.Lock)
                    {
                        _mapInstanceDictionary.Values.ToList().ForEach(m => EventHelper.Instance.RunEvent(new EventContainer(m, EventActionType.ScriptEnd, (byte)1)));
                        Dispose();
                    }
                });
                _disposable = Observable.Interval(TimeSpan.FromMilliseconds(100)).Subscribe(x =>
                {
                    if (InstanceBag.Lives - InstanceBag.DeadList.Count < 0)
                    {
                        _mapInstanceDictionary.Values.ToList().ForEach(m => EventHelper.Instance.RunEvent(new EventContainer(m, EventActionType.ScriptEnd, (byte)3)));
                        Dispose();
                        _disposable.Dispose();
                    }
                    if (InstanceBag.Clock.SecondsRemaining <= 0)
                    {
                        _mapInstanceDictionary.Values.ToList().ForEach(m => EventHelper.Instance.RunEvent(new EventContainer(m, EventActionType.ScriptEnd, (byte)1)));
                        Dispose();
                        _disposable.Dispose();
                    }
                });

                GenerateEvent(FirstMap).ForEach(e => EventHelper.Instance.RunEvent(e));
            }
        }

        private ThreadSafeGenericList<EventContainer> GenerateEvent(MapInstance parentMapInstance)
        {
            // Needs Optimization, look into it.
            ThreadSafeGenericList<EventContainer> evts = new ThreadSafeGenericList<EventContainer>();

            if (Model.InstanceEvents.CreateMap != null)
            {
                Parallel.ForEach(Model.InstanceEvents.CreateMap, createMap =>
                {
                    MapInstance mapInstance = _mapInstanceDictionary.FirstOrDefault(s => s.Key == createMap.Map).Value ?? parentMapInstance;

                    if (mapInstance == null)
                    {
                        return;
                    }

                    // SummonMonster
                    evts.AddRange(SummonMonster(mapInstance, createMap.SummonMonster));

                    // SummonNpc
                    evts.AddRange(SummonNpc(mapInstance, createMap.SummonNpc));

                    // SpawnPortal
                    evts.AddRange(SpawnPortal(mapInstance, createMap.SpawnPortal));

                    // SpawnButton
                    evts.AddRange(SpawnButton(mapInstance, createMap.SpawnButton));

                    // OnCharacterDiscoveringMap
                    evts.AddRange(OnCharacterDiscoveringMap(mapInstance, createMap));

                    // GenerateClock
                    if (createMap.GenerateClock != null)
                    {
                        evts.Add(new EventContainer(mapInstance, EventActionType.Clock, createMap.GenerateClock.Value));
                    }

                    // OnMoveOnMap
                    if (createMap.OnMoveOnMap != null)
                    {
                        Parallel.ForEach(createMap.OnMoveOnMap, onMoveOnMap => evts.AddRange(OnMoveOnMap(mapInstance, onMoveOnMap)));
                    }

                    // OnLockerOpen
                    if (createMap.OnLockerOpen != null)
                    {
                        List<EventContainer> onLockerOpen = new List<EventContainer>();

                        // ChangePortalType
                        if (createMap.OnLockerOpen.ChangePortalType != null)
                        {
                            onLockerOpen.Add(new EventContainer(mapInstance, EventActionType.ChangePortalType, new Tuple<int, PortalType>(createMap.OnLockerOpen.ChangePortalType.IdOnMap, (PortalType)createMap.OnLockerOpen.ChangePortalType.Type)));
                        }

                        // SendMessage
                        if (createMap.OnLockerOpen.SendMessage != null)
                        {
                            onLockerOpen.Add(new EventContainer(mapInstance, EventActionType.SendPacket, UserInterfaceHelper.GenerateMsg(createMap.OnLockerOpen.SendMessage.Value, createMap.OnLockerOpen.SendMessage.Type)));
                        }

                        // RefreshMapItems
                        if (createMap.OnLockerOpen.RefreshMapItems != null)
                        {
                            onLockerOpen.Add(new EventContainer(mapInstance, EventActionType.RefreshMapItems, null));
                        }

                        // Set Monster Lockers
                        if (createMap.OnLockerOpen.SetMonsterLockers != null)
                        {
                            onLockerOpen.Add(new EventContainer(mapInstance, EventActionType.SetMonsterLockers, createMap.OnLockerOpen.SetMonsterLockers.Value));
                        }
                        // Set Button Lockers
                        if (createMap.OnLockerOpen.SetButtonLockers != null)
                        {
                            onLockerOpen.Add(new EventContainer(mapInstance, EventActionType.SetButtonLockers, createMap.OnLockerOpen.SetButtonLockers.Value));
                        }
                        onLockerOpen.AddRange(SummonMonster(mapInstance, createMap.OnLockerOpen.SummonMonster, true));

                        evts.Add(new EventContainer(mapInstance, EventActionType.RegisterEvent, new Tuple<string, List<EventContainer>>(nameof(XMLModel.ScriptedInstance.Events.OnLockerOpen), onLockerOpen)));
                    }

                    // OnAreaEntry
                    if (createMap.OnAreaEntry != null)
                    {
                        foreach (XMLModel.ScriptedInstance.Events.OnAreaEntry onAreaEntry in createMap.OnAreaEntry)
                        {
                            List<EventContainer> onAreaEntryEvents = new List<EventContainer>();
                            onAreaEntryEvents.AddRange(SummonMonster(mapInstance, onAreaEntry.SummonMonster));
                            evts.Add(new EventContainer(mapInstance, EventActionType.SetAreaEntry, new ZoneEvent { X = onAreaEntry.PositionX, Y = onAreaEntry.PositionY, Range = onAreaEntry.Range, Events = onAreaEntryEvents }));
                        }
                    }

                    // SetButtonLockers
                    if (createMap.SetButtonLockers != null)
                    {
                        evts.Add(new EventContainer(mapInstance, EventActionType.SetButtonLockers, createMap.SetButtonLockers.Value));
                    }

                    // SetMonsterLockers
                    if (createMap.SetMonsterLockers != null)
                    {
                        evts.Add(new EventContainer(mapInstance, EventActionType.SetMonsterLockers, createMap.SetMonsterLockers.Value));
                    }
                });
            }

            return evts;
        }

        private List<EventContainer> OnCharacterDiscoveringMap(MapInstance mapInstance, XMLModel.ScriptedInstance.Objects.CreateMap createMap)
        {
            List<EventContainer> evts = new List<EventContainer>();

            // OnCharacterDiscoveringMap
            if (createMap.OnCharacterDiscoveringMap != null)
            {
                List<EventContainer> onDiscoverEvents = new List<EventContainer>();

                // GenerateMapClock
                if (createMap.OnCharacterDiscoveringMap.GenerateMapClock != null)
                {
                    onDiscoverEvents.Add(new EventContainer(mapInstance, EventActionType.MapClock, createMap.OnCharacterDiscoveringMap.GenerateMapClock.Value));
                }

                // NpcDialog
                if (createMap.OnCharacterDiscoveringMap.NpcDialog != null)
                {
                    onDiscoverEvents.Add(new EventContainer(mapInstance, EventActionType.NpcDialog, createMap.OnCharacterDiscoveringMap.NpcDialog.Value));
                }

                // SendMessage
                if (createMap.OnCharacterDiscoveringMap.SendMessage != null)
                {
                    onDiscoverEvents.Add(new EventContainer(mapInstance, EventActionType.SendPacket, UserInterfaceHelper.GenerateMsg(createMap.OnCharacterDiscoveringMap.SendMessage.Value, createMap.OnCharacterDiscoveringMap.SendMessage.Type)));
                }

                // SendPacket
                if (createMap.OnCharacterDiscoveringMap.SendPacket != null)
                {
                    onDiscoverEvents.Add(new EventContainer(mapInstance, EventActionType.SendPacket, createMap.OnCharacterDiscoveringMap.SendPacket.Value));
                }

                // SummonMonster
                onDiscoverEvents.AddRange(SummonMonster(mapInstance, createMap.OnCharacterDiscoveringMap.SummonMonster));

                // SummonNpc
                onDiscoverEvents.AddRange(SummonNpc(mapInstance, createMap.OnCharacterDiscoveringMap.SummonNpc));

                // SpawnPortal
                onDiscoverEvents.AddRange(SpawnPortal(mapInstance, createMap.OnCharacterDiscoveringMap.SpawnPortal));

                // OnMoveOnMap
                if (createMap.OnCharacterDiscoveringMap.OnMoveOnMap != null)
                {
                    onDiscoverEvents.AddRange(OnMoveOnMap(mapInstance, createMap.OnCharacterDiscoveringMap.OnMoveOnMap));
                }

                // Set Monster Lockers
                if(createMap.OnCharacterDiscoveringMap.SetMonsterLockers != null)
                {
                    onDiscoverEvents.Add(new EventContainer(mapInstance, EventActionType.SetMonsterLockers, createMap.OnCharacterDiscoveringMap.SetMonsterLockers.Value));
                }
                // Set Button Lockers
                if (createMap.OnCharacterDiscoveringMap.SetButtonLockers != null)
                {
                    onDiscoverEvents.Add(new EventContainer(mapInstance, EventActionType.SetButtonLockers, createMap.OnCharacterDiscoveringMap.SetButtonLockers.Value));
                }

                evts.Add(new EventContainer(mapInstance, EventActionType.RegisterEvent, new Tuple<string, List<EventContainer>>(nameof(XMLModel.ScriptedInstance.Events.OnCharacterDiscoveringMap), onDiscoverEvents)));
            }

            return evts;
        }

        private static List<EventContainer> OnMapClean(MapInstance mapInstance, XMLModel.ScriptedInstance.Events.OnMapClean onMapClean)
        {
            List<EventContainer> evts = new List<EventContainer>();

            // OnMapClean
            if (onMapClean != null)
            {
                List<EventContainer> onMapCleanEvents = new List<EventContainer>();

                // ChangePortalType
                if (onMapClean.ChangePortalType != null)
                {
                    foreach (XMLModel.ScriptedInstance.Events.ChangePortalType changePortalType in onMapClean.ChangePortalType)
                    {
                        onMapCleanEvents.Add(new EventContainer(mapInstance, EventActionType.ChangePortalType, new Tuple<int, PortalType>(changePortalType.IdOnMap, (PortalType)changePortalType.Type)));
                    }
                }

                // RefreshMapItems
                if (onMapClean.RefreshMapItems != null)
                {
                    onMapCleanEvents.Add(new EventContainer(mapInstance, EventActionType.RefreshMapItems, null));
                }

                // SendMessage
                if (onMapClean.SendMessage != null)
                {
                    onMapCleanEvents.Add(new EventContainer(mapInstance, EventActionType.SendPacket, UserInterfaceHelper.GenerateMsg(onMapClean.SendMessage.Value, onMapClean.SendMessage.Type)));
                }

                // SendPacket
                if (onMapClean.SendPacket != null)
                {
                    onMapCleanEvents.Add(new EventContainer(mapInstance, EventActionType.SendPacket, onMapClean.SendPacket.Value));
                }

                // NpcDialog
                if (onMapClean.NpcDialog != null)
                {
                    onMapCleanEvents.Add(new EventContainer(mapInstance, EventActionType.NpcDialog, onMapClean.NpcDialog.Value));
                }

                evts.Add(new EventContainer(mapInstance, EventActionType.RegisterEvent, new Tuple<string, List<EventContainer>>(nameof(XMLModel.ScriptedInstance.Events.OnMapClean), onMapCleanEvents)));
            }

            return evts;
        }

        private List<EventContainer> OnMoveOnMap(MapInstance mapInstance, XMLModel.ScriptedInstance.Events.OnMoveOnMap onMoveOnMap)
        {
            List<EventContainer> evts = new List<EventContainer>();

            // OnMoveOnMap
            if (onMoveOnMap != null)
            {
                List<EventContainer> waveEvent = new List<EventContainer>();
                List<EventContainer> onMoveOnMapEvents = new List<EventContainer>();

                // SendMessage
                if (onMoveOnMap.SendMessage != null)
                {
                    onMoveOnMapEvents.Add(new EventContainer(mapInstance, EventActionType.SendPacket, UserInterfaceHelper.GenerateMsg(onMoveOnMap.SendMessage.Value, onMoveOnMap.SendMessage.Type)));
                }

                // SendPacket
                if (onMoveOnMap.SendPacket != null)
                {
                    onMoveOnMapEvents.Add(new EventContainer(mapInstance, EventActionType.SendPacket, onMoveOnMap.SendPacket.Value));
                }

                // GenerateClock
                if (onMoveOnMap.GenerateClock != null)
                {
                    onMoveOnMapEvents.Add(new EventContainer(mapInstance, EventActionType.Clock, onMoveOnMap.GenerateClock.Value));
                }

                // StartClock
                if (onMoveOnMap.StartClock != null)
                {
                    List<EventContainer> onStop = new List<EventContainer>();
                    List<EventContainer> onTimeout = new List<EventContainer>();

                    // OnStop
                    if (onMoveOnMap.StartClock.OnStop != null)
                    {
                        foreach (XMLModel.ScriptedInstance.Events.ChangePortalType changePortalType in onMoveOnMap.StartClock.OnStop.ChangePortalType)
                        {
                            onStop.Add(new EventContainer(mapInstance, EventActionType.ChangePortalType, new Tuple<int, PortalType>(changePortalType.IdOnMap, (PortalType)changePortalType.Type)));
                        }
                        if (onMoveOnMap.StartClock.OnStop.SendMessage != null)
                        {
                            onStop.Add(new EventContainer(mapInstance, EventActionType.SendPacket, UserInterfaceHelper.GenerateMsg(onMoveOnMap.StartClock.OnStop.SendMessage.Value, onMoveOnMap.StartClock.OnStop.SendMessage.Type)));
                        }
                        if (onMoveOnMap.StartClock.OnStop.SendPacket != null)
                        {
                            onStop.Add(new EventContainer(mapInstance, EventActionType.SendPacket, onMoveOnMap.StartClock.OnStop.SendPacket.Value));
                        }
                        if (onMoveOnMap.StartClock.OnStop.RefreshMapItems != null)
                        {
                            onStop.Add(new EventContainer(mapInstance, EventActionType.RefreshMapItems, null));
                        }
                    }

                    // OnTimeout
                    if (onMoveOnMap.StartClock.OnTimeout != null)
                    {
                        foreach (XMLModel.ScriptedInstance.Events.ChangePortalType changePortalType in onMoveOnMap.StartClock.OnTimeout.ChangePortalType)
                        {
                            onTimeout.Add(new EventContainer(mapInstance, EventActionType.ChangePortalType, new Tuple<int, PortalType>(changePortalType.IdOnMap, (PortalType)changePortalType.Type)));
                        }
                        if (onMoveOnMap.StartClock.OnTimeout.SendMessage != null)
                        {
                            if (onMoveOnMap.StartClock.OnStop != null)
                            {
                                onTimeout.Add(new EventContainer(mapInstance, EventActionType.SendPacket,
                                    UserInterfaceHelper.GenerateMsg(onMoveOnMap.StartClock.OnTimeout.SendMessage.Value,
                                        onMoveOnMap.StartClock.OnStop.SendMessage.Type)));
                            }
                        }
                        if (onMoveOnMap.StartClock.OnTimeout.SendPacket != null)
                        {
                            onTimeout.Add(new EventContainer(mapInstance, EventActionType.SendPacket, onMoveOnMap.StartClock.OnTimeout.SendPacket.Value));
                        }
                        if (onMoveOnMap.StartClock.OnTimeout.RefreshMapItems != null)
                        {
                            onTimeout.Add(new EventContainer(mapInstance, EventActionType.RefreshMapItems, null));
                        }
                        if (onMoveOnMap.StartClock.OnTimeout.End != null)
                        {
                            onTimeout.Add(new EventContainer(mapInstance, EventActionType.ScriptEnd, onMoveOnMap.StartClock.OnTimeout.End.Type));
                        }
                    }

                    onMoveOnMapEvents.Add(new EventContainer(mapInstance, EventActionType.StartClock, new Tuple<List<EventContainer>, List<EventContainer>>(onStop, onTimeout)));
                }

                // StartMapClock
                if (onMoveOnMap.StartMapClock != null)
                {
                    List<EventContainer> onStop = new List<EventContainer>();
                    List<EventContainer> onTimeout = new List<EventContainer>();

                    // OnStop
                    if (onMoveOnMap.StartMapClock.OnStop != null)
                    {
                        foreach (XMLModel.ScriptedInstance.Events.ChangePortalType changePortalType in onMoveOnMap.StartMapClock.OnStop.ChangePortalType)
                        {
                            onStop.Add(new EventContainer(mapInstance, EventActionType.ChangePortalType, new Tuple<int, PortalType>(changePortalType.IdOnMap, (PortalType)changePortalType.Type)));
                        }
                        if (onMoveOnMap.StartMapClock.OnStop.SendMessage != null)
                        {
                            onStop.Add(new EventContainer(mapInstance, EventActionType.SendPacket, UserInterfaceHelper.GenerateMsg(onMoveOnMap.StartMapClock.OnStop.SendMessage.Value, onMoveOnMap.StartMapClock.OnStop.SendMessage.Type)));
                        }
                        if (onMoveOnMap.StartMapClock.OnStop.SendPacket != null)
                        {
                            onStop.Add(new EventContainer(mapInstance, EventActionType.SendPacket, onMoveOnMap.StartMapClock.OnStop.SendPacket.Value));
                        }
                        if (onMoveOnMap.StartMapClock.OnStop.RefreshMapItems != null)
                        {
                            onStop.Add(new EventContainer(mapInstance, EventActionType.RefreshMapItems, null));
                        }
                    }

                    // OnTimeout
                    if (onMoveOnMap.StartMapClock.OnTimeout != null)
                    {
                        foreach (XMLModel.ScriptedInstance.Events.ChangePortalType changePortalType in onMoveOnMap.StartMapClock.OnTimeout.ChangePortalType)
                        {
                            onTimeout.Add(new EventContainer(mapInstance, EventActionType.ChangePortalType, new Tuple<int, PortalType>(changePortalType.IdOnMap, (PortalType)changePortalType.Type)));
                        }
                        if (onMoveOnMap.StartMapClock.OnTimeout.SendMessage != null)
                        {
                            onTimeout.Add(new EventContainer(mapInstance, EventActionType.SendPacket, UserInterfaceHelper.GenerateMsg(onMoveOnMap.StartMapClock.OnTimeout.SendMessage.Value, onMoveOnMap.StartMapClock.OnTimeout.SendMessage.Type)));
                        }
                        if (onMoveOnMap.StartMapClock.OnTimeout.SendPacket != null)
                        {
                            onTimeout.Add(new EventContainer(mapInstance, EventActionType.SendPacket, onMoveOnMap.StartMapClock.OnTimeout.SendPacket.Value));
                        }
                        if (onMoveOnMap.StartMapClock.OnTimeout.RefreshMapItems != null)
                        {
                            onTimeout.Add(new EventContainer(mapInstance, EventActionType.RefreshMapItems, null));
                        }
                        if (onMoveOnMap.StartMapClock.OnTimeout.End != null)
                        {
                            onTimeout.Add(new EventContainer(mapInstance, EventActionType.ScriptEnd, onMoveOnMap.StartMapClock.OnTimeout.End.Type));
                        }
                    }

                    onMoveOnMapEvents.Add(new EventContainer(mapInstance, EventActionType.StartMapClock, new Tuple<List<EventContainer>, List<EventContainer>>(onStop, onTimeout)));
                }

                // Wave
                if (onMoveOnMap.Wave != null)
                {
                    foreach (XMLModel.ScriptedInstance.Objects.Wave wave in onMoveOnMap.Wave)
                    {
                        // SummonMonster
                        waveEvent.AddRange(SummonMonster(mapInstance, wave.SummonMonster));

                        // SendMessage
                        if (wave.SendMessage != null)
                        {
                            waveEvent.Add(new EventContainer(mapInstance, EventActionType.SendPacket, UserInterfaceHelper.GenerateMsg(wave.SendMessage.Value, wave.SendMessage.Type)));
                        }

                        onMoveOnMapEvents.Add(new EventContainer(mapInstance, EventActionType.RegisterWave, new EventWave(wave.Delay, waveEvent, wave.Offset)));
                    }
                }

                // SummonMonster
                onMoveOnMapEvents.AddRange(SummonMonster(mapInstance, onMoveOnMap.SummonMonster));

                // OnMapClean
                onMoveOnMapEvents.AddRange(OnMapClean(mapInstance, onMoveOnMap.OnMapClean));

                // Set Monster Lockers
                if (onMoveOnMap.SetMonsterLockers != null)
                {
                    onMoveOnMapEvents.Add(new EventContainer(mapInstance, EventActionType.SetMonsterLockers, onMoveOnMap.SetMonsterLockers.Value));
                }
                // Set Button Lockers
                if (onMoveOnMap.SetButtonLockers != null)
                {
                    onMoveOnMapEvents.Add(new EventContainer(mapInstance, EventActionType.SetButtonLockers, onMoveOnMap.SetButtonLockers.Value));
                }

                evts.Add(new EventContainer(mapInstance, EventActionType.RegisterEvent, new Tuple<string, List<EventContainer>>(nameof(XMLModel.ScriptedInstance.Events.OnMoveOnMap), onMoveOnMapEvents)));
            }

            return evts;
        }

        private List<EventContainer> SpawnButton(MapInstance mapInstance, XMLModel.ScriptedInstance.Events.SpawnButton[] spawnButton)
        {
            List<EventContainer> evts = new List<EventContainer>();

            // SpawnButton
            if (spawnButton != null)
            {
                foreach (XMLModel.ScriptedInstance.Events.SpawnButton spawn in spawnButton)
                {
                    short positionX = spawn.PositionX;
                    short positionY = spawn.PositionY;

                    if (positionX == 0 || positionY == 0)
                    {
                        MapCell cell = mapInstance?.Map?.GetRandomPosition();
                        if (cell != null)
                        {
                            positionX = cell.X;
                            positionY = cell.Y;
                        }
                    }

                    MapButton button = new MapButton(spawn.Id, positionX, positionY, spawn.VNumEnabled, spawn.VNumDisabled, new List<EventContainer>(), new List<EventContainer>(), new List<EventContainer>());

                    // OnFirstEnable
                    if (spawn.OnFirstEnable != null)
                    {
                        List<EventContainer> onFirst = new List<EventContainer>();

                        // SummonMonster
                        onFirst.AddRange(SummonMonster(mapInstance, spawn.OnFirstEnable.SummonMonster));

                        // Teleport
                        if (spawn.OnFirstEnable.Teleport != null)
                        {
                            onFirst.Add(new EventContainer(mapInstance, EventActionType.Teleport, new Tuple<short, short, short, short>(spawn.OnFirstEnable.Teleport.PositionX, spawn.OnFirstEnable.Teleport.PositionY, spawn.OnFirstEnable.Teleport.DestinationX, spawn.OnFirstEnable.Teleport.DestinationY)));
                        }

                        // RemoveButtonLocker
                        if (spawn.OnFirstEnable.RemoveButtonLocker != null)
                        {
                            onFirst.Add(new EventContainer(mapInstance, EventActionType.RemoveButtonLocker, null));
                        }

                        // RefreshRaidGoals
                        if (spawn.OnFirstEnable.RefreshRaidGoals != null)
                        {
                            onFirst.Add(new EventContainer(mapInstance, EventActionType.RefreshRaidGoal, null));
                        }

                        // SendMessage
                        if (spawn.OnFirstEnable.SendMessage != null)
                        {
                            onFirst.Add(new EventContainer(mapInstance, EventActionType.SendPacket, UserInterfaceHelper.GenerateMsg(spawn.OnFirstEnable.SendMessage.Value, spawn.OnFirstEnable.SendMessage.Type)));
                        }

                        // OnMapClean
                        if (spawn.OnFirstEnable.OnMapClean != null)
                        {
                            onFirst.AddRange(OnMapClean(mapInstance, spawn.OnFirstEnable.OnMapClean));
                        }

                        button.FirstEnableEvents.AddRange(onFirst);
                    }

                    // OnEnable & Teleport
                    if (spawn.OnEnable?.Teleport != null)
                    {
                        button.EnableEvents.Add(new EventContainer(mapInstance, EventActionType.Teleport, new Tuple<short, short, short, short>(spawn.OnEnable.Teleport.PositionX, spawn.OnEnable.Teleport.PositionY, spawn.OnEnable.Teleport.DestinationX, spawn.OnEnable.Teleport.DestinationY)));
                    }

                    // OnDisable & Teleport
                    if (spawn.OnDisable?.Teleport != null)
                    {
                        button.DisableEvents.Add(new EventContainer(mapInstance, EventActionType.Teleport, new Tuple<short, short, short, short>(spawn.OnDisable.Teleport.PositionX, spawn.OnDisable.Teleport.PositionY, spawn.OnDisable.Teleport.DestinationX, spawn.OnDisable.Teleport.DestinationY)));
                    }

                    evts.Add(new EventContainer(mapInstance, EventActionType.SpawnButton, button));
                }
            }

            return evts;
        }

        private List<EventContainer> SpawnPortal(MapInstance mapInstance, XMLModel.ScriptedInstance.Events.SpawnPortal[] spawnPortal)
        {
            List<EventContainer> evts = new List<EventContainer>();

            // SpawnPortal
            if (spawnPortal != null)
            {
                foreach (XMLModel.ScriptedInstance.Events.SpawnPortal portalEvent in spawnPortal)
                {
                    _mapInstanceDictionary.TryGetValue(portalEvent.ToMap, out MapInstance destinationMap);
                    Portal portal = new Portal
                    {
                        PortalId = portalEvent.IdOnMap,
                        SourceX = portalEvent.PositionX,
                        SourceY = portalEvent.PositionY,
                        Type = portalEvent.Type,
                        DestinationX = portalEvent.ToX,
                        DestinationY = portalEvent.ToY,
                        DestinationMapId = (short)(destinationMap?.MapInstanceId == default ? -1 : 0),
                        SourceMapInstanceId = mapInstance.MapInstanceId,
                        DestinationMapInstanceId = destinationMap?.MapInstanceId ?? Guid.Empty
                    };

                    // OnTraversal
                    if (portalEvent.OnTraversal?.End != null)
                    {
                        portal.OnTraversalEvents.Add(new EventContainer(mapInstance, EventActionType.ScriptEnd, portalEvent.OnTraversal.End.Type));
                    }

                    evts.Add(new EventContainer(mapInstance, EventActionType.SpawnPortal, portal));
                }
            }

            return evts;
        }

        private List<EventContainer> SummonMonster(MapInstance mapInstance, XMLModel.ScriptedInstance.Events.SummonMonster[] summonMonster, bool isChildMonster = false)
        {
            List<EventContainer> evts = new List<EventContainer>();

            // SummonMonster
            if (summonMonster != null)
            {
                foreach (XMLModel.ScriptedInstance.Events.SummonMonster summon in summonMonster)
                {
                    short positionX = summon.PositionX;
                    short positionY = summon.PositionY;
                    if (positionX == 0 || positionY == 0)
                    {
                        MapCell cell = mapInstance?.Map?.GetRandomPosition();
                        if (cell != null)
                        {
                            positionX = cell.X;
                            positionY = cell.Y;
                        }
                    }
                    MonsterAmount++;
                    MonsterToSummon monster = new MonsterToSummon(summon.VNum, new MapCell { X = positionX, Y = positionY }, -1, summon.Move, summon.IsTarget, summon.IsBonus, summon.IsHostile, summon.IsBoss);

                    // OnDeath
                    if (summon.OnDeath != null)
                    {
                        // RemoveButtonLocker
                        if (summon.OnDeath.RemoveButtonLocker != null)
                        {
                            monster.DeathEvents.Add(new EventContainer(mapInstance, EventActionType.RemoveButtonLocker, null));
                        }

                        // RemoveMonsterLocker
                        if (summon.OnDeath.RemoveMonsterLocker != null)
                        {
                            monster.DeathEvents.Add(new EventContainer(mapInstance, EventActionType.RemoveMonsterLocker, null));
                        }

                        // ChangePortalType
                        if (summon.OnDeath.ChangePortalType != null)
                        {
                            foreach (XMLModel.ScriptedInstance.Events.ChangePortalType changePortalType in summon.OnDeath.ChangePortalType)
                            {
                                monster.DeathEvents.Add(new EventContainer(mapInstance, EventActionType.ChangePortalType, new Tuple<int, PortalType>(changePortalType.IdOnMap, (PortalType)changePortalType.Type)));
                            }
                        }

                        // SendMessage
                        if (summon.OnDeath.SendMessage != null)
                        {
                            monster.DeathEvents.Add(new EventContainer(mapInstance, EventActionType.SendPacket, UserInterfaceHelper.GenerateMsg(summon.OnDeath.SendMessage.Value, summon.OnDeath.SendMessage.Type)));
                        }

                        // SendPacket
                        if (summon.OnDeath.SendPacket != null)
                        {
                            monster.DeathEvents.Add(new EventContainer(mapInstance, EventActionType.SendPacket, summon.OnDeath.SendPacket.Value));
                        }

                        // RefreshRaidGoals
                        if (summon.OnDeath.RefreshRaidGoals != null)
                        {
                            monster.DeathEvents.Add(new EventContainer(mapInstance, EventActionType.RefreshRaidGoal, null));
                        }

                        // ThrowItem
                        if (summon.OnDeath.ThrowItem != null)
                        {
                            foreach (XMLModel.ScriptedInstance.Events.ThrowItem throwItem in summon.OnDeath.ThrowItem)
                            {
                                monster.DeathEvents.Add(new EventContainer(mapInstance, EventActionType.ThrowItems, new Tuple<int, short, byte, int, int>(-1, throwItem.VNum, throwItem.PackAmount == 0 ? (byte)1 : throwItem.PackAmount, throwItem.MinAmount == 0 ? 1 : throwItem.MinAmount, throwItem.MaxAmount == 0 ? 1 : throwItem.MaxAmount)));
                            }
                        }

                        // End
                        if (summon.OnDeath.End != null)
                        {
                            monster.DeathEvents.Add(new EventContainer(mapInstance, EventActionType.ScriptEnd, summon.OnDeath.End.Type));
                        }

                        // StopMapClock
                        if (summon.OnDeath.StopMapClock != null)
                        {
                            monster.DeathEvents.Add(new EventContainer(mapInstance, EventActionType.StopMapClock, null));
                        }

                        // RefreshMapItems
                        if (summon.OnDeath.RefreshMapItems != null)
                        {
                            monster.DeathEvents.Add(new EventContainer(mapInstance, EventActionType.RefreshMapItems, null));
                        }

                        // SummonMonster Child
                        if (!isChildMonster)
                        {
                            monster.DeathEvents.AddRange(SummonMonster(mapInstance, summon.OnDeath?.SummonMonster, true));
                        }
                    }

                    // OnNoticing
                    if (summon.OnNoticing != null)
                    {
                        // Effect
                        if (summon.OnNoticing.Effect != null)
                        {
                            monster.NoticingEvents.Add(new EventContainer(mapInstance, EventActionType.Effect, summon.OnNoticing.Effect.Value));
                        }

                        // Move
                        if (summon.OnNoticing.Move != null)
                        {
                            List<EventContainer> events = new List<EventContainer>();

                            // Effect
                            if (summon.OnNoticing.Move.Effect != null)
                            {
                                events.Add(new EventContainer(mapInstance, EventActionType.Effect, summon.OnNoticing.Move.Effect.Value));
                            }

                            // review OnTarget
                            //if (summon.OnNoticing.Move.OnTarget != null)
                            //{
                            //    summon.OnNoticing.Move.OnTarget.Move
                            //    foreach ()
                            //    events.Add(new EventContainer(mapInstance, EventActionType.ONTARGET, summon.OnNoticing.Move.OnTarget.));
                            //}

                            monster.NoticingEvents.Add(new EventContainer(mapInstance, EventActionType.Move, new ZoneEvent { X = summon.OnNoticing.Move.PositionX, Y = summon.OnNoticing.Move.PositionY, Events = events }));
                        }

                        // SummonMonster Child
                        if (!isChildMonster)
                        {
                            monster.NoticingEvents.AddRange(SummonMonster(mapInstance, summon.OnDeath?.SummonMonster, true));
                        }
                    }

                    evts.Add(new EventContainer(mapInstance, EventActionType.SpawnMonster, monster));
                }
            }

            return evts;
        }

        private List<EventContainer> SummonNpc(MapInstance mapInstance, XMLModel.ScriptedInstance.Events.SummonNpc[] summonNpc)
        {
            List<EventContainer> evts = new List<EventContainer>();

            if (summonNpc != null)
            {
                foreach (XMLModel.ScriptedInstance.Events.SummonNpc summon in summonNpc)
                {
                    short positionX = summon.PositionX;
                    short positionY = summon.PositionY;

                    if (positionX == 0 || positionY == 0)
                    {
                        MapCell cell = mapInstance?.Map?.GetRandomPosition();
                        if (cell != null)
                        {
                            positionX = cell.X;
                            positionY = cell.Y;
                        }
                    }

                    NpcAmount++;
                    NpcToSummon npcToSummon = new NpcToSummon(summon.VNum, new MapCell { X = positionX, Y = positionY }, -1, summon.IsMate, summon.IsProtected, summon.Move);

                    // OnDeath
                    if (summon.OnDeath != null)
                    {
                        // RemoveButtonLocker
                        if (summon.OnDeath.RemoveButtonLocker != null)
                        {
                            npcToSummon.DeathEvents.Add(new EventContainer(mapInstance, EventActionType.RemoveButtonLocker, null));
                        }

                        // RemoveMonsterLocker
                        if (summon.OnDeath.RemoveMonsterLocker != null)
                        {
                            npcToSummon.DeathEvents.Add(new EventContainer(mapInstance, EventActionType.RemoveMonsterLocker, null));
                        }

                        // ChangePortalType
                        if (summon.OnDeath.ChangePortalType != null)
                        {
                            foreach (XMLModel.ScriptedInstance.Events.ChangePortalType changePortalType in summon.OnDeath.ChangePortalType)
                            {
                                npcToSummon.DeathEvents.Add(new EventContainer(mapInstance, EventActionType.ChangePortalType, new Tuple<int, PortalType>(changePortalType.IdOnMap, (PortalType)changePortalType.Type)));
                            }
                        }

                        // End
                        if (summon.OnDeath.End != null)
                        {
                            npcToSummon.DeathEvents.Add(new EventContainer(mapInstance, EventActionType.ScriptEnd, summon.OnDeath.End.Type));
                        }

                        // RefreshRaidGoals
                        if (summon.OnDeath.RefreshRaidGoals != null)
                        {
                            npcToSummon.DeathEvents.Add(new EventContainer(mapInstance, EventActionType.RefreshRaidGoal, null));
                        }

                        // RefreshMapItems
                        if (summon.OnDeath.RefreshRaidGoals != null)
                        {
                            npcToSummon.DeathEvents.Add(new EventContainer(mapInstance, EventActionType.RefreshMapItems, null));
                        }

                        // ThrowItems
                        if (summon.OnDeath.ThrowItem != null)
                        {
                            foreach (XMLModel.ScriptedInstance.Events.ThrowItem throwItem in summon.OnDeath.ThrowItem)
                            {
                                npcToSummon.DeathEvents.Add(new EventContainer(mapInstance, EventActionType.ThrowItems, new Tuple<int, short, byte, int, int>(-1, throwItem.VNum, throwItem.PackAmount == 0 ? (byte)1 : throwItem.PackAmount, throwItem.MinAmount == 0 ? 1 : throwItem.MinAmount, throwItem.MaxAmount == 0 ? 1 : throwItem.MaxAmount)));
                            }
                        }
                    }

                    evts.Add(new EventContainer(mapInstance, EventActionType.SpawnNpc, npcToSummon));
                }
            }

            return evts;
        }

        #endregion
    }
}