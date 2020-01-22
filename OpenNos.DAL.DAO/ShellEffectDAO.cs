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
    public class ShellEffectDAO : IShellEffectDAO
    {
        #region Methods

        public DeleteResult DeleteByEquipmentSerialId(Guid id)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    List<ShellEffect> deleteentities = context.ShellEffect.Where(s => s.EquipmentSerialId == id).ToList();
                    if (deleteentities.Count != 0)
                    {
                        context.ShellEffect.RemoveRange(deleteentities);
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

        public ShellEffectDTO InsertOrUpdate(ShellEffectDTO shelleffect)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    long shelleffectId = shelleffect.ShellEffectId;
                    ShellEffect entity = context.ShellEffect.FirstOrDefault(c => c.ShellEffectId.Equals(shelleffectId));

                    if (entity == null)
                    {
                        return Insert(shelleffect, context);
                    }
                    return Update(entity, shelleffect, context);
                }
            }
            catch (Exception e)
            {
                Logger.Error(string.Format(Language.Instance.GetMessageFromKey("INSERT_ERROR"), shelleffect, e.Message), e);
                return shelleffect;
            }
        }

        public void InsertOrUpdateFromList(List<ShellEffectDTO> shellEffects, Guid equipmentSerialId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    void Insert(ShellEffectDTO shelleffect)
                    {
                        ShellEffect entity = new ShellEffect();
                        Mapper.Mappers.ShellEffectMapper.ToShellEffect(shelleffect, entity);
                        context.ShellEffect.Add(entity);
                        context.SaveChanges();
                        shelleffect.ShellEffectId = entity.ShellEffectId;
                    }

                    void Update(ShellEffect entity, ShellEffectDTO shelleffect)
                    {
                        if (entity != null)
                        {
                            Mapper.Mappers.ShellEffectMapper.ToShellEffect(shelleffect, entity);
                        }
                    }

                    foreach (ShellEffectDTO item in shellEffects)
                    {
                        item.EquipmentSerialId = equipmentSerialId;
                        ShellEffect entity = context.ShellEffect.FirstOrDefault(c => c.ShellEffectId == item.ShellEffectId);

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

        public IEnumerable<ShellEffectDTO> LoadByEquipmentSerialId(Guid id)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<ShellEffectDTO> result = new List<ShellEffectDTO>();
                foreach (ShellEffect entity in context.ShellEffect.AsNoTracking().Where(c => c.EquipmentSerialId == id))
                {
                    ShellEffectDTO dto = new ShellEffectDTO();
                    Mapper.Mappers.ShellEffectMapper.ToShellEffectDTO(entity, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        private static ShellEffectDTO Insert(ShellEffectDTO shelleffect, OpenNosContext context)
        {
            ShellEffect entity = new ShellEffect();
            Mapper.Mappers.ShellEffectMapper.ToShellEffect(shelleffect, entity);
            context.ShellEffect.Add(entity);
            context.SaveChanges();
            if (Mapper.Mappers.ShellEffectMapper.ToShellEffectDTO(entity, shelleffect))
            {
                return shelleffect;
            }

            return null;
        }

        private static ShellEffectDTO Update(ShellEffect entity, ShellEffectDTO shelleffect, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.Mappers.ShellEffectMapper.ToShellEffect(shelleffect, entity);
                context.SaveChanges();
            }

            if (Mapper.Mappers.ShellEffectMapper.ToShellEffectDTO(entity, shelleffect))
            {
                return shelleffect;
            }

            return null;
        }

        #endregion
    }
}