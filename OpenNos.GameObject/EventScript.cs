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
using OpenNos.Data;
using OpenNos.GameObject.Networking;
using OpenNos.XMLModel.Event.Model;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace OpenNos.GameObject
{
    public class EventScript : EventScriptDTO
    {
        #region Instantiation

        public EventScript(EventScriptDTO input)
        {
            Script = input.Script;
            DateStart = input.DateStart;
            DateEnd = input.DateEnd;
            EventScriptId = input.EventScriptId;
        }

        #endregion

        #region Properties

        public EventModel Model { get; set; }

        #endregion

        public List<MapNpc> Npcs { get; set; }

        public List<MapMonster> Monsters { get; set; }

        public List<short> UnlockItems { get; set; }

        public List<DropDTO> Drops { get; set; }

        public List<Portal> Portals { get; set; }

        public List<short> ItemExpire { get; set; }

        #region Methods

        public void LoadGlobals()
        {
            if (Script != null)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(EventModel));
                using (StringReader textReader = new StringReader(Script))
                {
                    Model = (EventModel)serializer.Deserialize(textReader);
                }

                if (Model?.Globals != null)
                {
                    UnlockItems = new List<short>();
                    Drops = new List<DropDTO>();
                    ItemExpire = new List<short>();
                    Npcs = new List<MapNpc>();
                    Portals = new List<Portal>();
                    Monsters = new List<MapMonster>();

                    if (Model.Globals.UnlockItems != null)
                    {
                        foreach (XMLModel.Event.Objects.EventItem item in Model.Globals.UnlockItems)
                        {
                            UnlockItems.Add(item.VNum);
                        }
                    }
                    if (Model.Globals.AddDrop != null)
                    {
                        foreach (XMLModel.Event.Events.AddDrop drop in Model.Globals.AddDrop)
                        {
                            Drops.Add(new DropDTO() { Amount = drop.Amount, ItemVNum = drop.VNum, DropChance = drop.Chance, DropId = ServerManager.Instance.GetNextDropId() });
                        }
                    }
                    if (Model.Globals.FinishEvents != null)
                    {
                        foreach (XMLModel.Event.Objects.EventItem item in Model.Globals.FinishEvents.ItemExpire)
                        {
                            ItemExpire.Add(item.VNum);
                        }
                    }
                    if (Model.Globals.SpawnNpc != null)
                    {
                        foreach (XMLModel.Event.Events.SpawnNpc spawnNpc in Model.Globals.SpawnNpc)
                        {
                            Npcs.Add(new MapNpc()
                            {
                                NpcVNum = spawnNpc.VNum,
                                Position = spawnNpc.Direction,
                                MapX = spawnNpc.PositionX,
                                MapY = spawnNpc.PositionY,
                                MapId = spawnNpc.MapId,
                                IsMoving = spawnNpc.Move,
                                Dialog = spawnNpc.Dialog
                            });
                        }
                    }
                    if (Model.Globals.SpawnMonster != null)
                    {
                        foreach (XMLModel.Event.Events.SpawnMonster spawnMonster in Model.Globals.SpawnMonster)
                        {
                            Monsters.Add(new MapMonster()
                            {
                                MonsterVNum = spawnMonster.VNum,
                                Position = spawnMonster.Direction,
                                MapX = spawnMonster.PositionX,
                                MapY = spawnMonster.PositionY,
                                MapId = spawnMonster.MapId,
                                IsMoving = spawnMonster.Move,
                                IsHostile = spawnMonster.IsHostile,
                                ShouldRespawn = spawnMonster.ShouldRespawn
                            });
                        }
                    }
                    if (Model.Globals.SpawnPortal != null)
                    {
                        foreach (XMLModel.Event.Events.SpawnPortal spawnPortal in Model.Globals.SpawnPortal)
                        {
                            Portals.Add(new Portal()
                            {
                                SourceMapId = spawnPortal.MapId,
                                DestinationMapId = spawnPortal.ToMap,
                                SourceX = spawnPortal.PositionX,
                                SourceY = spawnPortal.PositionY,
                                DestinationX = spawnPortal.ToX,
                                DestinationY = spawnPortal.ToY
                            });
                            Portals.Add(new Portal()
                            {
                                DestinationMapId = spawnPortal.MapId,
                                SourceMapId = spawnPortal.ToMap,
                                DestinationX = spawnPortal.PositionX,
                                DestinationY = spawnPortal.PositionY,
                                SourceX = spawnPortal.ToX,
                                SourceY = spawnPortal.ToY
                            });
                        }
                    }
                }
            }
        }

        #endregion
    }
}