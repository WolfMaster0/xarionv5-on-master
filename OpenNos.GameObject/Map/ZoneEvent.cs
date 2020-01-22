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
using OpenNos.GameObject.Event;

namespace OpenNos.GameObject
{
    public class ZoneEvent
    {
        #region Instantiation

        public ZoneEvent()
        {
            Events = new List<EventContainer>();
            Range = 1;
        }

        #endregion

        #region Properties

        public List<EventContainer> Events { get; set; }

        public short Range { get; set; }

        public short X { get; set; }

        public short Y { get; set; }

        #endregion

        #region Methods

        public bool InZone(short positionX, short positionY) => positionX <= X + Range && positionX >= X - Range && positionY <= Y + Range && positionY >= Y - Range;

        #endregion
    }
}