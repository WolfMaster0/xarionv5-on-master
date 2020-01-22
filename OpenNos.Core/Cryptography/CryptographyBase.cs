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
using System.Security.Cryptography;
using System.Text;

namespace OpenNos.Core.Cryptography
{
    public abstract class CryptographyBase
    {
        #region Instantiation

        protected CryptographyBase(bool hasCustomParameter) => HasCustomParameter = hasCustomParameter;

        #endregion

        #region Properties

        public bool HasCustomParameter { get; set; }

        #endregion

        #region Methods

        public static string Sha512(string inputString)
        {
            using (SHA512 hash = SHA512.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(inputString)).Select(item => item.ToString("x2")));
            }
        }

        public abstract string Decrypt(byte[] data, int sessionId = 0);

        public abstract string DecryptCustomParameter(byte[] data);

        public abstract byte[] Encrypt(string data);

        #endregion
    }
}