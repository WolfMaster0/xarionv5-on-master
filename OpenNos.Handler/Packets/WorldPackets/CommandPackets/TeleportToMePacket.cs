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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$TeleportToMe", Authority = AuthorityType.GameMaster)]
    public class TeleportToMePacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public string CharacterName { get; set; }

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
                TeleportToMePacket packetDefinition = new TeleportToMePacket();
                if (!string.IsNullOrWhiteSpace(packetSplit[2]))
                {
                    packetDefinition._isParsed = true;
                    packetDefinition.CharacterName = packetSplit[2];
                }
                packetDefinition.ExecuteHandler(sess);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(TeleportToMePacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$TeleportToMe NAME";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(),
                    $"[TeleportToMe]CharacterName: {CharacterName}");

                if (CharacterName == "*")
                {
                    Parallel.ForEach(
                        ServerManager.Instance.Sessions.Where(s =>
                            s.Character != null && s.Character.CharacterId != session.Character.CharacterId), sess =>
                        {
                            // clear any shop or trade on target character
                            sess.Character.CloseShop();
                            sess.Character.CloseExchangeOrTrade();
                            if (!sess.Character.IsChangingMapInstance && session.HasCurrentMapInstance)
                            {
                                List<MapCell> possibilities = new List<MapCell>();
                                for (short x = -6, y = -6; x < 6 && y < 6; x++, y++)
                                {
                                    possibilities.Add(new MapCell { X = x, Y = y });
                                }

                                short mapXPossibility = session.Character.PositionX;
                                short mapYPossibility = session.Character.PositionY;
                                foreach (MapCell possibility in possibilities.OrderBy(s => ServerManager.RandomNumber()))
                                {
                                    mapXPossibility = (short)(session.Character.PositionX + possibility.X);
                                    mapYPossibility = (short)(session.Character.PositionY + possibility.Y);
                                    if (!session.CurrentMapInstance.Map.IsBlockedZone(mapXPossibility, mapYPossibility))
                                    {
                                        break;
                                    }
                                }

                                if (session.Character.Miniland == session.Character.MapInstance)
                                {
                                    ServerManager.Instance.JoinMiniland(sess, session);
                                }
                                else
                                {
                                    ServerManager.Instance.ChangeMapInstance(sess.Character.CharacterId,
                                        session.Character.MapInstanceId, mapXPossibility, mapYPossibility);
                                }
                            }
                        });
                }
                else
                {
                    ClientSession targetSession =
                        ServerManager.Instance.GetSessionByCharacterName(CharacterName);
                    if (targetSession?.Character.IsChangingMapInstance == false)
                    {
                        targetSession.Character.CloseShop();
                        targetSession.Character.CloseExchangeOrTrade();
                        ServerManager.Instance.ChangeMapInstance(targetSession.Character.CharacterId,
                            session.Character.MapInstanceId, (short)(session.Character.PositionX + 1),
                            (short)(session.Character.PositionY + 1));
                    }
                    else
                    {
                        session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED"), 0));
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