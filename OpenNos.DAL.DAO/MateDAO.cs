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
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenNos.DAL.EF;
using OpenNos.DAL.EF.Context;

namespace OpenNos.DAL.DAO
{
    public class MateDAO : IMateDAO
    {
        #region Methods

        public DeleteResult Delete(long id)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    Mate mate = context.Mate.FirstOrDefault(c => c.MateId.Equals(id));
                    if (mate != null)
                    {
                        context.Mate.Remove(mate);
                        context.SaveChanges();
                    }

                    return DeleteResult.Deleted;
                }
            }
            catch (Exception e)
            {
                Logger.Error(string.Format(Language.Instance.GetMessageFromKey("DELETE_MATE_ERROR"), e.Message), e);
                return DeleteResult.Error;
            }
        }

        public SaveResult InsertOrUpdate(ref MateDTO mate)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    long mateId = mate.MateId;
                    Mate entity = context.Mate.FirstOrDefault(c => c.MateId.Equals(mateId));

                    if (entity == null)
                    {
                        mate = Insert(mate, context);
                        return SaveResult.Inserted;
                    }

                    mate = Update(entity, mate, context);
                    return SaveResult.Updated;
                }
            }
            catch (Exception e)
            {
                Logger.Error(string.Format(Language.Instance.GetMessageFromKey("INSERT_ERROR"), mate, e.Message), e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<MateDTO> LoadByCharacterId(long characterId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<MateDTO> result = new List<MateDTO>();
                foreach (Mate mate in context.Mate.AsNoTracking().Where(s => s.CharacterId == characterId))
                {
                    MateDTO dto = new MateDTO();
                    Mapper.Mappers.MateMapper.ToMateDTO(mate, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        private static MateDTO Insert(MateDTO mate, OpenNosContext context)
        {
            Mate entity = new Mate();
            Mapper.Mappers.MateMapper.ToMate(mate, entity);
            context.Mate.Add(entity);
            context.SaveChanges();
            if (Mapper.Mappers.MateMapper.ToMateDTO(entity, mate))
            {
                return mate;
            }

            return null;
        }

        private static MateDTO Update(Mate entity, MateDTO character, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.Mappers.MateMapper.ToMate(character, entity);
                context.SaveChanges();
            }

            if (Mapper.Mappers.MateMapper.ToMateDTO(entity, character))
            {
                return character;
            }

            return null;
        }

        #endregion
    }
}