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
    public class FamilyCharacterDAO : IFamilyCharacterDAO
    {
        #region Methods

        public DeleteResult Delete(string characterName)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    Character character = context.Character.FirstOrDefault(c => c.Name.Equals(characterName));
                    FamilyCharacter familyCharacter = context.FamilyCharacter.FirstOrDefault(c => c.CharacterId.Equals(character.CharacterId));
                    if (character != null && familyCharacter != null)
                    {
                        context.FamilyCharacter.Remove(familyCharacter);
                        context.SaveChanges();
                    }

                    return DeleteResult.Deleted;
                }
            }
            catch (Exception e)
            {
                Logger.Error(string.Format(Language.Instance.GetMessageFromKey("DELETE_FAMILYCHARACTER_ERROR"), e.Message), e);
                return DeleteResult.Error;
            }
        }

        public SaveResult InsertOrUpdate(ref FamilyCharacterDTO character)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    long familyCharacterId = character.FamilyCharacterId;
                    FamilyCharacter entity = context.FamilyCharacter.FirstOrDefault(c => c.FamilyCharacterId.Equals(familyCharacterId));

                    if (entity == null)
                    {
                        character = Insert(character, context);
                        return SaveResult.Inserted;
                    }

                    character = Update(entity, character, context);
                    return SaveResult.Updated;
                }
            }
            catch (Exception e)
            {
                Logger.Error(string.Format(Language.Instance.GetMessageFromKey("INSERT_ERROR"), character, e.Message), e);
                return SaveResult.Error;
            }
        }

        public FamilyCharacterDTO LoadByCharacterId(long characterId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    FamilyCharacterDTO dto = new FamilyCharacterDTO();
                    if (Mapper.Mappers.FamilyCharacterMapper.ToFamilyCharacterDTO(context.FamilyCharacter.AsNoTracking().FirstOrDefault(c => c.CharacterId == characterId), dto))
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

        public IList<FamilyCharacterDTO> LoadByFamilyId(long familyId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<FamilyCharacterDTO> result = new List<FamilyCharacterDTO>();
                foreach (FamilyCharacter entity in context.FamilyCharacter.AsNoTracking().Where(fc => fc.FamilyId.Equals(familyId)))
                {
                    FamilyCharacterDTO dto = new FamilyCharacterDTO();
                    Mapper.Mappers.FamilyCharacterMapper.ToFamilyCharacterDTO(entity, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public FamilyCharacterDTO LoadById(long familyCharacterId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    FamilyCharacterDTO dto = new FamilyCharacterDTO();
                    if (Mapper.Mappers.FamilyCharacterMapper.ToFamilyCharacterDTO(context.FamilyCharacter.AsNoTracking().FirstOrDefault(c => c.FamilyCharacterId.Equals(familyCharacterId)), dto))
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

        private static FamilyCharacterDTO Insert(FamilyCharacterDTO character, OpenNosContext context)
        {
            FamilyCharacter entity = new FamilyCharacter();
            Mapper.Mappers.FamilyCharacterMapper.ToFamilyCharacter(character, entity);
            context.FamilyCharacter.Add(entity);
            context.SaveChanges();
            if (Mapper.Mappers.FamilyCharacterMapper.ToFamilyCharacterDTO(entity, character))
            {
                return character;
            }

            return null;
        }

        private static FamilyCharacterDTO Update(FamilyCharacter entity, FamilyCharacterDTO character, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.Mappers.FamilyCharacterMapper.ToFamilyCharacter(character, entity);
                context.SaveChanges();
            }
            if (Mapper.Mappers.FamilyCharacterMapper.ToFamilyCharacterDTO(entity, character))
            {
                return character;
            }

            return null;
        }

        #endregion
    }
}