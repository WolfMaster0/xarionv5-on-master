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
using System.ComponentModel.DataAnnotations;

namespace OpenNos.DAL.EF
{
    public class GeneralLog
    {
        #region Properties

        public virtual Account Account { get; set; }

        public long? AccountId { get; set; }

        public virtual Character Character { get; set; }

        public long? CharacterId { get; set; }

        [MaxLength(255)]
        public string IpAddress { get; set; }

        [MaxLength(255)]
        public string LogData { get; set; }

        [Key]
        public long LogId { get; set; }

        public string LogType { get; set; }

        public DateTime Timestamp { get; set; }

        #endregion
    }
}