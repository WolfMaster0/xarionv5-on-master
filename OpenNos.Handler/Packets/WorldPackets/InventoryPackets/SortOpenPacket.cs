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

using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.InventoryPackets
{
    [PacketHeader("sortopen")]
    public class SortOpenPacket
    {
        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            SortOpenPacket packetDefinition = new SortOpenPacket();
            packetDefinition.ExecuteHandler(session as ClientSession);
        }

        public static void Register() => PacketFacility.AddHandler(typeof(SortOpenPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            bool gravity = true;
            while (gravity)
            {
                gravity = false;
                for (short i = 0; i < 2; i++)
                {
                    for (short x = 0; x < 44; x++)
                    {
                        InventoryType type = i == 0 ? InventoryType.Specialist : InventoryType.Costume;
                        if (session.Character.Inventory.LoadBySlotAndType<ItemInstance>(x, type) == null
                            && session.Character.Inventory.LoadBySlotAndType<ItemInstance>((short)(x + 1), type)
                            != null)
                        {
                            session.Character.Inventory.MoveItem(type, type, (short)(x + 1), 1, x,
                                out ItemInstance _, out ItemInstance invdest);
                            session.SendPacket(invdest.GenerateInventoryAdd());
                            session.Character.DeleteItem(type, (short)(x + 1));
                            gravity = true;
                        }
                    }

                    session.Character.Inventory.Reorder(session,
                        i == 0 ? InventoryType.Specialist : InventoryType.Costume);
                }
            }
        }

        #endregion
    }
}