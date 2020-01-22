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

namespace OpenNos.GameObject
{
    public class ExchangeInfo
    {
        #region Instantiation

        public ExchangeInfo()
        {
            Confirmed = false;
            Gold = 0;
            TargetCharacterId = -1;
            ExchangeList = new List<ItemInstance>();
            Validated = false;
        }

        #endregion

        #region Properties

        public bool Confirmed { get; set; }

        public List<ItemInstance> ExchangeList { get; set; }

        public long Gold { get; set; }

        public long TargetCharacterId { get; set; }

        public bool Validated { get; set; }

        public long GoldBank { get; set; }

        #endregion
    }
}