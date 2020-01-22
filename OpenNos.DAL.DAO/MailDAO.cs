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
using System.Data.Entity.Validation;
using System.Linq;
using OpenNos.DAL.EF.Context;

namespace OpenNos.DAL.DAO
{
    public class MailDAO : IMailDAO
    {
        #region Methods

        public DeleteResult DeleteById(long mailId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    Mail mail = context.Mail.First(i => i.MailId.Equals(mailId));

                    if (mail != null)
                    {
                        context.Mail.Remove(mail);
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

        public SaveResult InsertOrUpdate(ref MailDTO mail)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    long mailId = mail.MailId;
                    Mail entity = context.Mail.FirstOrDefault(c => c.MailId.Equals(mailId));

                    if (entity == null)
                    {
                        mail = Insert(mail, context);
                        return SaveResult.Inserted;
                    }

                    mail.MailId = entity.MailId;
                    mail = Update(entity, mail, context);
                    return SaveResult.Updated;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<MailDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<MailDTO> result = new List<MailDTO>();
                foreach (Mail mail in context.Mail.AsNoTracking())
                {
                    MailDTO dto = new MailDTO();
                    Mapper.Mappers.MailMapper.ToMailDTO(mail, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public MailDTO LoadById(long mailId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    MailDTO dto = new MailDTO();
                    if (Mapper.Mappers.MailMapper.ToMailDTO(context.Mail.AsNoTracking().FirstOrDefault(i => i.MailId.Equals(mailId)), dto))
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

        public IEnumerable<MailDTO> LoadSentByCharacter(long characterId)
        {
            //Where(s => s.SenderId == CharacterId && s.IsSenderCopy && MailList.All(m => m.Value.MailId != s.MailId))
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<MailDTO> result = new List<MailDTO>();
                foreach (Mail mail in context.Mail.AsNoTracking().Where(s => s.SenderId == characterId && s.IsSenderCopy).Take(40))
                {
                    MailDTO dto = new MailDTO();
                    Mapper.Mappers.MailMapper.ToMailDTO(mail, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public IEnumerable<MailDTO> LoadSentToCharacter(long characterId)
        {
            //s => s.ReceiverId == CharacterId && !s.IsSenderCopy && MailList.All(m => m.Value.MailId != s.MailId)).Take(50)
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<MailDTO> result = new List<MailDTO>();
                foreach (Mail mail in context.Mail.AsNoTracking().Where(s => s.ReceiverId == characterId && !s.IsSenderCopy).Take(40))
                {
                    MailDTO dto = new MailDTO();
                    Mapper.Mappers.MailMapper.ToMailDTO(mail, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        private static MailDTO Insert(MailDTO mail, OpenNosContext context)
        {
            try
            {
                Mail entity = new Mail();
                Mapper.Mappers.MailMapper.ToMail(mail, entity);
                context.Mail.Add(entity);
                context.SaveChanges();
                if (Mapper.Mappers.MailMapper.ToMailDTO(entity, mail))
                {
                    return mail;
                }

                return null;
            }
            catch (DbEntityValidationException dbEx)
            {
                Exception raise = dbEx;
                foreach (DbEntityValidationResult validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (DbValidationError validationError in validationErrors.ValidationErrors)
                    {
                        // raise a new exception nesting the current instance as InnerException
                        Logger.Error(new InvalidOperationException($"{validationErrors.Entry.Entity}:{validationError.ErrorMessage}", raise));
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        private static MailDTO Update(Mail entity, MailDTO respawn, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.Mappers.MailMapper.ToMail(respawn, entity);
                context.SaveChanges();
            }
            if (Mapper.Mappers.MailMapper.ToMailDTO(entity, respawn))
            {
                return respawn;
            }

            return null;
        }

        #endregion
    }
}