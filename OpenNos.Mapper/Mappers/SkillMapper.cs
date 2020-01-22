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
    public static class SkillMapper
    {
        #region Methods

        public static bool ToSkill(SkillDTO input, Skill output)
        {
            if (input == null)
            {
                return false;
            }
            output.AttackAnimation = input.AttackAnimation;
            output.CastAnimation = input.CastAnimation;
            output.CastEffect = input.CastEffect;
            output.CastId = input.CastId;
            output.CastTime = input.CastTime;
            output.Class = input.Class;
            output.Cooldown = input.Cooldown;
            output.CPCost = input.CPCost;
            output.Duration = input.Duration;
            output.Effect = input.Effect;
            output.Element = input.Element;
            output.HitType = input.HitType;
            output.ItemVNum = input.ItemVNum;
            output.Level = input.Level;
            output.LevelMinimum = input.LevelMinimum;
            output.MinimumAdventurerLevel = input.MinimumAdventurerLevel;
            output.MinimumArcherLevel = input.MinimumArcherLevel;
            output.MinimumMagicianLevel = output.MinimumMagicianLevel;
            output.MinimumSwordmanLevel = input.MinimumSwordmanLevel;
            output.MpCost = input.MpCost;
            output.Name = input.Name;
            output.Price = input.Price;
            output.Range = input.Range;
            output.SkillType = input.SkillType;
            output.SkillVNum = input.SkillVNum;
            output.TargetRange = input.TargetRange;
            output.TargetType = input.TargetType;
            output.Type = input.Type;
            output.UpgradeSkill = input.UpgradeSkill;
            output.UpgradeType = input.UpgradeType;
            return true;
        }

        public static bool ToSkillDTO(Skill input, SkillDTO output)
        {
            if (input == null)
            {
                return false;
            }
            output.AttackAnimation = input.AttackAnimation;
            output.CastAnimation = input.CastAnimation;
            output.CastEffect = input.CastEffect;
            output.CastId = input.CastId;
            output.CastTime = input.CastTime;
            output.Class = input.Class;
            output.Cooldown = input.Cooldown;
            output.CPCost = input.CPCost;
            output.Duration = input.Duration;
            output.Effect = input.Effect;
            output.Element = input.Element;
            output.HitType = input.HitType;
            output.ItemVNum = input.ItemVNum;
            output.Level = input.Level;
            output.LevelMinimum = input.LevelMinimum;
            output.MinimumAdventurerLevel = input.MinimumAdventurerLevel;
            output.MinimumArcherLevel = input.MinimumArcherLevel;
            output.MinimumMagicianLevel = input.MinimumMagicianLevel;
            output.MinimumSwordmanLevel = input.MinimumSwordmanLevel;
            output.MpCost = input.MpCost;
            output.Name = input.Name;
            output.Price = input.Price;
            output.Range = input.Range;
            output.SkillType = input.SkillType;
            output.SkillVNum = input.SkillVNum;
            output.TargetRange = input.TargetRange;
            output.TargetType = input.TargetType;
            output.Type = input.Type;
            output.UpgradeSkill = input.UpgradeSkill;
            output.UpgradeType = input.UpgradeType;
            return true;
        }

        #endregion
    }
}