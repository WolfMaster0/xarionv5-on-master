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
    public static class CardMapper
    {
        #region Methods

        public static bool ToCard(CardDTO input, Card output)
        {
            if (input == null)
            {
                return false;
            }
            output.BuffType = input.BuffType;
            output.CardId = input.CardId;
            output.Delay = input.Delay;
            output.Duration = input.Duration;
            output.EffectId = input.EffectId;
            output.Level = input.Level;
            output.Name = input.Name;
            output.Propability = input.Propability;
            output.TimeoutBuff = input.TimeoutBuff;
            output.TimeoutBuffChance = input.TimeoutBuffChance;
            return true;
        }

        public static bool ToCardDTO(Card input, CardDTO output)
        {
            if (input == null)
            {
                return false;
            }
            output.BuffType = input.BuffType;
            output.CardId = input.CardId;
            output.Delay = input.Delay;
            output.Duration = input.Duration;
            output.EffectId = input.EffectId;
            output.Level = input.Level;
            output.Name = input.Name;
            output.Propability = input.Propability;
            output.TimeoutBuff = input.TimeoutBuff;
            output.TimeoutBuffChance = input.TimeoutBuffChance;
            return true;
        }

        #endregion
    }
}