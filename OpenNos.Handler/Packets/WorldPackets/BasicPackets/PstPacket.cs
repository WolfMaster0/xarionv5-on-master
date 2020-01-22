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

using System;
using OpenNos.Core;
using OpenNos.Core.Extensions;
using OpenNos.Core.Serializing;
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;

namespace OpenNos.Handler.Packets.WorldPackets.BasicPackets
{
    [PacketHeader("pst")]
    public class PstPacket
    {
        #region Properties

        public int Argument { get; set; }

        public string Data { get; set; }

        public long Id { get; set; }

        public string Receiver { get; set; }

        public int Type { get; set; }

        public int Unknown1 { get; set; }

        public int Unknown2 { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(new[] { ' ' }, 9);
            if (packetSplit.Length < 5)
            {
                return;
            }
            PstPacket packetDefinition = new PstPacket();
            if (int.TryParse(packetSplit[2], out int argument)
                && int.TryParse(packetSplit[3], out int type)
                && long.TryParse(packetSplit[4], out long id))
            {
                packetDefinition.Argument = argument;
                packetDefinition.Type = type;
                packetDefinition.Id = id;
                packetDefinition.Unknown1 = packetSplit.Length > 5 && int.TryParse(packetSplit[5], out int unknown1) ? unknown1 : 0;
                packetDefinition.Unknown2 = packetSplit.Length > 6 && int.TryParse(packetSplit[6], out int unknown2) ? unknown2 : 0;
                packetDefinition.Receiver = packetSplit.Length > 7 ? packetSplit[7] : string.Empty;
                packetDefinition.Data = packetSplit.Length > 8 ? packetSplit[8] : null;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(PstPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (Data != null)
            {
                CharacterDTO receiver = DAOFactory.CharacterDAO.LoadByName(Receiver);
                if (receiver != null)
                {
                    string[] datasplit = Data.Split(' ');
                    if (datasplit.Length < 2)
                    {
                        return;
                    }

                    if (datasplit[1].Length > 250)
                    {
                        //PenaltyLogDTO log = new PenaltyLogDTO
                        //{
                        //    AccountId = Session.Character.AccountId,
                        //    Reason = "You are an idiot!",
                        //    Penalty = PenaltyType.Banned,
                        //    DateStart = DateTime.UtcNow,
                        //    DateEnd = DateTime.UtcNow.AddYears(69),
                        //    AdminName = "Your mom's ass"
                        //};
                        //Session.Character.InsertOrUpdatePenalty(log);
                        //ServerManager.Instance.Kick(Session.Character.Name);
                        return;
                    }

                    ItemInstance headWearable =
                        session.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.Hat, InventoryType.Wear);
                    byte color = headWearable?.Item.IsColored == true
                        ? (byte)headWearable.Design
                        : (byte)session.Character.HairColor;
                    MailDTO mailcopy = new MailDTO
                    {
                        AttachmentAmount = 0,
                        IsOpened = false,
                        Date = DateTime.UtcNow,
                        Title = datasplit[0],
                        Message = datasplit[1],
                        ReceiverId = receiver.CharacterId,
                        SenderId = session.Character.CharacterId,
                        IsSenderCopy = true,
                        SenderClass = session.Character.Class,
                        SenderGender = session.Character.Gender,
                        SenderHairColor = Enum.IsDefined(typeof(HairColorType), color) ? (HairColorType)color : 0,
                        SenderHairStyle = session.Character.HairStyle,
                        EqPacket = session.Character.GenerateEqListForPacket(),
                        SenderMorphId = session.Character.Morph == 0
                            ? (short)-1
                            : (short)(session.Character.Morph > short.MaxValue ? 0 : session.Character.Morph)
                    };
                    MailDTO mail = new MailDTO
                    {
                        AttachmentAmount = 0,
                        IsOpened = false,
                        Date = DateTime.UtcNow,
                        Title = datasplit[0],
                        Message = datasplit[1],
                        ReceiverId = receiver.CharacterId,
                        SenderId = session.Character.CharacterId,
                        IsSenderCopy = false,
                        SenderClass = session.Character.Class,
                        SenderGender = session.Character.Gender,
                        SenderHairColor = Enum.IsDefined(typeof(HairColorType), color) ? (HairColorType)color : 0,
                        SenderHairStyle = session.Character.HairStyle,
                        EqPacket = session.Character.GenerateEqListForPacket(),
                        SenderMorphId = session.Character.Morph == 0
                            ? (short)-1
                            : (short)(session.Character.Morph > short.MaxValue ? 0 : session.Character.Morph)
                    };

                    MailServiceClient.Instance.SendMail(mailcopy);
                    MailServiceClient.Instance.SendMail(mail);

                    //Session.Character.MailList.Add((Session.Character.MailList.Count > 0 ? Session.Character.MailList.OrderBy(s => s.Key).Last().Key : 0) + 1, mailcopy);
                    session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MAILED"),
                        11));

                    //Session.SendPacket(Session.Character.GeneratePost(mailcopy, 2));
                }
                else
                {
                    session.SendPacket(
                        session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                }
            }
            else if (Id.TryCastToInt(out int id) && Type.TryCastToByte(out byte type))
            {
                if (Argument == 3)
                {
                    if (session.Character.MailList.ContainsKey(id))
                    {
                        if (!session.Character.MailList[id].IsOpened)
                        {
                            session.Character.MailList[id].IsOpened = true;
                            MailDTO mailUpdate = session.Character.MailList[id];
                            DAOFactory.MailDAO.InsertOrUpdate(ref mailUpdate);
                        }

                        session.SendPacket(session.Character.GeneratePostMessage(session.Character.MailList[id], type));
                    }
                }
                else if (Argument == 2)
                {
                    if (session.Character.MailList.ContainsKey(id))
                    {
                        MailDTO mail = session.Character.MailList[id];
                        session.SendPacket(
                            session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MAIL_DELETED"), 11));
                        session.SendPacket($"post 2 {type} {id}");
                        if (DAOFactory.MailDAO.LoadById(mail.MailId) != null)
                        {
                            DAOFactory.MailDAO.DeleteById(mail.MailId);
                        }

                        if (session.Character.MailList.ContainsKey(id))
                        {
                            session.Character.MailList.Remove(id);
                        }
                    }
                }
            }
        }

        #endregion
    }
}