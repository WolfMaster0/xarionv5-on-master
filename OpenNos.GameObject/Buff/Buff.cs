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
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject
{
    public class Buff
    {
        #region Members

        public int Level;

        #endregion

        #region Instantiation

        public Buff(short id, byte level = 10, bool isPermaBuff = false)
        {
            Card = ServerManager.GetCard(id);
            Level = level;
            IsPermaBuff = isPermaBuff;
        }

        #endregion

        #region Properties

        public bool IsPermaBuff { get; set; }

        public Card Card { get; set; }

        public int RemainingTime { get; set; }

        public DateTime Start { get; set; }

        public bool StaticBuff { get; set; }

        #endregion
    }
}