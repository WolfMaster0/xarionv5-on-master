﻿// This file is part of the OpenNos NosTale Emulator Project.
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

using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$RemovePortal", Authority = AuthorityType.GameMaster)]
    public class RemovePortalPacket
    {
        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            RemovePortalPacket packetDefinition = new RemovePortalPacket();
            packetDefinition.ExecuteHandler(session as ClientSession);
        }

        public static void Register() => PacketFacility.AddHandler(typeof(RemovePortalPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$RemovePortal";

        private void ExecuteHandler(ClientSession session)
        {
            if (session.HasCurrentMapInstance)
            {
                Portal portal = session.CurrentMapInstance.Portals.Find(s =>
                    s.SourceMapInstanceId == session.Character.MapInstanceId && Map.GetDistance(
                        new MapCell { X = s.SourceX, Y = s.SourceY },
                        new MapCell { X = session.Character.PositionX, Y = session.Character.PositionY }) < 10);
                if (portal != null)
                {
                    Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(),
                        $"[RemovePortal]MapId: {portal.SourceMapId} MapX: {portal.SourceX} MapY: {portal.SourceY}");
                    session.SendPacket(session.Character.GenerateSay(
                        string.Format(Language.Instance.GetMessageFromKey("NEAREST_PORTAL"), portal.SourceMapId,
                            portal.SourceX, portal.SourceY), 12));
                    if (DAOFactory.PortalDAO.LoadById(portal.PortalId) != null)
                    {
                        DAOFactory.PortalDAO.DeleteById(portal.PortalId);
                    }
                    session.CurrentMapInstance.Portals.Remove(portal);
                    portal.SourceX = -1;
                    portal.SourceY = -1;
                    portal.IsDisabled = true;
                    session.CurrentMapInstance.Broadcast(portal.GenerateGp());
                }
                else
                {
                    session.SendPacket(
                        session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NO_PORTAL_FOUND"), 11));
                }
            }
        }

        #endregion
    }
}