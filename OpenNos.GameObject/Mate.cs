/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using OpenNos.Core.Threading;
using OpenNos.GameObject.Npc;
using OpenNos.PathFinder;
using static OpenNos.Domain.BCardType;

namespace OpenNos.GameObject
{
    public class Mate : MateDTO
    {
        #region Members

        private NpcMonster _monster;

        private Character _owner;

        public readonly object PveLockObject;

        #endregion

        #region Instantiation

        public Mate()
        {
            PveLockObject = new object();
            Buff = new ThreadSafeSortedList<short, Buff>();
            GenerateMateTransportId();
            LastLoyaltyEffect = DateTime.UtcNow;
            IsAlive = true;
        }

        public Mate(MateDTO input) : this()
        {
            Attack = input.Attack;
            CanPickUp = input.CanPickUp;
            CharacterId = input.CharacterId;
            Defence = input.Defence;
            Direction = input.Direction;
            Experience = input.Experience;
            Hp = input.Hp;
            IsSummonable = input.IsSummonable;
            IsTeamMember = input.IsTeamMember;
            Level = input.Level;
            Loyalty = input.Loyalty;
            MapX = input.MapX;
            MapY = input.MapY;
            MateId = input.MateId;
            MateType = input.MateType;
            Mp = input.Mp;
            Name = input.Name;
            NpcMonsterVNum = input.NpcMonsterVNum;
            Skin = input.Skin;
            MateSlot = input.MateSlot;
            PartnerSlot = input.PartnerSlot;
        }

        public Mate(Character owner, NpcMonster npcMonster, byte level, MateType mateType) : this()
        {
            NpcMonsterVNum = npcMonster.NpcMonsterVNum;
            Monster = npcMonster;
            Level = level;
            Hp = MaxHp;
            Mp = MaxMp;
            Name = npcMonster.Name;
            MateType = mateType;
            Loyalty = 1000;
            PositionY = MapX = (short)(owner.PositionY + 1);
            PositionX = MapY = (short)(owner.PositionX + 1);
            Direction = 2;
            CharacterId = owner.CharacterId;
            Owner = owner;
        }

        #endregion

        #region Properties

        public bool NoAttack { get; private set; }

        public bool NoMove { get; private set; }

        public double LastSp { get; set; }

        public int SpCooldown { get; set; }

        public DateTime LastLoyalty { get; set; }

        public DateTime LastLoyaltyEffect { get; set; }

        public DateTime LastHealth { get; set; }

        public DateTime LastDeath { get; set; }

        public DateTime LastSkillUse { get; set; }

        public DateTime LastDefense { get; set; }

        public bool IsAlive { get; set; }

        public IDisposable Life { get; private set; }

        public IDisposable Death { get; set; }

        public ItemInstance ArmorInstance { get; set; }

        public ItemInstance BootsInstance { get; set; }

        public ThreadSafeSortedList<short, Buff> Buff { get; }

        public ItemInstance GlovesInstance { get; set; }

        public bool IsSitting { get; set; }

        public bool IsUsingSp { get; set; }

        public DateTime LastSpeedChange { get; set; }

        public int MagicalDefense => MagicalDefenseLoad();

        public int MateTransportId { get; private set; }

        public int MaxHp => HpLoad();

        public int MaxMp => MpLoad();

        public int MeleeDefense => MeleeDefenseLoad();

        public int MeleeDefenseDodge => MeleeDefenseDodgeLoad();

        public NpcMonster Monster
        {
            get => _monster ?? ServerManager.GetNpcMonster(NpcMonsterVNum);

            set => _monster = value;
        }

        public Character Owner
        {
            get => _owner ?? ServerManager.Instance.GetSessionByCharacterId(CharacterId)?.Character;
            set => _owner = value;
        }

        public short PositionX { get; set; }

        public short PositionY { get; set; }

        public int RangeDefense => RangeDefenseLoad();

        public int RangeDefenseDodge => RangeDefenseDodgeLoad();

        public Skill[] Skills { get; set; }

        public PartnerSkillsDTO PartnerSkills => ServerManager.Instance.PartnerSkills.FirstOrDefault(s => s.PartnerVnum == SpInstance?.ItemVNum);

        public byte Speed
        {
            get
            {
                var bonusSpeed = (byte)(GetBuff(CardType.Move, (byte)AdditionalTypes.Move.SetMovementNegated)[0]
                                          + GetBuff(CardType.Move,
                                              (byte)AdditionalTypes.Move.MovementSpeedIncreased)[0]
                                          + GetBuff(CardType.Move,
                                              (byte)AdditionalTypes.Move.MovementSpeedDecreased)[0]);
                bonusSpeed += Owner != null && Owner.Buff.ContainsKey(122) ? (byte)(2) : (byte)(0);

                if (Monster.Speed + bonusSpeed > 59)
                {
                    return 59;
                }

                return (byte)(Monster.Speed + bonusSpeed + 2);
            }
            set
            {
                LastSpeedChange = DateTime.UtcNow;
                Monster.Speed = value > 59 ? (byte)59 : value;
            }
        }

