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
using OpenNos.Core.Threading;
using System;

namespace OpenNos.Core.Networking.Communication.Scs.Client
{
    /// <summary>
    /// This class is used to automatically re-connect to server if disconnected. It attempts to
    /// reconnect to server periodically until connection established.
    /// </summary>
    public class ClientReConnecter : IDisposable
    {
        #region Members

        /// <summary>
        /// Reference to client object.
        /// </summary>
        private readonly IConnectableClient _client;

        /// <summary>
        /// Timer to attempt ro reconnect periodically.
        /// </summary>
        private readonly Timer _reconnectTimer;

        /// <summary>
        /// Indicates the dispose state of this object.
        /// </summary>
        private volatile bool _disposed;

        #endregion

        #region Instantiation

        /// <summary>
        /// Creates a new ClientReConnecter object. It is not needed to start ClientReConnecter since
        /// it automatically starts when the client disconnected.
        /// </summary>
        /// <param name="client">Reference to client object</param>
        /// <exception cref="ArgumentNullException">
        /// Throws ArgumentNullException if client is null.
        /// </exception>
        public ClientReConnecter(IConnectableClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _client.Disconnected += Client_Disconnected;
            _reconnectTimer = new Timer(20000);
            _reconnectTimer.Elapsed += ReconnectTimer_Elapsed;
            _reconnectTimer.Start();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Reconnect check period.
        /// Default: 20 seconds.
        /// </summary>
        public int ReConnectCheckPeriod
        {
            get => _reconnectTimer.Period;
            set => _reconnectTimer.Period = value;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Disposes this object. Does nothing if already disposed.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _client.Disconnected -= Client_Disconnected;
                    _reconnectTimer.Stop();
                    _client.Dispose();
                    _reconnectTimer.Dispose();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Handles Disconnected event of _client object.
        /// </summary>
        /// <param name="sender">Source of the event</param>
        /// <param name="e">Event arguments</param>
        private void Client_Disconnected(object sender, EventArgs e) => _reconnectTimer.Start();

        /// <summary>
        /// Hadles Elapsed event of _reconnectTimer.
        /// </summary>
        /// <param name="sender">Source of the event</param>
        /// <param name="e">Event arguments</param>
        private void ReconnectTimer_Elapsed(object sender, EventArgs e)
        {
            if (_disposed || _client.CommunicationState == CommunicationStates.Connected)
            {
                _reconnectTimer.Stop();
                return;
            }

            try
            {
                _client.Connect();
                _reconnectTimer.Stop();
            }
            catch
            {
                // No need to catch since it will try to re-connect again
            }
        }

        #endregion
    }
}