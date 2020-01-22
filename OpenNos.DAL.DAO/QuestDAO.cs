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
    public class QuestDAO : IQuestDAO
    {
        #region Methods

        public DeleteResult DeleteById(long id)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    Quest deleteEntity = context.Quest.Find(id);
                    if (deleteEntity != null)
                    {
                        context.Quest.Remove(deleteEntity);
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

        public QuestDTO InsertOrUpdate(QuestDTO quest)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    Quest entity = context.Quest.Find(quest.QuestId);

                    if (entity == null)
                    {
                        return Insert(quest, context);
                    }
                    return Update(entity, quest, context);
                }
            }
            catch (Exception e)
            {
                Logger.Error(string.Format(Language.Instance.GetMessageFromKey("INSERT_ERROR"), quest, e.Message), e);
                return quest;
            }
        }

        public void InsertOrUpdateFromList(List<QuestDTO> questList)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    void Insert(QuestDTO quest)
                    {
                        Quest entity = new Quest();
                        Mapper.Mappers.QuestMapper.ToQuest(quest, entity);
                        context.Quest.Add(entity);
                    }

                    void Update(Quest entity, QuestDTO quest)
                    {
                        if (entity != null)
                        {
                            Mapper.Mappers.QuestMapper.ToQuest(quest, entity);
                            context.SaveChanges();
                        }
                    }

                    foreach (QuestDTO item in questList)
                    {
                        Quest entity = context.Quest.Find(item.QuestId);

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

        public IEnumerable<QuestDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                List<QuestDTO> result = new List<QuestDTO>();
                foreach (Quest entity in context.Quest.AsNoTracking())
                {
                    QuestDTO dto = new QuestDTO();
                    Mapper.Mappers.QuestMapper.ToQuestDTO(entity, dto);
                    result.Add(dto);
                }
                return result;
            }
        }

        public QuestDTO LoadById(long id)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                QuestDTO dto = new QuestDTO();
                if (Mapper.Mappers.QuestMapper.ToQuestDTO(context.Quest.AsNoTracking().FirstOrDefault(s=>s.QuestId == id), dto))
                {
                    return dto;
                }

                return null;
            }
        }

        private static QuestDTO Insert(QuestDTO quest, OpenNosContext context)
        {
            Quest entity = new Quest();
            Mapper.Mappers.QuestMapper.ToQuest(quest, entity);
            context.Quest.Add(entity);
            context.SaveChanges();
            if (Mapper.Mappers.QuestMapper.ToQuestDTO(entity, quest))
            {
                return quest;
            }

            return null;
        }

        private static QuestDTO Update(Quest entity, QuestDTO quest, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.Mappers.QuestMapper.ToQuest(quest, entity);
                context.SaveChanges();
            }

            if (Mapper.Mappers.QuestMapper.ToQuestDTO(entity, quest))
            {
                return quest;
            }

            return null;
        }

        #endregion
    }
}