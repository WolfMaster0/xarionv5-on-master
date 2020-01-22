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

using System.Linq;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.MinilandPackets
{
    [PacketHeader("addobj")]
    public class MinilandAddObjectPacket
    {
        public short Slot { get; set; }

        public short PositionX { get; set; }

        public short PositionY { get; set; }

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 5)
            {
                return;
            }
            MinilandAddObjectPacket packetDefinition = new MinilandAddObjectPacket();
            if (short.TryParse(packetSplit[2], out short slot)
                && short.TryParse(packetSplit[2], out short positionX)
                && short.TryParse(packetSplit[2], out short positionY))
            {
                packetDefinition.Slot = slot;
                packetDefinition.PositionX = positionX;
                packetDefinition.PositionY = positionY;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(MinilandAddObjectPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            ItemInstance minilandobject =
                session.Character.Inventory.LoadBySlotAndType<ItemInstance>(Slot, InventoryType.Miniland);
            if (minilandobject != null)
            {
                if (session.Character.MinilandObjects.All(s => s.ItemInstanceId != minilandobject.Id))
                {
                    if (session.Character.MinilandState == MinilandState.Lock)
                    {
                        MinilandObject minilandobj = new MinilandObject
                        {
                            CharacterId = session.Character.CharacterId,
                            ItemInstance = minilandobject,
                            ItemInstanceId = minilandobject.Id,
                            MapX = PositionX,
                            MapY = PositionY,
                            Level1BoxAmount = 0,
                            Level2BoxAmount = 0,
                            Level3BoxAmount = 0,
                            Level4BoxAmount = 0,
                            Level5BoxAmount = 0
                        };

                        if (minilandobject.Item.ItemType == ItemType.House)
                        {
                            switch (minilandobject.Item.ItemSubType)
                            {
                                case 2:
                                    minilandobj.MapX = 31;
                                    minilandobj.MapY = 3;
                                    break;

                                case 0:
                                    minilandobj.MapX = 24;
                                    minilandobj.MapY = 7;
                                    break;

                                case 1:
                                    minilandobj.MapX = 21;
                                    minilandobj.MapY = 4;
                                    break;
                            }

                            MinilandObject min = session.Character.MinilandObjects.Find(s =>
                                s.ItemInstance.Item.ItemType == ItemType.House && s.ItemInstance.Item.ItemSubType
                                == minilandobject.Item.ItemSubType);
                            if (min != null)
                            {
                                MinilandRemoveObject.HandlePacket(session, $"1 rmobj {Slot}");
                            }
                        }

                        if (minilandobject.Item.IsMinilandObject)
                        {
                            session.Character.WareHouseSize = minilandobject.Item.MinilandObjectPoint;
                        }

                        session.Character.MinilandObjects.Add(minilandobj);
                        session.SendPacket(minilandobj.GenerateMinilandEffect(false));
                        session.SendPacket(session.Character.GenerateMinilandPoint());
                        session.SendPacket(minilandobj.GenerateMinilandObject(false));
                    }
                    else
                    {
                        session.SendPacket(
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("MINILAND_NEED_LOCK"),
                                0));
                    }
                }
                else
                {
                    session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(
                            Language.Instance.GetMessageFromKey("ALREADY_THIS_MINILANDOBJECT"), 0));
                }
            }
        }
    }
}