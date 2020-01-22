// This file is part of the OpenNos NosTale Emulator Project.
//
// This program is licensed under a deviated version of the Fair Source License, granting you a
// non-exclusive, non-transferable, royalty-free and fully-paid-up license, under all of the
// Licensor's copyright and patent rights, to use, copy, prepare derivative works of, publicly
// perform and display the Software, subject to the conditions found in the LICENSE file.
//
// THIS FILE IS PROVIDED "AS IS", WITHOUT WARRANTY OR CONDITION, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. THE AUTHORS HEREBY DISCLAIM ALL LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT
// OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE.

using System;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$Rarify", Authority = AuthorityType.GameMaster)]
    public class RarifyPacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public RarifyMode Mode { get; set; }

        public RarifyProtection Protection { get; set; }

        public short Slot { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            if (session is ClientSession sess)
            {
                string[] packetSplit = packet.Split(' ');
                if (packetSplit.Length < 5)
                {
                    sess.SendPacket(sess.Character.GenerateSay(ReturnHelp(), 10));
                    return;
                }
                RarifyPacket packetDefinition = new RarifyPacket();
                if (short.TryParse(packetSplit[2], out short slot) && Enum.TryParse(packetSplit[3], out RarifyMode mode) && Enum.TryParse(packetSplit[4], out RarifyProtection protection))
                {
                    packetDefinition._isParsed = true;
                    packetDefinition.Slot = slot;
                    packetDefinition.Mode = mode;
                    packetDefinition.Protection = protection;
                }
                packetDefinition.ExecuteHandler(sess);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(RarifyPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$Rarify SLOT MODE PROTECTION";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(),
                    $"[Rarify]Slot: {Slot} Mode: {Mode} Protection: {Protection}");

                if (Slot >= 0)
                {
                    ItemInstance wearableInstance = session.Character.Inventory.LoadBySlotAndType(Slot, 0);
                    wearableInstance?.RarifyItem(session, Mode, Protection);
                }
            }
            else
            {
                session.SendPacket(session.Character.GenerateSay(ReturnHelp(), 10));
            }
        }

        #endregion
    }
}