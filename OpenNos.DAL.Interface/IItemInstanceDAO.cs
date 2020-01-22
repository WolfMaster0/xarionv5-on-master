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
using OpenNos.Domain;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.Interface
{
    public interface IItemInstanceDAO
    {
        #region Methods

        DeleteResult DeleteGuidList(IEnumerable<Guid> guids);

        SaveResult InsertOrUpdateFromList(IEnumerable<ItemInstanceDTO> items);

        DeleteResult DeleteFromSlotAndType(long characterId, short slot, InventoryType type);

        DeleteResult DeleteByVNum(short vNum);

        IEnumerable<ItemInstanceDTO> LoadByCharacterId(long characterId);

        ItemInstanceDTO LoadBySlotAndType(long characterId, short slot, InventoryType type);

        IEnumerable<ItemInstanceDTO> LoadByType(long characterId, InventoryType type);

        IList<Guid> LoadSlotAndTypeByCharacterId(long characterId);

        DeleteResult Delete(Guid id);

        ItemInstanceDTO InsertOrUpdate(ItemInstanceDTO dto);

        IEnumerable<ItemInstanceDTO> InsertOrUpdate(IEnumerable<ItemInstanceDTO> dtos);

        ItemInstanceDTO LoadById(Guid id);

        #endregion
    }
}