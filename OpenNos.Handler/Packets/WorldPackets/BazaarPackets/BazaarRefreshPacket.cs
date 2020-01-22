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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.BazaarPackets
{
    [PacketHeader("c_blist")]
    public class BazaarRefreshPacket
    {
        #region Properties

        public int Index { get; set; }

        public string ItemVNumFilter { get; set; }

        public byte LevelFilter { get; set; }

        public byte OrderFilter { get; set; }

        public byte RareFilter { get; set; }

        public byte SubTypeFilter { get; set; }

        public BazaarListType TypeFilter { get; set; }

        public byte UpgradeFilter { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            packet = packet.Replace("  ", " ");
            string[] packetSplit = packet.Split(new[] { ' ' }, 11);
            if (packetSplit.Length < 11)
            {
                return;
            }
            BazaarRefreshPacket packetDefinition = new BazaarRefreshPacket();
            if (int.TryParse(packetSplit[2], out int index)
                && Enum.TryParse(packetSplit[3], out BazaarListType typeFilter)
                && byte.TryParse(packetSplit[4], out byte subTypeFilter)
                && byte.TryParse(packetSplit[5], out byte levelFilter)
                && byte.TryParse(packetSplit[6], out byte rareFilter)
                && byte.TryParse(packetSplit[7], out byte upgradeFilter)
                && byte.TryParse(packetSplit[8], out byte orderFilter)
                && !string.IsNullOrEmpty(packetSplit[10]))
            {
                packetDefinition.Index = index;
                packetDefinition.TypeFilter = typeFilter;
                packetDefinition.SubTypeFilter = subTypeFilter;
                packetDefinition.LevelFilter = levelFilter;
                packetDefinition.RareFilter = rareFilter;
                packetDefinition.UpgradeFilter = upgradeFilter;
                packetDefinition.OrderFilter = orderFilter;
                packetDefinition.ItemVNumFilter = packetSplit[10];
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(BazaarRefreshPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.InBazaarRefreshMode);
            string itembazar = string.Empty;
            List<string> itemssearch = ItemVNumFilter == "0" ? new List<string>() : ItemVNumFilter.Split(' ').ToList();
            List<BazaarItemLink> bzlist = new List<BazaarItemLink>();
            BazaarItemLink[] billist = new BazaarItemLink[ServerManager.Instance.BazaarList.Count + 20];
            ServerManager.Instance.BazaarList.CopyTo(billist);
            try
            {
                foreach (BazaarItemLink bz in billist)
                {
                    if (bz?.Item == null)
                    {
                        continue;
                    }

                    switch (TypeFilter)
                    {
                        case BazaarListType.Weapon:
                            if (bz.Item.Item.Type == InventoryType.Equipment && bz.Item.Item.ItemType == ItemType.Weapon && (SubTypeFilter == 0 || ((bz.Item.Item.Class + 1 >> SubTypeFilter) & 1) == 1) && ((LevelFilter == 0 || (LevelFilter == 11 && bz.Item.Item.IsHeroic) || (bz.Item.Item.LevelMinimum < (LevelFilter * 10) + 1 && bz.Item.Item.LevelMinimum >= (LevelFilter * 10) - 9)) && ((RareFilter == 0 || RareFilter == bz.Item.Rare + 1) && (UpgradeFilter == 0 || UpgradeFilter == bz.Item.Upgrade + 1))))
                            {
                                bzlist.Add(bz);
                            }
                            break;

                        case BazaarListType.Armor:
                            if (bz.Item.Item.Type == InventoryType.Equipment && bz.Item.Item.ItemType == ItemType.Armor && (SubTypeFilter == 0 || ((bz.Item.Item.Class + 1 >> SubTypeFilter) & 1) == 1) && ((LevelFilter == 0 || (LevelFilter == 11 && bz.Item.Item.IsHeroic) || (bz.Item.Item.LevelMinimum < (LevelFilter * 10) + 1 && bz.Item.Item.LevelMinimum >= (LevelFilter * 10) - 9)) && ((RareFilter == 0 || RareFilter == bz.Item.Rare + 1) && (UpgradeFilter == 0 || UpgradeFilter == bz.Item.Upgrade + 1))))
                            {
                                bzlist.Add(bz);
                            }
                            break;

                        case BazaarListType.Equipment:
                            if (bz.Item.Item.Type == InventoryType.Equipment && bz.Item.Item.ItemType == ItemType.Fashion && ((SubTypeFilter == 0 || (SubTypeFilter == 2 && bz.Item.Item.EquipmentSlot == EquipmentType.Mask) || ((SubTypeFilter == 1 && bz.Item.Item.EquipmentSlot == EquipmentType.Hat) || (SubTypeFilter == 6 && bz.Item.Item.EquipmentSlot == EquipmentType.CostumeHat) || (SubTypeFilter == 5 && bz.Item.Item.EquipmentSlot == EquipmentType.CostumeSuit) || (SubTypeFilter == 3 && bz.Item.Item.EquipmentSlot == EquipmentType.Gloves) || (SubTypeFilter == 4 && bz.Item.Item.EquipmentSlot == EquipmentType.Boots))) && (LevelFilter == 0 || (LevelFilter == 11 && bz.Item.Item.IsHeroic) || (bz.Item.Item.LevelMinimum < (LevelFilter * 10) + 1 && bz.Item.Item.LevelMinimum >= (LevelFilter * 10) - 9))))
                            {
                                bzlist.Add(bz);
                            }
                            break;

                        case BazaarListType.Jewelery:
                            if (bz.Item.Item.Type == InventoryType.Equipment && bz.Item.Item.ItemType == ItemType.Jewelery && ((SubTypeFilter == 0 || (SubTypeFilter == 2 && bz.Item.Item.EquipmentSlot == EquipmentType.Ring) || (SubTypeFilter == 1 && bz.Item.Item.EquipmentSlot == EquipmentType.Necklace) || (SubTypeFilter == 5 && bz.Item.Item.EquipmentSlot == EquipmentType.Amulet) || (SubTypeFilter == 3 && bz.Item.Item.EquipmentSlot == EquipmentType.Bracelet) || (SubTypeFilter == 4 && (bz.Item.Item.EquipmentSlot == EquipmentType.Fairy || (bz.Item.Item.ItemType == ItemType.Box && bz.Item.Item.ItemSubType == 5)))) && (LevelFilter == 0 || (LevelFilter == 11 && bz.Item.Item.IsHeroic) || (bz.Item.Item.LevelMinimum < (LevelFilter * 10) + 1 && bz.Item.Item.LevelMinimum >= (LevelFilter * 10) - 9))))
                            {
                                bzlist.Add(bz);
                            }
                            break;

                        case BazaarListType.Specialist:
                            if (bz.Item.Item.Type == InventoryType.Equipment && bz.Item.Item.ItemType == ItemType.Box && bz.Item.Item.ItemSubType == 2)
                            {
                                if (SubTypeFilter == 0 && ((LevelFilter == 0 || (bz.Item.SpLevel < (LevelFilter * 10) + 1 && bz.Item.SpLevel >= (LevelFilter * 10) - 9)) && ((UpgradeFilter == 0 || UpgradeFilter == bz.Item.Upgrade + 1) && (SubTypeFilter == 0 || (SubTypeFilter == 1 && bz.Item.HoldingVNum == 0) || (SubTypeFilter == 2 && bz.Item.HoldingVNum != 0)))))
                                {
                                    bzlist.Add(bz);
                                }
                                else if (bz.Item.HoldingVNum == 0 && (SubTypeFilter == 1 && ((LevelFilter == 0 || (bz.Item.SpLevel < (LevelFilter * 10) + 1 && bz.Item.SpLevel >= (LevelFilter * 10) - 9)) && ((UpgradeFilter == 0 || UpgradeFilter == bz.Item.Upgrade + 1) && (SubTypeFilter == 0 || (SubTypeFilter == 1 && bz.Item.HoldingVNum == 0) || (SubTypeFilter == 2 && bz.Item.HoldingVNum != 0))))))
                                {
                                    bzlist.Add(bz);
                                }
                                else
                                {
                                    Item bazaarItem = ServerManager.GetItem(bz.Item.HoldingVNum);
                                    if (bazaarItem != null
                                    && ((SubTypeFilter == 2 && bazaarItem.Morph == 10)
                                    || (SubTypeFilter == 3 && bazaarItem.Morph == 11)
                                    || (SubTypeFilter == 4 && bazaarItem.Morph == 2)
                                    || (SubTypeFilter == 5 && bazaarItem.Morph == 3)
                                    || (SubTypeFilter == 6 && bazaarItem.Morph == 13)
                                    || (SubTypeFilter == 7 && bazaarItem.Morph == 5)
                                    || (SubTypeFilter == 8 && bazaarItem.Morph == 12)
                                    || (SubTypeFilter == 9 && bazaarItem.Morph == 4)
                                    || (SubTypeFilter == 10 && bazaarItem.Morph == 7)
                                    || (SubTypeFilter == 11 && bazaarItem.Morph == 15)
                                    || (SubTypeFilter == 12 && bazaarItem.Morph == 6)
                                    || (SubTypeFilter == 13 && bazaarItem.Morph == 14)
                                    || (SubTypeFilter == 14 && bazaarItem.Morph == 9)
                                    || (SubTypeFilter == 15 && bazaarItem.Morph == 8)
                                    || (SubTypeFilter == 16 && bazaarItem.Morph == 1)
                                    || (SubTypeFilter == 17 && bazaarItem.Morph == 16)
                                    || (SubTypeFilter == 18 && bazaarItem.Morph == 17)
                                    || ((SubTypeFilter == 19 && bazaarItem.Morph == 18)
                                    || (SubTypeFilter == 20 && bazaarItem.Morph == 19)
                                    || (SubTypeFilter == 21 && bazaarItem.Morph == 20)
                                    || (SubTypeFilter == 22 && bazaarItem.Morph == 21)
                                    || (SubTypeFilter == 23 && bazaarItem.Morph == 22)
                                    || (SubTypeFilter == 24 && bazaarItem.Morph == 23)
                                    || (SubTypeFilter == 25 && bazaarItem.Morph == 24)
                                    || (SubTypeFilter == 26 && bazaarItem.Morph == 25)
                                    || (SubTypeFilter == 27 && bazaarItem.Morph == 26)
                                    || (SubTypeFilter == 28 && bazaarItem.Morph == 27)
                                    || (SubTypeFilter == 29 && bazaarItem.Morph == 28)))
                                    && (LevelFilter == 0 || (bz.Item.SpLevel < (LevelFilter * 10) + 1 && bz.Item.SpLevel >= (LevelFilter * 10) - 9))
                                    && ((UpgradeFilter == 0 || UpgradeFilter == bz.Item.Upgrade + 1)
                                    && (SubTypeFilter == 0 || (SubTypeFilter == 1 && bz.Item.HoldingVNum == 0)
                                    || (SubTypeFilter >= 2 && bz.Item.HoldingVNum != 0))))
                                    {
                                        bzlist.Add(bz);
                                    }
                                }
                            }
                            break;

                        case BazaarListType.Pet:
                            if (bz.Item.Item.Type == InventoryType.Equipment && bz.Item.Item.ItemType == ItemType.Box && bz.Item.Item.ItemSubType == 0 && (LevelFilter == 0 || (bz.Item.SpLevel < (LevelFilter * 10) + 1 && bz.Item.SpLevel >= (LevelFilter * 10) - 9)) && (SubTypeFilter == 0 || (SubTypeFilter == 1 && bz.Item.HoldingVNum == 0) || (SubTypeFilter == 2 && bz.Item.HoldingVNum != 0)))
                            {
                                bzlist.Add(bz);
                            }
                            break;

                        case BazaarListType.Npc:
                            if (bz.Item.Item.Type == InventoryType.Equipment && bz.Item.Item.ItemType == ItemType.Box && bz.Item.Item.ItemSubType == 1 && (LevelFilter == 0 || (bz.Item.SpLevel < (LevelFilter * 10) + 1 && bz.Item.SpLevel >= (LevelFilter * 10) - 9)) && (SubTypeFilter == 0 || (SubTypeFilter == 1 && bz.Item.HoldingVNum == 0) || (SubTypeFilter == 2 && bz.Item.HoldingVNum != 0)))
                            {
                                bzlist.Add(bz);
                            }
                            break;

                        case BazaarListType.Shell:
                            if (bz.Item.Item.Type == InventoryType.Equipment && bz.Item.Item.ItemType == ItemType.Shell && (SubTypeFilter == 0 || SubTypeFilter == bz.Item.Item.ItemSubType + 1) && ((RareFilter == 0 || RareFilter == bz.Item.Rare + 1) && (LevelFilter == 0 || (bz.Item.Upgrade < (LevelFilter * 10) + 1 && bz.Item.Upgrade >= (LevelFilter * 10) - 9))))
                            {
                                bzlist.Add(bz);
                            }
                            break;

                        case BazaarListType.Main:
                            if (bz.Item.Item.Type == InventoryType.Main && (SubTypeFilter == 0 || (SubTypeFilter == 1 && bz.Item.Item.ItemType == ItemType.Main) || (SubTypeFilter == 2 && bz.Item.Item.ItemType == ItemType.Upgrade) || (SubTypeFilter == 3 && bz.Item.Item.ItemType == ItemType.Production) || (SubTypeFilter == 4 && bz.Item.Item.ItemType == ItemType.Special) || (SubTypeFilter == 5 && bz.Item.Item.ItemType == ItemType.Potion) || (SubTypeFilter == 6 && bz.Item.Item.ItemType == ItemType.Event)))
                            {
                                bzlist.Add(bz);
                            }
                            break;

                        case BazaarListType.Usable:
                            if (bz.Item.Item.Type == InventoryType.Etc && (SubTypeFilter == 0 || (SubTypeFilter == 1 && bz.Item.Item.ItemType == ItemType.Food) || ((SubTypeFilter == 2 && bz.Item.Item.ItemType == ItemType.Snack) || (SubTypeFilter == 3 && bz.Item.Item.ItemType == ItemType.Magical) || (SubTypeFilter == 4 && bz.Item.Item.ItemType == ItemType.Part) || (SubTypeFilter == 5 && bz.Item.Item.ItemType == ItemType.Teacher) || (SubTypeFilter == 6 && bz.Item.Item.ItemType == ItemType.Sell))))
                            {
                                bzlist.Add(bz);
                            }
                            break;

                        case BazaarListType.Other:
                            if (bz.Item.Item.Type == InventoryType.Equipment && bz.Item.Item.ItemType == ItemType.Box && !bz.Item.Item.IsHolder)
                            {
                                bzlist.Add(bz);
                            }
                            break;

                        case BazaarListType.Vehicle:
                            if (bz.Item.Item.ItemType == ItemType.Box && bz.Item.Item.ItemSubType == 4 && (SubTypeFilter == 0 || (SubTypeFilter == 1 && bz.Item.HoldingVNum == 0) || (SubTypeFilter == 2 && bz.Item.HoldingVNum != 0)))
                            {
                                bzlist.Add(bz);
                            }
                            break;

                        default:
                            bzlist.Add(bz);
                            break;
                    }
                }

                List<BazaarItemLink> bzlistsearched;

                if (TypeFilter == BazaarListType.Specialist)
                {
                    bzlistsearched = bzlist.Where(s => itemssearch.Contains(s.Item.HoldingVNum.ToString())).ToList();
                }
                else
                {
                    bzlistsearched = bzlist.Where(s => itemssearch.Contains(s.Item.ItemVNum.ToString())).ToList();
                }

                //price up price down quantity up quantity down
                List<BazaarItemLink> definitivelist = itemssearch.Count > 0 ? bzlistsearched : bzlist;
                switch (OrderFilter)
                {
                    case 0:
                        definitivelist = definitivelist.OrderBy(s => s.Item.Item.Name).ThenBy(s => s.BazaarItem.Price).ThenBy(s=>s.BazaarItem.BazaarItemId).ToList();
                        break;

                    case 1:
                        definitivelist = definitivelist.OrderBy(s => s.Item.Item.Name).ThenByDescending(s => s.BazaarItem.Price).ThenBy(s=>s.BazaarItem.BazaarItemId).ToList();
                        break;

                    case 2:
                        definitivelist = definitivelist.OrderBy(s => s.Item.Item.Name).ThenBy(s => s.BazaarItem.Amount).ThenBy(s=>s.BazaarItem.BazaarItemId).ToList();
                        break;

                    case 3:
                        definitivelist = definitivelist.OrderBy(s => s.Item.Item.Name).ThenByDescending(s => s.BazaarItem.Amount).ThenBy(s=>s.BazaarItem.BazaarItemId).ToList();
                        break;

                    default:
                        definitivelist = definitivelist.OrderBy(s => s.Item.Item.Name).ToList();
                        break;
                }
                foreach (BazaarItemLink bzlink in definitivelist.Where(s => (s.BazaarItem.DateStart.AddHours(s.BazaarItem.Duration) - DateTime.UtcNow).TotalMinutes > 0 && s.Item.Amount > 0).Skip(Index * 50).Take(50))
                {
                    long time = (long)(bzlink.BazaarItem.DateStart.AddHours(bzlink.BazaarItem.Duration) - DateTime.UtcNow).TotalMinutes;
                    string info = string.Empty;
                    if (bzlink.Item.Item.Type == InventoryType.Equipment)
                    {
                        info = (bzlink.Item.Item.EquipmentSlot != EquipmentType.Sp ?
                            bzlink.Item?.GenerateEInfo() : bzlink.Item.Item.SpType == 0 && bzlink.Item.Item.ItemSubType == 4 ?
                            bzlink.Item?.GeneratePslInfo() : bzlink.Item?.GenerateSlInfo(callFormBazaar: true)).Replace(' ', '^').Replace("slinfo^", "").Replace("e_info^", "");
                    }
                    itembazar += $"{bzlink.BazaarItem.BazaarItemId}|{bzlink.BazaarItem.SellerId}|{bzlink.OwnerName}|{bzlink.Item.Item.VNum}|{bzlink.Item.Amount}|{(bzlink.BazaarItem.IsPackage ? 1 : 0)}|{bzlink.BazaarItem.Price}|{time}|2|0|{bzlink.Item.Rare}|{bzlink.Item.Upgrade}|{info} ";
                }

                session.SendPacket($"rc_blist {Index} {itembazar}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        #endregion
    }
}