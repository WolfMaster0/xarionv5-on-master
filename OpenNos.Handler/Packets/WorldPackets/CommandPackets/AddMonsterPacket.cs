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
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.Npc;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$AddMonster", Authority = AuthorityType.GameMaster)]
    public class AddMonsterPacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public bool IsMoving { get; set; }

        public short MonsterVNum { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (!(session is ClientSession sess))
            {
                return;
            }
            if (packetSplit.Length < 4)
            {
                sess.SendPacket(sess.Character.GenerateSay(ReturnHelp(), 10));
                return;
            }
            AddMonsterPacket packetDefinition = new AddMonsterPacket();
            if (short.TryParse(packetSplit[2], out short vnum))
            {
                packetDefinition._isParsed = true;
                packetDefinition.MonsterVNum = vnum;
                packetDefinition.IsMoving = packetSplit[3] == "1";
            }
            packetDefinition.ExecuteHandler(sess);
        }

        public static void Register() => PacketFacility.AddHandler(typeof(AddMonsterPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$AddMonster VNUM MOVE";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(),
                    $"[AddMonster]NpcMonsterVNum: {MonsterVNum} IsMoving: {IsMoving}");

                if (!session.HasCurrentMapInstance)
                {
                    return;
                }

                NpcMonster npcmonster = ServerManager.GetNpcMonster(MonsterVNum);
                if (npcmonster == null)
                {
                    return;
                }

                MapMonsterDTO monst = new MapMonsterDTO
                {
                    MonsterVNum = MonsterVNum,
                    MapY = session.Character.PositionY,
                    MapX = session.Character.PositionX,
                    MapId = session.Character.MapInstance.Map.MapId,
                    Position = session.Character.Direction,
                    IsMoving = IsMoving,
                    MapMonsterId = DAOFactory.MapMonsterDAO.GetNextMapMonsterId()
                };
                if (!DAOFactory.MapMonsterDAO.DoesMonsterExist(monst.MapMonsterId))
                {
                    DAOFactory.MapMonsterDAO.Insert(monst);
                    MapMonsterDTO monster = DAOFactory.MapMonsterDAO.LoadById(monst.MapMonsterId);
                    if (monster != null)
                    {
                        MapMonster mapMonster = new MapMonster(monster);
                        mapMonster.Initialize(session.CurrentMapInstance);
                        session.CurrentMapInstance.AddMonster(mapMonster);
                        session.CurrentMapInstance.Broadcast(mapMonster.GenerateIn());
                    }
                }

                session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
            }
            else
            {
                session.SendPacket(session.Character.GenerateSay(ReturnHelp(), 10));
            }
        }

        #endregion
    }
}