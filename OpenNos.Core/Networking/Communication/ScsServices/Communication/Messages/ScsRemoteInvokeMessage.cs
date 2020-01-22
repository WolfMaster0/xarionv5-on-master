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
using OpenNos.Core.Networking.Communication.Scs.Communication.Messages;
using System;

namespace OpenNos.Core.Networking.Communication.ScsServices.Communication.Messages
{
    /// <summary>
    /// This message is sent to invoke a method of a remote application.
    /// </summary>
    [Serializable]
    public class ScsRemoteInvokeMessage : ScsMessage
    {
        #region Properties

        /// <summary>
        /// Method of remote application to invoke.
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// Parameters of method.
        /// </summary>
        public object[] Parameters { get; set; }

        /// <summary>
        /// Name of the remove service class.
        /// </summary>
        public string ServiceClassName { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Represents this object as string.
        /// </summary>
        /// <returns>String representation of this object</returns>
        public override string ToString() => $"ScsRemoteInvokeMessage: {ServiceClassName}.{MethodName}(...)";

        #endregion
    }
}