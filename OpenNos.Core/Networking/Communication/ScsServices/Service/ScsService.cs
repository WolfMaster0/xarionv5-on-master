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

namespace OpenNos.Core.Networking.Communication.ScsServices.Service
{
    /// <summary>
    /// Base class for all services that is serviced by IScsServiceApplication. A class must be
    /// derived from ScsService to serve as a SCS service.
    /// </summary>
    public abstract class ScsService
    {
        #region Members

        /// <summary>
        /// The current client for a thread that called service method.
        /// </summary>
        [ThreadStatic]
        private static IScsServiceClient _currentClient;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current client which called this service method.
        /// </summary>
        /// <remarks>
        /// This property is thread-safe, if returns correct client when called in a service method
        /// if the method is called by SCS system, else throws exception.
        /// </remarks>
        public IScsServiceClient CurrentClient
        {
            get => GetCurrentClient();
            set => _currentClient = value;
        }

        #endregion

        #region Methods

        private static IScsServiceClient GetCurrentClient()
        {
            if (_currentClient != null)
            {
                return _currentClient;
            }
            throw new ArgumentNullException(string.Empty, "Client channel can not be obtained. CurrentClient property must be called by the thread which runs the service method.");
        }

        #endregion
    }
}