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
using OpenNos.Core.Networking.Communication.Scs.Communication.Protocols;
using System;
using OpenNos.Core.Threading;

namespace OpenNos.Core.Networking.Communication.Scs.Server
{
    /// <summary>
    /// Represents a SCS server that is used to accept and manage client connections.
    /// </summary>
    public interface IScsServer
    {
        #region Events

        /// <summary>
        /// This event is raised when a new client connected to the server.
        /// </summary>
        event EventHandler<ServerClientEventArgs> ClientConnected;

        /// <summary>
        /// This event is raised when a client disconnected from the server.
        /// </summary>
        event EventHandler<ServerClientEventArgs> ClientDisconnected;

        #endregion

        #region Properties

        /// <summary>
        /// A collection of clients that are connected to the server.
        /// </summary>
        ThreadSafeSortedList<long, IScsServerClient> Clients { get; }

        /// <summary>
        /// Gets/sets wire protocol factory to create IWireProtocol objects.
        /// </summary>
        IScsWireProtocolFactory WireProtocolFactory { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Starts the server.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the server.
        /// </summary>
        void Stop();

        #endregion
    }
}