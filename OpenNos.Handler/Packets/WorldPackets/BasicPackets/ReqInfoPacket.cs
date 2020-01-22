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
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.Npc;

namespace OpenNos.Handler.Packets.WorldPackets.BasicPackets
{
    [PacketHeader("req_info")]
    public class ReqInfoPacket
    {
        #region Properties

        public int? MateVNum { get; set; }

        public long TargetVNum { get; set; }

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
            ReqInfoPacket packetDefinition = new ReqInfoPacket();
            if (byte.TryParse(packetSplit[2], out byte type) && long.TryParse(packetSplit[3], out long targetVNum))
            {
                packetDefinition.Type = type;
                packetDefinition.TargetVNum = targetVNum;
                packetDefinition.MateVNum = packetSplit.Length >= 5
                    && int.TryParse(packetSplit[4], out int mateVNum) ? mateVNum : (int?)null;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(ReqInfoPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (Type == 6)
            {
                if (MateVNum.HasValue)
                {
                    Mate mate = session.CurrentMapInstance.Sessions
                        .FirstOrDefault(s =>
                            s.Character?.Mates != null
                            && s.Character.Mates.Any(o => o.MateTransportId == MateVNum.Value))?.Character
                        .Mates.Find(o => o.MateTransportId == MateVNum.Value);
                    session.SendPacket(mate?.GenerateEInfo());
                }
            }
            else if (Type == 5)
            {
                NpcMonster npc = ServerManager.GetNpcMonster((short)TargetVNum);
                if (npc != null)
                {
                    session.SendPacket(npc.GenerateEInfo());
                }
            }
            else if (Type == 12)
            {
                ItemInstance inv =
                    session.Character.Inventory.LoadBySlotAndType((short) TargetVNum, InventoryType.Equipment);
                if (inv != null)
                {
                    if (inv.BoundCharacterId == null)
                    {
                        session.SendPacket($"r_info {inv.ItemVNum} 0 0");
                    }
                    else if (inv.BoundCharacterId == session.Character.CharacterId)
                    {
                        session.SendPacket($"r_info {inv.ItemVNum} 1 0");
                    }
                    else
                    {
                        session.SendPacket($"r_info {inv.ItemVNum} 2 0");
                    }
                }
            }
            else
            {
                session.SendPacket(ServerManager.Instance.GetSessionByCharacterId(TargetVNum)?.Character
                    ?.GenerateReqInfo());
            }
        }

        #endregion
    }
}