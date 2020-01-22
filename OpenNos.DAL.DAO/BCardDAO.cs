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
    public class BCardDAO : IBCardDAO
    {
        #region Methods

        public BCardDTO Insert(ref BCardDTO cardObject)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    BCard entity = new BCard();
                    Mapper.Mappers.BCardMapper.ToBCard(cardObject, entity);
                    context.BCard.Add(entity);
                    context.SaveChanges();
                    if (Mapper.Mappers.BCardMapper.ToBCardDTO(entity, cardObject))
                    {
                        return cardObject;
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

        public void Insert(List<BCardDTO> cards)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (BCardDTO card in cards)
                    {
                        BCard entity = new BCard();
                        Mapper.Mappers.BCardMapper.ToBCard(card, entity);
                        context.BCard.Add(entity);
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

        public IEnumerable<BCardDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<BCardDTO> result = new List<BCardDTO>();
                foreach (BCard card in context.BCard.AsNoTracking())
                {
                    BCardDTO dto = new BCardDTO();
                    Mapper.Mappers.BCardMapper.ToBCardDTO(card, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public IEnumerable<BCardDTO> LoadByCardId(short cardId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<BCardDTO> result = new List<BCardDTO>();
                foreach (BCard card in context.BCard.AsNoTracking().Where(s => s.CardId == cardId))
                {
                    BCardDTO dto = new BCardDTO();
                    Mapper.Mappers.BCardMapper.ToBCardDTO(card, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public BCardDTO LoadById(short cardId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    BCardDTO dto = new BCardDTO();
                    if (Mapper.Mappers.BCardMapper.ToBCardDTO(context.BCard.AsNoTracking().FirstOrDefault(s => s.BCardId.Equals(cardId)), dto))
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

        public IEnumerable<BCardDTO> LoadByItemVNum(short vNum)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<BCardDTO> result = new List<BCardDTO>();
                foreach (BCard card in context.BCard.AsNoTracking().Where(s => s.ItemVNum == vNum))
                {
                    BCardDTO dto = new BCardDTO();
                    Mapper.Mappers.BCardMapper.ToBCardDTO(card, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public IEnumerable<BCardDTO> LoadByNpcMonsterVNum(short vNum)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<BCardDTO> result = new List<BCardDTO>();
                foreach (BCard card in context.BCard.AsNoTracking().Where(s => s.NpcMonsterVNum == vNum))
                {
                    BCardDTO dto = new BCardDTO();
                    Mapper.Mappers.BCardMapper.ToBCardDTO(card, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public IEnumerable<BCardDTO> LoadBySkillVNum(short vNum)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<BCardDTO> result = new List<BCardDTO>();
                foreach (BCard card in context.BCard.AsNoTracking().Where(s => s.SkillVNum == vNum))
                {
                    BCardDTO dto = new BCardDTO();
                    Mapper.Mappers.BCardMapper.ToBCardDTO(card, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        #endregion
    }
}