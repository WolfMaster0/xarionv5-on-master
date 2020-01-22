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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenNos.DAL.EF
{
    public sealed class Skill
    {
        #region Instantiation

        public Skill()
        {
            CharacterSkill = new HashSet<CharacterSkill>();
            Combo = new HashSet<Combo>();
            NpcMonsterSkill = new HashSet<NpcMonsterSkill>();
            ShopSkill = new HashSet<ShopSkill>();
            BCards = new HashSet<BCard>();
        }

        #endregion

        #region Properties

        public short AttackAnimation { get; set; }

        public ICollection<BCard> BCards { get; }

        public short CastAnimation { get; set; }

        public short CastEffect { get; set; }

        public short CastId { get; set; }

        public short CastTime { get; set; }

        public ICollection<CharacterSkill> CharacterSkill { get; }

        public byte Class { get; set; }

        public ICollection<Combo> Combo { get; }

        public short Cooldown { get; set; }

        public byte CPCost { get; set; }

        public short Duration { get; set; }

        public short Effect { get; set; }

        public byte Element { get; set; }

        public byte HitType { get; set; }

        public short ItemVNum { get; set; }

        public byte Level { get; set; }

        public byte LevelMinimum { get; set; }

        public byte MinimumAdventurerLevel { get; set; }

        public byte MinimumArcherLevel { get; set; }

        public byte MinimumMagicianLevel { get; set; }

        public byte MinimumSwordmanLevel { get; set; }

        public short MpCost { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        public ICollection<NpcMonsterSkill> NpcMonsterSkill { get; }

        public int Price { get; set; }

        public byte Range { get; set; }

        public ICollection<ShopSkill> ShopSkill { get; }

        public byte SkillType { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short SkillVNum { get; set; }

        public byte TargetRange { get; set; }

        public byte TargetType { get; set; }

        public byte Type { get; set; }

        public short UpgradeSkill { get; set; }

        public short UpgradeType { get; set; }

        #endregion
    }
}