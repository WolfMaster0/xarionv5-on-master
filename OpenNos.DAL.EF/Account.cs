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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OpenNos.DAL.EF
{
    public sealed class Account
    {
        #region Instantiation

        public Account()
        {
            Character = new HashSet<Character>();
            GeneralLog = new HashSet<GeneralLog>();
            PenaltyLog = new HashSet<PenaltyLog>();
            MultiAccountException = new HashSet<MultiAccountException>();
        }

        #endregion

        #region Properties

        public long AccountId { get; set; }

        public AuthorityType Authority { get; set; }

        public ICollection<MultiAccountException> MultiAccountException { get; }

        public ICollection<Character> Character { get; }

        [MaxLength(255)]
        public string Email { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public ICollection<GeneralLog> GeneralLog { get; }

        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Password { get; set; }

        public ICollection<PenaltyLog> PenaltyLog { get; }

        public long ReferrerId { get; set; }

        [MaxLength(45)]
        public string RegistrationIP { get; set; }

        [MaxLength(32)]
        public string VerificationToken { get; set; }

        [MaxLength(32)]
        public string TotpSecret { get; set; }

        [MaxLength(255)]
        public string TotpResetPassword { get; set; }

        public bool TotpVerified { get; set; }


        #endregion
    }
}