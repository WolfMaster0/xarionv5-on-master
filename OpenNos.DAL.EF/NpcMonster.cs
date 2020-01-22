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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenNos.DAL.EF
{
    public sealed class NpcMonster
    {
        #region Instantiation

        public NpcMonster()
        {
            Drop = new HashSet<Drop>();
            MapMonster = new HashSet<MapMonster>();
            MapNpc = new HashSet<MapNpc>();
            NpcMonsterSkill = new HashSet<NpcMonsterSkill>();
            Mate = new HashSet<Mate>();
            BCards = new HashSet<BCard>();
            MonsterType = MonsterType.Unknown;
        }

        #endregion

        #region Properties

        public byte AmountRequired { get; set; }

        public byte AttackClass { get; set; }

        public byte AttackUpgrade { get; set; }

        public byte BasicArea { get; set; }

        public short BasicCooldown { get; set; }

        public byte BasicRange { get; set; }

        public short BasicSkill { get; set; }

        public ICollection<BCard> BCards { get; }

        public bool Catch { get; set; }

        public short CloseDefence { get; set; }

        public short Concentrate { get; set; }

        public byte CriticalChance { get; set; }

        public short CriticalRate { get; set; }

        public short DamageMaximum { get; set; }

        public short DamageMinimum { get; set; }

        public short DarkResistance { get; set; }

        public short DefenceDodge { get; set; }

        public byte DefenceUpgrade { get; set; }

        public short DistanceDefence { get; set; }

        public short DistanceDefenceDodge { get; set; }

        public ICollection<Drop> Drop { get; }

        public byte Element { get; set; }

        public short ElementRate { get; set; }

        public short FireResistance { get; set; }

        public byte HeroLevel { get; set; }

        public int HeroXP { get; set; }

        public bool IsHostile { get; set; }

        public int JobXP { get; set; }

        public byte Level { get; set; }

        public short LightResistance { get; set; }

        public short MagicDefence { get; set; }

        public ICollection<MapMonster> MapMonster { get; }

        public ICollection<MapNpc> MapNpc { get; }

        public ICollection<Mate> Mate { get; }

        public int MaxHP { get; set; }

        public int MaxMP { get; set; }

        public MonsterType MonsterType { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        public bool NoAggresiveIcon { get; set; }

        public byte NoticeRange { get; set; }

        public ICollection<NpcMonsterSkill> NpcMonsterSkill { get; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short NpcMonsterVNum { get; set; }

        public byte Race { get; set; }

        public byte RaceType { get; set; }

        public int RespawnTime { get; set; }

        public byte Speed { get; set; }

        public short VNumRequired { get; set; }

        public short WaterResistance { get; set; }

        public int XP { get; set; }

        public PartnerSpecialistType SpecialistType { get; set; }

        #endregion
    }
}