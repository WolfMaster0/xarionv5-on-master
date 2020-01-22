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
using OpenNos.GameObject.Helpers;
using System.Collections.Generic;
using OpenNos.GameObject.Event;

namespace OpenNos.GameObject
{
    public class MapButton
    {
        #region Instantiation

        public MapButton(int id, short positionX, short positionY, short enabledVNum, short disabledVNum, List<EventContainer> disableEvents, List<EventContainer> enableEvents, List<EventContainer> firstEnableEvents)
        {
            MapButtonId = id;
            PositionX = positionX;
            PositionY = positionY;
            EnabledVNum = enabledVNum;
            DisabledVNum = disabledVNum;
            DisableEvents = disableEvents;
            EnableEvents = enableEvents;
            FirstEnableEvents = firstEnableEvents;
        }

        #endregion

        #region Properties

        public short DisabledVNum { get; set; }

        public List<EventContainer> DisableEvents { get; set; }

        public short EnabledVNum { get; set; }

        public List<EventContainer> EnableEvents { get; set; }

        public List<EventContainer> FirstEnableEvents { get; set; }

        public int MapButtonId { get; set; }

        public short PositionX { get; set; }

        public short PositionY { get; set; }

        public bool State { get; set; }

        #endregion

        #region Methods

        public string GenerateIn() => StaticPacketHelper.In(Domain.UserType.Object, State ? EnabledVNum : DisabledVNum, MapButtonId, PositionX, PositionY, 1, 0, 0, 0, 0, false);

        public void RunAction()
        {
            State = !State;
            if (State)
            {
                EnableEvents.ForEach(e => EventHelper.Instance.RunEvent(e));
                FirstEnableEvents.ForEach(e => EventHelper.Instance.RunEvent(e));
                FirstEnableEvents.RemoveAll(s => s != null);
            }
            else
            {
                DisableEvents.ForEach(e => EventHelper.Instance.RunEvent(e));
            }
        }

        #endregion
    }
}