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
using System;

namespace OpenNos.Data
{
    [Serializable]
    public class BCardDTO
    {
        #region Properties

        public short BCardId { get; set; }

        public short? CardId { get; set; }

        public byte CastType { get; set; }

        public int FirstData { get; set; }

        public bool IsLevelDivided { get; set; }

        public bool IsLevelScaled { get; set; }

        public short? ItemVNum { get; set; }

        public short? NpcMonsterVNum { get; set; }

        public int SecondData { get; set; }

        public short? SkillVNum { get; set; }

        public byte SubType { get; set; }

        public int ThirdData { get; set; }

        public byte Type { get; set; }

        #endregion
    }
}