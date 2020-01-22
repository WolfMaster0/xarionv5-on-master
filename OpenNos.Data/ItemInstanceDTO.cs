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
using OpenNos.Domain;
using System;
using OpenNos.Data.Base;

namespace OpenNos.Data
{
    [Serializable]
    public class ItemInstanceDTO : SynchronizableBaseDTO
    {
        #region Properties
        public short HoldingVNum { get; set; }

        public short SlDamage { get; set; }

        public short SlDefence { get; set; }

        public short SlElement { get; set; }

        public short SlHP { get; set; }

        public byte SpDamage { get; set; }

        public byte SpDark { get; set; }

        public byte SpDefence { get; set; }

        public byte SpElement { get; set; }

        public byte SpFire { get; set; }

        public byte SpHP { get; set; }

        public byte SpLevel { get; set; }

        public byte SpLight { get; set; }

        public byte SpStoneUpgrade { get; set; }

        public byte SpWater { get; set; }

        public byte Ammo { get; set; }

        public byte Cellon { get; set; }

        public short CloseDefence { get; set; }

        public short Concentrate { get; set; }

        public short CriticalDodge { get; set; }

        public byte CriticalLuckRate { get; set; }

        public short CriticalRate { get; set; }

        public short DamageMaximum { get; set; }

        public short DamageMinimum { get; set; }

        public byte DarkElement { get; set; }

        public short DarkResistance { get; set; }

        public short DefenceDodge { get; set; }

        public short DistanceDefence { get; set; }

        public short DistanceDefenceDodge { get; set; }

        public short ElementRate { get; set; }

        public Guid EquipmentSerialId { get; set; }

        public byte FireElement { get; set; }

        public short FireResistance { get; set; }

        public short HitRate { get; set; }

        public short HP { get; set; }

        public bool IsEmpty { get; set; }

        public bool IsFixed { get; set; }

        public byte LightElement { get; set; }

        public short LightResistance { get; set; }

        public short MagicDefence { get; set; }

        public short MaxElementRate { get; set; }

        public short MP { get; set; }

        public byte WaterElement { get; set; }

        public short WaterResistance { get; set; }

        public long XP { get; set; }

        public byte Amount { get; set; }

        public long? BoundCharacterId { get; set; }

        public long CharacterId { get; set; }

        public short Design { get; set; }

        public int DurabilityPoint { get; set; }

        public DateTime? ItemDeleteTime { get; set; }

        public short ItemVNum { get; set; }

        public sbyte Rare { get; set; }

        public short Slot { get; set; }

        public InventoryType Type { get; set; }

        public byte Upgrade { get; set; }

        public short ShellRarity { get; set; }

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