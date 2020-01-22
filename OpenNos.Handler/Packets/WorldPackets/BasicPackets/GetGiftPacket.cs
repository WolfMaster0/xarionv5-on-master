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
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.GameLog.LogHelper;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.BasicPackets
{
    [PacketHeader("pcl")]
    public class GetGiftPacket
    {
        #region Properties

        public int GiftId { get; set; }

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
            GetGiftPacket packetDefinition = new GetGiftPacket();
            if (byte.TryParse(packetSplit[2], out byte type) && int.TryParse(packetSplit[3], out int giftId))
            {
                packetDefinition.Type = type;
                packetDefinition.GiftId = giftId;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(GetGiftPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            int giftId = GiftId;
            if (session.Character.MailList.ContainsKey(giftId))
            {
                MailDTO mail = session.Character.MailList[giftId];
                if (Type == 4 && mail.AttachmentVNum != null)
                {
                    if (session.Character.Inventory.CanAddItem((short)mail.AttachmentVNum))
                    {
                        ItemInstance newInv = session.Character.Inventory.AddNewToInventory((short)mail.AttachmentVNum,
                                mail.AttachmentAmount, upgrade: mail.AttachmentUpgrade,
                                rare: (sbyte)mail.AttachmentRarity)
                            .FirstOrDefault();
                        if (newInv != null)
                        {
                            if (newInv.Rare != 0)
                            {
                                bool isPartner = (newInv.ItemVNum >= 990 && newInv.ItemVNum <= 992) || (newInv.ItemVNum >= 995 && newInv.ItemVNum <= 997);
                                newInv.SetRarityPoint(isPartner);
                            }

                            GameLogger.Instance.LogParcelReceive(ServerManager.Instance.ChannelId,
                                session.Character.Name, session.Character.CharacterId, mail);

                            session.SendPacket(session.Character.GenerateSay(
                                string.Format(Language.Instance.GetMessageFromKey("ITEM_GIFTED"), newInv.Item.Name,
                                    mail.AttachmentAmount), 12));

                            DAOFactory.MailDAO.DeleteById(mail.MailId);

                            session.SendPacket($"parcel 2 1 {giftId}");

                            session.Character.MailList.Remove(giftId);
                        }
                    }
                    else
                    {
                        session.SendPacket("parcel 5 1 0");
                        session.SendPacket(
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"),
                                0));
                    }
                }
                else if (Type == 5)
                {
                    session.SendPacket($"parcel 7 1 {giftId}");

                    if (DAOFactory.MailDAO.LoadById(mail.MailId) != null)
                    {
                        DAOFactory.MailDAO.DeleteById(mail.MailId);
                    }

                    if (session.Character.MailList.ContainsKey(giftId))
                    {
                        session.Character.MailList.Remove(giftId);
                    }
                }
            }
        }

        #endregion
    }
}