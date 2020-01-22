// This file is part of the OpenNos NosTale Emulator Project.
//
// This program is licensed under a deviated version of the Fair Source License, granting you a
// non-exclusive, non-transferable, royalty-free and fully-paid-up license, under all of the
// Licensor's copyright and patent rights, to use, copy, prepare derivative works of, publicly
// perform and display the Software, subject to the conditions found in the LICENSE file.
//
// THIS FILE IS PROVIDED "AS IS", WITHOUT WARRANTY OR CONDITION, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. THE AUTHORS HEREBY DISCLAIM ALL LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT
// OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE.
using OpenNos.Core;
using OpenNos.DAL.EF;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenNos.DAL.EF.Context;

namespace OpenNos.DAL.DAO
{
    public sealed class ItemInstanceDAO : IItemInstanceDAO
    {
        #region Methods

        public DeleteResult Delete(Guid id)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                ItemInstance entity = context.ItemInstance.FirstOrDefault(i => i.Id == id);
                if (entity != null)
                {
                    context.ItemInstance.Remove(entity);
                    context.SaveChanges();
                }
                return DeleteResult.Deleted;
            }
        }

        public DeleteResult DeleteFromSlotAndType(long characterId, short slot, InventoryType type)
        {
            try
            {
                ItemInstanceDTO dto = LoadBySlotAndType(characterId, slot, type);
                if (dto != null)
                {
                    return Delete(dto.Id);
                }

                return DeleteResult.Unknown;
            }
            catch (Exception e)
            {
                Logger.Error($"characterId: {characterId} slot: {slot} type: {type}", e);
                return DeleteResult.Error;
            }
        }

        public DeleteResult DeleteGuidList(IEnumerable<Guid> guids)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                Guid[] enumerable = guids as Guid[] ?? guids.ToArray();
                try
                {
                    foreach (Guid id in enumerable)
                    {
                        ItemInstance entity = context.ItemInstance.FirstOrDefault(i => i.Id == id);
                        if (entity != null && entity.Type != InventoryType.FamilyWareHouse)
                        {
                            context.ItemInstance.Remove(entity);
                        }
                    }
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Logger.LogUserEventError("DELETEGUIDLIST_EXCEPTION", "Saving Process", "Items were not deleted!", ex);
                    foreach (Guid id in enumerable)
                    {
                        try
                        {
                            Delete(id);
                        }
                        catch (Exception exc)
                        {
                            // TODO: Work on: statement conflicted with the REFERENCE constraint
                            //       "FK_dbo.BazaarItem_dbo.ItemInstance_ItemInstanceId". The
                            //       conflict occurred in database "opennos", table "dbo.BazaarItem",
                            //       column 'ItemInstanceId'.
                            Logger.LogUserEventError("ONSAVEDELETION_EXCEPTION", "Saving Process", $"FALLBACK FUNCTION FAILED! Detailed Item Information: Item ID = {id}", exc);
                        }
                    }
                }
                return DeleteResult.Deleted;
            }
        }

        public IEnumerable<ItemInstanceDTO> InsertOrUpdate(IEnumerable<ItemInstanceDTO> dtos)
        {
            try
            {
                IList<ItemInstanceDTO> results = new List<ItemInstanceDTO>();
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    foreach (ItemInstanceDTO dto in dtos)
                    {
                        results.Add(InsertOrUpdate(context, dto));
                    }
                }
                return results;
            }
            catch (Exception e)
            {
                Logger.Error($"Message: {e.Message}", e);
                return Enumerable.Empty<ItemInstanceDTO>();
            }
        }

        public ItemInstanceDTO InsertOrUpdate(ItemInstanceDTO dto)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    return InsertOrUpdate(context, dto);
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Message: {e.Message}", e);
                return null;
            }
        }

        public SaveResult InsertOrUpdateFromList(IEnumerable<ItemInstanceDTO> items)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    void Insert(ItemInstanceDTO iteminstance)
                    {
                        ItemInstance entity = new ItemInstance();
                        Map(iteminstance, entity);
                        context.ItemInstance.Add(entity);
                        context.SaveChanges();
                        iteminstance.Id = entity.Id;
                    }

                    void Update(ItemInstance entity, ItemInstanceDTO iteminstance)
                    {
                        if (entity != null)
                        {
                            Map(iteminstance, entity);
                        }
                    }

                    foreach (ItemInstanceDTO item in items)
                    {
                        ItemInstance entity = context.ItemInstance.FirstOrDefault(c => c.Id == item.Id);

                        if (entity == null)
                        {
                            Insert(item);
                        }
                        else
                        {
                            Update(entity, item);
                        }
                    }

                    context.SaveChanges();
                    return SaveResult.Updated;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<ItemInstanceDTO> LoadByCharacterId(long characterId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<ItemInstanceDTO> result = new List<ItemInstanceDTO>();
                foreach (ItemInstance itemInstance in context.ItemInstance.AsNoTracking().Where(i => i.CharacterId.Equals(characterId)))
                {
                    ItemInstanceDTO output = new ItemInstanceDTO();
                    Map(itemInstance, output);
                    result.Add(output);
                }
                return result;
            }
        }

        public ItemInstanceDTO LoadById(Guid id)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                ItemInstanceDTO itemInstanceDTO = new ItemInstanceDTO();
                if (Map(context.ItemInstance.AsNoTracking().FirstOrDefault(i => i.Id.Equals(id)), itemInstanceDTO))
                {
                    return itemInstanceDTO;
                }

                return null;
            }
        }

        public ItemInstanceDTO LoadBySlotAndType(long characterId, short slot, InventoryType type)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    ItemInstance entity = context.ItemInstance.AsNoTracking().FirstOrDefault(i => i.CharacterId == characterId && i.Slot == slot && i.Type == type);
                    ItemInstanceDTO output = new ItemInstanceDTO();
                    if (Map(entity, output))
                    {
                        return output;
                    }

                    return null;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public IEnumerable<ItemInstanceDTO> LoadByType(long characterId, InventoryType type)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<ItemInstanceDTO> result = new List<ItemInstanceDTO>();
                foreach (ItemInstance itemInstance in context.ItemInstance.AsNoTracking().Where(i => i.CharacterId == characterId && i.Type == type))
                {
                    ItemInstanceDTO output = new ItemInstanceDTO();
                    Map(itemInstance, output);
                    result.Add(output);
                }
                return result;
            }
        }

        public IList<Guid> LoadSlotAndTypeByCharacterId(long characterId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    return context.ItemInstance.AsNoTracking().Where(i => i.CharacterId.Equals(characterId)).Select(i => i.Id).ToList();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        private static ItemInstanceDTO Insert(ItemInstanceDTO dto, OpenNosContext context)
        {
            ItemInstance entity = new ItemInstance();
            Map(dto, entity);
            context.Set<ItemInstance>().Add(entity);
            context.SaveChanges();
            if (Map(entity, dto))
            {
                return dto;
            }

            return null;
        }

        private static ItemInstanceDTO InsertOrUpdate(OpenNosContext context, ItemInstanceDTO dto)
        {
            try
            {
                ItemInstance entity = context.ItemInstance.FirstOrDefault(c => c.Id == dto.Id);
                return entity == null ? Insert(dto, context) : Update(entity, dto, context);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        private static ItemInstanceDTO Update(ItemInstance entity, ItemInstanceDTO inventory, OpenNosContext context)
        {
            if (entity != null)
            {
                Map(inventory, entity);
                context.SaveChanges();
            }
            if (Map(entity, inventory))
            {
                return inventory;
            }
            return null;
        }

        private static bool Map(ItemInstance input, ItemInstanceDTO output)
        {
            if (input == null)
            {
                return false;
            }
            Mapper.Mappers.ItemInstanceMapper.ToItemInstanceDTO(input, output);
            if (output.EquipmentSerialId == Guid.Empty)
            {
                output.EquipmentSerialId = Guid.NewGuid();
            }
            return true;
        }

        private static void Map(ItemInstanceDTO input, ItemInstance output)
        {
            if (input == null)
            {
                return;
            }
            Mapper.Mappers.ItemInstanceMapper.ToItemInstance(input, output);
            if (output.EquipmentSerialId == Guid.Empty)
            {
                output.EquipmentSerialId = Guid.NewGuid();
            }
        }

        public DeleteResult DeleteByVNum(short vNum)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                IEnumerable<ItemInstance> entity = context.ItemInstance.Where(i => i.ItemVNum == vNum);
                context.Set<ItemInstance>().RemoveRange(entity);
                context.SaveChanges();
                return DeleteResult.Deleted;
            }
        }

        #endregion
    }
}