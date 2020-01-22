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
using OpenNos.Core.Networking.Communication.Scs.Communication.Channels;
using OpenNos.Core.Networking.Communication.Scs.Communication.Channels.Tcp;
using OpenNos.Core.Networking.Communication.Scs.Communication.EndPoints.Tcp;
using System.Net;
using System.Net.Sockets;

namespace OpenNos.Core.Networking.Communication.Scs.Client.Tcp
{
    /// <summary>
    /// This class is used to communicate with server over TCP/IP protocol.
    /// </summary>
    public class ScsTcpClient : ScsClientBase
    {
        #region Members

        /// <summary>
        /// The endpoint address of the server.
        /// </summary>
        private readonly ScsTcpEndPoint _serverEndPoint;

        /// <summary>
        /// The existing socket information or <c>null</c>.
        /// </summary>
        private SocketInformation? _existingSocketInformation;

        #endregion

        #region Instantiation

        /// <summary>
        /// Creates a new ScsTcpClient object.
        /// </summary>
        /// <param name="serverEndPoint">The endpoint address to connect to the server</param>
        /// <param name="existingSocketInformation">The existing socket information.</param>
        public ScsTcpClient(ScsTcpEndPoint serverEndPoint, SocketInformation? existingSocketInformation)
        {
            _serverEndPoint = serverEndPoint;
            _existingSocketInformation = existingSocketInformation;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a communication channel using ServerIpAddress and ServerPort.
        /// </summary>
        /// <returns>Ready communication channel to communicate</returns>
        protected override ICommunicationChannel CreateCommunicationChannel()
        {
            Socket socket;

            if (_existingSocketInformation.HasValue)
            {
                socket = new Socket(_existingSocketInformation.Value);
                _existingSocketInformation = null;
            }
            else
            {
                socket = TcpHelper.ConnectToServer(new IPEndPoint(_serverEndPoint.IpAddress, _serverEndPoint.TcpPort), ConnectTimeout);
            }

            return new TcpCommunicationChannel(socket);
        }

        #endregion
    }
}