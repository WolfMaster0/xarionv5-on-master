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

namespace OpenNos.Core.Networking.Communication.Scs.Communication.Channels
{
    /// <summary>
    /// Represents a communication listener. A connection listener is used to accept incoming client
    /// connection requests.
    /// </summary>
    public interface IConnectionListener
    {
        #region Events

        /// <summary>
        /// This event is raised when a new communication channel connected.
        /// </summary>
        event EventHandler<CommunicationChannelEventArgs> CommunicationChannelConnected;

        #endregion

        #region Methods

        /// <summary>
        /// Starts listening incoming connections.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops listening incoming connections.
        /// </summary>
        void Stop();

        #endregion
    }
}