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
    public class MinilandObjectDTO
    {
        #region Properties

        public long CharacterId { get; set; }

        public Guid? ItemInstanceId { get; set; }

        public byte Level1BoxAmount { get; set; }

        public byte Level2BoxAmount { get; set; }

        public byte Level3BoxAmount { get; set; }

        public byte Level4BoxAmount { get; set; }

        public byte Level5BoxAmount { get; set; }

        public short MapX { get; set; }

        public short MapY { get; set; }

        public long MinilandObjectId { get; set; }

        #endregion
    }
}