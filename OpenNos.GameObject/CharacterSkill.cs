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
using System;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject
{
    public class CharacterSkill : CharacterSkillDTO
    {
        #region Members

        private Skill _skill;

        #endregion

        #region Instantiation

        public CharacterSkill(CharacterSkillDTO characterSkill)
        {
            CharacterId = characterSkill.CharacterId;
            Id = characterSkill.Id;
            SkillVNum = characterSkill.SkillVNum;
            LastUse = DateTime.UtcNow.AddHours(-1);
            Hit = 0;
        }

        public CharacterSkill()
        {
            LastUse = DateTime.UtcNow.AddHours(-1);
            Hit = 0;
        }

        #endregion

        #region Properties

        public short Hit { get; set; }

        public DateTime LastUse
        {
            get; set;
        }

        public Skill Skill => _skill ?? (_skill = ServerManager.GetSkill(SkillVNum));

        #endregion

        #region Methods

        public bool CanBeUsed() => Skill != null && LastUse.AddMilliseconds(Skill.Cooldown * 100) < DateTime.UtcNow;

        #endregion
    }
}