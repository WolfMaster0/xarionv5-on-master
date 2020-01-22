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

using System.ComponentModel.DataAnnotations;
using OpenNos.Domain;

namespace OpenNos.DAL.EF
{
    public class Mate
    {
        #region Properties

        public byte Attack { get; set; }

        public bool CanPickUp { get; set; }

        public virtual Character Character { get; set; }

        public long CharacterId { get; set; }

        public byte Defence { get; set; }

        public byte Direction { get; set; }

        public long Experience { get; set; }

        public int Hp { get; set; }

        public bool IsSummonable { get; set; }

        public bool IsTeamMember { get; set; }

        public byte Level { get; set; }

        public short Loyalty { get; set; }

        public short MapX { get; set; }

        public short MapY { get; set; }

        [Key]
        public long MateId { get; set; }

        public MateType MateType { get; set; }

        public int Mp { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        public virtual NpcMonster NpcMonster { get; set; }

        public short NpcMonsterVNum { get; set; }

        public short Skin { get; set; }

        public short MateSlot { get; set; }

        public short PartnerSlot { get; set; }


        #endregion
    }
}