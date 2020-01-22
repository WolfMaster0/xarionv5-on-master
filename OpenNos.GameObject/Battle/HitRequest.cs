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
using OpenNos.Domain;
using System;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Battle
{
    public class HitRequest
    {
        #region Instantiation

        public HitRequest(TargetHitType targetHitType, ClientSession session, Mate mate, Skill skill)
        {
            HitTimestamp = DateTime.UtcNow;
            Mate = mate;
            Skill = skill;
            TargetHitType = targetHitType;
            Session = session;
        }

        public HitRequest(TargetHitType targetHitType, ClientSession session, Skill skill, short? skillEffect = null, short? mapX = null, short? mapY = null, ComboDTO skillCombo = null, bool showTargetAnimation = false, int directDamage = 0)
        {
            HitTimestamp = DateTime.UtcNow;
            Session = session;
            Skill = skill;
            TargetHitType = targetHitType;
            SkillEffect = skillEffect ?? skill?.Effect ?? 0;
            ShowTargetHitAnimation = showTargetAnimation;
            DirectDamage = directDamage;

            if (mapX.HasValue)
            {
                MapX = mapX.Value;
            }

            if (mapY.HasValue)
            {
                MapY = mapY.Value;
            }

            if (skillCombo != null)
            {
                SkillCombo = skillCombo;
            }
        }

        #endregion

        #region Properties

        public int DirectDamage { get; }

        public DateTime HitTimestamp { get; set; }

        public Mate Mate { get; set; }

        public short MapX { get; set; }

        public short MapY { get; set; }

        public ClientSession Session { get; set; }

        /// <summary>
        /// Some AOE Skills need to show additional SU packet for Animation
        /// </summary>
        public bool ShowTargetHitAnimation { get; set; }

        public Skill Skill { get; set; }

        public ComboDTO SkillCombo { get; set; }

        public short SkillEffect { get; set; }

        public NpcMonsterSkill NpcMonsterSkill { get; set; }

        public TargetHitType TargetHitType { get; set; }

        #endregion
    }
}