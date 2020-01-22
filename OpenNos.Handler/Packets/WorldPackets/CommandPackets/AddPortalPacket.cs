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
using OpenNos.GameObject.Networking;
using OpenNos.Handler.Packets.SharedPacketMethods;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$AddPortal", Authority = AuthorityType.GameMaster)]
    public class AddPortalPacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public short DestinationMapId { get; set; }

        public short DestinationX { get; set; }

        public short DestinationY { get; set; }

        public PortalType? PortalType { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (session is ClientSession sess)
            {
                if (packetSplit.Length < 5)
                {
                    sess.SendPacket(sess.Character.GenerateSay(ReturnHelp(), 10));
                    return;
                }

                AddPortalPacket packetDefinition = new AddPortalPacket();
                if (short.TryParse(packetSplit[2], out short mapId) && short.TryParse(packetSplit[3], out short mapX)
                    && short.TryParse(packetSplit[4], out short mapY))
                {
                    packetDefinition._isParsed = true;
                    packetDefinition.DestinationMapId = mapId;
                    packetDefinition.DestinationX = mapX;
                    packetDefinition.DestinationY = mapY;
                    if (packetSplit.Length > 5 && Enum.TryParse(packetSplit[5], out PortalType type))
                    {
                        packetDefinition.PortalType = type;
                    }
                }

                packetDefinition.ExecuteHandler(sess);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(AddPortalPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$AddPortal MAPID DESTX DESTY PORTALTYPE(?)";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(),
                    $"[AddPortal]DestinationMapId: {DestinationMapId} DestinationMapX: {DestinationX} DestinationY: {DestinationY}");

                session.AddPortal(DestinationMapId, DestinationX, DestinationY,
                    PortalType == null ? (short)-1 : (short)PortalType, true);
            }
            else
            {
                session.SendPacket(session.Character.GenerateSay(ReturnHelp(), 10));
            }
        }

        #endregion
    }
}