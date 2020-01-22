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
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using System;

namespace OpenNos.Core.Handling
{
    public class HandlerMethodReference
    {
        #region Instantiation

        public HandlerMethodReference(Type packetBaseParameterType)
        {
            PacketDefinitionParameterType = packetBaseParameterType;
            PacketHeaderAttribute headerAttribute = (PacketHeaderAttribute)Array.Find(PacketDefinitionParameterType.GetCustomAttributes(true), ca => ca.GetType().Equals(typeof(PacketHeaderAttribute)));
            Amount = headerAttribute?.Amount ?? 1;
            Identification = headerAttribute?.Identification;
            PassNonParseablePacket = headerAttribute?.PassNonParseablePacket ?? false;
            CharacterRequired = headerAttribute?.CharacterRequired ?? true;
            Authority = headerAttribute?.Authority ?? AuthorityType.User;
        }

        #endregion

        #region Properties

        public int Amount { get; }

        public AuthorityType Authority { get; }

        public bool CharacterRequired { get; }

        /// <summary>
        /// String identification of the Packet
        /// </summary>
        public string[] Identification { get; }

        public Type PacketDefinitionParameterType { get; }

        public bool PassNonParseablePacket { get; }

        #endregion
    }
}