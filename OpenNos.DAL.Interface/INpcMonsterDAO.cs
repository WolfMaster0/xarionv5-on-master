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
    public interface INpcMonsterDAO
    {
        #region Methods

        /// <summary>
        /// Used for inserting single object into entity
        /// </summary>
        /// <param name="npc"></param>
        /// <returns></returns>
        NpcMonsterDTO Insert(NpcMonsterDTO npc);

        /// <summary>
        /// Used for inserting list of data to entity
        /// </summary>
        /// <param name="npcMonsters"></param>
        void Insert(List<NpcMonsterDTO> npcMonsters);

        /// <summary>
        /// Inser or Update data in entity
        /// </summary>
        /// <param name="npcMonster"></param>
        /// <returns></returns>
        SaveResult InsertOrUpdate(ref NpcMonsterDTO npcMonster);

        /// <summary>
        /// Used for loading all monsters from entity
        /// </summary>
        /// <returns></returns>
        IEnumerable<NpcMonsterDTO> LoadAll();

        /// <summary>
        /// Used for loading monsters with specified VNum
        /// </summary>
        /// <param name="npcMonsterVNum"></param>
        /// <returns></returns>
        NpcMonsterDTO LoadByVNum(short npcMonsterVNum);

        #endregion
    }
}