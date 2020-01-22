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
using OpenNos.Core.Networking.Communication.Scs.Communication.Messages;
using OpenNos.Core.Networking.Communication.Scs.Communication.Protocols;
using System;
using System.Threading.Tasks;

namespace OpenNos.Core.Networking.Communication.Scs.Communication.Messengers
{
    /// <summary>
    /// Represents an object that can send and receive messages.
    /// </summary>
    public interface IMessenger
    {
        #region Events

        /// <summary>
        /// This event is raised when a new message is received.
        /// </summary>
        event EventHandler<MessageEventArgs> MessageReceived;

        /// <summary>
        /// This event is raised when a new message is sent without any error. It does not guaranties
        /// that message is properly handled and processed by remote application.
        /// </summary>
        event EventHandler<MessageEventArgs> MessageSent;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the time of the last succesfully received message.
        /// </summary>
        DateTime LastReceivedMessageTime { get; }

        /// <summary>
        /// Gets the time of the last succesfully sent message.
        /// </summary>
        DateTime LastSentMessageTime { get; }

        /// <summary>
        /// Gets/sets wire protocol that is used while reading and writing messages.
        /// </summary>
        IScsWireProtocol WireProtocol { get; set; }

        #endregion

        #region Methods

        Task ClearLowPriorityQueueAsync();

        /// <summary>
        /// Sends a message to the remote application.
        /// </summary>
        /// <param name="message">Message to be sent</param>
        /// <param name="priority">Message priority to send</param>
        void SendMessage(IScsMessage message, byte priority);

        #endregion
    }
}