// This file is part of the OpenNos NosTale Emulator Project.
//
// This program is licensed under a deviated version of the Fair Source License, granting you a
// non-exclusive, non-transferable, royalty-free and fully-paid-up license, under all of the
// Licensor's copyright and patent rights, to use, copy, prepare derivative works of, publicly
// perform and display the Software, subject to the conditions found in the LICENSE file.
//
// THIS FILE IS PROVIDED "AS IS", WITHOUT WARRANTY OR CONDITION, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. THE AUTHORS HEREBY DISCLAIM ALL LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT
// OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE.
using OpenNos.Core;
using OpenNos.DAL.EF;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenNos.DAL.EF.Context;

namespace OpenNos.DAL.DAO
{
    public class CharacterDAO : ICharacterDAO
    {
        #region Methods

        public DeleteResult DeleteByPrimaryKey(long accountId, byte characterSlot)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    // actually a Character wont be deleted, it just will be disabled for future traces
                    Character character = context.Character.SingleOrDefault(c => c.AccountId.Equals(accountId) && c.Slot.Equals(characterSlot) && c.State.Equals((byte)CharacterState.Active));

                    if (character != null)
                    {
                        character.State = (byte)CharacterState.Inactive;
                        context.SaveChanges();
                    }

                    return DeleteResult.Deleted;
                }
            }
            catch (Exception e)
            {
                Logger.Error(string.Format(Language.Instance.GetMessageFromKey("DELETE_CHARACTER_ERROR"), characterSlot, e.Message), e);
                return DeleteResult.Error;
            }
        }

        /// <summary>
        /// Returns first 30 occurences of highest Compliment
        /// </summary>
        /// <returns></returns>
        public List<CharacterDTO> GetTopCompliment()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<CharacterDTO> result = new List<CharacterDTO>();
                foreach (Character entity in context.Character.AsNoTracking().Where(c => c.Account.Authority == AuthorityType.User || c.Account.Authority == AuthorityType.Moderator && !c.Account.PenaltyLog.Any(l => l.Penalty == PenaltyType.Banned && l.DateEnd > DateTime.UtcNow)).OrderByDescending(c => c.Compliment).Take(30))
                {
                    CharacterDTO dto = new CharacterDTO();
                    Mapper.Mappers.CharacterMapper.ToCharacterDTO(entity, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        /// <summary>
        /// Returns first 30 occurences of highest Act4Points
        /// </summary>
        /// <returns></returns>
        public List<CharacterDTO> GetTopPoints()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<CharacterDTO> result = new List<CharacterDTO>();
                foreach (Character entity in context.Character.AsNoTracking().Where(c => c.Account.Authority == AuthorityType.User || c.Account.Authority == AuthorityType.Moderator && !c.Account.PenaltyLog.Any(l => l.Penalty == PenaltyType.Banned && l.DateEnd > DateTime.UtcNow)).OrderByDescending(c => c.Act4Points).Take(30))
                {
                    CharacterDTO dto = new CharacterDTO();
                    Mapper.Mappers.CharacterMapper.ToCharacterDTO(entity, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        /// <summary>
        /// Returns first 30 occurences of highest Reputation
        /// </summary>
        /// <returns></returns>
        public List<CharacterDTO> GetTopReputation()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<CharacterDTO> result = new List<CharacterDTO>();
                foreach (Character entity in context.Character.AsNoTracking().Where(c => c.Account.Authority == AuthorityType.User || c.Account.Authority == AuthorityType.Moderator && !c.Account.PenaltyLog.Any(l => l.Penalty == PenaltyType.Banned && l.DateEnd > DateTime.UtcNow)).OrderByDescending(c => c.Reputation).Take(43))
                {
                    CharacterDTO dto = new CharacterDTO();
                    Mapper.Mappers.CharacterMapper.ToCharacterDTO(entity, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public SaveResult InsertOrUpdate(ref CharacterDTO character)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    long characterId = character.CharacterId;
                    Character entity = context.Character.FirstOrDefault(c => c.CharacterId.Equals(characterId));
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

        public IEnumerable<CharacterDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<CharacterDTO> result = new List<CharacterDTO>();
                foreach (Character chara in context.Character.AsNoTracking())
                {
                    CharacterDTO dto = new CharacterDTO();
                    Mapper.Mappers.CharacterMapper.ToCharacterDTO(chara, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public IEnumerable<CharacterDTO> LoadAllByAccount(long accountId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<CharacterDTO> result = new List<CharacterDTO>();
                foreach (Character entity in context.Character.AsNoTracking().Where(c => c.AccountId.Equals(accountId)).OrderByDescending(c => c.Slot))
                {
                    CharacterDTO dto = new CharacterDTO();
                    Mapper.Mappers.CharacterMapper.ToCharacterDTO(entity, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public IEnumerable<CharacterDTO> LoadByAccount(long accountId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<CharacterDTO> result = new List<CharacterDTO>();
                foreach (Character entity in context.Character.AsNoTracking().Where(c => c.AccountId.Equals(accountId) && c.State.Equals((byte)CharacterState.Active)).OrderByDescending(c => c.Slot))
                {
                    CharacterDTO dto = new CharacterDTO();
                    Mapper.Mappers.CharacterMapper.ToCharacterDTO(entity, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public CharacterDTO LoadById(long characterId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    CharacterDTO dto = new CharacterDTO();
                    if (Mapper.Mappers.CharacterMapper.ToCharacterDTO(context.Character.AsNoTracking().FirstOrDefault(c => c.CharacterId.Equals(characterId)), dto))
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

        public CharacterDTO LoadByName(string name)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    CharacterDTO dto = new CharacterDTO();
                    if (Mapper.Mappers.CharacterMapper.ToCharacterDTO(context.Character.AsNoTracking().SingleOrDefault(c => c.Name.Equals(name)), dto))
                    {
                        return dto;
                    }

                    return null;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            return null;
        }

        public CharacterDTO LoadBySlot(long accountId, byte slot)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    CharacterDTO dto = new CharacterDTO();
                    if (Mapper.Mappers.CharacterMapper.ToCharacterDTO(context.Character.AsNoTracking().SingleOrDefault(c => c.AccountId.Equals(accountId) && c.Slot.Equals(slot) && c.State.Equals((byte)CharacterState.Active)), dto))
                    {
                        return dto;
                    }
                }
            }
            catch (Exception e)
            {
                // for now we shall ban the user that has 2 characters per slot and ask him to
                // explain what he did to make it happen. might also be saving issue.
                Logger.Error($"There should be only 1 character per slot, AccountId: {accountId} Slot: {slot}", e);
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    PenaltyLogDTO penalty = new PenaltyLogDTO() { AccountId = accountId, DateStart = DateTime.UtcNow, DateEnd = DateTime.UtcNow.AddYears(15), Penalty = PenaltyType.Banned, Reason = $"OpenBuster: Multiple characters in slot:{slot} recognized", AdminName = "OpenBuster" };
                    PenaltyLog penaltyLog = new PenaltyLog();
                    if (Mapper.Mappers.PenaltyLogMapper.ToPenaltyLog(penalty, penaltyLog))
                    {
                        context.PenaltyLog.Add(penaltyLog);
                        context.SaveChanges();
                    }
                }
            }
            return null;
        }

        private static CharacterDTO Insert(CharacterDTO character, OpenNosContext context)
        {
            Character entity = new Character();
            Mapper.Mappers.CharacterMapper.ToCharacter(character, entity);
            context.Character.Add(entity);
            context.SaveChanges();
            if (Mapper.Mappers.CharacterMapper.ToCharacterDTO(entity, character))
            {
                return character;
            }
            return null;
        }

        private static CharacterDTO Update(Character entity, CharacterDTO character, OpenNosContext context)
        {
            if (entity != null)
            {
                // State Updates should only occur upon deleting character, so outside of this method.
                byte state = entity.State;
                Mapper.Mappers.CharacterMapper.ToCharacter(character, entity);
                entity.State = state;

                context.SaveChanges();
            }

            if (Mapper.Mappers.CharacterMapper.ToCharacterDTO(entity, character))
            {
                return character;
            }

            return null;
        }

        #endregion
    }
}