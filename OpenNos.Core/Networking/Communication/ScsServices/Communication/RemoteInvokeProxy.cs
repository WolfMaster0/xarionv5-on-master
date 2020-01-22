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
using OpenNos.Core.Networking.Communication.ScsServices.Communication.Messages;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace OpenNos.Core.Networking.Communication.ScsServices.Communication
{
    /// <summary>
    /// This class is used to generate a dynamic proxy to invoke remote methods. It translates method
    /// invocations to messaging.
    /// </summary>
    /// <typeparam name="TProxy">Type of the proxy class/interface</typeparam>
    /// <typeparam name="TMessenger">
    /// Type of the messenger object that is used to send/receive messages
    /// </typeparam>
    public class RemoteInvokeProxy<TProxy, TMessenger> : RealProxy where TMessenger : IMessenger
    {
        #region Members

        /// <summary>
        /// Messenger object that is used to send/receive messages.
        /// </summary>
        private readonly RequestReplyMessenger<TMessenger> _clientMessenger;

        #endregion

        #region Instantiation

        /// <summary>
        /// Creates a new RemoteInvokeProxy object.
        /// </summary>
        /// <param name="clientMessenger">Messenger object that is used to send/receive messages</param>
        public RemoteInvokeProxy(RequestReplyMessenger<TMessenger> clientMessenger) : base(typeof(TProxy)) => _clientMessenger = clientMessenger;

        #endregion

        #region Methods

        /// <summary>
        /// Overrides message calls and translates them to messages to remote application.
        /// </summary>
        /// <param name="msg">Method invoke message (from RealProxy base class)</param>
        /// <returns>Method invoke return message (to RealProxy base class)</returns>
        public override IMessage Invoke(IMessage msg)
        {
            if (!(msg is IMethodCallMessage message))
            {
                return null;
            }

            ScsRemoteInvokeMessage requestMessage = new ScsRemoteInvokeMessage
            {
                ServiceClassName = typeof(TProxy).Name,
                MethodName = message.MethodName,
                //Parameters = message.InArgs
                Parameters = message.Args
            };

            if (!(_clientMessenger.SendMessageAndWaitForResponse(requestMessage, 10) is ScsRemoteInvokeReturnMessage responseMessage))
            {
                return null;
            }

            object[] args = null;
            int length = 0;

            if (responseMessage.Parameters != null)
            {
                args = responseMessage.Parameters;
                length = args.Length;
            }

            return responseMessage.RemoteException != null
                       ? new ReturnMessage(responseMessage.RemoteException, message)
                       : new ReturnMessage(responseMessage.ReturnValue, args, length, message.LogicalCallContext, message);
        }

        #endregion
    }
}