        public ItemInstance SpInstance { get; set; }

        public ItemInstance WeaponInstance { get; set; }

        public DateTime LastMonsterAggro { get; set; }

        public Node[][] BrushFireJagged { get; set; }

        public int DamageMinimum => DamageMinimumData();

        public int DamageMaximum => DamageMaximumData();

        public int Concentrate => ConcentrateData();

        #endregion

        #region Methods

        #region Inventory

        public List<ItemInstance> GetInventory()
        {
            if (PartnerSlot >= 0 && PartnerSlot <= 11)
            {
                return Owner.Inventory.Where(s => s.Type == (InventoryType)(PartnerSlot + 13));
            }

            return new List<ItemInstance>();
        }

        public void LoadInventory()
        {
            List<ItemInstance> inv = GetInventory();
            if (inv.Count == 0)
            {
                return;
            }

            WeaponInstance = inv.Find(s => s.Item.EquipmentSlot == EquipmentType.MainWeapon);
            ArmorInstance = inv.Find(s => s.Item.EquipmentSlot == EquipmentType.Armor);
            GlovesInstance = inv.Find(s => s.Item.EquipmentSlot == EquipmentType.Gloves);
            BootsInstance = inv.Find(s => s.Item.EquipmentSlot == EquipmentType.Boots);
            SpInstance = inv.Find(s => s.Item.EquipmentSlot == EquipmentType.Sp);
        }

        #endregion

        #region Packets

        public string GeneratePsd(int cooldown = 0) => $"psd {cooldown}";

        public string GenerateBf(short cardId, int remainingTime, short level) => $"bf 2 {MateTransportId} 0.{cardId}.{remainingTime} {level}";

        public string GenerateCtl() => $"ctl 2 {MateTransportId} 3";

        public string GeneratePski()
        {
            if (SpInstance != null && IsUsingSp && PartnerSkills != null)
            {
                return $"pski {(SpInstance.FirstPartnerSkill > 0 ? $"{SpInstance.FirstPartnerSkill}" : "")} " +
                            $"{(SpInstance.SecondPartnerSkill > 0 ? $"{SpInstance.SecondPartnerSkill}" : "")} " +
                            $"{(SpInstance.ThirdPartnerSkill > 0 ? $"{SpInstance.ThirdPartnerSkill}" : "")}";
            }

            return "dpski";
        }

        public string GeneratePst() => $"pst 2 {MateTransportId} {(int)MateType} {(int)(Hp / (float)MaxHp * 100)} {(int)(Mp / (float)MaxMp * 100)} {Hp} {Mp} 0 0 0";

        public string GenerateCMode(short morphId) => $"c_mode 2 {MateTransportId} {morphId} 0 0";

        public string GenerateCond() => $"cond 2 {MateTransportId} 0 {(Loyalty == 0 ? 1 : 0)} {Speed}";

        public string GenerateEInfo() =>
            "e_info " +
            "10 " +
            $"{NpcMonsterVNum} " +
            $"{Level} " +
            $"{Monster.Element} " +
            $"{Monster.AttackClass} " +
            $"{Monster.ElementRate} " +
            $"{(MateType == MateType.Partner ? WeaponInstance?.Upgrade : Attack)} " +
            $"{DamageMinimum} " +
            $"{DamageMaximum} " +
            $"{Concentrate} " +
            $"{Monster.CriticalChance} " +
            $"{Monster.CriticalRate} " +
            $"{(MateType == MateType.Partner ? ArmorInstance?.Upgrade : Defence)} " +
            $"{MeleeDefense} " +
            $"{MeleeDefenseDodge} " +
            $"{RangeDefense} " +
            $"{RangeDefenseDodge} " +
            $"{MagicalDefense} " +
            $"{Monster.FireResistance + (GlovesInstance?.FireResistance + BootsInstance?.FireResistance)} " +
            $"{Monster.WaterResistance + (GlovesInstance?.WaterResistance + BootsInstance?.WaterResistance)} " +
            $"{Monster.LightResistance + (GlovesInstance?.LightResistance + BootsInstance?.LightResistance)} " +
            $"{Monster.DarkResistance + (GlovesInstance?.DarkResistance + BootsInstance?.DarkResistance)} " +
            $"{MaxHp} " +
            $"{MaxMp} " +
            "-1 " +
            $"{Name.Replace(' ', '^')}";

