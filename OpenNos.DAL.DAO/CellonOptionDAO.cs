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
using System.Linq;
using OpenNos.DAL.EF.Context;

namespace OpenNos.DAL.DAO
{
    public class CellonOptionDAO : ICellonOptionDAO
    {
        #region Methods

        public DeleteResult DeleteByEquipmentSerialId(Guid id)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    List<CellonOption> deleteentities = context.CellonOption.Where(s => s.EquipmentSerialId == id).ToList();
                    if (deleteentities.Count != 0)
                    {
                        context.CellonOption.RemoveRange(deleteentities);
                        context.SaveChanges();
                    }

                    return DeleteResult.Deleted;
                }
            }
            catch (Exception e)
            {
                Logger.Error(string.Format(Language.Instance.GetMessageFromKey("DELETE_ERROR"), id, e.Message), e);
                return DeleteResult.Error;
            }
        }

        public IEnumerable<CellonOptionDTO> GetOptionsByWearableInstanceId(Guid wearableInstanceId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<CellonOptionDTO> result = new List<CellonOptionDTO>();
                foreach (CellonOption entity in context.CellonOption.AsNoTracking().Where(c => c.EquipmentSerialId == wearableInstanceId))
                {
                    CellonOptionDTO dto = new CellonOptionDTO();
                    Mapper.Mappers.CellonOptionMapper.ToCellonOptionDTO(entity, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public CellonOptionDTO InsertOrUpdate(CellonOptionDTO cellonOption)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    long cellonOptionId = cellonOption.CellonOptionId;
                    CellonOption entity = context.CellonOption.FirstOrDefault(c => c.CellonOptionId.Equals(cellonOptionId));

                    if (entity == null)
                    {
                        return Insert(cellonOption, context);
                    }
                    return Update(entity, cellonOption, context);
                }
            }
            catch (Exception e)
            {
                Logger.Error(string.Format(Language.Instance.GetMessageFromKey("INSERT_ERROR"), cellonOption, e.Message), e);
                return cellonOption;
            }
        }

        public void InsertOrUpdateFromList(List<CellonOptionDTO> cellonOption, Guid equipmentSerialId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    void Insert(CellonOptionDTO cellonoption)
                    {
                        CellonOption entity = new CellonOption();
                        Mapper.Mappers.CellonOptionMapper.ToCellonOption(cellonoption, entity);
                        context.CellonOption.Add(entity);
                        context.SaveChanges();
                        cellonoption.CellonOptionId = entity.CellonOptionId;
                    }

                    void Update(CellonOption entity, CellonOptionDTO cellonoption)
                    {
                        if (entity != null)
                        {
                            Mapper.Mappers.CellonOptionMapper.ToCellonOption(cellonoption, entity);
                        }
                    }

                    foreach (CellonOptionDTO item in cellonOption)
                    {
                        item.EquipmentSerialId = equipmentSerialId;
                        CellonOption entity = context.CellonOption.FirstOrDefault(c => c.CellonOptionId == item.CellonOptionId);

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
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        private static CellonOptionDTO Insert(CellonOptionDTO cellonOption, OpenNosContext context)
        {
            CellonOption entity = new CellonOption();
            Mapper.Mappers.CellonOptionMapper.ToCellonOption(cellonOption, entity);
            context.CellonOption.Add(entity);
            context.SaveChanges();
            if (Mapper.Mappers.CellonOptionMapper.ToCellonOptionDTO(entity, cellonOption))
            {
                return cellonOption;
            }

            return null;
        }

        private static CellonOptionDTO Update(CellonOption entity, CellonOptionDTO cellonOption, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.Mappers.CellonOptionMapper.ToCellonOption(cellonOption, entity);
                context.SaveChanges();
            }

            if (Mapper.Mappers.CellonOptionMapper.ToCellonOptionDTO(entity, cellonOption))
            {
                return cellonOption;
            }

            return null;
        }

        #endregion
    }
}