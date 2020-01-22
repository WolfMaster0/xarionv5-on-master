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

namespace OpenNos.Core.Networking.Communication.Scs.Server.Tcp
{
    /// <summary>
    /// This class is used to create a TCP server.
    /// </summary>
    public class ScsTcpServer : ScsServerBase
    {
        #region Members

        /// <summary>
        /// The endpoint address of the server to listen incoming connections.
        /// </summary>
        private readonly ScsTcpEndPoint _endPoint;

        #endregion

        #region Instantiation

        /// <summary>
        /// Creates a new ScsTcpServer object.
        /// </summary>
        /// <param name="endPoint">The endpoint address of the server to listen incoming connections</param>
        public ScsTcpServer(ScsTcpEndPoint endPoint) => _endPoint = endPoint;

        #endregion

        #region Methods

        /// <summary>
        /// Creates a TCP connection listener.
        /// </summary>
        /// <returns>Created listener object</returns>
        protected override IConnectionListener CreateConnectionListener() => new TcpConnectionListener(_endPoint);

        #endregion
    }
}