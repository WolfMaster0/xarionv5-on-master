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
using OpenNos.Domain;
using System;
using System.Linq;

namespace OpenNos.Core.Serializing
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class PacketHeaderAttribute : Attribute
    {
        #region Instantiation

        public PacketHeaderAttribute(int amount = 1, params string[] identification)
        {
            Identification = identification.Select(t => t.ToLower()).ToArray();
            Amount = amount;
        }

        public PacketHeaderAttribute(params string[] identification) => Identification = identification.Select(t => t.ToLower()).ToArray();

        #endregion

        #region Properties

        /// <summary>
        /// Amount of required packets
        /// </summary>
        public int Amount { get; }

        /// <summary>
        /// Permission to handle the packet
        /// </summary>
        public AuthorityType Authority { get; set; }

        /// <summary>
        /// String identification of the Packet
        /// </summary>
        public string[] Identification { get; set; }

        /// <summary>
        /// Specifies if packet needs character to execute handling
        /// </summary>
        public bool CharacterRequired { get; set; }

        /// <summary>
        /// Pass the packet to handler method even if the serialization has failed.
        /// </summary>
        public bool PassNonParseablePacket { get; set; }

        #endregion
    }
}
