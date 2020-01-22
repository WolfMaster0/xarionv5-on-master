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

using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$Teleport", Authority = AuthorityType.GameMaster)]
    public class TeleportPacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public string Data { get; set; }

        public short X { get; set; }

        public short Y { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            if (session is ClientSession sess)
            {
                string[] packetSplit = packet.Split(' ');
                if (packetSplit.Length < 3)
                {
                    sess.SendPacket(sess.Character.GenerateSay(ReturnHelp(), 10));
                    return;
                }
                TeleportPacket packetDefinition = new TeleportPacket();
                if (!string.IsNullOrWhiteSpace(packetSplit[2]))
                {
                    packetDefinition._isParsed = true;
                    packetDefinition.Data = packetSplit[2];
                    if (packetSplit.Length > 4 && short.TryParse(packetSplit[3], out short x) && short.TryParse(packetSplit[4], out short y))
                    {
                        packetDefinition.X = x;
                        packetDefinition.Y = y;
                    }
                }
                packetDefinition.ExecuteHandler(sess);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(TeleportPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$Teleport NAME / MAPID MAPX MAPY";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                session.Character.CloseShop();
                session.Character.CloseExchangeOrTrade();

                if (session.Character.IsChangingMapInstance)
                {
                    return;
                }

                ClientSession sess = ServerManager.Instance.GetSessionByCharacterName(Data);

                if (sess != null)
                {
                    Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(),
                        $"[Teleport]CharacterName: {Data}");

                    short mapX = sess.Character.PositionX;
                    short mapY = sess.Character.PositionY;
                    if (sess.Character.Miniland == sess.Character.MapInstance)
                    {
                        ServerManager.Instance.JoinMiniland(session, sess);
                    }
                    else
                    {
                        ServerManager.Instance.ChangeMapInstance(session.Character.CharacterId,
                            sess.Character.MapInstanceId, mapX, mapY);
                    }
                }
                else if (short.TryParse(Data, out short mapId))
                {
                    Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(),
                        $"[Teleport]MapId: {Data} MapX: {X} MapY: {Y}");
                    if (ServerManager.GetBaseMapInstanceIdByMapId(mapId) != default)
                    {
                        ServerManager.Instance.ChangeMap(session.Character.CharacterId, mapId, X, Y);
                    }
                    else
                    {
                        session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("MAP_NOT_FOUND"), 0));
                    }
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