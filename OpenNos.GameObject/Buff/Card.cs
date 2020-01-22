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
    public class Card : CardDTO
    {
        #region Instantiation

        public Card()
        {
        }

        public Card(CardDTO input)
        {
            BuffType = input.BuffType;
            CardId = input.CardId;
            Delay = input.Delay;
            Duration = input.Duration;
            EffectId = input.EffectId;
            Level = input.Level;
            Name = input.Name;
            Propability = input.Propability;
            TimeoutBuff = input.TimeoutBuff;
            TimeoutBuffChance = input.TimeoutBuffChance;
        }

        #endregion

        #region Properties

        public List<BCard> BCards { get; set; }

        #endregion
    }
}