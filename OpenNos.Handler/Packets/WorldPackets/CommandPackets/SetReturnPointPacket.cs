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
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$SetReturnPoint", Authority = AuthorityType.User)]
    public class SetReturnPointPacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public byte ReturnPointId { get; set; }

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
                SetReturnPointPacket packetDefinition = new SetReturnPointPacket();
                if (byte.TryParse(packetSplit[2], out byte returnPointId))
                {
                    packetDefinition._isParsed = true;
                    packetDefinition.ReturnPointId = returnPointId;
                }
                packetDefinition.ExecuteHandler(sess);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(SetReturnPointPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$SetReturnPoint RETURNPOINTID";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                if (session.Character.StaticBonusList.All(s => s.StaticBonusType != StaticBonusType.MultipleReturns))
                {
                    session.SendPacket(
                        session.Character.GenerateSay(Language.Instance.GetMessageFromKey("RETURNPOINT_NOT_PERMITTED"),
                            10));
                    return;
                }

                if (session.Character.MapInstance.MapInstanceType != MapInstanceType.BaseMapInstance)
                {
                    session.SendPacket(
                        session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_USE_THAT"), 10));
                    return;
                }

                if (ReturnPointId < 1 || ReturnPointId > 5)
                {
                    return;
                }

                if (session.HasCurrentMapInstance
                    && session.CurrentMapInstance.Map.MapTypes.Any(s => s.MapTypeId == (short)MapTypeEnum.Act4))
                {
                    RespawnDTO resp =
                        session.Character.Respawns.Find(s => s.RespawnMapTypeId == ReturnPointId + 50);
                    if (resp == null)
                    {
                        resp = new RespawnDTO
                        {
                            CharacterId = session.Character.CharacterId,
                            MapId = session.Character.MapId,
                            X = session.Character.MapX,
                            Y = session.Character.MapY,
                            RespawnMapTypeId = ReturnPointId + 50
                        };
                        session.Character.Respawns.Add(resp);
                    }
                    else
                    {
                        resp.X = session.Character.PositionX;
                        resp.Y = session.Character.PositionY;
                        resp.MapId = session.Character.MapInstance.Map.MapId;
                    }

                    session.SendPacket(
                        session.Character.GenerateSay(Language.Instance.GetMessageFromKey("RETURNPOINT_SET"), 10));
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