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

namespace OpenNos.Core.Networking.Communication.ScsServices.Client
{
    /// <summary>
    /// This class is used to build service clients to remotely invoke methods of a SCS service.
    /// </summary>
    public static class ScsServiceClientBuilder
    {
        #region Methods

        /// <summary>
        /// Creates a client to connect to a SCS service.
        /// </summary>
        /// <typeparam name="T">Type of service interface for remote method call</typeparam>
        /// <param name="endpoint">EndPoint of the server</param>
        /// <param name="clientObject">
        /// Client-side object that handles remote method calls from server to client. May be null if
        /// client has no methods to be invoked by server
        /// </param>
        /// <returns>Created client object to connect to the server</returns>
        public static IScsServiceClient<T> CreateClient<T>(ScsEndPoint endpoint, object clientObject = null) where T : class => new ScsServiceClient<T>(endpoint.CreateClient(), clientObject);

        /// <summary>
        /// Creates a client to connect to a SCS service.
        /// </summary>
        /// <typeparam name="T">Type of service interface for remote method call</typeparam>
        /// <param name="endpointAddress">EndPoint address of the server</param>
        /// <param name="clientObject">
        /// Client-side object that handles remote method calls from server to client. May be null if
        /// client has no methods to be invoked by server
        /// </param>
        /// <returns>Created client object to connect to the server</returns>
        public static IScsServiceClient<T> CreateClient<T>(string endpointAddress, object clientObject = null) where T : class => CreateClient<T>(ScsEndPoint.CreateEndPoint(endpointAddress), clientObject);

        #endregion
    }
}