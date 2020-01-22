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
using OpenNos.Core.Networking.Communication.Scs.Communication.EndPoints;
using OpenNos.Core.Networking.Communication.Scs.Communication.Messages;
using OpenNos.Core.Networking.Communication.Scs.Communication.Protocols;
using System;
using System.Threading.Tasks;

namespace OpenNos.Core.Networking.Communication.Scs.Communication.Channels
{
    /// <summary>
    /// This class provides base functionality for all communication channel Classs.
    /// </summary>
    public abstract class CommunicationChannelBase : ICommunicationChannel
    {
        #region Instantiation

        /// <summary>
        /// Constructor.
        /// </summary>
        protected CommunicationChannelBase()
        {
            CommunicationState = CommunicationStates.Disconnected;
            LastReceivedMessageTime = DateTime.MinValue;
            LastSentMessageTime = DateTime.MinValue;
        }

        #endregion

        #region Events

        /// <summary>
        /// This event is raised when communication channel closed.
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
        /// Gets the current communication state.
        /// </summary>
        public CommunicationStates CommunicationState { get; protected set; }

        /// <summary>
        /// Gets the time of the last succesfully received message.
        /// </summary>
        public DateTime LastReceivedMessageTime { get; protected set; }

        /// <summary>
        /// Gets the time of the last succesfully sent message.
        /// </summary>
        public DateTime LastSentMessageTime { get; protected set; }

        /// <summary>
        /// Gets endpoint of remote application.
        /// </summary>
        public abstract ScsEndPoint RemoteEndPoint { get; }

        /// <summary>
        /// Gets/sets wire protocol that the channel uses. This property must set before first communication.
        /// </summary>
        public IScsWireProtocol WireProtocol { get; set; }

        #endregion

        #region Methods

        public abstract Task ClearLowPriorityQueueAsync();

        /// <summary>
        /// Disconnects from remote application and closes this channel.
        /// </summary>
        public abstract void Disconnect();

        /// <summary>
        /// Sends a message to the remote application.
        /// </summary>
        /// <param name="message">Message to be sent</param>
        /// <param name="priority">Priority of message to send</param>
        /// <exception cref="ArgumentNullException">
        /// Throws ArgumentNullException if message is null
        /// </exception>
        public void SendMessage(IScsMessage message, byte priority)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            SendMessagePublic(message, priority);
        }

        /// <summary>
        /// Starts the communication with remote application.
        /// </summary>
        public void Start()
        {
            StartPublic();
            CommunicationState = CommunicationStates.Connected;
        }

        /// <summary>
        /// Raises Disconnected event.
        /// </summary>
        protected virtual void OnDisconnected() => Disconnected?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raises MessageReceived event.
        /// </summary>
        /// <param name="message">Received message</param>
        /// <param name="receivedTimestamp">Message reception timestamp</param>
        protected virtual void OnMessageReceived(IScsMessage message, DateTime receivedTimestamp) => MessageReceived?.Invoke(this, new MessageEventArgs(message, receivedTimestamp));

        /// <summary>
        /// Raises MessageSent event.
        /// </summary>
        /// <param name="message">Received message</param>
        protected virtual void OnMessageSent(IScsMessage message) => MessageSent?.Invoke(this, new MessageEventArgs(message, DateTime.UtcNow));

        /// <summary>
        /// Sends a message to the remote application. This method is overrided by derived Classs to
        /// really send to message.
        /// </summary>
        /// <param name="message">Message to be sent</param>
        /// <param name="priority">Priority of message to send</param>
        protected abstract void SendMessagePublic(IScsMessage message, byte priority);

        /// <summary>
        /// Starts the communication with remote application really.
        /// </summary>
        protected abstract void StartPublic();

        #endregion
    }
}