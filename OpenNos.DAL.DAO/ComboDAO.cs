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
using System;
using System.Collections.Generic;
using System.Linq;
using OpenNos.DAL.EF.Context;

namespace OpenNos.DAL.DAO
{
    public class ComboDAO : IComboDAO
    {
        #region Methods

        public void Insert(List<ComboDTO> combos)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (ComboDTO combo in combos)
                    {
                        Combo entity = new Combo();
                        Mapper.Mappers.ComboMapper.ToCombo(combo, entity);
                        context.Combo.Add(entity);
                    }
                    context.Configuration.AutoDetectChangesEnabled = true;
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public ComboDTO Insert(ComboDTO combo)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    Combo entity = new Combo();
                    Mapper.Mappers.ComboMapper.ToCombo(combo, entity);
                    context.Combo.Add(entity);
                    context.SaveChanges();
                    if (Mapper.Mappers.ComboMapper.ToComboDTO(entity, combo))
                    {
                        return combo;
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

        public IEnumerable<ComboDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<ComboDTO> result = new List<ComboDTO>();
                foreach (Combo combo in context.Combo.AsNoTracking())
                {
                    ComboDTO dto = new ComboDTO();
                    Mapper.Mappers.ComboMapper.ToComboDTO(combo, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public ComboDTO LoadById(short comboId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    ComboDTO dto = new ComboDTO();
                    if (Mapper.Mappers.ComboMapper.ToComboDTO(context.Combo.AsNoTracking().FirstOrDefault(s => s.SkillVNum.Equals(comboId)), dto))
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

        public IEnumerable<ComboDTO> LoadBySkillVnum(short skillVNum)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<ComboDTO> result = new List<ComboDTO>();
                foreach (Combo combo in context.Combo.AsNoTracking().Where(c => c.SkillVNum == skillVNum))
                {
                    ComboDTO dto = new ComboDTO();
                    Mapper.Mappers.ComboMapper.ToComboDTO(combo, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public IEnumerable<ComboDTO> LoadByVNumHitAndEffect(short skillVNum, short hit, short effect)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<ComboDTO> result = new List<ComboDTO>();
                foreach (Combo combo in context.Combo.AsNoTracking().Where(s => s.SkillVNum == skillVNum && s.Hit == hit && s.Effect == effect))
                {
                    ComboDTO dto = new ComboDTO();
                    Mapper.Mappers.ComboMapper.ToComboDTO(combo, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        #endregion
    }
}