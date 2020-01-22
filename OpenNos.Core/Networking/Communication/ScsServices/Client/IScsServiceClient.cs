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
using OpenNos.Core.Networking.Communication.Scs.Client;
using OpenNos.Core.Networking.Communication.Scs.Communication.Channels;

namespace OpenNos.Core.Networking.Communication.ScsServices.Client
{
    /// <summary>
    /// Represents a service client that consumes a SCS service.
    /// </summary>
    /// <typeparam name="T">Type of service interface</typeparam>
    public interface IScsServiceClient<out T> : IConnectableClient where T : class
    {
        #region Properties

        /// <summary>
        /// Gets the communication channel for client.
        /// </summary>
        ICommunicationChannel CommunicationChannel { get; }

        /// <summary>
        /// Reference to the service proxy to invoke remote service methods.
        /// </summary>
        T ServiceProxy { get; }

        /// <summary>
        /// Timeout value when invoking a service method. If timeout occurs before end of remote
        /// method call, an exception is thrown. Use -1 for no timeout (wait indefinite). Default
        /// value: 60000 (1 minute).
        /// </summary>
        int Timeout { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a service proxy for the specified <typeparamref name="TServiceInterface"/>.
        /// </summary>
        /// <typeparam name="TServiceInterface">the service interface type</typeparam>
        /// <returns></returns>
        TServiceInterface GetServiceProxy<TServiceInterface>();

        #endregion
    }
}