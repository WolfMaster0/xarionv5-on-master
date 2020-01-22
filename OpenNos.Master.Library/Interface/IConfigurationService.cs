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
using OpenNos.SCS.Communication.ScsServices.Service;
using OpenNos.Master.Library.Data;
using System;

namespace OpenNos.Master.Library.Interface
{
    [ScsService(Version = "1.1.0.0")]
    public interface IConfigurationService
    {
        /// <summary>
        /// Authenticates a Client to the Service
        /// </summary>
        /// <param name="authKey">The private Authentication key</param>
        /// <param name="serverId"></param>
        /// <returns>true if successful, else false</returns>
        bool Authenticate(string authKey, Guid serverId);

        /// <summary>
        /// Get the Configuration Object from the Service
        /// </summary>
        /// <returns></returns>
        ConfigurationObject GetConfigurationObject();

        /// <summary>
        /// Gets the Session Limit without having to authenticate first (Workaround for the Authentication issue)
        /// </summary>
        /// <returns>Session Limit</returns>
        int GetSlotCount();

        /// <summary>
        /// Update the Configuration Object to the Service
        /// </summary>
        /// <param name="configurationObject"></param>
        void UpdateConfigurationObject(ConfigurationObject configurationObject);
    }
}