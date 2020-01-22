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
using System.Runtime.Serialization;

namespace OpenNos.Core.Networking.Communication.ScsServices.Communication.Messages
{
    /// <summary>
    /// Exception thrown when service invocation target errors.
    /// </summary>
    [Serializable]
    public class ScsRemoteInvocationException : ScsRemoteException
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ScsRemoteInvocationException"/> class.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="serviceVersion">The service version.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ScsRemoteInvocationException(string serviceType, string serviceVersion, string methodName, string message, Exception innerException)
            : base(message, innerException)
        {
            MethodName = methodName;
            ServiceType = serviceType;
            ServiceVersion = serviceVersion;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScsRemoteInvocationException"/> class.
        /// </summary>
        /// <param name="serializationInfo"></param>
        /// <param name="context"></param>
        protected ScsRemoteInvocationException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {
            MethodName = serializationInfo.GetString(nameof(MethodName));
            ServiceType = serializationInfo.GetString(nameof(ServiceType));
            ServiceVersion = serializationInfo.GetString(nameof(ServiceVersion));
        }

        public ScsRemoteInvocationException()
        {
        }

        public ScsRemoteInvocationException(string message) : base(message)
        {
        }

        public ScsRemoteInvocationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the name of the invoked method.
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// Gets the type of the service class.
        /// </summary>
        public string ServiceType { get; }

        /// <summary>
        /// Gets the service version.
        /// </summary>
        public string ServiceVersion { get; }

        #endregion

        #region Overrides of Exception

        /// <inheritdoc />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(MethodName), MethodName);
            info.AddValue(nameof(ServiceType), ServiceType);
            info.AddValue(nameof(ServiceVersion), ServiceVersion);
        }

        #endregion
    }
}