        public string GenerateIn(bool foe = false, bool isAct4 = false)
        {
            if (Owner.Invisible || Owner.Invisible || !IsAlive)
            {
                return string.Empty;
            }

            var name = Name.Replace(' ', '^');
            if (foe)
            {
                name = "!§$%&/()=?*+~#";
            }

            var faction = 0;
            if (isAct4)
            {
                faction = (byte)Owner.Faction + 2;
            }

            if (SpInstance != null && IsUsingSp)
            {
                var spName = ServerManager.Instance.PartnerSkills.FirstOrDefault(s => s.PartnerVnum == SpInstance?.ItemVNum)?.IdentifierKey;
                return "in " +
                    "2 " +
                    $"{NpcMonsterVNum} " +
                    $"{MateTransportId} " +
                    $"{(IsTeamMember ? PositionX : MapX)} " +
                    $"{(IsTeamMember ? PositionY : MapY)} " +
                    $"{Direction} " +
                    $"{(int)(Hp / (float)MaxHp * 100)} " +
                    $"{(int)(Mp / (float)MaxMp * 100)} " +
                    "0 " +
                    $"{faction} " +
                    "3 " +
                    $"{CharacterId} " +
                    "1 " +
                    "0 " +
                    $"{(IsUsingSp && SpInstance != null ? SpInstance.Item.Morph : (Skin != 0 ? Skin : -1))} " +
                    $"{(spName != null ? Language.Instance.GetMessageFromKey(spName)?.Replace(' ', '^') : name)} " +
                    "1 " +
                    "1 " +
                    "1 " +
                    $"{SpInstance.FirstPartnerSkill} " +
                    $"{SpInstance.SecondPartnerSkill} " +
                    $"{SpInstance.ThirdPartnerSkill} " +
                    $"{(SpInstance.FirstPartnerSkillRank == PartnerSkillRankType.SRank ? "4237" : "0")} " +
                    $"{(SpInstance.SecondPartnerSkillRank == PartnerSkillRankType.SRank ? "4238" : "0")} " +
                    $"{(SpInstance.ThirdPartnerSkillRank == PartnerSkillRankType.SRank ? "4239" : "0")} 0";
            }

            return
                "in " +
                "2 " +
                $"{NpcMonsterVNum} " +
                $"{MateTransportId} " +
                $"{(IsTeamMember ? PositionX : MapX)} " +
                $"{(IsTeamMember ? PositionY : MapY)} " +
                $"{Direction} " +
                $"{(int)(Hp / (float)MaxHp * 100)} " +
                $"{(int)(Mp / (float)MaxMp * 100)} " +
                "0 " +
                $"{faction} " +
                "3 " +
                $"{CharacterId} " +
                "1 " +
                "0 " +
                $"{(IsUsingSp && SpInstance != null ? SpInstance.Item.Morph : (Skin != 0 ? Skin : -1))} " +
                $"{name} " +
                $"{(MateType == MateType.Pet ? 2 : 1)} " +
                "1 " +
                "0 " +
                "0 " +
                "0 " +
                "0 " +
                "0 " +
                "0 " +
                "0 " +
                "0";
        }

        public string GenerateOut() => $"out 2 {MateTransportId}";

        public string GenerateRest()
        {
            IsSitting = !IsSitting;
            return $"rest 2 {MateTransportId} {(IsSitting ? 1 : 0)}";
        }

