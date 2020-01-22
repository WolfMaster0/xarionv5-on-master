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

namespace OpenNos.GameObject
{
    public class Act4Stat
    {
        #region Members

        private DateTime _latestUpdate;
        private DateTime _nextMonth;

        private int _percentage;

        private short _totalTime;

        #endregion

        #region Instantiation

        public Act4Stat()
        {
            DateTime olddate = DateTime.UtcNow.AddMonths(1);
            _nextMonth = new DateTime(olddate.Year, olddate.Month, 1, 0, 0, 0, olddate.Kind);
            _latestUpdate = DateTime.UtcNow;
        }

        #endregion

        #region Properties

        public short CurrentTime => Mode == 0 ? (short)0 : (short)(_latestUpdate.AddSeconds(_totalTime) - DateTime.UtcNow).TotalSeconds;

        public bool IsBerios { get; set; }

        public bool IsCalvina { get; set; }

        public bool IsHatus { get; set; }

        public bool IsMorcos { get; set; }

        public int MinutesUntilReset
        {
            get
            {
                if (_nextMonth < DateTime.UtcNow)
                {
                    DateTime olddate = DateTime.UtcNow.AddMonths(1);
                    _nextMonth = new DateTime(olddate.Year, olddate.Month, 1, 0, 0, 0, olddate.Kind);
                }
                return (int) (_nextMonth - DateTime.UtcNow).TotalMinutes;
            }
        }

        public byte Mode { get; set; }

        public int Percentage
        {
            get => Mode == 0 ? _percentage : 0;
            set => _percentage = value;
        }

        public short TotalTime
        {
            get => Mode == 0 ? (short)0 : _totalTime;
            set
            {
                _latestUpdate = DateTime.UtcNow;
                _totalTime = value;
            }
        }

        #endregion
    }
}