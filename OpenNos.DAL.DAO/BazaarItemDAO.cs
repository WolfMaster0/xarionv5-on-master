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
using OpenNos.Core;
using OpenNos.DAL.EF;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using OpenNos.DAL.EF.Context;

namespace OpenNos.DAL.DAO
{
    public class BazaarItemDAO : IBazaarItemDAO
    {
        #region Methods

        public DeleteResult Delete(long bazaarItemId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    BazaarItem bazaarItem = context.BazaarItem.FirstOrDefault(c => c.BazaarItemId.Equals(bazaarItemId));

                    if (bazaarItem != null)
                    {
                        context.BazaarItem.Remove(bazaarItem);
                        context.SaveChanges();
                    }

                    return DeleteResult.Deleted;
                }
            }
            catch (Exception e)
            {
                Logger.Error(string.Format(Language.Instance.GetMessageFromKey("DELETE_ERROR"), bazaarItemId, e.Message), e);
                return DeleteResult.Error;
            }
        }

        public SaveResult InsertOrUpdate(ref BazaarItemDTO bazaarItem)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    long bazaarItemId = bazaarItem.BazaarItemId;
                    BazaarItem entity = context.BazaarItem.FirstOrDefault(c => c.BazaarItemId.Equals(bazaarItemId));

                    if (entity == null)
                    {
                        bazaarItem = Insert(bazaarItem, context);
                        return SaveResult.Inserted;
                    }

                    bazaarItem = Update(entity, bazaarItem, context);
                    return SaveResult.Updated;
                }
            }
            catch (Exception e)
            {
                Logger.Error($"BazaarItemId: {bazaarItem.BazaarItemId} Message: {e.Message}", e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<BazaarItemDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<BazaarItemDTO> result = new List<BazaarItemDTO>();
                foreach (BazaarItem bazaarItem in context.BazaarItem.AsNoTracking())
                {
                    BazaarItemDTO dto = new BazaarItemDTO();
                    Mapper.Mappers.BazaarItemMapper.ToBazaarItemDTO(bazaarItem, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public BazaarItemDTO LoadById(long bazaarItemId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    BazaarItemDTO dto = new BazaarItemDTO();
                    if (Mapper.Mappers.BazaarItemMapper.ToBazaarItemDTO(context.BazaarItem.AsNoTracking().FirstOrDefault(i => i.BazaarItemId.Equals(bazaarItemId)), dto))
                    {
                        return dto;
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

        public void RemoveOutDated()
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    foreach (BazaarItem entity in context.BazaarItem.Where(e => DbFunctions.AddDays(DbFunctions.AddHours(e.DateStart, e.Duration), e.MedalUsed ? 30 : 7) < DateTime.UtcNow))
                    {
                        context.BazaarItem.Remove(entity);
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        private static BazaarItemDTO Insert(BazaarItemDTO bazaarItem, OpenNosContext context)
        {
            BazaarItem entity = new BazaarItem();
            Mapper.Mappers.BazaarItemMapper.ToBazaarItem(bazaarItem, entity);
            context.BazaarItem.Add(entity);
            context.SaveChanges();
            if (Mapper.Mappers.BazaarItemMapper.ToBazaarItemDTO(entity, bazaarItem))
            {
                return bazaarItem;
            }

            return null;
        }

        private static BazaarItemDTO Update(BazaarItem entity, BazaarItemDTO bazaarItem, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.Mappers.BazaarItemMapper.ToBazaarItem(bazaarItem, entity);
                context.SaveChanges();
            }
            if (Mapper.Mappers.BazaarItemMapper.ToBazaarItemDTO(entity, bazaarItem))
            {
                return bazaarItem;
            }

            return null;
        }

        #endregion
    }
}