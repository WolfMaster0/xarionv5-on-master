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
using OpenNos.Data;
using System.Collections.Generic;

namespace OpenNos.GameObject
{
    public class Skill : SkillDTO
    {
        #region Instantiation

        public Skill()
        {
            Combos = new List<ComboDTO>();
            BCards = new List<BCard>();
        }

        public Skill(SkillDTO input)
        {
            AttackAnimation = input.AttackAnimation;
            CastAnimation = input.CastAnimation;
            CastEffect = input.CastEffect;
            CastId = input.CastId;
            CastTime = input.CastTime;
            Class = input.Class;
            Cooldown = input.Cooldown;
            CPCost = input.CPCost;
            Duration = input.Duration;
            Effect = input.Effect;
            Element = input.Element;
            HitType = input.HitType;
            ItemVNum = input.ItemVNum;
            Level = input.Level;
            LevelMinimum = input.LevelMinimum;
            MinimumAdventurerLevel = input.MinimumAdventurerLevel;
            MinimumArcherLevel = input.MinimumArcherLevel;
            MinimumMagicianLevel = input.MinimumMagicianLevel;
            MinimumSwordmanLevel = input.MinimumSwordmanLevel;
            MpCost = input.MpCost;
            Name = input.Name;
            Price = input.Price;
            Range = input.Range;
            SkillType = input.SkillType;
            SkillVNum = input.SkillVNum;
            TargetRange = input.TargetRange;
            TargetType = input.TargetType;
            Type = input.Type;
            UpgradeSkill = input.UpgradeSkill;
            UpgradeType = input.UpgradeType;
            Combos = new List<ComboDTO>();
            BCards = new List<BCard>();
        }

        #endregion

        #region Properties

        public List<BCard> BCards { get; set; }

        public List<ComboDTO> Combos { get; set; }

        #endregion
    }
}