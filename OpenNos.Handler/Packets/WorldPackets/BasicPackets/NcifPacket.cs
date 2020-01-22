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

using System.Linq;
using System.Threading.Tasks;
using OpenNos.Core.Serializing;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.Npc;

namespace OpenNos.Handler.Packets.WorldPackets.BasicPackets
{
    [PacketHeader("ncif")]
    public class NcifPacket
    {
        #region Properties

        public long TargetId { get; set; }

        public byte Type { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 4)
            {
                return;
            }
            NcifPacket packetDefinition = new NcifPacket();
            if (byte.TryParse(packetSplit[2], out byte type) && long.TryParse(packetSplit[3], out long targetId))
            {
                packetDefinition.TargetId = targetId;
                packetDefinition.Type = type;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(NcifPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            switch (Type)
            {
                // characters
                case 1:
                    session.SendPacket(ServerManager.Instance.GetSessionByCharacterId(TargetId)?.Character
                        ?.GenerateStatInfo());
                    break;

                // npcs/mates
                case 2:
                    if (session.HasCurrentMapInstance)
                    {
                        session.CurrentMapInstance.Npcs.Where(n => n.MapNpcId == (int)TargetId).ToList()
                            .ForEach(npc =>
                            {
                                NpcMonster npcinfo = ServerManager.GetNpcMonster(npc.NpcVNum);
                                if (npcinfo == null)
                                {
                                    return;
                                }

                                session.Character.LastNpcMonsterId = npc.MapNpcId;
                                session.SendPacket(
                                    $"st 2 {TargetId} {npcinfo.Level} {npcinfo.HeroLevel} 100 100 50000 50000");
                            });
                        Parallel.ForEach(session.CurrentMapInstance.Sessions, sess =>
                        {
                            Mate mate = sess.Character.Mates.Find(
                                s => s.MateTransportId == (int)TargetId);
                            if (mate != null)
                            {
                                session.SendPacket(mate.GenerateStatInfo());
                            }
                        });
                    }

                    break;

                // monsters
                case 3:
                    if (session.HasCurrentMapInstance)
                    {
                        session.CurrentMapInstance.Monsters.Where(m => m.MapMonsterId == (int)TargetId)
                            .ToList().ForEach(monster =>
                            {
                                NpcMonster monsterinfo = ServerManager.GetNpcMonster(monster.MonsterVNum);
                                if (monsterinfo == null)
                                {
                                    return;
                                }

                                session.Character.LastNpcMonsterId = monster.MapMonsterId;
                                session.SendPacket(
                                    $"st 3 {TargetId} {monsterinfo.Level} {monsterinfo.HeroLevel} {(int)(monster.CurrentHp / (float)monster.MaxHp * 100)} {(int)(monster.CurrentMp / (float)monster.MaxMp * 100)} {monster.CurrentHp} {monster.CurrentMp}{monster.Buff.GetAllItems().Aggregate(string.Empty, (current, buff) => current + $" {buff.Card.CardId}")}");
                            });
                    }

                    break;
            }
        }

        #endregion
    }
}