        public string GenerateScPacket()
        {
            var xp = XpLoad();
            if (xp > int.MaxValue)
            {
                xp = (int)(xp / 100);
            }

            double currentXp = Experience;

            if (currentXp > int.MaxValue)
            {
                currentXp = (int)(currentXp / 100);
            }

            var partnerSkills = ServerManager.Instance.PartnerSkills.FirstOrDefault(s => s.PartnerVnum == SpInstance?.ItemVNum);
            switch (MateType)
            {
                case MateType.Partner:
                    return
                        "sc_n " +
                        $"{PartnerSlot} " +
                        $"{NpcMonsterVNum} " +
                        $"{MateTransportId} " +
                        $"{Level} " +
                        $"{Loyalty} " +
                        $"{currentXp} " +
                        $"{(WeaponInstance != null ? $"{WeaponInstance.ItemVNum}.{WeaponInstance.Rare}.{WeaponInstance.Upgrade}" : "-1")} " +
                        $"{(ArmorInstance != null ? $"{ArmorInstance.ItemVNum}.{ArmorInstance.Rare}.{ArmorInstance.Upgrade}" : "-1")} " +
                        $"{(GlovesInstance != null ? $"{GlovesInstance.ItemVNum}.0.0" : "-1")} " +
                        $"{(BootsInstance != null ? $"{BootsInstance.ItemVNum}.0.0" : "-1")} " +
                        "0 " +
                        "0 " +
                        "1 " +
                        $"{WeaponInstance?.Upgrade ?? 0} " +
                        $"{DamageMinimum + (WeaponInstance?.BaseMinDamage ?? 0)} " +
                        $"{DamageMaximum + (WeaponInstance?.BaseMaxDamage ?? 0)} " +
                        $"{Concentrate + (WeaponInstance?.BaseHitRate ?? 0)} " +
                        $"{Monster.CriticalChance + (WeaponInstance?.CriticalLuckRate ?? 0)} " +
                        $"{Monster.CriticalRate + (WeaponInstance?.CriticalRate ?? 0)} " +
                        $"{ArmorInstance?.Upgrade ?? 0} " +
                        $"{MeleeDefense + MeleeDefense + (ArmorInstance?.BaseCloseDefence ?? 0) + (GlovesInstance?.CloseDefence ?? 0) + (BootsInstance?.CloseDefence ?? 0)} " +
                        $"{MeleeDefenseDodge + MeleeDefenseDodge + (ArmorInstance?.BaseDefenceDodge ?? 0) + (GlovesInstance?.DefenceDodge ?? 0) + (BootsInstance?.DefenceDodge ?? 0)} " +
                        $"{RangeDefense + RangeDefense + (ArmorInstance?.BaseDistanceDefence ?? 0) + (GlovesInstance?.DistanceDefence ?? 0) + (BootsInstance?.DistanceDefence ?? 0)} " +
                        $"{RangeDefenseDodge + RangeDefenseDodge + (ArmorInstance?.BaseDistanceDefenceDodge ?? 0) + (GlovesInstance?.DistanceDefenceDodge ?? 0) + (BootsInstance?.DistanceDefenceDodge ?? 0)} " +
                        $"{MagicalDefense + MagicalDefense + (ArmorInstance?.BaseMagicDefence ?? 0) + (GlovesInstance?.MagicDefence ?? 0) + (BootsInstance?.MagicDefence ?? 0)} " +
                        $"{0 /*SP Element*/} " +
                        $"{Monster.FireResistance + (GlovesInstance?.FireResistance ?? 0) + (BootsInstance?.FireResistance ?? 0)} " +
                        $"{Monster.WaterResistance + (GlovesInstance?.WaterResistance ?? 0) + (BootsInstance?.WaterResistance ?? 0)} " +
                        $"{Monster.LightResistance + (GlovesInstance?.LightResistance ?? 0) + (BootsInstance?.LightResistance ?? 0)} " +
                        $"{Monster.DarkResistance + (GlovesInstance?.DarkResistance ?? 0) + (BootsInstance?.DarkResistance ?? 0)} " +
                        $"{Hp} " +
                        $"{MaxHp} " +
                        $"{Mp} " +
                        $"{MaxMp} " +
                        "0 " +
                        $"{xp} " +
                        $"{(PartnerSkills != null && IsUsingSp ? Language.Instance.GetMessageFromKey(partnerSkills?.IdentifierKey)?.Replace(' ', '^') : Name.Replace(' ', '^'))} " +
                        $"{(IsUsingSp && SpInstance != null ? SpInstance.Item.Morph : Skin != 0 ? Skin : -1)} " +
                        $"{(IsSummonable ? 1 : 0)} " +
                        $"{(SpInstance != null ? $"{SpInstance.ItemVNum}.{SpInstance.Agility}" : "-1")} " +
                        $"{(SpInstance != null ? $"{(SpInstance.FirstPartnerSkill)}.{(byte)SpInstance.FirstPartnerSkillRank}" : "-1")} " +
                        $"{(SpInstance != null ? $"{(SpInstance.SecondPartnerSkill)}.{(byte)SpInstance.SecondPartnerSkillRank}" : "-1")} " +
                        $"{(SpInstance != null ? $"{(SpInstance.ThirdPartnerSkill)}.{(byte)SpInstance.ThirdPartnerSkillRank}" : "-1")}";

                case MateType.Pet:
                    return
                        "sc_p " +
                        $"{MateSlot} " +
                        $"{NpcMonsterVNum} " +
                        $"{MateTransportId} " +
                        $"{Level} " +
                        $"{Loyalty} " +
                        $"{currentXp} " +
                        "0 " +
                        $"{Monster.AttackUpgrade + Attack} " +
                        $"{DamageMinimum} " +
                        $"{DamageMaximum} " +
                        $"{Concentrate} " +
                        $"{Monster.CriticalChance} " +
                        $"{Monster.CriticalRate} " +
                        $"{Defence} " +
                        $"{MeleeDefense} " +
                        $"{MeleeDefenseDodge} " +
                        $"{RangeDefense} " +
                        $"{RangeDefenseDodge} " +
                        $"{MagicalDefense} " +
                        $"{Monster.Element} " +
                        $"{Monster.FireResistance} " +
                        $"{Monster.WaterResistance} " +
                        $"{Monster.LightResistance} " +
                        $"{Monster.DarkResistance} " +
                        $"{Hp} " +
                        $"{MaxHp} " +
                        $"{Mp} " +
                        $"{MaxMp} " +
                        "0 " +
                        $"{xp} " +
                        $"{(CanPickUp ? 1 : 0)} " +
                        $"{Name.Replace(' ', '^')} " +
                        $"{(IsSummonable ? 1 : 0)}";
            }

            return string.Empty;
        }

        public string GenerateStatInfo() =>
            $"st 2 {MateTransportId} {Level} 0 {Hp / MaxHp * 100} {Mp / MaxMp * 100} {Hp} {Mp}{Buff.GetAllItems().Aggregate(string.Empty, (current, buff) => current + $" {buff.Card.CardId}.{buff.Level}")}";

