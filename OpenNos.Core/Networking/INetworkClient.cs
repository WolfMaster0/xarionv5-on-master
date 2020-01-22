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
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenNos.Core.Cryptography;
using OpenNos.Core.Networking.Communication.Scs.Communication.Messages;

namespace OpenNos.Core.Networking
{
    public interface INetworkClient
    {
        #region Events

        event EventHandler<MessageEventArgs> MessageReceived;

        #endregion

        #region Properties

        long ClientId { get; set; }

        string IpAddress { get; }

        bool IsConnected { get; }

        bool IsDisposing { get; set; }

        #endregion

        #region Methods

        Task ClearLowPriorityQueueAsync();

        void Disconnect();

        void Initialize(CryptographyBase encryptor);

        void SendPacket(string packet, byte priority = 10);

        void SendPacketFormat(string packet, params object[] param);

        void SendPackets(IEnumerable<string> packets, byte priority = 10);

        void SetClientSession(object clientSession);

        #endregion
    }
}