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
    /// This class provides base functionality for communication listener Classs.
    /// </summary>
    public abstract class ConnectionListenerBase : IConnectionListener
    {
        #region Events

        /// <summary>
        /// This event is raised when a new communication channel is connected.
        /// </summary>
        public event EventHandler<CommunicationChannelEventArgs> CommunicationChannelConnected;

        #endregion

        #region Methods

        /// <summary>
        /// Starts listening incoming connections.
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Stops listening incoming connections.
        /// </summary>
        public abstract void Stop();

        /// <summary>
        /// Raises CommunicationChannelConnected event.
        /// </summary>
        /// <param name="client"></param>
        protected virtual void OnCommunicationChannelConnected(ICommunicationChannel client) => CommunicationChannelConnected?.Invoke(this, new CommunicationChannelEventArgs(client));

        #endregion
    }
}