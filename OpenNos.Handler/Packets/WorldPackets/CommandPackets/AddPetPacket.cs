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
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.Npc;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$AddPet", Authority = AuthorityType.GameMaster)]
    public class AddPetPacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public byte Level { get; set; }

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
            AddPetPacket packetDefinition = new AddPetPacket();
            if (short.TryParse(packetSplit[2], out short vnum) && byte.TryParse(packetSplit[3], out byte level))
            {
                packetDefinition._isParsed = true;
                packetDefinition.MonsterVNum = vnum;
                packetDefinition.Level = level;
            }
            packetDefinition.ExecuteHandler(sess);
        }

        public static void Register() => PacketFacility.AddHandler(typeof(AddPetPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$AddPet VNUM LEVEL";

        private void AddMate(ClientSession session, short vnum, byte level, MateType mateType)
        {
            NpcMonster mateNpc = ServerManager.GetNpcMonster(vnum);
            if (session.CurrentMapInstance == session.Character.Miniland && mateNpc != null)
            {
                level = level == 0 ? (byte)1 : level;
                Mate mate = new Mate(session.Character, mateNpc, level, mateType);
                session.Character.AddPet(mate);
            }
            else
            {
                session.SendPacket(
                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_IN_MINILAND"), 0));
            }
        }

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(),
                    $"[AddPet]NpcMonsterVNum: {MonsterVNum} Level: {Level}");

                AddMate(session, MonsterVNum, Level, MateType.Pet);
            }
            else
            {
                session.SendPacket(session.Character.GenerateSay(ReturnHelp(), 10));
            }
        }

        #endregion
    }
}