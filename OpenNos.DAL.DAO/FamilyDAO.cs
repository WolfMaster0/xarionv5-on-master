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
    public class FamilyDAO : IFamilyDAO
    {
        #region Methods

        public DeleteResult Delete(long familyId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    Family fam = context.Family.FirstOrDefault(c => c.FamilyId == familyId);

                    if (fam != null)
                    {
                        context.Family.Remove(fam);
                        context.SaveChanges();
                    }

                    return DeleteResult.Deleted;
                }
            }
            catch (Exception e)
            {
                Logger.Error(string.Format(Language.Instance.GetMessageFromKey("DELETE_ERROR"), familyId, e.Message), e);
                return DeleteResult.Error;
            }
        }

        public SaveResult InsertOrUpdate(ref FamilyDTO family)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    long accountId = family.FamilyId;
                    Family entity = context.Family.FirstOrDefault(c => c.FamilyId.Equals(accountId));

                    if (entity == null)
                    {
                        family = Insert(family, context);
                        return SaveResult.Inserted;
                    }

                    family = Update(entity, family, context);
                    return SaveResult.Updated;
                }
            }
            catch (Exception e)
            {
                Logger.Error(string.Format(Language.Instance.GetMessageFromKey("UPDATE_FAMILY_ERROR"), family.FamilyId, e.Message), e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<FamilyDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<FamilyDTO> result = new List<FamilyDTO>();
                foreach (Family entity in context.Family.AsNoTracking())
                {
                    FamilyDTO dto = new FamilyDTO();
                    Mapper.Mappers.FamilyMapper.ToFamilyDTO(entity, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public FamilyDTO LoadByCharacterId(long characterId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    FamilyCharacter familyCharacter = context.FamilyCharacter.AsNoTracking().FirstOrDefault(fc => fc.Character.CharacterId.Equals(characterId));
                    if (familyCharacter != null)
                    {
                        Family family = context.Family.AsNoTracking().FirstOrDefault(a => a.FamilyId.Equals(familyCharacter.FamilyId));
                        if (family != null)
                        {
                            FamilyDTO dto = new FamilyDTO();
                            if (Mapper.Mappers.FamilyMapper.ToFamilyDTO(family, dto))
                            {
                                return dto;
                            }

                            return null;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            return null;
        }

        public FamilyDTO LoadById(long familyId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    Family family = context.Family.AsNoTracking().FirstOrDefault(a => a.FamilyId.Equals(familyId));
                    if (family != null)
                    {
                        FamilyDTO dto = new FamilyDTO();
                        if (Mapper.Mappers.FamilyMapper.ToFamilyDTO(family, dto))
                        {
                            return dto;
                        }

                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            return null;
        }

        public FamilyDTO LoadByName(string name)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    Family family = context.Family.AsNoTracking().FirstOrDefault(a => a.Name.Equals(name));
                    if (family != null)
                    {
                        FamilyDTO dto = new FamilyDTO();
                        if (Mapper.Mappers.FamilyMapper.ToFamilyDTO(family, dto))
                        {
                            return dto;
                        }

                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            return null;
        }

        private static FamilyDTO Insert(FamilyDTO family, OpenNosContext context)
        {
            Family entity = new Family();
            Mapper.Mappers.FamilyMapper.ToFamily(family, entity);
            context.Family.Add(entity);
            context.SaveChanges();
            if (Mapper.Mappers.FamilyMapper.ToFamilyDTO(entity, family))
            {
                return family;
            }

            return null;
        }

        private static FamilyDTO Update(Family entity, FamilyDTO family, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.Mappers.FamilyMapper.ToFamily(family, entity);
                context.SaveChanges();
            }
            if (Mapper.Mappers.FamilyMapper.ToFamilyDTO(entity, family))
            {
                return family;
            }

            return null;
        }

        #endregion
    }
}