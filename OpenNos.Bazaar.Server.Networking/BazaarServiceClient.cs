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
using OpenNos.Core;
using OpenNos.Core.Networking.Communication.Scs.Communication;
using OpenNos.Core.Networking.Communication.Scs.Communication.EndPoints.Tcp;
using OpenNos.Core.Networking.Communication.ScsServices.Client;
using System;
using System.Configuration;

namespace OpenNos.Bazaar.Server.Networking
{
    public class BazaarServiceClient : IBazaarService
    {
        #region Members

        private static BazaarServiceClient _instance;

        private readonly IScsServiceClient<IBazaarService> _client;

        #endregion

        #region Instantiation

        public BazaarServiceClient()
        {
            string ip = ConfigurationManager.AppSettings["BazaarServerIP"];
            int port = Convert.ToInt32(ConfigurationManager.AppSettings["BazaarServerPort"]);
            _client = ScsServiceClientBuilder.CreateClient<IBazaarService>(new ScsTcpEndPoint(ip, port));
            System.Threading.Thread.Sleep(1000);
            while (_client.CommunicationState != CommunicationStates.Connected)
            {
                try
                {
                    _client.Connect();
                }
                catch (Exception)
                {
                    Logger.Error(Language.Instance.GetMessageFromKey("RETRY_CONNECTION"), memberName: nameof(BazaarServiceClient));
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }

        #endregion

        #region Properties

        public static BazaarServiceClient Instance => _instance ?? (_instance = new BazaarServiceClient());

        public CommunicationStates CommunicationState => _client.CommunicationState;

        #endregion

        #region Methods

        public bool Authenticate(string authKey) => _client.ServiceProxy.Authenticate(authKey);

#pragma warning disable RCS1079 // Throwing of new NotImplementedException.
        public void BazaarRefresh(long bazaarItemId) => throw new NotImplementedException();
#pragma warning restore RCS1079 // Throwing of new NotImplementedException.

        #endregion
    }
}