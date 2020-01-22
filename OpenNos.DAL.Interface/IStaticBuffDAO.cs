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
using OpenNos.Data;
using OpenNos.Data.Enums;
using System.Collections.Generic;

namespace OpenNos.DAL.Interface
{
    public interface IStaticBuffDAO
    {
        #region Methods

        void Delete(short bonusToDelete, long characterId);

        /// <summary>
        /// Inserts new object to database context
        /// </summary>
        /// <param name="staticBuff"></param>
        /// <returns></returns>
        SaveResult InsertOrUpdate(ref StaticBuffDTO staticBuff);

        /// <summary>
        /// Loads staticBonus by characterid
        /// </summary>
        /// <param name="characterId"></param>
        /// <returns></returns>
        IEnumerable<StaticBuffDTO> LoadByCharacterId(long characterId);

        /// <summary>
        /// Loads by CharacterId
        /// </summary>
        /// <param name="characterId"></param>
        /// <returns>IEnumerable list of CardIds</returns>
        IEnumerable<short> LoadByTypeCharacterId(long characterId);

        #endregion
    }
}