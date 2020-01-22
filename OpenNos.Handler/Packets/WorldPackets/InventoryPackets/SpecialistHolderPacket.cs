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
    [PacketHeader("s_carrier")]
    public class SpecialistHolderPacket
    {
        #region Properties

        public byte HolderSlot { get; set; }

        public byte Slot { get; set; }

        public byte HolderType { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 4)
            {
                return;
            }
            SpecialistHolderPacket packetDefinition = new SpecialistHolderPacket();
            if (byte.TryParse(packetSplit[2], out byte slot)
                && byte.TryParse(packetSplit[3], out byte holderSlot))
            {
                packetDefinition.Slot = slot;
                packetDefinition.HolderSlot = holderSlot;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }

            if (byte.TryParse(packetSplit[4], out var type))
            {
                packetDefinition.HolderType = type;
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(SpecialistHolderPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (HolderType == 0)
            {
                ItemInstance specialist =
                    session.Character.Inventory.LoadBySlotAndType(Slot, InventoryType.Equipment);
                ItemInstance holder = session.Character.Inventory.LoadBySlotAndType(HolderSlot,
                    InventoryType.Equipment);
                if (specialist != null && holder != null)
                {
                    holder.HoldingVNum = specialist.ItemVNum;
                    holder.SlDamage = specialist.SlDamage;
                    holder.SlDefence = specialist.SlDefence;
                    holder.SlElement = specialist.SlElement;
                    holder.SlHP = specialist.SlHP;
                    holder.SpDamage = specialist.SpDamage;
                    holder.SpDark = specialist.SpDark;
                    holder.SpDefence = specialist.SpDefence;
                    holder.SpElement = specialist.SpElement;
                    holder.SpFire = specialist.SpFire;
                    holder.SpHP = specialist.SpHP;
                    holder.SpLevel = specialist.SpLevel;
                    holder.SpLight = specialist.SpLight;
                    holder.SpStoneUpgrade = specialist.SpStoneUpgrade;
                    holder.SpWater = specialist.SpWater;
                    holder.Upgrade = specialist.Upgrade;
                    holder.XP = specialist.XP;
                    holder.EquipmentSerialId = specialist.EquipmentSerialId;
                    session.SendPacket("shop_end 2");
                    session.Character.Inventory.RemoveItemFromInventory(specialist.Id);
                }
            }
            else if (HolderType == 1)
            {
                ItemInstance specialist = session.Character.Inventory.LoadBySlotAndType(Slot, InventoryType.Equipment);
                ItemInstance holder = session.Character.Inventory.LoadBySlotAndType(HolderSlot, InventoryType.Equipment);

                if (specialist == null || holder == null)
                {
                    return;
                }

                holder.HoldingVNum = specialist.ItemVNum;
                holder.FirstPartnerSkill = specialist.FirstPartnerSkill;
                holder.SecondPartnerSkill = specialist.SecondPartnerSkill;
                holder.ThirdPartnerSkill = specialist.ThirdPartnerSkill;
                holder.FirstPartnerSkillRank = specialist.FirstPartnerSkillRank;
                holder.SecondPartnerSkillRank = specialist.SecondPartnerSkillRank;
                holder.ThirdPartnerSkillRank = specialist.ThirdPartnerSkillRank;
                session.SendPacket("shop_end 2");
                session.Character.Inventory.RemoveItemFromInventory(specialist.Id);
            }
        }

        #endregion
    }
}