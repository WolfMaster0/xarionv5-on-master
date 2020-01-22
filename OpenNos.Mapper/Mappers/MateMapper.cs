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
using OpenNos.DAL.EF;

namespace OpenNos.Mapper.Mappers
{
    public static class MateMapper
    {
        #region Methods

        public static bool ToMate(MateDTO input, Mate output)
        {
            if (input == null)
            {
                return false;
            }
            output.Attack = input.Attack;
            output.CanPickUp = input.CanPickUp;
            output.CharacterId = input.CharacterId;
            output.Defence = input.Defence;
            output.Direction = input.Direction;
            output.Experience = input.Experience;
            output.Hp = input.Hp;
            output.IsSummonable = input.IsSummonable;
            output.IsTeamMember = input.IsTeamMember;
            output.Level = input.Level;
            output.Loyalty = input.Loyalty;
            output.MapX = input.MapX;
            output.MapY = input.MapY;
            output.MateId = input.MateId;
            output.MateType = input.MateType;
            output.Mp = input.Mp;
            output.Name = input.Name;
            output.NpcMonsterVNum = input.NpcMonsterVNum;
            output.Skin = input.Skin;
            output.MateSlot = input.MateSlot;
            output.PartnerSlot = input.PartnerSlot;
            return true;
        }

        public static bool ToMateDTO(Mate input, MateDTO output)
        {
            if (input == null)
            {
                return false;
            }
            output.Attack = input.Attack;
            output.CanPickUp = input.CanPickUp;
            output.CharacterId = input.CharacterId;
            output.Defence = input.Defence;
            output.Direction = input.Direction;
            output.Experience = input.Experience;
            output.Hp = input.Hp;
            output.IsSummonable = input.IsSummonable;
            output.IsTeamMember = input.IsTeamMember;
            output.Level = input.Level;
            output.Loyalty = input.Loyalty;
            output.MapX = input.MapX;
            output.MapY = input.MapY;
            output.MateId = input.MateId;
            output.MateType = input.MateType;
            output.Mp = input.Mp;
            output.Name = input.Name;
            output.NpcMonsterVNum = input.NpcMonsterVNum;
            output.Skin = input.Skin;
            output.MateSlot = input.MateSlot;
            output.PartnerSlot = input.PartnerSlot;
            return true;
        }

        #endregion
    }
}