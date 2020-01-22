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
using OpenNos.Core.Networking.Communication.Scs.Communication;
using OpenNos.Core.Networking.Communication.Scs.Communication.Channels;
using OpenNos.Core.Networking.Communication.Scs.Communication.Messages;
using OpenNos.Core.Networking.Communication.Scs.Communication.Protocols;
using OpenNos.Core.Threading;
using System;
using System.Threading.Tasks;

namespace OpenNos.Core.Networking.Communication.Scs.Client
{
    /// <summary>
    /// This class provides base functionality for client Classs.
    /// </summary>
    public abstract class ScsClientBase : IScsClient
    {
        #region Members

        /// <summary>
        /// Default timeout value for connecting a server.
        /// </summary>
        private const int Defaulttimeout = 15000;

        /// <summary>
        /// This timer is used to send PingMessage messages to server periodically.
        /// </summary>
        private readonly Timer _pingTimer;

        private bool _disposed;

        private IScsWireProtocol _wireProtocol;

        #endregion

        #region Instantiation

        /// <summary>
        /// Constructor.
        /// </summary>
        protected ScsClientBase()
        {
            _pingTimer = new Timer(30000);
            _pingTimer.Elapsed += PingTimer_Elapsed;
            ConnectTimeout = Defaulttimeout;
            WireProtocol = WireProtocolManager.GetDefaultWireProtocol();
        }

        #endregion

        #region Events

        /// <summary>
        /// This event is raised when communication channel closed.
        /// </summary>
        public event EventHandler Connected;

        /// <summary>
        /// This event is raised when client disconnected from server.
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

        /// <inheritdoc/>
        public ICommunicationChannel CommunicationChannel { get; private set; }

        /// <summary>
        /// Gets the communication state of the Client.
        /// </summary>
        public CommunicationStates CommunicationState => CommunicationChannel != null
                           ? CommunicationChannel.CommunicationState
                           : CommunicationStates.Disconnected;

        /// <summary>
        /// Timeout for connecting to a server (as milliseconds). Default value: 15 seconds (15000 ms).
        /// </summary>
        public int ConnectTimeout { get; set; }

        /// <summary>
        /// Gets the time of the last succesfully received message.
        /// </summary>
        public DateTime LastReceivedMessageTime => CommunicationChannel != null
                           ? CommunicationChannel.LastReceivedMessageTime
                           : DateTime.MinValue;

        /// <summary>
        /// Gets the time of the last succesfully received message.
        /// </summary>
        public DateTime LastSentMessageTime => CommunicationChannel != null
                           ? CommunicationChannel.LastSentMessageTime
                           : DateTime.MinValue;

        /// <summary>
        /// Gets/sets wire protocol that is used while reading and writing messages.
        /// </summary>
        public IScsWireProtocol WireProtocol
        {
            get => _wireProtocol;

            set
            {
                if (CommunicationState == CommunicationStates.Connected)
                {
                    throw new ApplicationException("Wire protocol can not be changed while connected to server.");
                }

                _wireProtocol = value;
            }
        }

        #endregion

        #region Methods

        public async Task ClearLowPriorityQueueAsync() => await CommunicationChannel.ClearLowPriorityQueueAsync().ConfigureAwait(false);

        /// <summary>
        /// Connects to server.
        /// </summary>
        public void Connect()
        {
            WireProtocol.Reset();
            CommunicationChannel = CreateCommunicationChannel();
            CommunicationChannel.WireProtocol = WireProtocol;
            CommunicationChannel.Disconnected += CommunicationChannel_Disconnected;
            CommunicationChannel.MessageReceived += CommunicationChannel_MessageReceived;
            CommunicationChannel.MessageSent += CommunicationChannel_MessageSent;
            CommunicationChannel.Start();
            _pingTimer.Start();
            OnConnected();
        }

        /// <summary>
        /// Disconnects from server. Does nothing if already disconnected.
        /// </summary>
        public void Disconnect()
        {
            if (CommunicationState != CommunicationStates.Connected)
            {
                return;
            }

            CommunicationChannel.Disconnect();
        }

        /// <summary>
        /// Disposes this object and closes underlying connection.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="message">Message to be sent</param>
        /// <param name="priority">Priority of message to send</param>
        /// <exception cref="CommunicationStateException">
        /// Throws a CommunicationStateException if client is not connected to the server.
        /// </exception>
        public void SendMessage(IScsMessage message, byte priority)
        {
            if (CommunicationState != CommunicationStates.Connected)
            {
                throw new CommunicationStateException("Client is not connected to the server.");
            }

            CommunicationChannel.SendMessage(message, priority);
        }

        /// <summary>
        /// This method is implemented by derived Classs to create appropriate communication channel.
        /// </summary>
        /// <returns>Ready communication channel to communicate</returns>
        protected abstract ICommunicationChannel CreateCommunicationChannel();

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Disconnect();
                    _pingTimer.Dispose();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Raises Connected event.
        /// </summary>
        protected virtual void OnConnected() => Connected?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raises Disconnected event.
        /// </summary>
        protected virtual void OnDisconnected() => Disconnected?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raises MessageReceived event.
        /// </summary>
        /// <param name="message">Received message</param>
        protected virtual void OnMessageReceived(IScsMessage message) => MessageReceived?.Invoke(this, new MessageEventArgs(message, DateTime.UtcNow));

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
        private void CommunicationChannel_Disconnected(object sender, EventArgs e)
        {
            _pingTimer.Stop();
            OnDisconnected();
        }

        /// <summary>
        /// Handles MessageReceived event of _communicationChannel object.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void CommunicationChannel_MessageReceived(object sender, MessageEventArgs e)
        {
            if (e.Message is ScsPingMessage)
            {
                return;
            }

            OnMessageReceived(e.Message);
        }

        /// <summary>
        /// Handles MessageSent event of _communicationChannel object.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void CommunicationChannel_MessageSent(object sender, MessageEventArgs e) => OnMessageSent(e.Message);

        /// <summary>
        /// Handles Elapsed event of _pingTimer to send PingMessage messages to server.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void PingTimer_Elapsed(object sender, EventArgs e)
        {
            if (CommunicationState != CommunicationStates.Connected)
            {
                return;
            }

            try
            {
                DateTime lastMinute = DateTime.UtcNow.AddMinutes(-1);
                if (CommunicationChannel.LastReceivedMessageTime > lastMinute || CommunicationChannel.LastSentMessageTime > lastMinute)
                {
                    return;
                }

                CommunicationChannel.SendMessage(new ScsPingMessage(), 10);
            }
            catch
            {
                // do nothing
            }
        }

        #endregion
    }
}