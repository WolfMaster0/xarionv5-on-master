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
using OpenNos.SCS.Communication.ScsServices.Service;
using OpenNos.Data;

namespace OpenNos.Master.Library.Interface
{
    [ScsService(Version = "1.1.0.0")]
    public interface IAuthentificationService
    {
        /// <summary>
        /// Authenticates a Client to the Service
        /// </summary>
        /// <param name="authKey">The private Authentication key</param>
        /// <returns>true if successful, else false</returns>
        bool Authenticate(string authKey);

        /// <summary>
        /// Checks if the given Credentials are Valid
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="passHash"></param>
        /// <returns></returns>
        AccountDTO ValidateAccount(string userName, string passHash);

        /// <summary>
        /// Checks if the given Credentials are Valid and return the CharacterDTO
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="characterName"></param>
        /// <param name="passHash"></param>
        /// <returns></returns>
        CharacterDTO ValidateAccountAndCharacter(string userName, string characterName, string passHash);
    }
}