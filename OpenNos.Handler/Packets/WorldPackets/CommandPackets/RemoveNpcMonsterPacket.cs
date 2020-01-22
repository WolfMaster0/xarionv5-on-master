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
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$RemoveNpcMonster", Authority = AuthorityType.GameMaster)]
    public class RemoveNpcMonsterPacket
    {
        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            RemoveNpcMonsterPacket packetDefinition = new RemoveNpcMonsterPacket();
            packetDefinition.ExecuteHandler(session as ClientSession);
        }

        public static void Register() => PacketFacility.AddHandler(typeof(RemoveNpcMonsterPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$RemoveNpcMonster";

        private void ExecuteHandler(ClientSession session)
        {
            if (session.HasCurrentMapInstance)
            {
                Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(),
                    $"[RemoveNpcMonster]NpcMonsterId: {session.Character.LastNpcMonsterId}");

                MapMonster monster = session.CurrentMapInstance.GetMonster(session.Character.LastNpcMonsterId);
                MapNpc npc = session.CurrentMapInstance.GetNpc(session.Character.LastNpcMonsterId);
                if (monster != null)
                {
                    if (monster.IsAlive)
                    {
                        session.CurrentMapInstance.Broadcast(StaticPacketHelper.Out(UserType.Monster,
                            monster.MapMonsterId));
                        session.SendPacket(session.Character.GenerateSay(
                            string.Format(Language.Instance.GetMessageFromKey("MONSTER_REMOVED"), monster.MapMonsterId,
                                monster.Monster.Name, monster.MapId, monster.MapX, monster.MapY), 12));
                        session.CurrentMapInstance.RemoveMonster(monster);
                        if (DAOFactory.MapMonsterDAO.LoadById(monster.MapMonsterId) != null)
                        {
                            DAOFactory.MapMonsterDAO.DeleteById(monster.MapMonsterId);
                        }
                    }
                    else
                    {
                        session.SendPacket(session.Character.GenerateSay(
                            string.Format(Language.Instance.GetMessageFromKey("MONSTER_NOT_ALIVE")), 11));
                    }
                }
                else if (npc != null)
                {
                    if (!npc.IsMate && !npc.IsDisabled && !npc.IsProtected)
                    {
                        int mapNpcId = npc.MapNpcId;

                        session.CurrentMapInstance.Broadcast(StaticPacketHelper.Out(UserType.Npc, mapNpcId));
                        session.SendPacket(session.Character.GenerateSay(
                            string.Format(Language.Instance.GetMessageFromKey("NPCMONSTER_REMOVED"), mapNpcId,
                                npc.Npc.Name, npc.MapId, npc.MapX, npc.MapY), 12));

                        session.CurrentMapInstance.RemoveNpc(npc);

                        if (DAOFactory.RecipeListDAO.LoadFirstByMapNpcId(mapNpcId) != null)
                        {
                            DAOFactory.RecipeListDAO.DeleteByMapNpcId(mapNpcId);
                        }

                        if (DAOFactory.ShopDAO.LoadByNpc(mapNpcId) != null)
                        {
                            DAOFactory.ShopDAO.DeleteById(mapNpcId);
                        }

                        if (DAOFactory.MapNpcDAO.LoadById(mapNpcId) != null)
                        {
                            DAOFactory.MapNpcDAO.DeleteById(mapNpcId);
                        }
                    }
                    else
                    {
                        session.SendPacket(session.Character.GenerateSay(
                            string.Format(Language.Instance.GetMessageFromKey("NPC_CANNOT_BE_REMOVED")), 11));
                    }
                }
                else
                {
                    session.SendPacket(
                        session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NPCMONSTER_NOT_FOUND"), 11));
                }
            }
        }

        #endregion
    }
}