using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.EventArguments;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;
using OpenNos.XMLModel.Quest.Model;
using OpenNos.XMLModel.Quest.Objects;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace OpenNos.GameObject
{
    public class QuestManager
    {
        #region Members

        private readonly XmlSerializer _serializer;
        private readonly ClientSession _session;

        #endregion

        #region Instantiation

        public QuestManager(ClientSession session)
        {
            _session = session;
            _serializer = new XmlSerializer(typeof(QuestModel));
            Quests = new ConcurrentDictionary<Guid, QuestModel>();
            InitializeEvents();
            LoadQuestProgress();
        }

        #endregion

        #region Properties

        public ConcurrentDictionary<Guid, QuestModel> Quests { get; }

        #endregion

        #region Methods

        public void AddQuest(long questId)
        {
            if (!Quests.Values.Any(s =>
                (s.QuestId == questId && s.QuestGoalType == QuestGoalType.MainQuest)
                || (s.QuestId == questId && !s.IsFinished)))
            {
                QuestDTO dto = ServerManager.Instance.QuestList[questId];
                if (dto == null)
                {
                    return;
                }
                QuestModel model;
                using (TextReader reader = new StringReader(dto.QuestData))
                {
                    model = (QuestModel)_serializer.Deserialize(reader);
                    model.QuestId = dto.QuestId;
                    model.Reward.QuestId = dto.NextQuestId;
                }

                model.QuestProgressId = Guid.NewGuid();

                Quests[model.QuestProgressId] = model;
                if (model.Script != null)
                {
                    _session.SendPacket($"script {model.Script.Type} {model.Script.Value}");
                }

                InitializeQuestUi();
            }
        }

        private void CharacterOnCapture(object sender, CaptureEventArgs eventArgs)
        {
            #region ObjectiveType.CaptureMonster

            if (Quests.Values.FirstOrDefault(s => !s.IsFinished && s.Objectives.Any(f =>
                                                      !s.IsFinished && f.Type == ObjectiveType.CaptureMonster
                                                      && eventArgs.Mate.NpcMonsterVNum == f.Param1)) is QuestModel model
                && Array.Find(model.Objectives,
                        f => f.Type == ObjectiveType.CaptureMonster && eventArgs.Mate.NpcMonsterVNum == f.Param1) is
                    Objective objective)
            {
                objective.CurrentAmount++;
                if (objective.CurrentAmount >= objective.GoalAmount)
                {
                    objective.IsFinished = true;
                }

                if (objective.Param2 == 1)
                {
                    _session.Character.Mates.Remove(eventArgs.Mate);
                    _session.SendPacket(UserInterfaceHelper.GeneratePClear());
                    _session.SendPackets(_session.Character.GenerateScP());
                    _session.SendPackets(_session.Character.GenerateScN());
                    _session.CurrentMapInstance?.Broadcast(eventArgs.Mate.GenerateOut());
                }

                UpdateQuest(model);
            }

            #endregion

            #region ObjectiveType.CaptureMonster2

            else if (Quests.Values.FirstOrDefault(s => !s.IsFinished && s.Objectives.Any(f =>
                                                           !s.IsFinished && f.Type == ObjectiveType.CaptureMonster2
                                                           && eventArgs.Mate.NpcMonsterVNum == f.Param1)) is QuestModel
                         captureModel
                     && Array.Find(captureModel.Objectives,
                             f => f.Type == ObjectiveType.CaptureMonster2
                                  && eventArgs.Mate.NpcMonsterVNum == f.Param1) is
                         Objective captureObjective)
            {
                captureObjective.CurrentAmount++;
                if (captureObjective.CurrentAmount >= captureObjective.GoalAmount)
                {
                    captureObjective.IsFinished = true;
                }

                if (captureObjective.Param2 == 1)
                {
                    _session.Character.Mates.Remove(eventArgs.Mate);
                    _session.SendPacket(UserInterfaceHelper.GeneratePClear());
                    _session.SendPackets(_session.Character.GenerateScP());
                    _session.SendPackets(_session.Character.GenerateScN());
                    _session.CurrentMapInstance?.Broadcast(eventArgs.Mate.GenerateOut());
                }

                UpdateQuest(captureModel);
            }

            #endregion
        }

        private void CharacterOnCraftRecipe(object sender, CraftRecipeEventArgs eventArgs)
        {
            #region ObjectiveType.Produce

            if (Quests.Values.FirstOrDefault(s => !s.IsFinished && s.Objectives.Any(f =>
                                                      !s.IsFinished && f.Type == ObjectiveType.Produce
                                                      && f.Param1 == eventArgs.Item.VNum)) is QuestModel model
                && Array.Find(model.Objectives,
                    f => f.Type == ObjectiveType.Produce && f.Param1 == eventArgs.Item.VNum) is Objective objective)
            {
                objective.CurrentAmount += eventArgs.Amount;
                if (objective.CurrentAmount >= objective.GoalAmount)
                {
                    objective.IsFinished = true;
                }

                if (objective.Param2 == 1)
                {
                    _session.Character.Inventory.RemoveItemAmount(eventArgs.Item.VNum, eventArgs.Amount);
                }

                UpdateQuest(model);
            }

            #endregion
        }

        private void CharacterOnDie(object sender, EventArgs eventArgs) => throw new NotImplementedException();

        private void CharacterOnFinishScriptedInstance(object sender, FinishScriptedInstanceEventArgs eventArgs)
        {
            #region ObjectiveType.CompleteRaid

            if (Quests.Values.FirstOrDefault(s => !s.IsFinished && s.Objectives.Any(f =>
                                                      !s.IsFinished && f.Type == ObjectiveType.CompleteRaid
                                                      && eventArgs.ScriptedInstanceType == ScriptedInstanceType.Raid
                                                      && eventArgs.Id == f.Param1)) is QuestModel model
                && Array.Find(model.Objectives,
                        f => f.Type == ObjectiveType.CompleteRaid
                             && eventArgs.ScriptedInstanceType == ScriptedInstanceType.Raid && eventArgs.Id == f.Param1) is
                    Objective objective)
            {
                objective.CurrentAmount++;
                if (objective.CurrentAmount >= objective.GoalAmount)
                {
                    objective.IsFinished = true;
                }

                UpdateQuest(model);
            }

            #endregion

            #region ObjectiveType.CompleteTimespace

            else if (Quests.Values.FirstOrDefault(s => !s.IsFinished && s.Objectives.Any(f =>
                                                           !s.IsFinished && f.Type == ObjectiveType.CompleteTimespace
                                                                         && eventArgs.ScriptedInstanceType
                                                                         == ScriptedInstanceType.TimeSpace && eventArgs.Id == f.Param1))
                         is QuestModel timespaceModel && Array.Find(timespaceModel.Objectives,
                         f => f.Type == ObjectiveType.CompleteTimespace
                              && eventArgs.ScriptedInstanceType == ScriptedInstanceType.TimeSpace
                              && eventArgs.Id == f.Param1) is Objective timespaceObjective)
            {
                timespaceObjective.CurrentAmount++;
                if (timespaceObjective.CurrentAmount >= timespaceObjective.GoalAmount)
                {
                    timespaceObjective.IsFinished = true;
                }

                UpdateQuest(timespaceModel);
            }

            #endregion

            #region ObjectiveType.EarnPointInTimeSpace

            else if (Quests.Values.FirstOrDefault(s => !s.IsFinished && s.Objectives.Any(f =>
                                                           !s.IsFinished && f.Type == ObjectiveType.EarnPointInTimeSpace
                                                           && eventArgs.ScriptedInstanceType
                                                           == ScriptedInstanceType.TimeSpace && eventArgs.Id == f.Param1))
                         is QuestModel timespacePointModel && Array.Find(timespacePointModel.Objectives,
                             f => f.Type == ObjectiveType.EarnPointInTimeSpace
                                  && eventArgs.ScriptedInstanceType == ScriptedInstanceType.TimeSpace
                                  && eventArgs.Id == f.Param1 && eventArgs.Score >= f.Param2) is Objective
                         timespacePointObjective)
            {
                timespacePointObjective.IsFinished = true;

                UpdateQuest(timespacePointModel);
            }

            #endregion
        }

        private void CharacterOnKill(object sender, KillEventArgs eventArgs)
        {
            #region ObjectiveType.KillMonster

            if (Quests.Values.FirstOrDefault(s => !s.IsFinished && s.Objectives.Any(f =>
                                                      !s.IsFinished && f.Type == ObjectiveType.KillMonster
                                                      && eventArgs.UserType == UserType.Monster
                                                      && eventArgs.KilledEntity is MapMonster monster
                                                      && f.Param1 == monster.MonsterVNum)) is QuestModel model
                && Array.Find(model.Objectives,
                        f => f.Type == ObjectiveType.KillMonster && eventArgs.UserType == UserType.Monster
                             && eventArgs.KilledEntity is MapMonster monster
                             && f.Param1 == monster.MonsterVNum) is Objective
                    objective)
            {
                objective.CurrentAmount++;
                if (objective.CurrentAmount >= objective.GoalAmount)
                {
                    objective.IsFinished = true;
                }

                UpdateQuest(model);
            }

            #endregion

            #region ObjectiveType.KillMonsterLevelLimit

            else if (Quests.Values.FirstOrDefault(s => !s.IsFinished && s.Objectives.Any(f =>
                                                           !s.IsFinished
                                                           && f.Type == ObjectiveType.KillMonsterLevelLimit
                                                           && eventArgs.UserType == UserType.Monster
                                                           && eventArgs.KilledEntity is MapMonster monster
                                                           && _session.Character.Level - f.Param1
                                                           <= monster.Monster.Level)) is QuestModel kModel
                     && Array.Find(kModel.Objectives,
                         f => f.Type == ObjectiveType.KillMonsterLevelLimit && eventArgs.UserType == UserType.Monster
                              && eventArgs.KilledEntity is MapMonster monster
                              && _session.Character.Level - f.Param1 <= monster.Monster.Level) is Objective kObjective)
            {
                kObjective.CurrentAmount++;
                if (kObjective.CurrentAmount >= kObjective.GoalAmount)
                {
                    kObjective.IsFinished = true;
                }

                UpdateQuest(kModel);
            }

            #endregion

            #region ObjectiveType.KillMonsterAndCollectItem

            else if (eventArgs.KilledEntity is MapMonster collectMonster
                     && Quests.Values.FirstOrDefault(s => !s.IsFinished && s.Objectives.Any(f =>
                                                           !s.IsFinished && f.Type == ObjectiveType.KillMonster
                                                           && eventArgs.UserType == UserType.Monster
                                                           && f.Param1 == collectMonster.MonsterVNum)) is QuestModel
                         collectModel
                     && Array.Find(collectModel.Objectives,
                         f => f.Type == ObjectiveType.KillMonster && eventArgs.UserType == UserType.Monster
                              && f.Param1 == collectMonster.MonsterVNum) is Objective collectObjective)
            {
                if (ServerManager.RandomNumber(0, 10000) < collectObjective.Param3)
                {
                    HandleItemDrop(new DropDTO() { ItemVNum = (short)collectObjective.Param2, Amount = 1 },
                        _session.Character.CharacterId, collectMonster.MapX, collectMonster.MapY);
                }

                UpdateQuest(collectModel);
            }

            #endregion

            #region ObjectiveType.AimedKill

            if (Quests.Values.FirstOrDefault(s =>
                        !s.IsFinished && ServerManager.Instance.ChannelId == 51 && s.Objectives.Any(f =>
                            !s.IsFinished && f.Type == ObjectiveType.AimedKill
                            && eventArgs.UserType == UserType.Player)) is
                    QuestModel aimKillModel
                && Array.Find(aimKillModel.Objectives,
                        f => f.Type == ObjectiveType.AimedKill && eventArgs.UserType == UserType.Player) is Objective
                    aimKillObjective)
            {
                aimKillObjective.CurrentAmount++;
                if (aimKillObjective.CurrentAmount >= aimKillObjective.GoalAmount)
                {
                    aimKillObjective.IsFinished = true;
                }

                UpdateQuest(aimKillModel);
            }

            #endregion
        }

        private void CharacterOnMineItem(object sender, MineItemEventArgs eventArgs)
        {
            #region ObjectiveType.CollectItemFromNpc

            if (Quests.Values.FirstOrDefault(s => !s.IsFinished && s.Objectives.Any(f =>
                                                      !s.IsFinished && f.Type == ObjectiveType.CollectItemFromNpc
                                                      && f.Param1 == eventArgs.Item.VNum)) is QuestModel model
                                                      && Array.Find(model.Objectives,
                        f => f.Type == ObjectiveType.CollectItemFromNpc && f.Param1 == eventArgs.Item.VNum) is Objective objective)
            {
                objective.CurrentAmount++;
                if (objective.CurrentAmount >= objective.GoalAmount)
                {
                    objective.IsFinished = true;
                }

                if (objective.Param2 == 1)
                {
                    _session.Character.Inventory.RemoveItemAmount(eventArgs.Item.VNum);
                }

                UpdateQuest(model);
            }

            #endregion
        }

        private void CharacterOnMove(object sender, MoveEventArgs eventArgs)
        {
            #region ObjectiveType.WalkPosition

            if (Quests.Values.FirstOrDefault(s => !s.IsFinished && s.Objectives.Any(f =>
                                                      !s.IsFinished && f.Type == ObjectiveType.WalkPosition
                                                      && f.Param1 == eventArgs.MapId
                                                      && f.Param2 > eventArgs.PositionX - 2
                                                      && f.Param2 < eventArgs.PositionX + 2
                                                      && f.Param3 > eventArgs.PositionY - 2
                                                      && f.Param3 < eventArgs.PositionY + 2)) is QuestModel model
                && Array.Find(model.Objectives,
                        f => f.Type == ObjectiveType.WalkPosition && f.Param1 == eventArgs.MapId
                             && f.Param2 > eventArgs.PositionX - 2 && f.Param2 < eventArgs.PositionX + 2
                             && f.Param3 > eventArgs.PositionY - 2 && f.Param3 < eventArgs.PositionY + 2) is Objective
                    objective)
            {
                _session.SendPacket(
                    $"targetoff {objective.Param2} {objective.Param3} {objective.Param1} {model.QuestDataVNum}");

                objective.IsFinished = true;
                UpdateQuest(model);
            }

            #endregion

            #region ObjectiveType.Kill

            if (_session.CurrentMapInstance.LastQuestSpawn.AddSeconds(30) < DateTime.UtcNow
                && Quests.Values.FirstOrDefault(s => !s.IsFinished && s.Objectives.Any(f =>
                                                      !s.IsFinished && f.Type == ObjectiveType.WalkPosition
                                                      && f.Param2 == eventArgs.MapId
                                                      && f.Param3 > eventArgs.PositionX - 2
                                                      && f.Param3 < eventArgs.PositionX + 2
                                                      && f.Param4 > eventArgs.PositionY - 2
                                                      && f.Param4 < eventArgs.PositionY + 2)) is QuestModel killModel
                && Array.Find(killModel.Objectives,
                        f => f.Type == ObjectiveType.WalkPosition && f.Param2 == eventArgs.MapId
                             && f.Param3 > eventArgs.PositionX - 2 && f.Param3 < eventArgs.PositionX + 2
                             && f.Param4 > eventArgs.PositionY - 2 && f.Param4 < eventArgs.PositionY + 2) is Objective
                    killObjective)
            {
                int i = 0;
                while (i < killObjective.Param5)
                {
                    short x = (short) ServerManager.RandomNumber(eventArgs.PositionX - 3, eventArgs.PositionX + 3);
                    short y = (short) ServerManager.RandomNumber(eventArgs.PositionY - 3, eventArgs.PositionY + 3);

                    if (!_session.CurrentMapInstance.Map.IsBlockedZone(x, y))
                    {
                        _session.CurrentMapInstance.SummonMonster(new MonsterToSummon((short) killObjective.Param1,
                            new MapCell {X = x, Y = y}, -1, true));
                        i++;
                    }
                }

                _session.CurrentMapInstance.LastQuestSpawn = DateTime.UtcNow;
            }

            #endregion
        }

        private void CharacterOnPickupItem(object sender, PickupItemEventArgs eventArgs)
        {
            #region ObjectiveType.CollectItem

            if (Quests.Values.FirstOrDefault(s => !s.IsFinished && s.Objectives.Any(f =>
                                                      !s.IsFinished && f.Type == ObjectiveType.CollectItem
                                                      && f.Param1 == eventArgs.Item.VNum)) is QuestModel model
                && Array.Find(model.Objectives,
                    f => f.Type == ObjectiveType.CollectItem && f.Param1 == eventArgs.Item.VNum) is Objective objective)
            {
                objective.CurrentAmount++;
                if (objective.CurrentAmount >= objective.GoalAmount)
                {
                    objective.IsFinished = true;
                }

                if (objective.Param2 == 1)
                {
                    _session.Character.Inventory.RemoveItemAmount(eventArgs.Item.VNum);
                }

                UpdateQuest(model);
            }

            #endregion

            #region ObjectiveType.KillMonsterAndCollectItem

            else if (Quests.Values.FirstOrDefault(s => !s.IsFinished && s.Objectives.Any(f =>
                                                           !s.IsFinished
                                                           && f.Type == ObjectiveType.KillMonsterAndCollectItem
                                                           && f.Param2 == eventArgs.Item.VNum)) is QuestModel
                         collectModel
                     && Array.Find(collectModel.Objectives,
                             f => f.Type == ObjectiveType.KillMonsterAndCollectItem
                                  && f.Param2 == eventArgs.Item.VNum) is
                         Objective collectObjective)
            {
                collectObjective.CurrentAmount++;
                if (collectObjective.CurrentAmount >= collectObjective.GoalAmount)
                {
                    collectObjective.IsFinished = true;
                }

                if (collectObjective.Param4 == 1)
                {
                    _session.Character.Inventory.RemoveItemAmount(eventArgs.Item.VNum);
                }

                UpdateQuest(collectModel);
            }

            #endregion

            #region ObjectiveType.KillMonsterAndCollectItem2

            else if (Quests.Values.FirstOrDefault(s => !s.IsFinished && s.Objectives.Any(f =>
                                                           !s.IsFinished
                                                           && f.Type == ObjectiveType.KillMonsterAndCollectItem2
                                                           && f.Param2 == eventArgs.Item.VNum)) is QuestModel
                         collect2Model
                     && Array.Find(collect2Model.Objectives,
                             f => f.Type == ObjectiveType.KillMonsterAndCollectItem
                                  && f.Param2 == eventArgs.Item.VNum) is
                         Objective collect2Objective)
            {
                collect2Objective.CurrentAmount++;
                if (collect2Objective.CurrentAmount >= collect2Objective.GoalAmount)
                {
                    collect2Objective.IsFinished = true;
                }

                if (collect2Objective.Param4 == 1)
                {
                    _session.Character.Inventory.RemoveItemAmount(eventArgs.Item.VNum);
                }

                UpdateQuest(collect2Model);
            }

            #endregion
        }

        private void CharacterOnTalk(object sender, TalkEventArgs eventArgs)
        {
            #region ObjectiveType.Talk

            if (Quests.Values.FirstOrDefault(s => !s.IsFinished && s.Objectives.Any(f =>
                                                      !s.IsFinished && f.Type == ObjectiveType.Talk
                                                      && f.Param1 == eventArgs.MapNpc.MapId
                                                      && f.Param2 == eventArgs.MapNpc.NpcVNum
                                                      && (f.Param3 == 0 || f.Param3 == eventArgs.MapNpc.MapNpcId))) is
                    QuestModel model && Array.Find(model.Objectives,
                    f => f.Type == ObjectiveType.Talk && f.Param1 == eventArgs.MapNpc.MapId
                         && f.Param2 == eventArgs.MapNpc.NpcVNum
                         && (f.Param3 == 0 || f.Param3 == eventArgs.MapNpc.MapNpcId)) is Objective objective)
            {
                objective.IsFinished = true;
                UpdateQuest(model);
            }

            #endregion

            #region ObjectiveType.TalkGetInformation

            else if (Quests.Values.FirstOrDefault(s => !s.IsFinished && s.Objectives.Any(f =>
                                                           !s.IsFinished
                                                           && f.Type == ObjectiveType.TalkGetInformation
                                                           && f.Param1 == eventArgs.MapNpc.MapId
                                                           && f.Param2 == eventArgs.MapNpc.NpcVNum
                                                           && (f.Param3 == 0 || f.Param3 == eventArgs.MapNpc.MapNpcId))) is
                         QuestModel infoModel && Array.Find(infoModel.Objectives,
                         f => f.Type == ObjectiveType.TalkGetInformation && f.Param1 == eventArgs.MapNpc.MapId
                              && f.Param2 == eventArgs.MapNpc.NpcVNum
                              && (f.Param3 == 0 || f.Param3 == eventArgs.MapNpc.MapNpcId)) is Objective infoObjective)
            {
                infoObjective.IsFinished = true;
                UpdateQuest(infoModel);
            }

            #endregion

            #region ObjectiveType.InspectNpcMonster

            else if (Quests.Values.FirstOrDefault(s => !s.IsFinished && s.Objectives.Any(f =>
                                                           !s.IsFinished && f.Type == ObjectiveType.InspectNpcMonster
                                                           && f.Param1 == eventArgs.MapNpc.MapId
                                                           && f.Param2 == eventArgs.MapNpc.NpcVNum
                                                           && (f.Param3 == 0 || f.Param3 == eventArgs.MapNpc.MapNpcId))) is
                         QuestModel inspectModel && Array.Find(inspectModel.Objectives,
                         f => f.Type == ObjectiveType.InspectNpcMonster && f.Param1 == eventArgs.MapNpc.MapId
                              && f.Param2 == eventArgs.MapNpc.NpcVNum
                              && (f.Param3 == 0 || f.Param3 == eventArgs.MapNpc.MapNpcId)) is Objective inspectObjective)
            {
                inspectObjective.IsFinished = true;
                UpdateQuest(inspectModel);
            }

            #endregion

            #region ObjectiveType.TalkWhenItemInInventory

            else if (Quests.Values.FirstOrDefault(s => !s.IsFinished && s.Objectives.Any(f =>
                                                           !s.IsFinished
                                                           && f.Type == ObjectiveType.TalkWhenItemInInventory
                                                           && f.Param1 == eventArgs.MapNpc.MapId
                                                           && f.Param2 == eventArgs.MapNpc.NpcVNum
                                                           && (f.Param3 == 0 || f.Param3 == eventArgs.MapNpc.MapNpcId))) is
                         QuestModel inventoryModel && Array.Find(inventoryModel.Objectives,
                             f => f.Type == ObjectiveType.TalkWhenItemInInventory
                                  && f.Param1 == eventArgs.MapNpc.MapId
                                  && f.Param2 == eventArgs.MapNpc.NpcVNum
                                  && (f.Param3 == 0 || f.Param3 == eventArgs.MapNpc.MapNpcId)
                                  && _session.Character.Inventory.CountItem(f.Param4) >= f.GoalAmount) is Objective
                         inventoryObjective)
            {
                _session.Character.Inventory.RemoveItemAmount(inventoryObjective.Param4, inventoryObjective.GoalAmount);
                inventoryObjective.IsFinished = true;
                UpdateQuest(inventoryModel);
            }

            #endregion

            #region ObjectiveType.DeliverItem

            else if (Quests.Values.FirstOrDefault(s => !s.IsFinished && s.Objectives.Any(f =>
                                                           !s.IsFinished && f.Type == ObjectiveType.DeliverItem
                                                           && f.Param1 == eventArgs.MapNpc.MapId
                                                           && f.Param2 == eventArgs.MapNpc.NpcVNum
                                                           && (f.Param3 == 0 || f.Param3 == eventArgs.MapNpc.MapNpcId))) is
                         QuestModel deliverItemModel && Array.Find(deliverItemModel.Objectives,
                             f => f.Type == ObjectiveType.DeliverItem && f.Param1 == eventArgs.MapNpc.MapId
                                  && f.Param2 == eventArgs.MapNpc.NpcVNum
                                  && (f.Param3 == 0 || f.Param3 == eventArgs.MapNpc.MapNpcId)
                                  && _session.Character.Inventory.CountItem(f.Param4) >= f.GoalAmount) is Objective
                         deliverItemObjective)
            {
                _session.Character.Inventory.RemoveItemAmount(deliverItemObjective.Param4,
                    deliverItemObjective.GoalAmount);
                deliverItemObjective.IsFinished = true;
                UpdateQuest(deliverItemModel);
            }

            #endregion

            #region ObjectiveType.DeliverGold

            else if (Quests.Values.FirstOrDefault(s => !s.IsFinished && s.Objectives.Any(f =>
                                                           !s.IsFinished && f.Type == ObjectiveType.DeliverGold
                                                           && f.Param1 == eventArgs.MapNpc.MapId
                                                           && f.Param2 == eventArgs.MapNpc.NpcVNum
                                                           && (f.Param3 == 0 || f.Param3 == eventArgs.MapNpc.MapNpcId))) is
                         QuestModel deliverGoldModel && Array.Find(deliverGoldModel.Objectives,
                         f => f.Type == ObjectiveType.DeliverGold && f.Param1 == eventArgs.MapNpc.MapId
                              && f.Param2 == eventArgs.MapNpc.NpcVNum
                              && (f.Param3 == 0 || f.Param3 == eventArgs.MapNpc.MapNpcId)
                              && _session.Character.Gold >= f.GoalAmount) is Objective deliverGoldObjective)
            {
                _session.Character.Gold -= deliverGoldObjective.GoalAmount;
                _session.SendPacket(_session.Character.GenerateGold());
                deliverGoldObjective.IsFinished = true;
                UpdateQuest(deliverGoldModel);
            }

            #endregion

            #region ObjectiveType.EquipAndTalk

            else if (Quests.Values.FirstOrDefault(s => !s.IsFinished && s.Objectives.Any(f =>
                                                           !s.IsFinished && f.Type == ObjectiveType.EquipAndTalk
                                                           && f.Param1 == eventArgs.MapNpc.MapId
                                                           && f.Param2 == eventArgs.MapNpc.NpcVNum
                                                           && (f.Param3 == 0 || f.Param3 == eventArgs.MapNpc.MapNpcId))) is
                         QuestModel equipModel && Array.Find(equipModel.Objectives,
                         f => f.Type == ObjectiveType.EquipAndTalk && f.Param1 == eventArgs.MapNpc.MapId
                              && f.Param2 == eventArgs.MapNpc.NpcVNum
                              && (f.Param3 == 0 || f.Param3 == eventArgs.MapNpc.MapNpcId)
                              && _session.Character.Inventory.Any(i =>
                                  i.Type == InventoryType.Wear && i.ItemVNum == f.Param4)) is Objective equipObjective)
            {
                equipObjective.IsFinished = true;
                UpdateQuest(equipModel);
            }

            #endregion
        }

        private void GetExp(byte type, long xp)
        {
            switch (type)
            {
                case 0:
                    {
                        _session.Character.LevelXp += xp;
                        double experience = _session.Character.XpLoad();
                        while (_session.Character.LevelXp >= experience)
                        {
                            _session.Character.LevelXp -= (long)experience;
                            _session.Character.Level++;
                            experience = _session.Character.XpLoad();
                            if (_session.Character.Level >= ServerManager.Instance.Configuration.MaxLevel)
                            {
                                _session.Character.Level = ServerManager.Instance.Configuration.MaxLevel;
                                _session.Character.LevelXp = 0;
                            }
                            else if (_session.Character.Level == ServerManager.Instance.Configuration.HeroicStartLevel)
                            {
                                _session.Character.HeroLevel = 1;
                                _session.Character.HeroXp = 0;
                            }

                            _session.Character.Hp = (int)_session.Character.HPLoad();
                            _session.Character.Mp = (int)_session.Character.MPLoad();
                            _session.SendPacket(_session.Character.GenerateStat());
                            if (_session.Character.Family != null)
                            {
                                if (_session.Character.Level > 20 && _session.Character.Level % 10 == 0)
                                {
                                    _session.Character.Family.InsertFamilyLog(FamilyLogType.LevelUp,
                                        _session.Character.Name, level: _session.Character.Level);
                                    _session.Character.Family.InsertFamilyLog(FamilyLogType.FamilyXP,
                                        _session.Character.Name, experience: 20 * _session.Character.Level);
                                    _session.Character.GenerateFamilyXp(20 * _session.Character.Level);
                                }
                                else if (_session.Character.Level > 80)
                                {
                                    _session.Character.Family.InsertFamilyLog(FamilyLogType.LevelUp,
                                        _session.Character.Name, level: _session.Character.Level);
                                }
                                else
                                {
                                    ServerManager.Instance.FamilyRefresh(_session.Character.Family.FamilyId);
                                    CommunicationServiceClient.Instance.SendMessageToCharacter(new ScsCharacterMessage
                                    {
                                        DestinationCharacterId = _session.Character.Family.FamilyId,
                                        SourceCharacterId = _session.Character.CharacterId,
                                        SourceWorldId = ServerManager.Instance.WorldId,
                                        Message = "fhis_stc",
                                        Type = MessageType.Family
                                    });
                                }
                            }

                            _session.SendPacket(_session.Character.GenerateLevelUp());
                            _session.Character.GetReferrerReward();
                            _session.SendPacket(
                                UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("LEVELUP"), 0));
                            _session.CurrentMapInstance?.Broadcast(
                                StaticPacketHelper.GenerateEff(UserType.Player, _session.Character.CharacterId, 6),
                                _session.Character.PositionX, _session.Character.PositionY);
                            _session.CurrentMapInstance?.Broadcast(
                                StaticPacketHelper.GenerateEff(UserType.Player, _session.Character.CharacterId, 198),
                                _session.Character.PositionX, _session.Character.PositionY);
                            ServerManager.Instance.UpdateGroup(_session.Character.CharacterId);
                        }

                        break;
                    }
                case 1:
                    {
                        double experience = _session.Character.JobXPLoad();
                        _session.Character.JobLevelXp += xp;
                        while (_session.Character.JobLevelXp >= experience)
                        {
                            _session.Character.JobLevelXp -= (long)experience;
                            _session.Character.JobLevel++;
                            experience = _session.Character.JobXPLoad();
                            if (_session.Character.JobLevel >= 20 && _session.Character.Class == 0)
                            {
                                _session.Character.JobLevel = 20;
                                _session.Character.JobLevelXp = 0;
                            }
                            else if (_session.Character.JobLevel >= ServerManager.Instance.Configuration.MaxJobLevel)
                            {
                                _session.Character.JobLevel = ServerManager.Instance.Configuration.MaxJobLevel;
                                _session.Character.JobLevelXp = 0;
                            }

                            _session.Character.Hp = (int)_session.Character.HPLoad();
                            _session.Character.Mp = (int)_session.Character.MPLoad();
                            _session.SendPacket(_session.Character.GenerateStat());
                            _session.SendPacket(_session.Character.GenerateLevelUp());
                            _session.SendPacket(
                                UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("JOB_LEVELUP"), 0));
                            _session.Character.LearnAdventurerSkill();
                            _session.CurrentMapInstance?.Broadcast(
                                StaticPacketHelper.GenerateEff(UserType.Player, _session.Character.CharacterId, 8),
                                _session.Character.PositionX, _session.Character.PositionY);
                            _session.CurrentMapInstance?.Broadcast(
                                StaticPacketHelper.GenerateEff(UserType.Player, _session.Character.CharacterId, 198),
                                _session.Character.PositionX, _session.Character.PositionY);
                        }

                        if (_session.Character.Inventory?.LoadBySlotAndType((byte)EquipmentType.Sp, InventoryType.Wear) is
                            ItemInstance specialist)
                        {
                            experience = _session.Character.SpXpLoad();

                            while (_session.Character.UseSp && specialist.XP >= experience)
                            {
                                specialist.XP -= (long)experience;
                                specialist.SpLevel++;
                                experience = _session.Character.SpXpLoad();
                                _session.SendPacket(_session.Character.GenerateStat());
                                _session.SendPacket(_session.Character.GenerateLevelUp());
                                if (specialist.SpLevel >= ServerManager.Instance.Configuration.MaxSpLevel)
                                {
                                    specialist.SpLevel = ServerManager.Instance.Configuration.MaxSpLevel;
                                    specialist.XP = 0;
                                }

                                _session.Character.LearnSpSkill();
                                _session.Character.Skills.ForEach(s => s.LastUse = DateTime.UtcNow.AddDays(-1));
                                _session.SendPacket(_session.Character.GenerateSki());
                                _session.SendPackets(_session.Character.GenerateQuicklist());

                                _session.SendPacket(
                                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("SP_LEVELUP"), 0));
                                _session.CurrentMapInstance?.Broadcast(
                                    StaticPacketHelper.GenerateEff(UserType.Player, _session.Character.CharacterId, 8),
                                    _session.Character.PositionX, _session.Character.PositionY);
                                _session.CurrentMapInstance?.Broadcast(
                                    StaticPacketHelper.GenerateEff(UserType.Player, _session.Character.CharacterId, 198),
                                    _session.Character.PositionX, _session.Character.PositionY);
                            }
                        }

                        break;
                    }
                case 2:
                    {
                        double experience = _session.Character.HeroXPLoad();
                        _session.Character.HeroXp += xp;
                        while (_session.Character.HeroXp >= experience)
                        {
                            _session.Character.HeroXp -= (long)experience;
                            _session.Character.HeroLevel++;
                            experience = _session.Character.HeroXPLoad();
                            if (_session.Character.HeroLevel >= ServerManager.Instance.Configuration.MaxHeroLevel)
                            {
                                _session.Character.HeroLevel = ServerManager.Instance.Configuration.MaxHeroLevel;
                                _session.Character.HeroXp = 0;
                            }

                            _session.Character.Hp = (int)_session.Character.HPLoad();
                            _session.Character.Mp = (int)_session.Character.MPLoad();
                            _session.SendPacket(_session.Character.GenerateStat());
                            _session.SendPacket(_session.Character.GenerateLevelUp());
                            _session.SendPacket(
                                UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("HERO_LEVELUP"), 0));
                            _session.CurrentMapInstance?.Broadcast(
                                StaticPacketHelper.GenerateEff(UserType.Player, _session.Character.CharacterId, 8),
                                _session.Character.PositionX, _session.Character.PositionY);
                            _session.CurrentMapInstance?.Broadcast(
                                StaticPacketHelper.GenerateEff(UserType.Player, _session.Character.CharacterId, 198),
                                _session.Character.PositionX, _session.Character.PositionY);
                        }

                        break;
                    }
            }
        }

        private void HandleItemDrop(DropDTO drop, long? owner, short posX, short posY)
        {
            Observable.Timer(TimeSpan.FromMilliseconds(500)).Subscribe(o =>
            {
                if (_session.HasCurrentMapInstance)
                {
                    if (_session.Character.StaticBonusList.Any(s => s.StaticBonusType == StaticBonusType.AutoLoot))
                    {
                        Item item = ServerManager.GetItem(drop.ItemVNum);
                        _session.Character.OnPickupItem(new PickupItemEventArgs(item));
                        if (item.ItemType == ItemType.Map)
                        {
                            if (item.Effect == 71)
                            {
                                _session.Character.SpPoint += item.EffectValue;
                                if (_session.Character.SpPoint > 10000)
                                {
                                    _session.Character.SpPoint = 10000;
                                }

                                _session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                    string.Format(Language.Instance.GetMessageFromKey("SP_POINTSADDED"),
                                        item.EffectValue), 0));
                                _session.SendPacket(_session.Character.GenerateSpPoint());
                            }

                            if (ServerManager.Instance.QuestModelList.FirstOrDefault(s =>
                                s.QuestGiver.QuestGiverId == item.VNum
                                && s.QuestGiver.Type == QuestGiverType.ItemLoot) is QuestModel model)
                            {
                                AddQuest(model.QuestId);
                            }
                        }
                        else
                        {
                            _session.Character.GiftAdd(drop.ItemVNum, (byte)drop.Amount);
                        }
                    }
                    else
                    {
                        _session.CurrentMapInstance.DropItemByMonster(owner, drop, posX, posY);
                    }
                }
            });
        }

        private void InitializeEvents()
        {
            _session.Character.CraftRecipe += CharacterOnCraftRecipe;
            _session.Character.Die += CharacterOnDie;
            _session.Character.FinishScriptedInstance += CharacterOnFinishScriptedInstance;
            _session.Character.Kill += CharacterOnKill;
            _session.Character.Move += CharacterOnMove;
            _session.Character.PickupItem += CharacterOnPickupItem;
            _session.Character.Talk += CharacterOnTalk;
            _session.Character.Capture += CharacterOnCapture;
            _session.Character.MineItem += CharacterOnMineItem;
        }

        private void InitializeQuestUi()
        {
            string packet = "qstlist";
            foreach (QuestModel model in Quests.Values.Where(s => !s.IsFinished))
            {
                packet += $" {(byte)model.QuestGoalType}.{model.QuestDataVNum}.{model.QuestDataVNum}.";
                byte i = 3;
                foreach (Objective objective in model.Objectives)
                {
                    packet +=
                        $"{(byte)objective.Type}.{objective.CurrentAmount}.{objective.GoalAmount}.{Convert.ToInt32(objective.IsFinished)}.";
                    i--;
                }

                while (i > 0)
                {
                    packet += "0.0.0.0.";
                    i--;
                }

                packet += $"{Convert.ToInt32(model.IsFinished)}";
            }

            _session.SendPacket(packet);

            if (Quests.Values.FirstOrDefault(s =>
                        !s.IsFinished && s.Objectives.Any(f => f.Type == ObjectiveType.WalkPosition)) is QuestModel walkModel
                && Array.Find(walkModel.Objectives, s => !s.IsFinished && s.Type == ObjectiveType.WalkPosition) is Objective walkObjective)
            {
                _session.SendPacket(
                    $"targetoff {walkObjective.Param2} {walkObjective.Param3} {walkObjective.Param1} {walkModel.QuestDataVNum}");
                _session.SendPacket(
                    $"target {walkObjective.Param2} {walkObjective.Param3} {walkObjective.Param1} {walkModel.QuestDataVNum}");
            }
            else if (Quests.Values.FirstOrDefault(s => !s.IsFinished && s.Objectives.Any(f =>
                                                           f.Type == ObjectiveType.KillMonster && f.Param2 != 0
                                                           && f.Param3 != 0 && f.Param4 != 0 && f.Param5 != 0)) is
                         QuestModel killWalkModel
                     && Array.Find(killWalkModel.Objectives,
                         s => !s.IsFinished && s.Type == ObjectiveType.KillMonster && s.Param2 != 0 && s.Param3 != 0
                              && s.Param4 != 0 && s.Param5 != 0) is Objective killWalkObjective)
            {
                _session.SendPacket(
                    $"targetoff {killWalkObjective.Param3} {killWalkObjective.Param4} {killWalkObjective.Param2} {killWalkModel.QuestDataVNum}");
                _session.SendPacket(
                    $"target {killWalkObjective.Param3} {killWalkObjective.Param4} {killWalkObjective.Param2} {killWalkModel.QuestDataVNum}");
            }
        }

        private void LoadQuestProgress()
        {
            foreach (QuestProgressDTO dto in DAOFactory.QuestProgressDAO.LoadByCharacterId(_session.Character.CharacterId))
            {
                if (dto.QuestData != null)
                {
                    using (TextReader reader = new StringReader(dto.QuestData))
                    {
                        QuestModel model = (QuestModel)_serializer.Deserialize(reader);
                        Quests[model.QuestProgressId] = model;
                    }
                }
            }
            InitializeQuestUi();
        }

        private void RefreshQuestUi()
        {
            string packet = "qsti";
            foreach (QuestModel model in Quests.Values.Where(s => !s.IsFinished))
            {
                packet += $" {(byte)model.QuestGoalType}.{model.QuestDataVNum}.{model.QuestDataVNum}.";
                byte i = 3;
                foreach (Objective objective in model.Objectives)
                {
                    packet +=
                        $"{(byte)objective.Type}.{objective.CurrentAmount}.{objective.GoalAmount}.{Convert.ToInt32(objective.IsFinished)}.";
                    i--;
                }

                while (i > 0)
                {
                    packet += "0.0.0.0.";
                    i--;
                }

                packet += $"{Convert.ToInt32(model.IsFinished)}";
            }

            _session.SendPacket(packet);

            if (Quests.Values.FirstOrDefault(s =>
                    !s.IsFinished && s.Objectives.Any(f => f.Type == ObjectiveType.WalkPosition))
                is QuestModel
                walkModel)
            {
                if (Array.Find(walkModel.Objectives,
                        s => !s.IsFinished && s.Type == ObjectiveType.WalkPosition) is Objective
                    walkObjective)
                {
                    _session.SendPacket(
                        $"target {walkObjective.Param2} {walkObjective.Param3} {walkObjective.Param1} {walkModel.QuestDataVNum}");
                }
            }
        }

        private void UpdateQuest(QuestModel model)
        {
            if (model.Objectives.All(s => s.IsFinished))
            {
                bool showLevelUp = false;
                if (model.Reward.ForceLevelUp > _session.Character.Level)
                {
                    _session.Character.Level = model.Reward.ForceLevelUp;
                    _session.Character.LevelXp = 0;
                    showLevelUp = true;
                }

                if (model.Reward.ForceJobUp > _session.Character.JobLevel)
                {
                    _session.Character.JobLevel = model.Reward.ForceJobUp;
                    _session.Character.JobLevelXp = 0;
                    showLevelUp = true;
                }

                if (model.Reward.ForceHeroUp > _session.Character.HeroLevel)
                {
                    _session.Character.HeroLevel = model.Reward.ForceHeroUp;
                    _session.Character.HeroXp = 0;
                    showLevelUp = true;
                }

                GetExp(0, model.Reward.LevelExp);
                GetExp(1, model.Reward.JobExp);
                GetExp(2, model.Reward.HeroExp);

                if (model.Reward.Gold > 0)
                {
                    _session.Character.Gold += model.Reward.Gold;
                    if (_session.Character.Gold > ServerManager.Instance.Configuration.MaxGold)
                    {
                        _session.Character.Gold = ServerManager.Instance.Configuration.MaxGold;
                    }

                    _session.SendPacket(_session.Character.GenerateGold());
                }

                if (model.Reward.Reputation > 0)
                {
                    _session.Character.Reputation += model.Reward.Reputation;
                    _session.SendPacket(_session.Character.GenerateFd());
                }

                if (model.Reward.Buff != -1)
                {
                    if (model.Reward.Buff == 378)
                    {
                        model.Reward.Buff += (short)ServerManager.RandomNumber(0, 2);
                    }

                    _session.Character.AddBuff(new Buff(model.Reward.Buff, _session.Character.Level));
                }

                if (model.Reward.TeleportPosition != null)
                {
                    ServerManager.Instance.ChangeMap(_session.Character.CharacterId,
                        model.Reward.TeleportPosition.MapId, model.Reward.TeleportPosition.MapX,
                        model.Reward.TeleportPosition.MapY);
                }

                if (model.Reward.GiftItems != null)
                {
                    foreach (XMLModel.Shared.Item item in model.Reward.GiftItems)
                    {
                        _session.Character.GiftAdd(item.VNum, item.Amount, design: item.Design, forceRandom: true);
                    }
                }
                if (model.Reward.DrawOneItems?.Length > 0 && model.Reward.DrawOneItems.OrderBy(s => ServerManager.RandomNumber()).ElementAt(0) is XMLModel.Shared.Item drawItem)
                {
                    _session.Character.GiftAdd(drawItem.VNum, drawItem.Amount, design: drawItem.Design);
                }
                _session.SendPacket(_session.Character.GenerateLev());
                if (showLevelUp)
                {
                    _session.SendPacket(_session.Character.GenerateStat());
                    _session.SendPacket(_session.Character.GenerateLevelUp());
                    _session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("LEVELUP"),
                        0));
                    _session.CurrentMapInstance?.Broadcast(
                        StaticPacketHelper.GenerateEff(UserType.Player, _session.Character.CharacterId, 6),
                        _session.Character.PositionX, _session.Character.PositionY);
                    _session.CurrentMapInstance?.Broadcast(
                        StaticPacketHelper.GenerateEff(UserType.Player, _session.Character.CharacterId, 198),
                        _session.Character.PositionX, _session.Character.PositionY);
                    ServerManager.Instance.UpdateGroup(_session.Character.CharacterId);
                }

                model.IsFinished = true;
            }

            RefreshQuestUi();

            if (model.IsFinished)
            {
                if (model.Reward.Script != null)
                {
                    _session.SendPacket($"script {model.Reward.Script.Type} {model.Reward.Script.Value}");
                }

                if (model.Reward.QuestId != -1)
                {
                    AddQuest(model.Reward.QuestId);
                }

                if (model.QuestGoalType == QuestGoalType.SideQuest)
                {
                    Quests.TryRemove(model.QuestProgressId, out QuestModel _);
                    DAOFactory.QuestProgressDAO.DeleteById(model.QuestProgressId);
                }

                InitializeQuestUi();
            }
        }

        #endregion
    }
}