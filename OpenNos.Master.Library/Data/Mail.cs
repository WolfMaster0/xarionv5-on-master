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
using OpenNos.Domain;
using System;

namespace OpenNos.Master.Library.Data
{
    [Serializable]
    public class Mail
    {
        #region Properties

        public byte AttachmentAmount { get; set; }

        public byte AttachmentRarity { get; set; }

        public byte AttachmentUpgrade { get; set; }

        public short? AttachmentVNum { get; set; }

        public DateTime Date { get; set; }

        public string EqPacket { get; set; }

        public bool IsOpened { get; set; }

        public bool IsSenderCopy { get; set; }

        public long MailId { get; set; }

        public string Message { get; set; }

        public long ReceiverId { get; set; }

        public ClassType SenderClass { get; set; }

        public GenderType SenderGender { get; set; }

        public HairColorType SenderHairColor { get; set; }

        public HairStyleType SenderHairStyle { get; set; }

        public long SenderId { get; set; }

        public short SenderMorphId { get; set; }

        public string Title { get; set; }

        #endregion
    }
}