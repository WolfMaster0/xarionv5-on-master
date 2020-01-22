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

namespace OpenNos.Data
{
    [Serializable]
    public class AccountDTO
    {
        #region Properties

        public long AccountId { get; set; }

        public AuthorityType Authority { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }

        public string Password { get; set; }

        public long ReferrerId { get; set; }

        public string RegistrationIP { get; set; }

        public string VerificationToken { get; set; }

        public string TotpSecret { get; set; }

        public string TotpResetPassword { get; set; }

        public bool TotpVerified { get; set; }

        public bool IsVerified { get; set; }


        #endregion
    }
}