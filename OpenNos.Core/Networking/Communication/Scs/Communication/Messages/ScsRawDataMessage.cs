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
    /// This message is used to send/receive a raw byte array as message data.
    /// </summary>
    [Serializable]
    public class ScsRawDataMessage : ScsMessage, IComparable
    {
        #region Instantiation

        /// <summary>
        /// Default empty constructor.
        /// </summary>
        private ScsRawDataMessage()
        {
        }

        /// <summary>
        /// Creates a new ScsRawDataMessage object with MessageData property.
        /// </summary>
        /// <param name="messageData">Message data that is being transmitted</param>
        public ScsRawDataMessage(byte[] messageData) => MessageData = messageData;

/*
        /// <summary>
        /// Creates a new reply ScsRawDataMessage object with MessageData property.
        /// </summary>
        /// <param name="messageData">Message data that is being transmitted</param>
        /// <param name="repliedMessageId">Replied message id if this is a reply for a message.</param>
        public ScsRawDataMessage(byte[] messageData, string repliedMessageId) : this(messageData) => RepliedMessageId = repliedMessageId;
*/

        #endregion

        #region Properties

        /// <summary>
        /// Message data that is being transmitted.
        /// </summary>
        public byte[] MessageData { get; }

        // ReSharper disable once UnassignedGetOnlyAutoProperty
        private int Priority { get; }

        #endregion

        #region Methods

        public int CompareTo(object obj) => CompareTo((ScsRawDataMessage)obj);

        private int CompareTo(ScsRawDataMessage other) => Priority.CompareTo(other.Priority);

        /// <summary>
        /// Creates a string to represents this object.
        /// </summary>
        /// <returns>A string to represents this object</returns>
        public override string ToString()
        {
            int messageLength = MessageData?.Length ?? 0;
            return string.IsNullOrEmpty(RepliedMessageId)
                       ? $"ScsRawDataMessage [{MessageId}]: {messageLength} bytes"
                       : $"ScsRawDataMessage [{MessageId}] Replied To [{RepliedMessageId}]: {messageLength} bytes";
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is null)
            {
                return false;
            }

            return false;
        }

        // ReSharper disable once FunctionRecursiveOnAllPaths
        public override int GetHashCode() => GetHashCode();

        public static bool operator ==(ScsRawDataMessage left, ScsRawDataMessage right)
        {
            if (left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

        public static bool operator !=(ScsRawDataMessage left, ScsRawDataMessage right) => !(left == right);

        public static bool operator <(ScsRawDataMessage left, ScsRawDataMessage right) => left is null ? !(right is null) : left.CompareTo(right) < 0;

        public static bool operator <=(ScsRawDataMessage left, ScsRawDataMessage right) => left is null || left.CompareTo(right) <= 0;

        public static bool operator >(ScsRawDataMessage left, ScsRawDataMessage right) => !(left is null) && left.CompareTo(right) > 0;

        public static bool operator >=(ScsRawDataMessage left, ScsRawDataMessage right) => left is null ? right is null : left.CompareTo(right) >= 0;

        #endregion
    }
}