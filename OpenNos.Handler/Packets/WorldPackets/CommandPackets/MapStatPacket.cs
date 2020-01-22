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

using System.Linq;
using OpenNos.Core.Serializing;
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$MapStat", Authority = AuthorityType.GameMaster)]
    public class MapStatPacket
    {
        #region Properties

        public short? MapId { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            if (session is ClientSession sess)
            {
                string[] packetSplit = packet.Split(' ');
                MapStatPacket packetDefinition = new MapStatPacket
                {
                    MapId = packetSplit.Length >= 3 && short.TryParse(packetSplit[2], out short mapId) ? mapId : (short?)null
                };
                packetDefinition.ExecuteHandler(sess);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(MapStatPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$MapStat MAPID(?)";

        private void ExecuteHandler(ClientSession session)
        {
            void SendMapStats(MapDTO map, MapInstance mapInstance)
            {
                if (map != null && mapInstance != null)
                {
                    session.SendPacket(session.Character.GenerateSay("-------------MapData-------------", 10));
                    session.SendPacket(session.Character.GenerateSay(
                        $"MapId: {map.MapId}\n" +
                        $"MapMusic: {map.Music}\n" +
                        $"MapName: {map.Name}\n" +
                        $"MapShopAllowed: {map.ShopAllowed}", 10));
                    session.SendPacket(session.Character.GenerateSay("---------------------------------", 10));
                    session.SendPacket(session.Character.GenerateSay("---------MapInstanceData---------", 10));
                    session.SendPacket(session.Character.GenerateSay(
                        $"MapInstanceId: {mapInstance.MapInstanceId}\n" +
                        $"MapInstanceType: {mapInstance.MapInstanceType}\n" +
                        $"MapMonsterCount: {mapInstance.Monsters.Count}\n" +
                        $"MapNpcCount: {mapInstance.Npcs.Count}\n" +
                        $"MapPortalsCount: {mapInstance.Portals.Count}\n" +
                        $"MapInstanceUserShopCount: {mapInstance.UserShops.Count}\n" +
                        $"SessionCount: {mapInstance.Sessions.Count()}\n" +
                        $"MapInstanceXpRate: {mapInstance.XpRate}\n" +
                        $"MapInstanceDropRate: {mapInstance.DropRate}\n" +
                        $"MapInstanceMusic: {mapInstance.InstanceMusic}\n" +
                        $"ShopsAllowed: {mapInstance.ShopAllowed}\n" +
                        $"IsPVP: {mapInstance.IsPvp}\n" +
                        $"IsSleeping: {mapInstance.IsSleeping}\n" +
                        $"Dance: {mapInstance.IsDancing}", 10));
                    session.SendPacket(session.Character.GenerateSay("---------------------------------", 10));
                }
            }

            if (MapId.HasValue)
            {
                MapDTO map = DAOFactory.MapDAO.LoadById(MapId.Value);
                MapInstance mapInstance = ServerManager.GetMapInstanceByMapId(MapId.Value);
                if (map != null && mapInstance != null)
                {
                    SendMapStats(map, mapInstance);
                }
            }
            else if (session.HasCurrentMapInstance)
            {
                MapDTO map = DAOFactory.MapDAO.LoadById(session.CurrentMapInstance.Map.MapId);
                if (map != null)
                {
                    SendMapStats(map, session.CurrentMapInstance);
                }
            }
        }

        #endregion
    }
}