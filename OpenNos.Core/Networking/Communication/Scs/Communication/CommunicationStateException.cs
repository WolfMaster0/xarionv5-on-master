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
using System.Runtime.Serialization;

namespace OpenNos.Core.Networking.Communication.Scs.Communication
{
    /// <summary>
    /// This application is thrown if communication is not expected state.
    /// </summary>
    [Serializable]
    public class CommunicationStateException : CommunicationException
    {
        #region Instantiation

        /// <summary>
        /// Contstructor.
        /// </summary>
        public CommunicationStateException()
        {
        }

        /// <summary>
        /// Contstructor for serializing.
        /// </summary>
        /// <param name="serializationInfo"></param>
        /// <param name="context"></param>
        protected CommunicationStateException(SerializationInfo serializationInfo, StreamingContext context) : base(serializationInfo, context)
        {
        }

        /// <summary>
        /// Contstructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        public CommunicationStateException(string message) : base(message)
        {
        }

        /// <summary>
        /// Contstructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public CommunicationStateException(string message, Exception innerException) : base(message, innerException)
        {
        }

        #endregion
    }
}