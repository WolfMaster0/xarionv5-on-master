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
using System.Collections.Generic;
using System.Linq;
using OpenNos.Core;
using OpenNos.Core.Cryptography;
using OpenNos.Core.Networking;
using OpenNos.Core.Networking.Communication.Scs.Communication.EndPoints.Tcp;
using OpenNos.Core.Networking.Communication.Scs.Server;

namespace OpenNos.GameObject.Networking
{
    public class NetworkManager<TEncryptorT> : SessionManager where TEncryptorT : CryptographyBase
    {
        #region Members

        private readonly TEncryptorT _encryptor;

        private readonly CryptographyBase _fallbackEncryptor;

        private readonly IScsServer _server;

        private IDictionary<string, DateTime> _connectionLog;

        #endregion

        #region Instantiation

        public NetworkManager(string ipAddress, int port, Type fallbackEncryptor, bool isWorldServer) : base(isWorldServer)
        {
            _encryptor = (TEncryptorT)Activator.CreateInstance(typeof(TEncryptorT));

            if (fallbackEncryptor != null)
            {
                _fallbackEncryptor = (CryptographyBase)Activator.CreateInstance(fallbackEncryptor);
            }

            _server = ScsServerFactory.CreateServer(new ScsTcpEndPoint(ipAddress, port));

            // Register events of the server to be informed about clients
            _server.ClientConnected += OnServerClientConnected;
            _server.ClientDisconnected += OnServerClientDisconnected;
            _server.WireProtocolFactory = new WireProtocolFactory();

            // Start the server
            _server.Start();

            // ReSharper disable once ExplicitCallerInfoArgument
            Logger.Info(Language.Instance.GetMessageFromKey("STARTED"), memberName: "NetworkManager");
        }

        #endregion

        #region Properties

        private IDictionary<string, DateTime> ConnectionLog => _connectionLog ?? (_connectionLog = new Dictionary<string, DateTime>());

        #endregion

        #region Methods

        public override void StopServer()
        {
            _server.Stop();
            _server.ClientConnected -= OnServerClientConnected;
            _server.ClientDisconnected -= OnServerClientDisconnected;
        }

        protected override ClientSession IntializeNewSession(INetworkClient client)
        {
            if (!CheckGeneralLog(client))
            {
                Logger.Warn(string.Format(Language.Instance.GetMessageFromKey("FORCED_DISCONNECT"), client.ClientId));
                client.Initialize(_fallbackEncryptor);
                client.SendPacket($"fail {Language.Instance.GetMessageFromKey("CONNECTION_LOST")}");
                client.Disconnect();
                return null;
            }
            ClientSession session = new ClientSession(client);
            session.Initialize(_encryptor, IsWorldServer);
            return session;
        }

        private bool CheckGeneralLog(INetworkClient client)
        {
            if (!client.IpAddress.Contains("127.0.0.1") && ServerManager.Instance.ChannelId != 51)
            {
                if (ConnectionLog.Count > 0)
                {
                    foreach (KeyValuePair<string, DateTime> item in ConnectionLog.Where(cl => cl.Key.Contains(client.IpAddress.Split(':')[1]) && (DateTime.UtcNow - cl.Value).TotalSeconds > 3).ToList())
                    {
                        ConnectionLog.Remove(item.Key);
                    }
                }

                if (ConnectionLog.Any(c => c.Key.Contains(client.IpAddress.Split(':')[1])))
                {
                    return false;
                }
                ConnectionLog.Add(client.IpAddress, DateTime.UtcNow);
                return true;
            }

            return true;
        }

        private void OnServerClientConnected(object sender, ServerClientEventArgs e) => AddSession(e.Client as NetworkClient);

        private void OnServerClientDisconnected(object sender, ServerClientEventArgs e) => RemoveSession(e.Client as NetworkClient);

        #endregion
    }
}