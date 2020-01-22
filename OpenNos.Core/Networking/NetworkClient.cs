﻿// This file is part of the OpenNos NosTale Emulator Project.
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
using OpenNos.Core.Cryptography;
using OpenNos.Core.Networking.Communication.Scs.Communication;
using OpenNos.Core.Networking.Communication.Scs.Communication.Channels;
using OpenNos.Core.Networking.Communication.Scs.Communication.Messages;
using OpenNos.Core.Networking.Communication.Scs.Server;

namespace OpenNos.Core.Networking
{
    public class NetworkClient : ScsServerClient, INetworkClient
    {
        #region Members

        private CryptographyBase _encryptor;

        #endregion

        #region Instantiation

        public NetworkClient(ICommunicationChannel communicationChannel) : base(communicationChannel)
        {
        }

        #endregion

        #region Properties

        public string IpAddress => RemoteEndPoint.ToString();

        public bool IsConnected => CommunicationState == CommunicationStates.Connected;

        public bool IsDisposing { get; set; }

        #endregion

        #region Methods

        public void Initialize(CryptographyBase encryptor) => _encryptor = encryptor;

        public void SendPacket(string packet, byte priority = 10)
        {
            if (!IsDisposing && !string.IsNullOrEmpty(packet))
            {
                ScsRawDataMessage rawMessage = new ScsRawDataMessage(_encryptor.Encrypt(packet));
                SendMessage(rawMessage, priority);
            }
        }

        public void SendPacketFormat(string packet, params object[] param) => SendPacket(string.Format(packet, param));

        public void SendPackets(IEnumerable<string> packets, byte priority = 10)
        {
            foreach (string packet in packets)
            {
                SendPacket(packet, priority);
            }
        }

        public void SetClientSession(object clientSession)
        {
        }

        #endregion
    }
}