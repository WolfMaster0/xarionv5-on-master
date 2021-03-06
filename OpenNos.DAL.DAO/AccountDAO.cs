﻿// This file is part of the OpenNos NosTale Emulator Project.
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
using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using OpenNos.DAL.EF.Context;

namespace OpenNos.DAL.DAO
{
    public class AccountDAO : IAccountDAO
    {
        #region Methods

        public DeleteResult Delete(long accountId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    Account account = context.Account.FirstOrDefault(c => c.AccountId.Equals(accountId));

                    if (account != null)
                    {
                        context.Account.Remove(account);
                        context.SaveChanges();
                    }

                    return DeleteResult.Deleted;
                }
            }
            catch (Exception e)
            {
                Logger.Error(string.Format(Language.Instance.GetMessageFromKey("DELETE_ACCOUNT_ERROR"), accountId, e.Message), e);
                return DeleteResult.Error;
            }
        }

        public SaveResult InsertOrUpdate(ref AccountDTO account)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    long accountId = account.AccountId;
                    Account entity = context.Account.FirstOrDefault(c => c.AccountId.Equals(accountId));

                    if (entity == null)
                    {
                        account = Insert(account, context);
                        return SaveResult.Inserted;
                    }
                    account = Update(entity, account, context);
                    return SaveResult.Updated;
                }
            }
            catch (Exception e)
            {
                Logger.Error(string.Format(Language.Instance.GetMessageFromKey("UPDATE_ACCOUNT_ERROR"), account.AccountId, e.Message), e);
                return SaveResult.Error;
            }
        }

        public AccountDTO LoadById(long accountId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    Account account = context.Account.AsNoTracking().FirstOrDefault(a => a.AccountId.Equals(accountId));
                    if (account != null)
                    {
                        AccountDTO accountDto = new AccountDTO();
                        if (Mapper.Mappers.AccountMapper.ToAccountDTO(account, accountDto))
                        {
                            return accountDto;
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

        public List<AccountDTO> LoadFamilyById(long accountId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    List<AccountDTO> result = new List<AccountDTO>();
                    Account account = context.Account.AsNoTracking().FirstOrDefault(a => a.AccountId.Equals(accountId));
                    if (account != null)
                    {
                        // TODO: Find by Last Login IPs (find a performant Cross-Platform way)
                        foreach (Account acc in context.Account.AsNoTracking().Where(s=>s.Email==account.Email || s.Password == account.Password))
                        {
                            AccountDTO accountDto = new AccountDTO();
                            if (Mapper.Mappers.AccountMapper.ToAccountDTO(acc, accountDto))
                            {
                                result.Add(accountDto);
                            }
                        }

                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            return null;
        }

        public AccountDTO LoadByName(string name)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    Account account = context.Account.AsNoTracking().FirstOrDefault(a => a.Name.Equals(name));
                    if (account != null)
                    {
                        AccountDTO accountDto = new AccountDTO();
                        if (Mapper.Mappers.AccountMapper.ToAccountDTO(account, accountDto))
                        {
                            return accountDto;
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

        public void WriteGeneralLog(long accountId, string ipAddress, long? characterId, GeneralLogType logType, string logData)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    GeneralLog log = new GeneralLog
                    {
                        AccountId = accountId,
                        IpAddress = ipAddress,
                        Timestamp = DateTime.UtcNow,
                        LogType = logType.ToString(),
                        LogData = logData,
                        CharacterId = characterId
                    };

                    context.GeneralLog.Add(log);
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        private static AccountDTO Insert(AccountDTO account, OpenNosContext context)
        {
            Account entity = new Account();
            Mapper.Mappers.AccountMapper.ToAccount(account, entity);
            context.Account.Add(entity);
            context.SaveChanges();
            Mapper.Mappers.AccountMapper.ToAccountDTO(entity, account);
            return account;
        }

        private static AccountDTO Update(Account entity, AccountDTO account, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.Mappers.AccountMapper.ToAccount(account, entity);
                context.Entry(entity).State = EntityState.Modified;
                context.SaveChanges();
            }
            if (Mapper.Mappers.AccountMapper.ToAccountDTO(entity, account))
            {
                return account;
            }

            return null;
        }

        #endregion
    }
}