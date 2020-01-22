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

using System.Collections.Generic;
using OpenNos.Data;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Npc
{
    public class NpcMonster : NpcMonsterDTO
    {
        #region Instantiation

        public NpcMonster()
        {
        }

        public NpcMonster(NpcMonsterDTO input)
        {
            AmountRequired = input.AmountRequired;
            AttackClass = input.AttackClass;
            AttackUpgrade = input.AttackUpgrade;
            BasicArea = input.BasicArea;
            BasicCooldown = input.BasicCooldown;
            BasicRange = input.BasicRange;
            BasicSkill = input.BasicSkill;
            Catch = input.Catch;
            CloseDefence = input.CloseDefence;
            Concentrate = input.Concentrate;
            CriticalChance = input.CriticalChance;
            CriticalRate = input.CriticalRate;
            DamageMaximum = input.DamageMaximum;
            DamageMinimum = input.DamageMinimum;
            DarkResistance = input.DarkResistance;
            DefenceDodge = input.DefenceDodge;
            DefenceUpgrade = input.DefenceUpgrade;
            DistanceDefence = input.DistanceDefence;
            DistanceDefenceDodge = input.DistanceDefenceDodge;
            Element = input.Element;
            ElementRate = input.ElementRate;
            FireResistance = input.FireResistance;
            HeroLevel = input.HeroLevel;
            HeroXp = input.HeroXp;
            IsHostile = input.IsHostile;
            JobXP = input.JobXP;
            Level = input.Level;
            LightResistance = input.LightResistance;
            MagicDefence = input.MagicDefence;
            MaxHP = input.MaxHP;
            MaxMP = input.MaxMP;
            MonsterType = input.MonsterType;
            Name = input.Name;
            NoAggresiveIcon = input.NoAggresiveIcon;
            NoticeRange = input.NoticeRange;
            NpcMonsterVNum = input.NpcMonsterVNum;
            Race = input.Race;
            RaceType = input.RaceType;
            RespawnTime = input.RespawnTime;
            Speed = input.Speed;
            VNumRequired = input.VNumRequired;
            WaterResistance = input.WaterResistance;
            XP = input.XP;
            SpecialistType = input.SpecialistType;
        }

        #endregion

        #region Properties

        public List<BCard> BCards { get; set; }

        public List<DropDTO> Drops { get; set; }

        public List<NpcMonsterSkill> Skills { get; set; }

        #endregion

        #region Methods

        public string GenerateEInfo() => $"e_info 10 {NpcMonsterVNum} {Level} {Element} {AttackClass} {ElementRate} {AttackUpgrade} {DamageMinimum} {DamageMaximum} {Concentrate} {CriticalChance} {CriticalRate} {DefenceUpgrade} {CloseDefence} {DefenceDodge} {DistanceDefence} {DistanceDefenceDodge} {MagicDefence} {FireResistance} {WaterResistance} {LightResistance} {DarkResistance} {MaxHP} {MaxMP} -1 {Name.Replace(' ', '^')}";

        // ReSharper disable once UnusedMember.Global
        public float GetRes(int skillelement)
        {
            switch (skillelement)
            {
                case 0:
                    return FireResistance / 100F;

                case 1:
                    return WaterResistance / 100F;

                case 2:
                    return LightResistance / 100F;

                case 3:
                    return DarkResistance / 100F;

                default:
                    return 0f;
            }
        }

        /// <summary>
        /// Intializes the GameObject, will be injected by AutoMapper after Entity -&gt; GO mapping
        /// </summary>
        public void Initialize()
        {
            Drops = ServerManager.Instance.GetDropsByMonsterVNum(NpcMonsterVNum);
            Skills = ServerManager.Instance.GetNpcMonsterSkillsByMonsterVNum(NpcMonsterVNum);
        }

        #endregion
    }
}