        public string GenPski()
        {
            var skills = string.Empty;
            if (Skills != null)
            {
                foreach (Skill s in Skills)
                {
                    skills += $" {s.SkillVNum}";
                }
            }

            return $"pski{skills}";
        }

        public string GenerateDm(int amount) => $"dm 2 {MateTransportId} {amount} 0";

        public string GenerateRc(int amount) => $"rc 2 {MateTransportId} {amount} 0";

        #endregion

        #region Stats

        private int HealthMpLoad()
        {
            var regen = GetBuff(CardType.Recovery, (byte)AdditionalTypes.Recovery.MPRecoveryIncreased)[0]
                - GetBuff(CardType.Recovery, (byte)AdditionalTypes.Recovery.MPRecoveryDecreased)[0];
            return IsSitting ? regen + 50 :
                (DateTime.UtcNow - LastDefense).TotalSeconds > 4 ? regen + 20 : 0;
        }

        private int HealthHpLoad()
        {
            var regen = GetBuff(CardType.Recovery, (byte)AdditionalTypes.Recovery.HPRecoveryIncreased)[0]
                - GetBuff(CardType.Recovery, (byte)AdditionalTypes.Recovery.HPRecoveryDecreased)[0];
            return IsSitting ? regen + 50 :
                (DateTime.UtcNow - LastDefense).TotalSeconds > 4 ? regen + 20 : 0;
        }

