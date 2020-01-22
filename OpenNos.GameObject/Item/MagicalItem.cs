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
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using System;
using System.Linq;
using System.Reactive.Linq;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject
{
    public class MagicalItem : Item
    {
        #region Instantiation

        public MagicalItem(ItemDTO item) : base(item)
        {
        }

        #endregion

        #region Methods

        public override void Use(ClientSession session, ref ItemInstance inv, byte option = 0, string[] packetsplit = null)
        {
            switch (Effect)
            {
                // airwaves - eventitems
                case 0:
                    if (inv.Item.ItemType == ItemType.Shell)
                    {
                        if (inv.ShellEffects.Count != 0 && packetsplit?.Length > 9
                            && byte.TryParse(packetsplit[9], out byte islot))
                        {
                            ItemInstance wearable =
                                session.Character.Inventory.LoadBySlotAndType<ItemInstance>(islot,
                                    InventoryType.Equipment);
                            if (wearable != null
                                && (wearable.Item.ItemType == ItemType.Weapon
                                 || wearable.Item.ItemType == ItemType.Armor)
                                && wearable.Rare >= inv.Rare
                                && (wearable.BoundCharacterId == null
                                 || wearable.BoundCharacterId == session.Character.CharacterId))
                            {
                                bool weapon;
                                if ((inv.ItemVNum >= 565 && inv.ItemVNum <= 576)
                                    || (inv.ItemVNum >= 589 && inv.ItemVNum <= 598))
                                {
                                    weapon = true;
                                }
                                else if ((inv.ItemVNum >= 577 && inv.ItemVNum <= 588)
                                         || (inv.ItemVNum >= 656 && inv.ItemVNum <= 664) || inv.ItemVNum == 599)
                                {
                                    weapon = false;
                                }
                                else
                                {
                                    return;
                                }

                                if ((wearable.Item.ItemType == ItemType.Weapon && weapon)
                                    || (wearable.Item.ItemType == ItemType.Armor && !weapon))
                                {
                                    if (wearable.ShellEffects.Count > 0 && ServerManager.RandomNumber() < 50)
                                    {
                                        
                                        session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                            Language.Instance.GetMessageFromKey("OPTION_APPLY_FAIL"), 0));
                                        return;
                                    }

                                    wearable.ShellEffects.Clear();
                                    DAOFactory.ShellEffectDAO.DeleteByEquipmentSerialId(wearable.EquipmentSerialId);
                                    wearable.ShellEffects.AddRange(inv.ShellEffects);
                                    wearable.ShellRarity = inv.Rare;
                                    wearable.BoundCharacterId = session.Character.CharacterId;
                                    if (wearable.EquipmentSerialId == Guid.Empty)
                                    {
                                        wearable.EquipmentSerialId = Guid.NewGuid();
                                    }

                                    DAOFactory.ShellEffectDAO.InsertOrUpdateFromList(wearable.ShellEffects,
                                        wearable.EquipmentSerialId);

                                    session.Character.DeleteItemByItemInstanceId(inv.Id);
                                    session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                        Language.Instance.GetMessageFromKey("OPTION_APPLY_SUCCESS"), 0));
                                }
                            }
                        }

                        return;
                    }

                    if (ItemType == ItemType.Event)
                    {
                        session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Player, session.Character.CharacterId, EffectValue));
                        if (MappingHelper.GuriItemEffects.ContainsKey(EffectValue))
                        {
                            session.CurrentMapInstance?.Broadcast(UserInterfaceHelper.GenerateGuri(19, 1, session.Character.CharacterId, MappingHelper.GuriItemEffects[EffectValue]), session.Character.MapX, session.Character.MapY);
                        }
                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                    }
                    break;

                //respawn objects
                case 1:
                    {
                        if (session.Character.MapInstance.MapInstanceType != MapInstanceType.BaseMapInstance || ServerManager.Instance.ChannelId == 51)
                        {
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_USE_THAT"), 10));
                            return;
                        }
                        if (packetsplit != null
                            && int.TryParse(packetsplit[2], out int type)
                            && int.TryParse(packetsplit[3], out int secondaryType)
                            && int.TryParse(packetsplit[4], out int inventoryType)
                            && int.TryParse(packetsplit[5], out int slot))
                        {
                            int packetType;
                            switch (EffectValue)
                            {
                                case 0:
                                    if (option == 0)
                                    {
                                        session.SendPacket(UserInterfaceHelper.GenerateDialog($"#u_i^{type}^{secondaryType}^{inventoryType}^{slot}^1 #u_i^{type}^{secondaryType}^{inventoryType}^{slot}^2 {Language.Instance.GetMessageFromKey("WANT_TO_SAVE_POSITION")}"));
                                    }
                                    else if (int.TryParse(packetsplit[6], out packetType))
                                    {
                                        switch (packetType)
                                        {
                                            case 1:
                                                session.SendPacket(UserInterfaceHelper.GenerateDelay(5000, 7, $"#u_i^{type}^{secondaryType}^{inventoryType}^{slot}^3"));
                                                break;

                                            case 2:
                                                session.SendPacket(UserInterfaceHelper.GenerateDelay(5000, 7, $"#u_i^{type}^{secondaryType}^{inventoryType}^{slot}^4"));
                                                break;

                                            case 3:
                                                session.Character.SetReturnPoint(session.Character.MapId, session.Character.MapX, session.Character.MapY);
                                                RespawnMapTypeDTO respawn = session.Character.Respawn;
                                                if (respawn.DefaultX != 0 && respawn.DefaultY != 0 && respawn.DefaultMapId != 0)
                                                {
                                                    ServerManager.Instance.ChangeMap(session.Character.CharacterId, respawn.DefaultMapId, (short)(respawn.DefaultX + ServerManager.RandomNumber(-5, 5)), (short)(respawn.DefaultY + ServerManager.RandomNumber(-5, 5)));
                                                }
                                                session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                                                break;

                                            case 4:
                                                RespawnMapTypeDTO respawnObj = session.Character.Respawn;
                                                if (respawnObj.DefaultX != 0 && respawnObj.DefaultY != 0 && respawnObj.DefaultMapId != 0)
                                                {
                                                    ServerManager.Instance.ChangeMap(session.Character.CharacterId, respawnObj.DefaultMapId, (short)(respawnObj.DefaultX + ServerManager.RandomNumber(-5, 5)), (short)(respawnObj.DefaultY + ServerManager.RandomNumber(-5, 5)));
                                                }
                                                session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                                                break;
                                        }
                                    }
                                    break;

                                case 1:
                                    if (int.TryParse(packetsplit[6], out packetType))
                                    {
                                        RespawnMapTypeDTO respawn = session.Character.Return;
                                        switch (packetType)
                                        {
                                            case 0:
                                                if (respawn.DefaultX != 0 && respawn.DefaultY != 0 && respawn.DefaultMapId != 0)
                                                {
                                                    session.SendPacket(UserInterfaceHelper.GenerateRp(respawn.DefaultMapId, respawn.DefaultX, respawn.DefaultY, $"#u_i^{type}^{secondaryType}^{inventoryType}^{slot}^1"));
                                                }
                                                break;

                                            case 1:
                                                session.SendPacket(UserInterfaceHelper.GenerateDelay(5000, 7, $"#u_i^{type}^{secondaryType}^{inventoryType}^{slot}^2"));
                                                break;

                                            case 2:
                                                if (respawn.DefaultX != 0 && respawn.DefaultY != 0 && respawn.DefaultMapId != 0)
                                                {
                                                    ServerManager.Instance.ChangeMap(session.Character.CharacterId, respawn.DefaultMapId, respawn.DefaultX, respawn.DefaultY);
                                                }
                                                session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                                                break;
                                        }
                                    }
                                    break;

                                case 2:
                                    if (option == 0)
                                    {
                                        session.SendPacket(UserInterfaceHelper.GenerateDelay(5000, 7, $"#u_i^{type}^{secondaryType}^{inventoryType}^{slot}^1"));
                                    }
                                    else
                                    {
                                        ServerManager.Instance.JoinMiniland(session, session);
                                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                                    }
                                    break;
                            }
                        }
                    }
                    break;

                // dyes or waxes
                case 10:
                case 11:
                    if (!session.Character.IsVehicled)
                    {
                        if (Effect == 10)
                        {
                            if (EffectValue == 99)
                            {
                                byte nextValue = (byte)ServerManager.RandomNumber(0, 127);
                                session.Character.HairColor = Enum.IsDefined(typeof(HairColorType), nextValue) ? (HairColorType)nextValue : 0;
                            }
                            else
                            {
                                session.Character.HairColor = Enum.IsDefined(typeof(HairColorType), (byte)EffectValue) ? (HairColorType)EffectValue : 0;
                            }
                        }
                        else if (Effect == 11)
                        {
                            if (session.Character.Class == (byte)ClassType.Adventurer && EffectValue > 1)
                            {
                                session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("ADVENTURERS_CANT_USE"), 10));
                                return;
                            }
                            if (session.Character.Gender != (GenderType)Sex && EffectValue != 3 && EffectValue != 2)
                            {
                                session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("CANNOT_USE"), 10));
                                return;
                            }
                            session.Character.HairStyle = Enum.IsDefined(typeof(HairStyleType), (byte)EffectValue) ? (HairStyleType)EffectValue : 0;
                        }
                        else
                        {
                            if (session.Character.Class == (byte)ClassType.Adventurer && EffectValue > 1)
                            {
                                session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("ADVENTURERS_CANT_USE"), 10));
                                return;
                            }
                            session.Character.HairStyle = Enum.IsDefined(typeof(HairStyleType), (byte)EffectValue) ? (HairStyleType)EffectValue : 0;
                        }
                        session.SendPacket(session.Character.GenerateEq());
                        session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateIn());
                        session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateGidx());
                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                    }
                    break;

                // dignity restoration
                case 14:
                    if ((EffectValue == 100 || EffectValue == 200) && session.Character.Dignity < 100 && !session.Character.IsVehicled)
                    {
                        session.Character.Dignity += EffectValue;
                        if (session.Character.Dignity > 100)
                        {
                            session.Character.Dignity = 100;
                        }
                        session.SendPacket(session.Character.GenerateFd());
                        session.SendPacket(StaticPacketHelper.GenerateEff(UserType.Player, session.Character.CharacterId, 49 - (byte)session.Character.Faction));
                        session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                        session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateGidx(), ReceiverType.AllExceptMe);
                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                    }
                    else if (EffectValue == 2000 && session.Character.Dignity < 100 && !session.Character.IsVehicled)
                    {
                        session.Character.Dignity = 100;
                        session.SendPacket(session.Character.GenerateFd());
                        session.SendPacket(StaticPacketHelper.GenerateEff(UserType.Player, session.Character.CharacterId, 49 - (byte)session.Character.Faction));
                        session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                        session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateGidx(), ReceiverType.AllExceptMe);
                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                    }
                    break;

                // speakers
                case 15:
                    if (!session.Character.IsVehicled && option == 0)
                    {
                        session.SendPacket(UserInterfaceHelper.GenerateGuri(10, 3, session.Character.CharacterId, 1));
                    }
                    break;

                // bubbles
                case 16:
                    if (!session.Character.IsVehicled && option == 0)
                    {
                        session.SendPacket(UserInterfaceHelper.GenerateGuri(10, 4, session.Character.CharacterId, 1));
                    }
                    break;

                // wigs
                case 30:
                    if (!session.Character.IsVehicled)
                    {
                        ItemInstance wig = session.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.Hat, InventoryType.Wear);
                        if (wig != null)
                        {
                            wig.Design = (byte)ServerManager.RandomNumber(0, 15);
                            session.SendPacket(session.Character.GenerateEq());
                            session.SendPacket(session.Character.GenerateEquipment());
                            session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateIn());
                            session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateGidx());
                            session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                        }
                        else
                        {
                            session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NO_WIG"), 0));
                        }
                    }
                    break;

                case 31:
                    if (!session.Character.IsVehicled && session.Character.HairStyle == HairStyleType.Hair7)
                    {
                        session.Character.HairStyle = HairStyleType.Hair8;
                        session.SendPacket(session.Character.GenerateEq());
                        session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateIn());
                        session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateGidx());
                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                        // idk how it works yet but seems like all characters with this hairstyle have DarkPurple hair
                        //session.Character.HairColor = HairColorType.DarkPurple;
                    }
                    break;

                case 300:
                    if (session.Character.Group != null && session.Character.Group.GroupType != GroupType.Group && session.Character.Group.IsLeader(session) && session.CurrentMapInstance.Portals.Any(s => s.Type == (short)PortalType.Raid))
                    {
                        int delay = 0;
                        foreach (ClientSession sess in session.Character.Group.Characters.GetAllItems())
                        {
                            Observable.Timer(TimeSpan.FromMilliseconds(delay)).Subscribe(o =>
                            {
                                if (sess?.Character != null && session.CurrentMapInstance != null &&
                                    session.Character != null)
                                {
                                    if (sess.CurrentMapInstance.MapInstanceType == MapInstanceType.BaseMapInstance)
                                    {
                                        ServerManager.Instance.ChangeMapInstance(sess.Character.CharacterId,
                                            session.CurrentMapInstance.MapInstanceId, session.Character.PositionX,
                                            session.Character.PositionY);
                                    }
                                    else
                                    {
                                        session.SendPacket(session.Character.GenerateSay($"{sess.Character.Name} can't be ported.", 10));
                                    }
                                }
                            });
                            delay += 100;
                        }
                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                    }
                    break;

                default:
                    if (!OpenBoxItem(session, inv))
                    {
                        Logger.Warn(string.Format(Language.Instance.GetMessageFromKey("NO_HANDLER_ITEM"), GetType(),
                            VNum, Effect, EffectValue));
                    }

                    break;
            }
        }

        #endregion
    }
}