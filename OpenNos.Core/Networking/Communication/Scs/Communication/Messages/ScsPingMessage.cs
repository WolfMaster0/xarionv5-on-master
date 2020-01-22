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

namespace OpenNos.Core.Networking.Communication.Scs.Communication.Messages
{
    /// <summary>
    /// This message is used to send/receive ping messages. Ping messages is used to keep connection
    /// alive between server and client.
    /// </summary>
    [Serializable]
    public sealed class ScsPingMessage : ScsMessage
    {
        #region Instantiation

        /// <summary>
        /// Creates a new PingMessage object.
        /// </summary>
        public ScsPingMessage()
        {
        }

        /// <summary>
        /// Creates a new reply PingMessage object.
        /// </summary>
        /// <param name="repliedMessageId">Replied message id if this is a reply for a message.</param>
        public ScsPingMessage(string repliedMessageId)
            : this() => RepliedMessageId = repliedMessageId;

        #endregion

        #region Methods

        /// <summary>
        /// Creates a string to represents this object.
        /// </summary>
        /// <returns>A string to represents this object</returns>
        public override string ToString()
        {
            return string.IsNullOrEmpty(RepliedMessageId)
                       ? $"ScsPingMessage [{MessageId}]"
                       : $"ScsPingMessage [{MessageId}] Replied To [{RepliedMessageId}]";
        }

        #endregion
    }
}