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
using System.ComponentModel.DataAnnotations.Schema;
using OpenNos.DAL.EF.Base;
using OpenNos.Domain;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace OpenNos.DAL.EF
{
    public sealed class ItemInstance : SynchronizableBaseEntity
    {
        #region Instantiation

        public ItemInstance()
        {
            BazaarItem = new HashSet<BazaarItem>();
            MinilandObject = new HashSet<MinilandObject>();
        }

        #endregion

        #region Properties

        public int Amount { get; set; }

        public ICollection<BazaarItem> BazaarItem { get; }

        public long? BazaarItemId { get; set; }

        [ForeignKey(nameof(BoundCharacterId))]
        public Character BoundCharacter { get; set; }

        public long? BoundCharacterId { get; set; }

        public Character Character { get; set; }

        [Index("IX_SlotAndType", 1, IsUnique = false, Order = 0)]
        public long CharacterId { get; set; }

        public short Design { get; set; }

        public int DurabilityPoint { get; set; }

        public Item Item { get; set; }

        public DateTime? ItemDeleteTime { get; set; }

        public short ItemVNum { get; set; }

        public ICollection<MinilandObject> MinilandObject { get; }

        public short Rare { get; set; }

        [Index("IX_SlotAndType", 2, IsUnique = false, Order = 1)]
        public short Slot { get; set; }

        [Index("IX_SlotAndType", 3, IsUnique = false, Order = 2)]
        public InventoryType Type { get; set; }

        public byte Upgrade { get; set; }

        public short? HoldingVNum { get; set; }

        public short? SlDamage { get; set; }

        public short? SlDefence { get; set; }

        public short? SlElement { get; set; }

        public short? SlHP { get; set; }

        public byte? SpDamage { get; set; }

        public byte? SpDark { get; set; }

        public byte? SpDefence { get; set; }

        public byte? SpElement { get; set; }

        public byte? SpFire { get; set; }

        public byte? SpHP { get; set; }

        public byte? SpLevel { get; set; }

        public byte? SpLight { get; set; }

        public byte? SpStoneUpgrade { get; set; }

        public byte? SpWater { get; set; }

        public byte? Ammo { get; set; }

        public byte? Cellon { get; set; }

        public short? CloseDefence { get; set; }

        public short? Concentrate { get; set; }

        public short? CriticalDodge { get; set; }

        public byte? CriticalLuckRate { get; set; }

        public short? CriticalRate { get; set; }

        public short? DamageMaximum { get; set; }

        public short? DamageMinimum { get; set; }

        public byte? DarkElement { get; set; }

        public short? DarkResistance { get; set; }

        public short? DefenceDodge { get; set; }

        public short? DistanceDefence { get; set; }

        public short? DistanceDefenceDodge { get; set; }

        public short? ElementRate { get; set; }

        public Guid? EquipmentSerialId { get; set; }

        public byte? FireElement { get; set; }

        public short? FireResistance { get; set; }

        public short? HitRate { get; set; }

        public short? HP { get; set; }

        public bool? IsEmpty { get; set; }

        public bool? IsFixed { get; set; }

        public byte? LightElement { get; set; }

        public short? LightResistance { get; set; }

        public short? MagicDefence { get; set; }

        public short? MaxElementRate { get; set; }

        public short? MP { get; set; }

        public byte? WaterElement { get; set; }

        public short? WaterResistance { get; set; }

        public long? XP { get; set; }

        public short? ShellRarity { get; set; }

        public string ItemOptions { get; set; }

        public byte Agility { get; set; }

        public short FirstPartnerSkill { get; set; }

        public short SecondPartnerSkill { get; set; }

        public short ThirdPartnerSkill { get; set; }

        public PartnerSkillRankType FirstPartnerSkillRank { get; set; }

        public PartnerSkillRankType SecondPartnerSkillRank { get; set; }

        public PartnerSkillRankType ThirdPartnerSkillRank { get; set; }

        public byte MinimumLevel { get; set; }

        public short BaseMinDamage { get; set; }

        public short BaseMaxDamage { get; set; }

        public short BaseConcentrate { get; set; }

        public short BaseHitRate { get; set; }

        public short BaseDefenceDodge { get; set; }

        public short BaseDistanceDefenceDodge { get; set; }

        public short BaseDistanceDefence { get; set; }

        public short BaseMagicDefence { get; set; }

        public short BaseCloseDefence { get; set; }


        #endregion
    }
}