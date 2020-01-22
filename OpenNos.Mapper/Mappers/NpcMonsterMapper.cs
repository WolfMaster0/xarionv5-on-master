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
using OpenNos.DAL.EF;
using OpenNos.Data;

namespace OpenNos.Mapper.Mappers
{
    public static class NpcMonsterMapper
    {
        #region Methods

        public static bool ToNpcMonster(NpcMonsterDTO input, NpcMonster output)
        {
            if (input == null)
            {
                return false;
            }
            output.AmountRequired = input.AmountRequired;
            output.AttackClass = input.AttackClass;
            output.AttackUpgrade = input.AttackUpgrade;
            output.BasicArea = input.BasicArea;
            output.BasicCooldown = input.BasicCooldown;
            output.BasicRange = input.BasicRange;
            output.BasicSkill = input.BasicSkill;
            output.Catch = input.Catch;
            output.CloseDefence = input.CloseDefence;
            output.Concentrate = input.Concentrate;
            output.CriticalChance = input.CriticalChance;
            output.CriticalRate = input.CriticalRate;
            output.DamageMaximum = input.DamageMaximum;
            output.DamageMinimum = input.DamageMinimum;
            output.DarkResistance = input.DarkResistance;
            output.DefenceDodge = input.DefenceDodge;
            output.DefenceUpgrade = input.DefenceUpgrade;
            output.DistanceDefence = input.DistanceDefence;
            output.DistanceDefenceDodge = input.DistanceDefenceDodge;
            output.Element = input.Element;
            output.ElementRate = input.ElementRate;
            output.FireResistance = input.FireResistance;
            output.HeroLevel = input.HeroLevel;
            output.HeroXP = input.HeroXp;
            output.IsHostile = input.IsHostile;
            output.JobXP = input.JobXP;
            output.Level = input.Level;
            output.LightResistance = input.LightResistance;
            output.MagicDefence = input.MagicDefence;
            output.MaxHP = input.MaxHP;
            output.MaxMP = input.MaxMP;
            output.MonsterType = input.MonsterType;
            output.Name = input.Name;
            output.NoAggresiveIcon = input.NoAggresiveIcon;
            output.NoticeRange = input.NoticeRange;
            output.NpcMonsterVNum = input.NpcMonsterVNum;
            output.Race = input.Race;
            output.RaceType = input.RaceType;
            output.RespawnTime = input.RespawnTime;
            output.Speed = input.Speed;
            output.VNumRequired = input.VNumRequired;
            output.WaterResistance = input.WaterResistance;
            output.XP = input.XP;
            output.SpecialistType = input.SpecialistType;
            return true;
        }

        public static bool ToNpcMonsterDTO(NpcMonster input, NpcMonsterDTO output)
        {
            if (input == null)
            {
                return false;
            }
            output.AmountRequired = input.AmountRequired;
            output.AttackClass = input.AttackClass;
            output.AttackUpgrade = input.AttackUpgrade;
            output.BasicArea = input.BasicArea;
            output.BasicCooldown = input.BasicCooldown;
            output.BasicRange = input.BasicRange;
            output.BasicSkill = input.BasicSkill;
            output.Catch = input.Catch;
            output.CloseDefence = input.CloseDefence;
            output.Concentrate = input.Concentrate;
            output.CriticalChance = input.CriticalChance;
            output.CriticalRate = input.CriticalRate;
            output.DamageMaximum = input.DamageMaximum;
            output.DamageMinimum = input.DamageMinimum;
            output.DarkResistance = input.DarkResistance;
            output.DefenceDodge = input.DefenceDodge;
            output.DefenceUpgrade = input.DefenceUpgrade;
            output.DistanceDefence = input.DistanceDefence;
            output.DistanceDefenceDodge = input.DistanceDefenceDodge;
            output.Element = input.Element;
            output.ElementRate = input.ElementRate;
            output.FireResistance = input.FireResistance;
            output.HeroLevel = input.HeroLevel;
            output.HeroXp = input.HeroXP;
            output.IsHostile = input.IsHostile;
            output.JobXP = input.JobXP;
            output.Level = input.Level;
            output.LightResistance = input.LightResistance;
            output.MagicDefence = input.MagicDefence;
            output.MaxHP = input.MaxHP;
            output.MaxMP = input.MaxMP;
            output.MonsterType = input.MonsterType;
            output.Name = input.Name;
            output.NoAggresiveIcon = input.NoAggresiveIcon;
            output.NoticeRange = input.NoticeRange;
            output.NpcMonsterVNum = input.NpcMonsterVNum;
            output.Race = input.Race;
            output.RaceType = input.RaceType;
            output.RespawnTime = input.RespawnTime;
            output.Speed = input.Speed;
            output.VNumRequired = input.VNumRequired;
            output.WaterResistance = input.WaterResistance;
            output.XP = input.XP;
            output.SpecialistType = input.SpecialistType;
            return true;
        }

        #endregion
    }
}