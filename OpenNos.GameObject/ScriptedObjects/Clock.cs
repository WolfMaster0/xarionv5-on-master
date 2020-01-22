// This file is part of the OpenNos NosTale Emulator Project.
//
// This program is licensed under a deviated version of the Fair Source License, granting you a
// non-exclusive, non-transferable, royalty-free and fully-paid-up license, under all of the
// Licensor's copyright and patent rights, to use, copy, prepare derivative works of, publicly
// perform and display the Software, subject to the conditions found in the LICENSE file.
//
// THIS FILE IS PROVIDED "AS IS", WITHOUT WARRANTY OR CONDITION, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. THE AUTHORS HEREBY DISCLAIM ALL LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT
// OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE.

using OpenNos.GameObject.Event;
using OpenNos.GameObject.Helpers;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace OpenNos.GameObject.ScriptedObjects
{
    public class Clock
    {
        #region Members

        private readonly IDisposable _tickDisposable;

        #endregion

        #region Instantiation

        public Clock(byte type)
        {
            StopEvents = new List<EventContainer>();
            TimeoutEvents = new List<EventContainer>();
            Type = type;
            SecondsRemaining = 1;
            _tickDisposable = Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(x => Tick());
        }

        #endregion

        #region Properties

        public bool Enabled { get; private set; }

        public int SecondsRemaining { get; set; }

        public List<EventContainer> StopEvents { get; set; }

        public List<EventContainer> TimeoutEvents { get; set; }

        public int TotalSecondsAmount { get; set; }

        public byte Type { get; set; }

        #endregion

        #region Methods

        public string GetClock() => $"evnt {Type} {(Enabled ? 0 : (Type != 3) ? -1 : 1)} {SecondsRemaining} {TotalSecondsAmount}";

        public void StartClock() => Enabled = true;

        public void StopClock()
        {
            Enabled = false;
            StopEvents.ForEach(e => EventHelper.Instance.RunEvent(e));
            StopEvents.RemoveAll(s => s != null);
            _tickDisposable.Dispose();
        }

        private void Tick()
        {
            if (Enabled)
            {
                if (SecondsRemaining > 0)
                {
                    SecondsRemaining -= 10;
                }
                else
                {
                    TimeoutEvents.ForEach(ev => EventHelper.Instance.RunEvent(ev));
                    TimeoutEvents.RemoveAll(s => s != null);
                }
            }
        }

        #endregion
    }
}