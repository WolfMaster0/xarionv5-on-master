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
using System.Reactive.Linq;
using System.Threading;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$ItemRain", Authority = AuthorityType.GameMaster)]
    public class ItemRainPacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public byte Amount { get; set; }

        public int Count { get; set; }

        public int Time { get; set; }

        public short VNum { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            if (session is ClientSession sess)
            {
                string[] packetSplit = packet.Split(' ');
                if (packetSplit.Length < 6)
                {
                    sess.SendPacket(sess.Character.GenerateSay(ReturnHelp(), 10));
                    return;
                }
                ItemRainPacket packetDefinition = new ItemRainPacket();
                if (short.TryParse(packetSplit[2], out short vnum) && byte.TryParse(packetSplit[3], out byte amount)
                    && int.TryParse(packetSplit[4], out int count) && int.TryParse(packetSplit[5], out int time))
                {
                    packetDefinition._isParsed = true;
                    packetDefinition.VNum = vnum;
                    packetDefinition.Amount = amount;
                    packetDefinition.Count = count;
                    packetDefinition.Time = time;
                }

                packetDefinition.ExecuteHandler(sess);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(ItemRainPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$ItemRain VNUM AMOUNT COUNT TIME";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                MapInstance instance = session.CurrentMapInstance;

                Observable.Timer(TimeSpan.FromSeconds(0)).Subscribe(observer =>
                {
                    for (int i = 0; i < Count; i++)
                    {
                        MapCell cell = instance.Map.GetRandomPosition();
                        MonsterMapItem droppedItem = new MonsterMapItem(cell.X, cell.Y, VNum, Amount);
                        instance.DroppedList[droppedItem.TransportId] = droppedItem;
                        instance.Broadcast(
                            $"drop {droppedItem.ItemVNum} {droppedItem.TransportId} {droppedItem.PositionX} {droppedItem.PositionY} {(droppedItem.GoldAmount > 1 ? droppedItem.GoldAmount : droppedItem.Amount)} 0 -1");

                        Thread.Sleep(Time * 1000 / Count);
                    }
                });
            }
            else
            {
                session.SendPacket(session.Character.GenerateSay(ReturnHelp(), 10));
            }
        }

        #endregion
    }
}