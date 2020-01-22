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
    public class QuestProgressDAO : IQuestProgressDAO
    {
        #region Methods

        public DeleteResult DeleteById(Guid id)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    QuestProgress deleteEntity = context.QuestProgress.FirstOrDefault(s => s.QuestProgressId == id);
                    if (deleteEntity != null)
                    {
                        context.QuestProgress.Remove(deleteEntity);
                        context.SaveChanges();
                    }

                    return DeleteResult.Deleted;
                }
            }
            catch (Exception e)
            {
                Logger.Error(string.Format(Language.Instance.GetMessageFromKey("DELETE_ERROR"), id, e.Message), e);
                return DeleteResult.Error;
            }
        }

        public QuestProgressDTO InsertOrUpdate(QuestProgressDTO questProgress)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    QuestProgress entity = context.QuestProgress.FirstOrDefault(s => s.QuestProgressId == questProgress.QuestProgressId);

                    if (entity == null)
                    {
                        return Insert(questProgress, context);
                    }
                    return Update(entity, questProgress, context);
                }
            }
            catch (Exception e)
            {
                Logger.Error(string.Format(Language.Instance.GetMessageFromKey("INSERT_ERROR"), questProgress, e.Message), e);
                return questProgress;
            }
        }

        public void InsertOrUpdateFromList(List<QuestProgressDTO> questProgressList)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    void Insert(QuestProgressDTO quest)
                    {
                        QuestProgress entity = new QuestProgress();
                        Mapper.Mappers.QuestProgressMapper.ToQuestProgress(quest, entity);
                        context.QuestProgress.Add(entity);
                    }

                    void Update(QuestProgress entity, QuestProgressDTO quest)
                    {
                        if (entity != null)
                        {
                            Mapper.Mappers.QuestProgressMapper.ToQuestProgress(quest, entity);
                            context.SaveChanges();
                        }
                    }

                    foreach (QuestProgressDTO item in questProgressList)
                    {
                        QuestProgress entity = context.QuestProgress.FirstOrDefault(s => s.QuestProgressId == item.QuestProgressId);

                        if (entity == null)
                        {
                            Insert(item);
                        }
                        else
                        {
                            Update(entity, item);
                        }
                    }

                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public IEnumerable<QuestProgressDTO> LoadByCharacterId(long characterId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    List<QuestProgressDTO> result = new List<QuestProgressDTO>();
                    foreach (QuestProgress entity in context.QuestProgress.AsNoTracking().Where(s => s.CharacterId == characterId))
                    {
                        QuestProgressDTO dto = new QuestProgressDTO();
                        Mapper.Mappers.QuestProgressMapper.ToQuestProgressDTO(entity, dto);
                        result.Add(dto);
                    }
                    return result;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public QuestProgressDTO LoadById(Guid id)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    QuestProgressDTO dto = new QuestProgressDTO();
                    if (Mapper.Mappers.QuestProgressMapper.ToQuestProgressDTO(context.QuestProgress.AsNoTracking().FirstOrDefault(s => s.QuestProgressId == id), dto))
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

        private static QuestProgressDTO Insert(QuestProgressDTO quest, OpenNosContext context)
        {
            QuestProgress entity = new QuestProgress();
            Mapper.Mappers.QuestProgressMapper.ToQuestProgress(quest, entity);
            context.QuestProgress.Add(entity);
            context.SaveChanges();
            if (Mapper.Mappers.QuestProgressMapper.ToQuestProgressDTO(entity, quest))
            {
                return quest;
            }

            return null;
        }

        private static QuestProgressDTO Update(QuestProgress entity, QuestProgressDTO quest, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.Mappers.QuestProgressMapper.ToQuestProgress(quest, entity);
                context.SaveChanges();
            }

            if (Mapper.Mappers.QuestProgressMapper.ToQuestProgressDTO(entity, quest))
            {
                return quest;
            }

            return null;
        }

        #endregion
    }
}