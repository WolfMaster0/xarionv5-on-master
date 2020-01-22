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
using Hik.Communication.ScsServices.Service;
using OpenNos.Bazaar.Server.Networking;
using System;
using System.Configuration;
using System.Security.Authentication;
using System.Threading;

namespace OpenNos.Bazaar.Server
{
    public class BazaarService : ScsService, IBazaarService
    {
        #region Methods

        public bool Authenticate(string authKey)
        {
            if (string.IsNullOrWhiteSpace(authKey))
            {
                throw new ArgumentNullException(nameof(authKey), "AuthKey cannot be null or whitespace, provide the service with adequate authentification key.");
            }

            if (authKey == ConfigurationManager.AppSettings["BazaarKey"])
            {
                BazaarManager.Instance.AuthentificatedClients.Add(CurrentClient.ClientId);
                return true;
            }
            else
            {
                throw new AuthenticationException("Authentication failed, Key provided was wrong, or Service configuration could not be found.");
            }

            throw new AuthenticationException("Authentication failed");
        }

        public void BazaarRefresh(long bazaarItemId)
        {
            BazaarManager.Instance.InBazaarRefreshMode = true;
            // for all connected clients update this
            BazaarManager.Instance.RefreshBazaar(bazaarItemId);
            //CommunicationServiceClient.Instance.UpdateBazaar(ServerGroup, bazaarItemId);
            SpinWait.SpinUntil(() => !BazaarManager.Instance.InBazaarRefreshMode);
        }

        #endregion
    }
}