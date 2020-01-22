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
using OpenNos.SCS.Communication.Scs.Communication.EndPoints.Tcp;
using OpenNos.SCS.Communication.ScsServices.Service;
using System;

namespace OpenNos.Master.Library.Data
{
    public class WorldServer
    {
        #region Instantiation

        public WorldServer(Guid id, ScsTcpEndPoint endpoint, int accountLimit, string worldGroup)
        {
            Id = id;
            Endpoint = endpoint;
            AccountLimit = accountLimit;
            WorldGroup = worldGroup;
        }

        #endregion

        #region Properties

        public int AccountLimit { get; }

        public int ChannelId { get; set; }

        public ScsTcpEndPoint Endpoint { get; }

        public Guid Id { get; }

        public IScsServiceClient CommunicationServiceClient { get; set; }

        public IScsServiceClient ConfigurationServiceClient { get; set; }

        public IScsServiceClient MailServiceClient { get; set; }

        public string WorldGroup { get; }

        #endregion
    }
}