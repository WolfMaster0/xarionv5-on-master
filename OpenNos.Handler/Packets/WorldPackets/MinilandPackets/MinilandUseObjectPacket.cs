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
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;
using OpenNos.Handler.Packets.SharedPacketMethods;
// ReSharper disable UnreachableCode

namespace OpenNos.Handler.Packets.WorldPackets.MinilandPackets
{
    [PacketHeader("useobj")]
    public class MinilandUseObjectPacket
    {
        #region Properties

        public short Slot { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 4)
            {
                return;
            }
            MinilandUseObjectPacket packetDefinition = new MinilandUseObjectPacket();
            if (short.TryParse(packetSplit[3], out short slot))
            {
                packetDefinition.Slot = slot;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(MinilandUseObjectPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            ClientSession client =
                ServerManager.Instance.Sessions.FirstOrDefault(s =>
                    s.Character?.Miniland == session.Character.MapInstance);
            ItemInstance minilandObjectItem =
                client?.Character.Inventory.LoadBySlotAndType<ItemInstance>(Slot, InventoryType.Miniland);
            if (minilandObjectItem != null)
            {
                MinilandObject minilandObject =
                    client.Character.MinilandObjects.Find(s => s.ItemInstanceId == minilandObjectItem.Id);
                if (minilandObject != null)
                {
                    if (!minilandObjectItem.Item.IsMinilandObject)
                    {
                        byte game = (byte)(minilandObject.ItemInstance.Item.EquipmentSlot == 0
                            ? 4 + (minilandObject.ItemInstance.ItemVNum % 10)
                            : (int)minilandObject.ItemInstance.Item.EquipmentSlot / 3);
                        const bool full = false;
                        session.SendPacket(
                            $"mlo_info {(client == session ? 1 : 0)} {minilandObjectItem.ItemVNum} {Slot} {session.Character.MinilandPoint} {(minilandObjectItem.DurabilityPoint < 1000 ? 1 : 0)} {(full ? 1 : 0)} 0 {SharedMinilandMethods.GetMinilandMaxPoint(game)[0]} {SharedMinilandMethods.GetMinilandMaxPoint(game)[0] + 1} {SharedMinilandMethods.GetMinilandMaxPoint(game)[1]} {SharedMinilandMethods.GetMinilandMaxPoint(game)[1] + 1} {SharedMinilandMethods.GetMinilandMaxPoint(game)[2]} {SharedMinilandMethods.GetMinilandMaxPoint(game)[2] + 2} {SharedMinilandMethods.GetMinilandMaxPoint(game)[3]} {SharedMinilandMethods.GetMinilandMaxPoint(game)[3] + 1} {SharedMinilandMethods.GetMinilandMaxPoint(game)[4]} {SharedMinilandMethods.GetMinilandMaxPoint(game)[4] + 1} {SharedMinilandMethods.GetMinilandMaxPoint(game)[5]}");
                    }
                    else
                    {
                        session.SendPacket(session.Character.GenerateStashAll());
                    }
                }
            }
        }

        #endregion
    }
}