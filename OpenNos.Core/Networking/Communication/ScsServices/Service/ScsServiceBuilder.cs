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
using OpenNos.Core.Networking.Communication.Scs.Communication.EndPoints;
using OpenNos.Core.Networking.Communication.Scs.Server;

namespace OpenNos.Core.Networking.Communication.ScsServices.Service
{
    /// <summary>
    /// This class is used to build ScsService applications.
    /// </summary>
    public static class ScsServiceBuilder
    {
        #region Methods

        /// <summary>
        /// Creates a new SCS Service application using an EndPoint.
        /// </summary>
        /// <param name="endPoint">EndPoint that represents address of the service</param>
        /// <returns>Created SCS service application</returns>
        public static IScsServiceApplication CreateService(ScsEndPoint endPoint) => new ScsServiceApplication(ScsServerFactory.CreateServer(endPoint));

        #endregion
    }
}