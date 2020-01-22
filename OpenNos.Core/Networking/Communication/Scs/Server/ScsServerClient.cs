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
using OpenNos.Core.Networking.Communication.Scs.Communication;
using OpenNos.Core.Networking.Communication.Scs.Communication.Channels;
using OpenNos.Core.Networking.Communication.Scs.Communication.EndPoints;
using OpenNos.Core.Networking.Communication.Scs.Communication.Messages;
using OpenNos.Core.Networking.Communication.Scs.Communication.Protocols;
using System;
using System.Threading.Tasks;

namespace OpenNos.Core.Networking.Communication.Scs.Server
{
    /// <summary>
    /// This class represents a client in server side.
    /// </summary>
    public class ScsServerClient : IScsServerClient
    {
        #region Members

        /// <summary>
        /// The communication channel that is used by client to send and receive messages.
        /// </summary>
        private readonly ICommunicationChannel _communicationChannel;

        #endregion

        #region Instantiation

        /// <summary>
        /// Creates a new ScsClient object.
        /// </summary>
        /// <param name="communicationChannel">
        /// The communication channel that is used by client to send and receive messages
        /// </param>
        public ScsServerClient(ICommunicationChannel communicationChannel)
        {
            _communicationChannel = communicationChannel;
            _communicationChannel.MessageReceived += CommunicationChannel_MessageReceived;
            _communicationChannel.MessageSent += CommunicationChannel_MessageSent;
            _communicationChannel.Disconnected += CommunicationChannel_Disconnected;
        }

        #endregion

        #region Events

        /// <summary>
        /// This event is raised when client is disconnected from server.
        /// </summary>
        public event EventHandler Disconnected;

        /// <summary>
        /// This event is raised when a new message is received.
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageReceived;

        /// <summary>
        /// This event is raised when a new message is sent without any error. It does not guaranties
        /// that message is properly handled and processed by remote application.
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageSent;

        #endregion

        #region Properties

        /// <summary>
        /// Unique identifier for this client in server.
        /// </summary>
        public long ClientId { get; set; }

        /// <summary>
        /// Gets the communication state of the Client.
        /// </summary>
        public CommunicationStates CommunicationState => _communicationChannel.CommunicationState;

        /// <summary>
        /// Gets the time of the last succesfully received message.
        /// </summary>
        public DateTime LastReceivedMessageTime => _communicationChannel.LastReceivedMessageTime;

        /// <summary>
        /// Gets the time of the last succesfully received message.
        /// </summary>
        public DateTime LastSentMessageTime => _communicationChannel.LastSentMessageTime;

        /// <summary>
        /// Gets endpoint of remote application.
        /// </summary>
        public ScsEndPoint RemoteEndPoint => _communicationChannel.RemoteEndPoint;

        /// <summary>
        /// Gets/sets wire protocol that is used while reading and writing messages.
        /// </summary>
        public IScsWireProtocol WireProtocol
        {
            get => _communicationChannel.WireProtocol;
            set => _communicationChannel.WireProtocol = value;
        }

        #endregion

        #region Methods

        public async Task ClearLowPriorityQueueAsync() => await _communicationChannel.ClearLowPriorityQueueAsync().ConfigureAwait(false);

        /// <summary>
        /// Disconnects from client and closes underlying communication channel.
        /// </summary>
        public void Disconnect() => _communicationChannel.Disconnect();

        /// <summary>
        /// Sends a message to the client.
        /// </summary>
        /// <param name="message">Message to be sent</param>
        /// <param name="priority">Message priority to send</param>
        public void SendMessage(IScsMessage message, byte priority) => _communicationChannel.SendMessage(message, priority);

        /// <summary>
        /// Raises MessageSent event.
        /// </summary>
        /// <param name="message">Received message</param>
        protected virtual void OnMessageSent(IScsMessage message) => MessageSent?.Invoke(this, new MessageEventArgs(message, DateTime.UtcNow));

        /// <summary>
        /// Handles Disconnected event of _communicationChannel object.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void CommunicationChannel_Disconnected(object sender, EventArgs e) => OnDisconnected();

        /// <summary>
        /// Handles MessageReceived event of _communicationChannel object.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void CommunicationChannel_MessageReceived(object sender, MessageEventArgs e)
        {
            IScsMessage message = e.Message;
            if (message is ScsPingMessage)
            {
                _communicationChannel.SendMessage(new ScsPingMessage { RepliedMessageId = message.MessageId }, 10);
                return;
            }

            OnMessageReceived(message);
        }

        /// <summary>
        /// Handles MessageSent event of _communicationChannel object.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void CommunicationChannel_MessageSent(object sender, MessageEventArgs e) => OnMessageSent(e.Message);

        /// <summary>
        /// Raises Disconnected event.
        /// </summary>
        private void OnDisconnected() => Disconnected?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raises MessageReceived event.
        /// </summary>
        /// <param name="message">Received message</param>
        private void OnMessageReceived(IScsMessage message) => MessageReceived?.Invoke(this, new MessageEventArgs(message, DateTime.UtcNow));

        #endregion
    }
}