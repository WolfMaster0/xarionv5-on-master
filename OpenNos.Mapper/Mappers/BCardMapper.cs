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
    public static class BCardMapper
    {
        #region Methods

        public static bool ToBCard(BCardDTO input, BCard output)
        {
            if (input == null)
            {
                return false;
            }
            output.BCardId = input.BCardId;
            output.CardId = input.CardId;
            output.CastType = input.CastType;
            output.FirstData = input.FirstData;
            output.IsLevelDivided = input.IsLevelDivided;
            output.IsLevelScaled = input.IsLevelScaled;
            output.ItemVNum = input.ItemVNum;
            output.NpcMonsterVNum = input.NpcMonsterVNum;
            output.SecondData = input.SecondData;
            output.SkillVNum = input.SkillVNum;
            output.SubType = input.SubType;
            output.ThirdData = input.ThirdData;
            output.Type = input.Type;
            return true;
        }

        public static bool ToBCardDTO(BCard input, BCardDTO output)
        {
            if (input == null)
            {
                return false;
            }
            output.BCardId = input.BCardId;
            output.CardId = input.CardId;
            output.CastType = input.CastType;
            output.FirstData = input.FirstData;
            output.IsLevelDivided = input.IsLevelDivided;
            output.IsLevelScaled = input.IsLevelScaled;
            output.ItemVNum = input.ItemVNum;
            output.NpcMonsterVNum = input.NpcMonsterVNum;
            output.SecondData = input.SecondData;
            output.SkillVNum = input.SkillVNum;
            output.SubType = input.SubType;
            output.ThirdData = input.ThirdData;
            output.Type = input.Type;
            return true;
        }

        #endregion
    }
}