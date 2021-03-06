﻿// This file is part of the OpenNos NosTale Emulator Project.
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
using OpenNos.Core.Handling;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenNos.Domain;

namespace OpenNos.Core.Serializing
{
    public static class PacketFacility
    {
        #region Members

        private static Dictionary<string[], HandlerMethodReference> _handlerInfo;

        private static Dictionary<string[], Action<object, string>> _handlers;

        private static Dictionary<HandlerMethodReference, Func<string>> _helpMessages;

        #endregion

        #region Properties

        public static bool IsInitialized { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the PacketFactory and generates the serialization informations based on the
        /// given header and action.
        /// </summary>
        /// <param name="type">Type of the packet to handle</param>
        /// <param name="action">Action executed at packet handling</param>
        /// <param name="helper">Action executed at help generation</param>
        public static void AddHandler(Type type, Action<object, string> action, Func<string> helper = null)
        {
            if (!IsInitialized)
            {
                _handlers = new Dictionary<string[], Action<object, string>>();
                _handlerInfo = new Dictionary<string[], HandlerMethodReference>();
                _helpMessages = new Dictionary<HandlerMethodReference, Func<string>>();
                IsInitialized = true;
            }

            HandlerMethodReference methodReference = new HandlerMethodReference(type);
            if (!_handlers.ContainsKey(methodReference.Identification))
            {
                _handlers.Add(methodReference.Identification, action);
            }
            if (!_handlerInfo.ContainsKey(methodReference.Identification))
            {
                _handlerInfo.Add(methodReference.Identification, methodReference);
            }
            if (helper != null && !_helpMessages.ContainsKey(methodReference))
            {
                _helpMessages.Add(methodReference, helper);
            }
        }

        /// <summary>
        /// Gets the <see cref="HandlerMethodReference"/> from given header.
        /// </summary>
        /// <param name="header"></param>
        /// <returns><see cref="HandlerMethodReference"/></returns>
        public static HandlerMethodReference GetHandlerMethodReference(string header) => _handlerInfo.FirstOrDefault(h => h.Key.Contains(header.ToLower())).Value;

        /// <summary>
        /// Handles received packet with given header
        /// </summary>
        /// <param name="session"></param>
        /// <param name="header"></param>
        /// <param name="packet"></param>
        public static void HandlePacket(object session, string header, string packet)
        {
            if (_handlers.Any(h => h.Key.Contains(header)))
            {
                _handlers.FirstOrDefault(h => h.Key.Contains(header)).Value(session, packet);
            }
        }

        public static IEnumerable<string> GetHelpMessages(AuthorityType authorityType) => _helpMessages.Where(s => s.Key.Authority <= authorityType).Select(s => s.Value());

        public static void Initialize(Type type)
        {
            foreach (Type t in type.Assembly.GetTypes().Where(s => s.Namespace == type.Namespace))
            {
                t.GetMethod("Register")?.Invoke(null, null);
            }
        }

        #endregion
    }
}