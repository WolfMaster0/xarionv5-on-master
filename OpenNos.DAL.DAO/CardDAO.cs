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
    public class CardDAO : ICardDAO
    {
        #region Methods

        public CardDTO Insert(ref CardDTO card)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    Card entity = new Card();
                    Mapper.Mappers.CardMapper.ToCard(card, entity);
                    context.Card.Add(entity);
                    context.SaveChanges();
                    if (Mapper.Mappers.CardMapper.ToCardDTO(entity, card))
                    {
                        return card;
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

        public void Insert(List<CardDTO> cards)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (CardDTO card in cards)
                    {
                        Card entity = new Card();
                        Mapper.Mappers.CardMapper.ToCard(card, entity);
                        context.Card.Add(entity);
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

        public IEnumerable<CardDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<CardDTO> result = new List<CardDTO>();
                foreach (Card card in context.Card.AsNoTracking())
                {
                    CardDTO dto = new CardDTO();
                    Mapper.Mappers.CardMapper.ToCardDTO(card, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public CardDTO LoadById(short cardId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    CardDTO dto = new CardDTO();
                    if (Mapper.Mappers.CardMapper.ToCardDTO(context.Card.AsNoTracking().FirstOrDefault(s => s.CardId.Equals(cardId)), dto))
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

        #endregion
    }
}