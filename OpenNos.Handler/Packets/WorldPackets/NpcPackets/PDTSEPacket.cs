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

using System;
using System.Linq;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.EventArguments;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.NpcPackets
{
    [PacketHeader("pdtse")]
    public class PdtsePacket
    {
        #region Properties

        public byte Type { get; set; }

        public short VNum { get; set; }

        public short SlotRequiredItem { get; set; }

        public InventoryType TypeRequiredItem { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 4)
            {
                return;
            }

            PdtsePacket packetDefinition = new PdtsePacket();
            if (byte.TryParse(packetSplit[2], out byte type) && short.TryParse(packetSplit[3], out short vNum))
            {
                packetDefinition.Type = type;
                packetDefinition.VNum = vNum;
                packetDefinition.TypeRequiredItem =
                    packetSplit.Length > 4 && Enum.TryParse(packetSplit[4], out InventoryType inventoryType)
                        ? inventoryType
                        : InventoryType.Equipment;
                packetDefinition.SlotRequiredItem =
                    packetSplit.Length > 5 && short.TryParse(packetSplit[5], out short slotReqItem)
                        ? slotReqItem
                        : (short) -1;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(PdtsePacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (!session.HasCurrentMapInstance)
            {
                return;
            }

            // this fixes an issue when a some retard would create a copy of recipe for single item vnum
            Recipe recipe = ServerManager.Instance.GetAllRecipes()
                .Find(s => s.ItemVNum == VNum && s.Amount > 0 && s.Items.Count > 0);
            if (Type == 1 && recipe != null)
            {
                string recipePacket = $"m_list 3 {recipe.Amount}";
                foreach (RecipeItemDTO ite in recipe.Items.Where(s =>
                    s.ItemVNum != session.Character.LastItemVNum || session.Character.LastItemVNum == 0))
                {
                    if (ite.Amount > 0)
                    {
                        recipePacket += $" {ite.ItemVNum} {ite.Amount}";
                    }
                }

                recipePacket += " -1";
                session.SendPacket(recipePacket);
            }
            else if (recipe != null)
            {
                // sequential
                //pdtse 0 4955 0 0 1
                // random
                //pdtse 0 4955 0 0 2
                if (recipe.Items.Count < 1 || recipe.Amount <= 0 || recipe.Items.Any(ite =>
                        session.Character.Inventory.CountItem(ite.ItemVNum) < ite.Amount))
                {
                    return;
                }

                if (session.Character.LastItemVNum != 0)
                {
                    if (!ServerManager.Instance.ItemHasRecipe(session.Character.LastItemVNum))
                    {
                        return;
                    }

                    session.Character.LastItemVNum = 0;
                }
                else if (!ServerManager.Instance.MapNpcHasRecipe(session.Character.LastNpcMonsterId))
                {
                    return;
                }

                ItemInstance inv = session.Character.Inventory.AddNewToInventory(recipe.ItemVNum, recipe.Amount)
                    .FirstOrDefault();
                if (inv != null)
                {
                    if (inv.Item.EquipmentSlot == EquipmentType.Armor
                        || inv.Item.EquipmentSlot == EquipmentType.MainWeapon
                        || inv.Item.EquipmentSlot == EquipmentType.SecondaryWeapon)
                    {
                        bool isPartner = (inv.ItemVNum >= 990 && inv.ItemVNum <= 992) || (inv.ItemVNum >= 995 && inv.ItemVNum <= 997);
                        inv.SetRarityPoint(isPartner);
                    }

                    ItemInstance itemInstance = null;
                    if (SlotRequiredItem != -1)
                    {
                        itemInstance =
                            session.Character.Inventory.LoadBySlotAndType(SlotRequiredItem, TypeRequiredItem);
                    }

                    foreach (RecipeItemDTO ite in recipe.Items)
                    {
                        if (itemInstance != null && (ite.ItemVNum == itemInstance.ItemVNum))
                        {
                            session.Character.Inventory.RemoveItemFromInventory(itemInstance.Id);
                        }
                        else
                        {
                            session.Character.Inventory.RemoveItemAmount(ite.ItemVNum, ite.Amount);
                        }
                    }

                    // pdti {WindowType} {inv.ItemVNum} {recipe.Amount} {Unknown} {inv.Upgrade} {inv.Rare}
                    session.SendPacket($"pdti 11 {inv.ItemVNum} {recipe.Amount} 29 {inv.Upgrade} {inv.Rare}");
                    session.SendPacket(UserInterfaceHelper.GenerateGuri(19, 1, session.Character.CharacterId, 1324));
                    session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        string.Format(Language.Instance.GetMessageFromKey("CRAFTED_OBJECT"), inv.Item.Name,
                            recipe.Amount), 0));

                    session.Character.OnCraftRecipe(new CraftRecipeEventArgs(inv.Item, recipe.Amount));
                }
            }
        }

        #endregion
    }
}