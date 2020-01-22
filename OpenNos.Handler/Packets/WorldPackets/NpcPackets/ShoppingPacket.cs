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
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.NpcPackets
{
    [PacketHeader("shopping")]
    public class ShoppingPacket
    {
        #region Properties

        public int NpcId { get; set; }

        public byte Type { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 6)
            {
                return;
            }
            ShoppingPacket packetDefinition = new ShoppingPacket();
            if (byte.TryParse(packetSplit[2], out byte type)
                && int.TryParse(packetSplit[5], out int npcId))
            {
                packetDefinition.Type = type;
                packetDefinition.NpcId = npcId;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(ShoppingPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            byte type = Type, typeshop = 0;
            int npcId = NpcId;
            if (session.Character.IsShopping || !session.HasCurrentMapInstance)
            {
                return;
            }

            MapNpc mapnpc = session.CurrentMapInstance.Npcs.Find(n => n.MapNpcId.Equals(npcId));
            if (mapnpc?.Shop == null)
            {
                return;
            }

            string shoplist = string.Empty;
            foreach (ShopItemDTO item in mapnpc.Shop.ShopItems.Where(s => s.Type.Equals(type)))
            {
                Item iteminfo = ServerManager.GetItem(item.ItemVNum);
                typeshop = 100;
                double percent = 1;
                switch (session.Character.GetDignityIco())
                {
                    case 3:
                        percent = 1.1;
                        typeshop = 110;
                        break;

                    case 4:
                        percent = 1.2;
                        typeshop = 120;
                        break;

                    case 5:
                        percent = 1.5;
                        typeshop = 150;
                        break;

                    case 6:
                        percent = 1.5;
                        typeshop = 150;
                        break;

                    default:
                        if (session.CurrentMapInstance.Map.MapTypes.Any(s => s.MapTypeId == (short)MapTypeEnum.Act4))
                        {
                            percent *= 1.5;
                            typeshop = 150;
                        }

                        break;
                }

                if (session.CurrentMapInstance.Map.MapTypes.Any(s =>
                    s.MapTypeId == (short)MapTypeEnum.Act4 && session.Character.GetDignityIco() == 3))
                {
                    percent = 1.6;
                    typeshop = 160;
                }
                else if (session.CurrentMapInstance.Map.MapTypes.Any(s =>
                    s.MapTypeId == (short)MapTypeEnum.Act4 && session.Character.GetDignityIco() == 4))
                {
                    percent = 1.7;
                    typeshop = 170;
                }
                else if (session.CurrentMapInstance.Map.MapTypes.Any(s =>
                    s.MapTypeId == (short)MapTypeEnum.Act4 && session.Character.GetDignityIco() == 5))
                {
                    percent = 2;
                    typeshop = 200;
                }
                else if
                (session.CurrentMapInstance.Map.MapTypes.Any(s =>
                    s.MapTypeId == (short)MapTypeEnum.Act4 && session.Character.GetDignityIco() == 6))
                {
                    percent = 2;
                    typeshop = 200;
                }

                if (iteminfo.ReputPrice > 0 && iteminfo.Type == 0)
                {
                    shoplist +=
                        $" {(byte)iteminfo.Type}.{item.Slot}.{item.ItemVNum}.{item.Rare}.{(iteminfo.IsColored ? item.Color : item.Upgrade)}.{iteminfo.ReputPrice}";
                }
                else if (iteminfo.ReputPrice > 0 && iteminfo.Type != 0)
                {
                    shoplist += $" {(byte)iteminfo.Type}.{item.Slot}.{item.ItemVNum}.-1.{iteminfo.ReputPrice}";
                }
                else if (iteminfo.Type != 0)
                {
                    shoplist += $" {(byte)iteminfo.Type}.{item.Slot}.{item.ItemVNum}.-1.{iteminfo.Price * percent}";
                }
                else
                {
                    shoplist +=
                        $" {(byte)iteminfo.Type}.{item.Slot}.{item.ItemVNum}.{item.Rare}.{(iteminfo.IsColored ? item.Color : item.Upgrade)}.{iteminfo.Price * percent}";
                }
            }

            foreach (ShopSkillDTO skill in mapnpc.Shop.ShopSkills.Where(s => s.Type.Equals(type)))
            {
                Skill skillinfo = ServerManager.GetSkill(skill.SkillVNum);

                if (skill.Type != 0)
                {
                    typeshop = 1;
                    if (skillinfo.Class == (byte)session.Character.Class)
                    {
                        shoplist += $" {skillinfo.SkillVNum}";
                    }
                }
                else
                {
                    shoplist += $" {skillinfo.SkillVNum}";
                }
            }

            session.SendPacket($"n_inv 2 {mapnpc.MapNpcId} 0 {typeshop}{shoplist}");
        }

        #endregion
    }
}