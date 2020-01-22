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
using OpenNos.Core.Networking.Communication.Scs.Communication.Messengers;
using OpenNos.Core.Networking.Communication.Scs.Server;

namespace OpenNos.Core.Networking.Communication.ScsServices.Service
{
    /// <summary>
    /// This class is used to create service client objects that is used in server-side.
    /// </summary>
    public static class ScsServiceClientFactory
    {
        #region Methods

        /// <summary>
        /// Creates a new service client object that is used in server-side.
        /// </summary>
        /// <param name="serverClient">Underlying server client object</param>
        /// <param name="requestReplyMessenger">
        /// RequestReplyMessenger object to send/receive messages over serverClient
        /// </param>
        /// <returns></returns>
        public static IScsServiceClient CreateServiceClient(IScsServerClient serverClient, RequestReplyMessenger<IScsServerClient> requestReplyMessenger) => new ScsServiceClient(serverClient, requestReplyMessenger);

        #endregion
    }
}