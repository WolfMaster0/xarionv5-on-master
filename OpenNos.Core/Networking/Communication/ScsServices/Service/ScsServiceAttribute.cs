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
    /// Any SCS Service interface class must has this attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
    public sealed class ScsServiceAttribute : Attribute
    {
        #region Instantiation

        /// <summary>
        /// Creates a new ScsServiceAttribute object.
        /// </summary>
        public ScsServiceAttribute() => Version = "NO_VERSION";

        #endregion

        #region Properties

        /// <summary>
        /// Service Version. This property can be used to indicate the code version. This value is
        /// sent to client application on an exception, so, client application can know that service
        /// version is changed. Default value: NO_VERSION.
        /// </summary>
        public string Version { get; set; }

        #endregion
    }
}