        public void GenerateXp(int mxp)
        {
            if (Level < ServerManager.Instance.Configuration.MaxLevel)
            {
                Experience += Owner != null && !Owner.Buff.ContainsKey(122) ? mxp : (int)(mxp * 1.5D);
                if (Experience >= XpLoad())
                {
                    if (Level + 1 < Owner.Level)
                    {
                        while (Experience >= XpLoad())
                        {

                            Experience -= (long)XpLoad();
                            Level++;
                            Hp = MaxHp;
                            Mp = MaxMp;
                            Owner?.Session.SendPacket(GenerateScPacket());
                            Owner?.Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Npc, MateTransportId, 8), PositionX, PositionY);
                            Owner?.Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Npc, MateTransportId, 198), PositionX, PositionY);
                        }
                    }
                    else
                    {
                        Experience = (long)XpLoad();
                        Owner?.Session.SendPacket(GenerateScPacket());
                    }
                }
            }

            Owner?.Session.SendPacket(GenerateScPacket());
        }

        public int HpLoad()
        {
            var multiplicator = 1.0;
            var hp = 0;

            multiplicator += GetBuff(CardType.BearSpirit, (byte)AdditionalTypes.BearSpirit.IncreaseMaximumHP)[0]
                / 100D;
            multiplicator += GetBuff(CardType.MaxHpmp, (byte)AdditionalTypes.MaxHpmp.IncreasesMaximumHP)[0] / 100D;
            hp += GetBuff(CardType.MaxHpmp, (byte)AdditionalTypes.MaxHpmp.MaximumHPIncreased)[0];
            hp -= GetBuff(CardType.MaxHpmp, (byte)AdditionalTypes.MaxHpmp.MaximumHPDecreased)[0];
            hp += GetBuff(CardType.MaxHpmp, (byte)AdditionalTypes.MaxHpmp.MaximumHpmpIncreased)[0];

            // Monster Bonus HP
            hp += MateHelper.Instance.HpData[Monster.Level];

            return (int)((MateHelper.Instance.HpData[Level] + hp) * multiplicator);
        }


        public int ConcentrateData() => MateHelper.Instance.Concentrate[(byte)(Monster.AttackClass == 2 ? 1 : 0), Level] + (Monster.Concentrate - MateHelper.Instance.Concentrate[(byte)(Monster.AttackClass == 2 ? 1 : 0), Monster.Level]);

        public int DamageMinimumData()
        {
            var bonus = Monster.DamageMinimum <= 0
                ? MateHelper.Instance.MinDamageData[(byte)(Monster.AttackClass == 2 ? 1 : 0), Monster.Level] / 3
                : Monster.DamageMinimum - MateHelper.Instance.MinDamageData[(byte)(Monster.AttackClass == 2 ? 1 : 0), Monster.Level];
            return MateHelper.Instance.MinDamageData[(byte)(Monster.AttackClass == 2 ? 1 : 0), Level] + bonus;
        }

        public int DamageMaximumData()
        {
            var bonus = Monster.DamageMaximum <= 0
                ? MateHelper.Instance.MaxDamageData[(byte)(Monster.AttackClass == 2 ? 1 : 0), Monster.Level] / 3
                : Monster.DamageMaximum - MateHelper.Instance.MaxDamageData[(byte)(Monster.AttackClass == 2 ? 1 : 0), Monster.Level];
            return MateHelper.Instance.MaxDamageData[(byte)(Monster.AttackClass == 2 ? 1 : 0), Level] + bonus;
        }

        public int MeleeDefenseLoad() => MateHelper.Instance.MeleeDefenseData[GetMateType(), Level > 0 ? Level - 1 : 0];

        public int MeleeDefenseDodgeLoad() => MateHelper.Instance.MeleeDefenseDodgeData[GetMateType(), Level > 0 ? Level - 1 : 0];

        public int RangeDefenseLoad() => MateHelper.Instance.RangeDefenseData[GetMateType(), Level > 0 ? Level - 1 : 0];

        public int RangeDefenseDodgeLoad() => MateHelper.Instance.RangeDefenseDodgeData[GetMateType(), Level > 0 ? Level - 1 : 0];

        public int MagicalDefenseLoad() => MateHelper.Instance.MagicDefenseData[GetMateType(), Level > 0 ? Level - 1 : 0];

        public int MpLoad()
        {
            var mp = 0;
            var multiplicator = 1.0;
            multiplicator += GetBuff(CardType.BearSpirit, (byte)AdditionalTypes.BearSpirit.IncreaseMaximumMP)[0]
                / 100D;
            multiplicator += GetBuff(CardType.MaxHpmp, (byte)AdditionalTypes.MaxHpmp.IncreasesMaximumMP)[0] / 100D;
            mp += GetBuff(CardType.MaxHpmp, (byte)AdditionalTypes.MaxHpmp.MaximumMPIncreased)[0];
            mp -= GetBuff(CardType.MaxHpmp, (byte)AdditionalTypes.MaxHpmp.MaximumHPDecreased)[0];
            mp += GetBuff(CardType.MaxHpmp, (byte)AdditionalTypes.MaxHpmp.MaximumHpmpIncreased)[0];

            // Monster Bonus MP
            mp += (Monster.Race == 0
                ? MateHelper.Instance.PrimaryMpData[Monster.Level]
                : MateHelper.Instance.SecondaryMpData[Monster.Level]);

            return (int)(((Monster.Race == 0
                ? MateHelper.Instance.PrimaryMpData[Level]
                : MateHelper.Instance.SecondaryMpData[Level]) + mp) * multiplicator);
        }

        private double XpLoad()
        {
            var index = Level - 1;
            double[] xpData = MateHelper.Instance.XpData;

            if (xpData != null && (index >= 0 && index < xpData.Length))
            {
                return xpData[index];
            }

            return 0;
        }

        #endregion

        #region Battle

        public void GetDamage(int damage, bool isNoKill = false)
        {
            LastDefense = DateTime.UtcNow;

            Hp -= damage;
            Owner.Session.SendPacket(GenerateStatInfo());
            Owner.Session.SendPacket(GeneratePst());

            if (Hp < 0)
            {
                Hp = 0;
                GenerateDeath();
            }

            if (Hp < 1 && isNoKill)
            {
                Hp = 1;
            }
        }

        public void GenerateDeath()
        {
            if (Hp > 0)
            {
                return;
            }

            if (Owner == null)
            {
                return;
            }

            LastDeath = DateTime.UtcNow;
            IsAlive = false;
            Hp = 0;
            Owner.Session.SendPacket(GenerateScPacket());
            Loyalty = (short)(Loyalty - 50 < 0 ? 0 : Loyalty - 50);
            Owner.Session.SendPacket(GenerateScPacket());
            Owner.Session.SendPacket(GenerateCond());

            if (MateType == MateType.Pet ? Owner.IsPetAutoRelive : Owner.IsPartnerAutoRelive)
            {
                if (Owner.Inventory.CountItem(MateType == MateType.Pet ? 2089 : 2329) >= 1)
                {
                    Owner.Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("DOLL_SAVED_MATE"), 0));
                    Owner.Inventory.RemoveItemAmount(MateType == MateType.Pet ? 2089 : 2329);
                    GenerateRevive();
                    return;
                }
                if (Owner.Inventory.CountItem(1012) >= 5)
                {
                    IsAlive = false;
                    LastDeath = DateTime.UtcNow;
                    Owner.Inventory.RemoveItemAmount(1012, 5);
                    Owner.Session.SendPacket(UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("WILL_BE_BACK"), MateType), 0));
                    Owner.MapInstance.Broadcast(GenerateStatInfo());

                    Death = Observable.Timer(TimeSpan.FromMinutes(3)).Subscribe(s =>
                    {
                        GenerateRevive();
                    });

                    return;
                }

                Owner.Session.SendPacket(UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_REQUIERED_ITEM"), ServerManager.GetItem(1012).Name), 0));
                if (MateType == MateType.Pet)
                {
                    Owner.IsPetAutoRelive = false;
                }
                else
                {
                    Owner.IsPartnerAutoRelive = false;
                }
            }

            Owner.Session.SendPacket(
                UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("BACK_TO_MINILAND"), 0));
            BackToMiniland();
        }

        #endregion

        #region Buffs

        public void AddBuff(Buff indicator)
        {
            if (indicator?.Card != null)
            {
                Buff[indicator.Card.CardId] = indicator;
                indicator.RemainingTime = indicator.Card.Duration;
                indicator.Start = DateTime.UtcNow;

                indicator.Card.BCards.ForEach(c => c.ApplyBCards(this));
                Observable.Timer(TimeSpan.FromMilliseconds(indicator.Card.Duration * 100)).Subscribe(o =>
                {
                    RemoveBuff(indicator.Card.CardId);
                    if (indicator.Card.TimeoutBuff != 0
                        && ServerManager.RandomNumber() < indicator.Card.TimeoutBuffChance)
                    {
                        AddBuff(new Buff(indicator.Card.TimeoutBuff, Monster.Level));
                    }
                });
                NoAttack |= indicator.Card.BCards.Any(s =>
                    s.Type == (byte)CardType.SpecialAttack
                    && s.SubType.Equals((byte)AdditionalTypes.SpecialAttack.NoAttack / 10));
                NoMove |= indicator.Card.BCards.Any(s =>
                    s.Type == (byte)CardType.Move
                    && s.SubType.Equals((byte)AdditionalTypes.Move.MovementImpossible / 10));
                Owner?.Session.SendPacket(GenerateBf(indicator.Card.CardId, indicator.RemainingTime, (short)(indicator.Level <= 0 ? (ValueType)Owner?.Level : indicator.Level)));
            }
        }

        public int[] GetBuff(CardType type, byte subtype)
        {
            var value1 = 0;
            var value2 = 0;

            foreach (Buff buff in Buff.Where(s => s?.Card?.BCards != null))
            {
                foreach (BCard entry in buff.Card.BCards.Where(s =>
                    s.Type.Equals((byte)type) && s.SubType.Equals(subtype)
                    && (s.CastType != 1
                        || (s.CastType == 1 &&
                            buff.Start.AddMilliseconds(buff.Card.Delay * 100) <
                            DateTime.UtcNow))))
                {
                    if (entry.IsLevelScaled)
                    {
                        if (entry.IsLevelDivided)
                        {
                            value1 += buff.Level / entry.FirstData;
                        }
                        else
                        {
                            value1 += entry.FirstData * buff.Level;
                        }
                    }
                    else
                    {
                        value1 += entry.FirstData;
                    }

                    value2 += entry.SecondData;
                }
            }

            return new[] { value1, value2 };
        }

        public bool HasBuff(CardType type, byte subType) => Buff.GetAllItems().Where(s => s?.Card?.BCards != null).SelectMany(s => s.Card.BCards).Any(s => s.Type == (byte)type && s.SubType == (subType / 10));

        private void RemoveBuff(short id)
        {
            if (Buff == null || !Buff.ContainsKey(id))
            {
                return;
            }

            Buff buff = Buff[id];

            if (buff != null)
            {
                Buff.Remove(id);

                NoAttack &= !buff.Card.BCards.Any(s => s.Type == (byte)CardType.SpecialAttack
                    && s.SubType == (byte)AdditionalTypes.SpecialAttack.NoAttack / 10);

                NoMove &= !buff.Card.BCards.Any(s => s.Type == (byte)CardType.Move
                    && s.SubType == (byte)AdditionalTypes.Move.MovementImpossible / 10);
                Owner?.Session.SendPacket(GenerateBf(buff.Card.CardId, 0, (short)Owner?.Level));
            }
        }

        #endregion

        #region PathFinding

        public void UpdateBushFire()
        {
            BrushFireJagged = BestFirstSearch.LoadBrushFireJagged(new GridPos { X = PositionX, Y = PositionY },
                Owner.Session.CurrentMapInstance.Map.JaggedGrid);
        }

        /// <summary>
        /// Checks if the current character is in range of the given position
        /// </summary>
        /// <param name="xCoordinate">The x coordinate of the object to check.</param>
        /// <param name="yCoordinate">The y coordinate of the object to check.</param>
        /// <param name="range">The range of the coordinates to be maximal distanced.</param>
        /// <returns>True if the object is in Range, False if not.</returns>
        public bool IsInRange(int xCoordinate, int yCoordinate, int range) =>
            Math.Abs(PositionX - xCoordinate) <= range && Math.Abs(PositionY - yCoordinate) <= range;

        #endregion

        #region Life

        public void StartLife()
        {
            if (Life == null)
            {
                Life = Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(s => { MateLife(); });
            }
        }

        public void MateLife()
        {
            if ((!IsTeamMember || Owner?.MapInstance == Owner?.Miniland && IsTeamMember) && Loyalty < 1000 && LastLoyalty.AddSeconds(20) < DateTime.Now)
            {
                Loyalty = (short)(Loyalty + 100 > 1000 ? 1000 : Loyalty + 100);
                LastLoyalty = DateTime.Now;
            }
            Owner?.Session?.SendPacket(GeneratePst());
            Owner?.Session?.SendPacket(GenerateCond());
            if (LastHealth.AddSeconds(IsSitting ? 1.5 : 2) > DateTime.UtcNow || !IsTeamMember)
            {
                return;
            }

            LastHealth = DateTime.UtcNow;
            if (LastDefense.AddSeconds(4) > DateTime.UtcNow || LastSkillUse.AddSeconds(2) > DateTime.UtcNow || Hp <= 0 || !IsTeamMember)
            {
                return;
            }

            Hp += Hp + HealthHpLoad() < HpLoad() ? HealthHpLoad() : HpLoad() - Hp;
            Mp += Mp + HealthMpLoad() < MpLoad() ? HealthMpLoad() : MpLoad() - Mp;

            if (Loyalty > 0 || LastLoyaltyEffect.AddSeconds(10) > DateTime.UtcNow || !IsTeamMember)
            {
                //Broadcast the broken heart effect when loyalty is too low
                return;
            }

            Owner.MapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Npc, MateTransportId, 5003));
            LastLoyaltyEffect = DateTime.UtcNow;
        }

        public void StopLife()
        {
            Life?.Dispose();
            Life = null;
        }

        public void GenerateRevive()
        {
            if (Owner == null)
            {
                return;
            }

            Owner.MapInstance?.Broadcast(GenerateOut());
            IsAlive = true;
            PositionY = (short)(Owner.PositionY + 1);
            PositionX = (short)(Owner.PositionX + 1);
            Hp = MaxHp / 2;
            Mp = MaxMp / 2;
            Owner.MapInstance?.Broadcast(GenerateIn());
            Owner.Session.SendPacket(GenerateCond());
            Owner.Session.SendPacket(Owner.GeneratePinit());
        }

        public void BackToMiniland()
        {
            if (!IsTeamMember || Owner?.MapInstance == null || Owner?.Session == null)
            {
                return;
            }

            IsTeamMember = false;
            IsAlive = true;
            LeaveTeam();
            Owner?.Session.SendPacket(Owner.GeneratePinit());
            Owner?.MapInstance.Broadcast(GenerateOut());

            if (IsUsingSp)
            {
                RemoveSp();
            }

            if (Owner?.MapInstance == Owner?.Miniland)
            {
                Owner?.Session.SendPacket(GenerateIn());
            }
        }

        #endregion

        #region Team

        public void JoinTeam(bool force = false)
        {
            if (Owner.Mates.Any(s => s.IsTeamMember && s.MateType == MateType) && !force)
            {
                return;
            }

            IsTeamMember = true;
            IsAlive = true;
            StartLife();
            Hp = MaxHp;
            Mp = MaxMp;

            MateHelper.Instance.AddPetBuff(Owner.Session, this);
        }

        public void LeaveTeam()
        {
            IsTeamMember = false;
            MateHelper.Instance.RemovePetBuffs(Owner?.Session, MateType);
            MateHelper.Instance.RemovePartnerBuffs(Owner?.Session, MateType);
            StopLife();
            MapX = ServerManager.MinilandRandomPos().X;
            MapY = ServerManager.MinilandRandomPos().Y;
            IsAlive = true;
            Hp = MaxHp;
            Mp = MaxMp;

            if (SpInstance == null || !IsUsingSp)
            {
                return;
            }

            RemoveSp();

            if (Owner?.MapInstance == Owner?.Miniland)
            {
                Owner?.MapInstance?.Broadcast(GenerateIn());
            }
        }

        #endregion

        #region Misc

        public void InitializeCountdown()
        {
            var currentRunningSeconds = (DateTime.UtcNow - Process.GetCurrentProcess().StartTime.AddHours(-2).AddSeconds(-50)).TotalSeconds;

            LastSp = currentRunningSeconds;
            SpCooldown = 30;
            Owner?.Session.SendPacket(GeneratePsd(SpCooldown));
            Owner?.Session.SendPacketAfter(GeneratePsd(), SpCooldown * 1000);
        }

        public void RemoveSp()
        {
            if (IsUsingSp)
            {
                InitializeCountdown();
            }

            IsUsingSp = false;
            Skills = null;
            Owner.MapInstance.Broadcast(GenerateCMode(-1));
            Owner.Session.SendPacket(GenerateCond());
            Owner.Session.SendPacket(GeneratePski());
            Owner.Session.SendPacket(GenerateScPacket());
            Owner.Session.Character.MapInstance.Broadcast(GenerateOut());
            Owner.Session.Character.MapInstance.Broadcast(GenerateIn());
            Owner.Session.SendPacket(Owner.GeneratePinit());
            MateHelper.Instance.RemovePartnerBuffs(Owner.Session, MateType);
        }

        private byte GetMateType() => 0;

        public void GenerateMateTransportId()
        {
            var nextId = ServerManager.Instance.MateIds.Count > 0 ? ServerManager.Instance.MateIds.Last() + 1 : 2000000;
            ServerManager.Instance.MateIds.Add(nextId);
            MateTransportId = nextId;
        }

        #endregion

        #endregion
    }
}