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
using OpenNos.DAL.EF;
using OpenNos.Data;

namespace OpenNos.Mapper.Mappers
{
    public static class MailMapper
    {
        #region Methods

        public static bool ToMail(MailDTO input, Mail output)
        {
            if (input == null)
            {
                return false;
            }
            output.AttachmentAmount = input.AttachmentAmount;
            output.AttachmentLevel = input.AttachmentLevel;
            output.AttachmentRarity = input.AttachmentRarity;
            output.AttachmentUpgrade = input.AttachmentUpgrade;
            output.AttachmentVNum = input.AttachmentVNum;
            output.Date = input.Date;
            output.EqPacket = input.EqPacket;
            output.IsOpened = input.IsOpened;
            output.IsSenderCopy = input.IsSenderCopy;
            output.MailId = input.MailId;
            output.Message = input.Message;
            output.ReceiverId = input.ReceiverId;
            output.SenderClass = input.SenderClass;
            output.SenderGender = input.SenderGender;
            output.SenderHairColor = input.SenderHairColor;
            output.SenderHairStyle = input.SenderHairStyle;
            output.SenderId = input.SenderId;
            output.SenderMorphId = input.SenderMorphId;
            output.Title = input.Title;
            return true;
        }

        public static bool ToMailDTO(Mail input, MailDTO output)
        {
            if (input == null)
            {
                return false;
            }
            output.AttachmentAmount = input.AttachmentAmount;
            output.AttachmentLevel = input.AttachmentLevel;
            output.AttachmentRarity = input.AttachmentRarity;
            output.AttachmentUpgrade = input.AttachmentUpgrade;
            output.AttachmentVNum = input.AttachmentVNum;
            output.Date = input.Date;
            output.EqPacket = input.EqPacket;
            output.IsOpened = input.IsOpened;
            output.IsSenderCopy = input.IsSenderCopy;
            output.MailId = input.MailId;
            output.Message = input.Message;
            output.ReceiverId = input.ReceiverId;
            output.SenderClass = input.SenderClass;
            output.SenderGender = input.SenderGender;
            output.SenderHairColor = input.SenderHairColor;
            output.SenderHairStyle = input.SenderHairStyle;
            output.SenderId = input.SenderId;
            output.SenderMorphId = input.SenderMorphId;
            output.Title = input.Title;
            return true;
        }

        #endregion
    }
}