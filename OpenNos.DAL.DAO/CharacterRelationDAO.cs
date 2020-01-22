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
    public class CharacterRelationDAO : ICharacterRelationDAO
    {
        #region Methods

        public DeleteResult Delete(long characterRelationId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    CharacterRelation relation = context.CharacterRelation.SingleOrDefault(c => c.CharacterRelationId.Equals(characterRelationId));

                    if (relation != null)
                    {
                        context.CharacterRelation.Remove(relation);
                        context.SaveChanges();
                    }

                    return DeleteResult.Deleted;
                }
            }
            catch (Exception e)
            {
                Logger.Error(string.Format(Language.Instance.GetMessageFromKey("DELETE_CHARACTER_ERROR"), characterRelationId, e.Message), e);
                return DeleteResult.Error;
            }
        }

        public SaveResult InsertOrUpdate(ref CharacterRelationDTO characterRelation)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    long characterId = characterRelation.CharacterId;
                    long relatedCharacterId = characterRelation.RelatedCharacterId;
                    CharacterRelation entity = context.CharacterRelation.FirstOrDefault(c => c.CharacterId.Equals(characterId) && c.RelatedCharacterId.Equals(relatedCharacterId));

                    if (entity == null)
                    {
                        characterRelation = Insert(characterRelation, context);
                        return SaveResult.Inserted;
                    }
                    characterRelation = Update(entity, characterRelation, context);
                    return SaveResult.Updated;
                }
            }
            catch (Exception e)
            {
                Logger.Error(string.Format(Language.Instance.GetMessageFromKey("UPDATE_CHARACTERRELATION_ERROR"), characterRelation.CharacterRelationId, e.Message), e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<CharacterRelationDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<CharacterRelationDTO> result = new List<CharacterRelationDTO>();
                foreach (CharacterRelation entity in context.CharacterRelation.AsNoTracking())
                {
                    CharacterRelationDTO dto = new CharacterRelationDTO();
                    Mapper.Mappers.CharacterRelationMapper.ToCharacterRelationDTO(entity, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public CharacterRelationDTO LoadById(long characterId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    CharacterRelationDTO dto = new CharacterRelationDTO();
                    if (Mapper.Mappers.CharacterRelationMapper.ToCharacterRelationDTO(context.CharacterRelation.AsNoTracking().FirstOrDefault(s => s.CharacterRelationId.Equals(characterId)), dto))
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

        private static CharacterRelationDTO Insert(CharacterRelationDTO relation, OpenNosContext context)
        {
            CharacterRelation entity = new CharacterRelation();
            Mapper.Mappers.CharacterRelationMapper.ToCharacterRelation(relation, entity);
            context.CharacterRelation.Add(entity);
            context.SaveChanges();
            if (Mapper.Mappers.CharacterRelationMapper.ToCharacterRelationDTO(entity, relation))
            {
                return relation;
            }

            return null;
        }

        private static CharacterRelationDTO Update(CharacterRelation entity, CharacterRelationDTO relation, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.Mappers.CharacterRelationMapper.ToCharacterRelation(relation, entity);
                context.SaveChanges();
            }

            if (Mapper.Mappers.CharacterRelationMapper.ToCharacterRelationDTO(entity, relation))
            {
                return relation;
            }

            return null;
        }

        #endregion
    }
}