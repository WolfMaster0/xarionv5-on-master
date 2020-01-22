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
using System;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject
{
    public abstract class MapItem
    {
        #region Members

        protected ItemInstance MapItemInstance;

        private readonly object _lockObject = new object();
        private long _transportId;

        #endregion

        #region Instantiation

        protected MapItem(short x, short y)
        {
            PositionX = x;
            PositionY = y;
            CreatedDate = DateTime.UtcNow;
            TransportId = 0;
        }

        #endregion

        #region Properties

        public abstract byte Amount { get; set; }

        public DateTime CreatedDate { get; }

        public abstract short ItemVNum { get; set; }

        public short PositionX { get; }

        public short PositionY { get; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public bool IsDroppedByMonster { get; set; }

        public long TransportId
        {
            get
            {
                lock (_lockObject)
                {
                    if (_transportId == 0)
                    {
                        _transportId = TransportFactory.Instance.GenerateTransportId();
                    }
                    return _transportId;
                }
            }

            private set => _transportId = value;
        }

        #endregion

        #region Methods

        public string GenerateIn() => StaticPacketHelper.In(Domain.UserType.Object, ItemVNum, TransportId, PositionX, PositionY, this is MonsterMapItem monsterMapItem && monsterMapItem.GoldAmount > 1 ? monsterMapItem.GoldAmount : Amount, 0, 0, 0, 0, false);

        public abstract ItemInstance GetItemInstance();

        #endregion
    }
}