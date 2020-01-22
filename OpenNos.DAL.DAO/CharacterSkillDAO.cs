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
    public class CharacterSkillDAO : ICharacterSkillDAO
    {
        #region Methods

        public DeleteResult Delete(long characterId, short skillVNum)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    CharacterSkill invItem = context.CharacterSkill.FirstOrDefault(i => i.CharacterId == characterId && i.SkillVNum == skillVNum);
                    if (invItem != null)
                    {
                        context.CharacterSkill.Remove(invItem);
                        context.SaveChanges();
                    }
                    return DeleteResult.Deleted;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return DeleteResult.Error;
            }
        }

        public DeleteResult Delete(Guid id)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                CharacterSkill entity = context.Set<CharacterSkill>().FirstOrDefault(i => i.Id == id);
                if (entity != null)
                {
                    context.Set<CharacterSkill>().Remove(entity);
                    context.SaveChanges();
                }
                return DeleteResult.Deleted;
            }
        }

        public IEnumerable<CharacterSkillDTO> InsertOrUpdate(IEnumerable<CharacterSkillDTO> dtos)
        {
            try
            {
                IList<CharacterSkillDTO> results = new List<CharacterSkillDTO>();
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    foreach (CharacterSkillDTO dto in dtos)
                    {
                        results.Add(InsertOrUpdate(context, dto));
                    }
                }
                return results;
            }
            catch (Exception e)
            {
                Logger.Error($"Message: {e.Message}", e);
                return Enumerable.Empty<CharacterSkillDTO>();
            }
        }

        public CharacterSkillDTO InsertOrUpdate(CharacterSkillDTO dto)
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

        public IEnumerable<CharacterSkillDTO> LoadByCharacterId(long characterId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<CharacterSkillDTO> result = new List<CharacterSkillDTO>();
                foreach (CharacterSkill entity in context.CharacterSkill.AsNoTracking().Where(i => i.CharacterId == characterId))
                {
                    CharacterSkillDTO output = new CharacterSkillDTO();
                    Mapper.Mappers.CharacterSkillMapper.ToCharacterSkillDTO(entity, output);
                    result.Add(output);
                }
                return result;
            }
        }

        public CharacterSkillDTO LoadById(Guid id)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                CharacterSkillDTO characterSkillDTO = new CharacterSkillDTO();
                if (Mapper.Mappers.CharacterSkillMapper.ToCharacterSkillDTO(context.CharacterSkill.AsNoTracking().FirstOrDefault(i => i.Id.Equals(id)), characterSkillDTO))
                {
                    return characterSkillDTO;
                }

                return null;
            }
        }

        public IEnumerable<Guid> LoadKeysByCharacterId(long characterId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    return context.CharacterSkill.AsNoTracking().Where(i => i.CharacterId == characterId).Select(c => c.Id).ToList();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        protected static CharacterSkillDTO Insert(CharacterSkillDTO dto, OpenNosContext context)
        {
            CharacterSkill entity = new CharacterSkill();
            Mapper.Mappers.CharacterSkillMapper.ToCharacterSkill(dto, entity);
            context.Set<CharacterSkill>().Add(entity);
            context.SaveChanges();
            if (Mapper.Mappers.CharacterSkillMapper.ToCharacterSkillDTO(entity, dto))
            {
                return dto;
            }

            return null;
        }

        protected static CharacterSkillDTO InsertOrUpdate(OpenNosContext context, CharacterSkillDTO dto)
        {
            Guid primaryKey = dto.Id;
            CharacterSkill entity = context.Set<CharacterSkill>().FirstOrDefault(c => c.Id == primaryKey);
            if (entity == null)
            {
                return Insert(dto, context);
            }
            else
            {
                return Update(entity, dto, context);
            }
        }

        protected static CharacterSkillDTO Update(CharacterSkill entity, CharacterSkillDTO inventory, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.Mappers.CharacterSkillMapper.ToCharacterSkill(inventory, entity);
                context.SaveChanges();
            }
            if (Mapper.Mappers.CharacterSkillMapper.ToCharacterSkillDTO(entity, inventory))
            {
                return inventory;
            }

            return null;
        }

        #endregion
    }
}