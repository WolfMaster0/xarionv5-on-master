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

using System;
using System.Collections.Generic;
using System.Linq;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.Npc;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$SummonNPC", Authority = AuthorityType.GameMaster)]
    public class SummonNpcPacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public byte Amount { get; set; }

        public bool IsMoving { get; set; }

        public short NpcMonsterVNum { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            if (session is ClientSession sess)
            {
                string[] packetSplit = packet.Split(' ');
                if (packetSplit.Length < 5)
                {
                    sess.SendPacket(sess.Character.GenerateSay(ReturnHelp(), 10));
                    return;
                }
                SummonNpcPacket packetDefinition = new SummonNpcPacket();
                if (short.TryParse(packetSplit[2], out short vnum) && byte.TryParse(packetSplit[3], out byte amount))
                {
                    packetDefinition._isParsed = true;
                    packetDefinition.NpcMonsterVNum = vnum;
                    packetDefinition.Amount = amount;
                    packetDefinition.IsMoving = packetSplit[4] == "1";
                }
                packetDefinition.ExecuteHandler(sess);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(SummonNpcPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$SummonNPC VNUM AMOUNT MOVE";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(),
                    $"[SummonNPC]NpcMonsterVNum: {NpcMonsterVNum} Amount: {Amount} IsMoving: {IsMoving}");

                if (session.IsOnMap && session.HasCurrentMapInstance)
                {
                    NpcMonster npcmonster = ServerManager.GetNpcMonster(NpcMonsterVNum);
                    if (npcmonster == null)
                    {
                        return;
                    }

                    Random random = new Random();
                    for (int i = 0; i < Amount; i++)
                    {
                        List<MapCell> possibilities = new List<MapCell>();
                        for (short x = -4; x < 5; x++)
                        {
                            for (short y = -4; y < 5; y++)
                            {
                                possibilities.Add(new MapCell { X = x, Y = y });
                            }
                        }

                        foreach (MapCell possibilitie in possibilities.OrderBy(s => random.Next()))
                        {
                            short mapx = (short)(session.Character.PositionX + possibilitie.X);
                            short mapy = (short)(session.Character.PositionY + possibilitie.Y);
                            if (!session.CurrentMapInstance?.Map.IsBlockedZone(mapx, mapy) ?? false)
                            {
                                break;
                            }
                        }

                        if (session.CurrentMapInstance != null)
                        {
                            MapNpc npc = new MapNpc
                            {
                                NpcVNum = NpcMonsterVNum,
                                MapY = session.Character.PositionY,
                                MapX = session.Character.PositionX,
                                MapId = session.Character.MapInstance.Map.MapId,
                                Position = session.Character.Direction,
                                IsMoving = IsMoving,
                                MapNpcId = session.CurrentMapInstance.GetNextNpcId()
                            };
                            npc.Initialize(session.CurrentMapInstance);
                            session.CurrentMapInstance.AddNpc(npc);
                            session.CurrentMapInstance.Broadcast(npc.GenerateIn());
                        }
                    }
                }
            }
            else
            {
                session.SendPacket(session.Character.GenerateSay(ReturnHelp(), 10));
            }
        }

        #endregion
    }
}