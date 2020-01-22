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
using OpenNos.Master.Library.Data;
using OpenNos.Master.Library.Interface;
using OpenNos.SCS.Communication.Scs.Communication;
using OpenNos.SCS.Communication.Scs.Communication.EndPoints.Tcp;
using OpenNos.SCS.Communication.ScsServices.Client;
using System;
using System.Configuration;

namespace OpenNos.Master.Library.Client
{
    public class ConfigurationServiceClient : IConfigurationService
    {
        #region Members

        private static ConfigurationServiceClient _instance;

        private readonly IScsServiceClient<IConfigurationService> _client;

        #endregion

        #region Instantiation

        public ConfigurationServiceClient()
        {
            string ip = ConfigurationManager.AppSettings["MasterIP"];
            int port = Convert.ToInt32(ConfigurationManager.AppSettings["MasterPort"]);
            ConfigurationClient confClient = new ConfigurationClient();
            _client = ScsServiceClientBuilder.CreateClient<IConfigurationService>(new ScsTcpEndPoint(ip, port), confClient);
            System.Threading.Thread.Sleep(1000);
            while (_client.CommunicationState != CommunicationStates.Connected)
            {
                try
                {
                    _client.Connect();
                }
                catch (Exception)
                {
                    Logger.Error(Language.Instance.GetMessageFromKey("RETRY_CONNECTION"), memberName: nameof(CommunicationServiceClient));
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }

        #endregion

        #region Events

        public event EventHandler ConfigurationUpdate;

        #endregion

        #region Properties

        public static ConfigurationServiceClient Instance => _instance ?? (_instance = new ConfigurationServiceClient());

        public CommunicationStates CommunicationState => _client.CommunicationState;

        #endregion

        #region Methods

        public bool Authenticate(string authKey, Guid serverId) => _client.ServiceProxy.Authenticate(authKey, serverId);

        public void UpdateConfigurationObject(ConfigurationObject configurationObject) => _client.ServiceProxy.UpdateConfigurationObject(configurationObject);

        public ConfigurationObject GetConfigurationObject() => _client.ServiceProxy.GetConfigurationObject();

        public int GetSlotCount() => _client.ServiceProxy.GetSlotCount();

        internal void OnConfigurationUpdated(ConfigurationObject configurationObject) => ConfigurationUpdate?.Invoke(configurationObject, null);

        #endregion
    }
}