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
using System.Text;

namespace OpenNos.Core.Cryptography
{
    public class LoginCryptography : CryptographyBase
    {
        #region Instantiation

        public LoginCryptography() : base(false)
        {
        }

        #endregion

        #region Methods

        public static string GetPassword(string password)
        {
            bool equal = password.Length % 2 == 0;
            string str = equal ? password.Remove(0, 3) : password.Remove(0, 4);
            StringBuilder decryptpass = new StringBuilder();

            for (int i = 0; i < str.Length; i += 2)
            {
                decryptpass.Append(str[i]);
            }
            if (decryptpass.Length % 2 != 0)
            {
                str = password.Remove(0, 2);
                decryptpass = decryptpass.Clear();
                for (int i = 0; i < str.Length; i += 2)
                {
                    decryptpass.Append(str[i]);
                }
            }

            StringBuilder passwd = new StringBuilder();
            for (int i = 0; i < decryptpass.Length; i += 2)
            {
                passwd.Append(Convert.ToChar(Convert.ToUInt32(decryptpass.ToString().Substring(i, 2), 16)));
            }
            return passwd.ToString();
        }

        public override string Decrypt(byte[] data, int sessionId = 0)
        {
            try
            {
                StringBuilder builder = new StringBuilder();
                foreach (byte character in data)
                {
                    if (character > 14)
                    {
                        builder.Append(Convert.ToChar((character - 15) ^ 195));
                    }
                    else
                    {
                        builder.Append(Convert.ToChar((256 - (15 - character)) ^ 195));
                    }
                }
                return builder.ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public override string DecryptCustomParameter(byte[] data) => throw new NotImplementedException();

        public override byte[] Encrypt(string data)
        {
            try
            {
                byte[] dataBytes = Encoding.Default.GetBytes(data);
                for (int i = 0; i < dataBytes.Length; i++)
                {
                    dataBytes[i] = Convert.ToByte(dataBytes[i] + 15);
                }
                dataBytes[dataBytes.Length - 1] = 25;
                return dataBytes;
            }
            catch (Exception)
            {
                return new byte[0];
            }
        }

        #endregion
    }
}