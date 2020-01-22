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

using System;
using System.Collections.Generic;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameLog.Networking;
using OpenNos.GameLog.Shared;

namespace OpenNos.GameLog.LogHelper
{
    public class GameLogger
    {
        private bool _isStandAlone;

        private static GameLogger _instance;

        public static GameLogger Instance => _instance;

        public static bool InitializeLogger(string authKey)
        {
            if (string.IsNullOrEmpty(authKey))
            {
                _instance = new GameLogger
                {
                    _isStandAlone = true
                };
                return true;
            }

            if (GameLogServiceClient.Instance.Authenticate(authKey))
            {
                _instance = new GameLogger
                {
                    _isStandAlone = false
                };
                return true;
            }

            return false;
        }

        public void LogTrade(int channelId, string senderName, long senderId,
            string receiverCharacterName, long receiverCharacterId, long goldAmount, long goldBankAmount,
            List<ItemInstanceDTO> items)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "EXCHANGE"},
                {"ReceiverCharacterName", receiverCharacterName},
                {"ReceiverCharacterId", receiverCharacterId.ToString()},
                {"GoldAmount", goldAmount.ToString()},
                {"GoldBankAmount", goldBankAmount.ToString()}
            };
            int i = 0;
            foreach (ItemInstanceDTO item in items)
            {
                content.Add($"Item{i}Id", item.Id.ToString());
                content.Add($"Item{i}SerialId", item.EquipmentSerialId.ToString());
                content.Add($"Item{i}BoundCharacterId", item.BoundCharacterId?.ToString());
                content.Add($"Item{i}VNum", item.ItemVNum.ToString());
                content.Add($"Item{i}HoldingVNum", item.HoldingVNum.ToString());
                content.Add($"Item{i}Amount", item.Amount.ToString());
                content.Add($"Item{i}Rare", item.Rare.ToString());
                content.Add($"Item{i}Upgrade", item.Upgrade.ToString());
                content.Add($"Item{i}SpStoneUpgrade", item.SpStoneUpgrade.ToString());
                content.Add($"Item{i}SpDamage", item.SpDamage.ToString());
                content.Add($"Item{i}SpDefence", item.SpDefence.ToString());
                content.Add($"Item{i}SpElement", item.SpElement.ToString());
                content.Add($"Item{i}SpHP", item.SpHP.ToString());
                content.Add($"Item{i}SpFire", item.SpFire.ToString());
                content.Add($"Item{i}SpWater", item.SpWater.ToString());
                content.Add($"Item{i}SpLight", item.SpLight.ToString());
                content.Add($"Item{i}SpDark", item.SpDark.ToString());
                content.Add($"Item{i}ElementRate", item.ElementRate.ToString());
                content.Add($"Item{i}FireResistance", item.FireResistance.ToString());
                content.Add($"Item{i}WaterResistance", item.WaterResistance.ToString());
                content.Add($"Item{i}LightResistance", item.LightResistance.ToString());
                content.Add($"Item{i}DarkResistance", item.DarkResistance.ToString());
                i++;
            }
            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogSpecialistUnwear(int channelId, string senderName, long senderId,
            int spCooldown)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "SPECIALIST_UNWEAR"},
                {"SpCooldown", spCooldown.ToString()}
            };
            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogSpecialistWear(int channelId, string senderName, long senderId, short morph)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "SPECIALIST_WEAR"},
                {"Morph", morph.ToString()}
            };
            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogRaidSuccess(int channelId, string senderName, long senderId, long raidId,
            List<CharacterDTO> characters)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content =
                new Dictionary<string, string>
                {
                    {"Type", "RAID_SUCCESS"},
                    {"RaidId", raidId.ToString()}
                };
            int i = 0;
            foreach (CharacterDTO dto in characters)
            {
                content.Add($"Character{i}Id", dto.CharacterId.ToString());
                content.Add($"Character{i}Name", dto.Name);
                i++;
            }

            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogRaidStart(int channelId, string senderName, long senderId, long raidId,
            List<CharacterDTO> characters)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content =
                new Dictionary<string, string>
                {
                    {"Type", "RAID_START"},
                    {"RaidId", raidId.ToString()}
                };
            int i = 0;
            foreach (CharacterDTO dto in characters)
            {
                content.Add($"Character{i}Id", dto.CharacterId.ToString());
                content.Add($"Character{i}Name", dto.Name);
                i++;
            }

            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogItemOption(int channelId, string senderName, long senderId,
            ItemInstanceDTO item, short[] data)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "ITEM_OPTION"},
                {"ItemId", item.Id.ToString()},
                {"ItemSerialId", item.EquipmentSerialId.ToString()},
                {"ItemVNum", item.ItemVNum.ToString()},
                {"CellonOptionType", ((CellonOptionType) data[0]).ToString()},
                {"Value", data[1].ToString()}
            };
            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogItemRarify(int channelId, string senderName, long senderId,
            ItemInstanceDTO item, sbyte result)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "ITEM_RARIFY"},
                {"ItemId", item.Id.ToString()},
                {"ItemSerialId", item.EquipmentSerialId.ToString()},
                {"ItemVNum", item.ItemVNum.ToString()},
                {"ItemRare", item.Rare.ToString()},
                {"Result", result.ToString()}
            };
            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogItemSum(int channelId, string senderName, long senderId,
            ItemInstanceDTO item, sbyte result)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "ITEM_UPGRADE"},
                {"ItemId", item.Id.ToString()},
                {"ItemSerialId", item.EquipmentSerialId.ToString()},
                {"ItemVNum", item.ItemVNum.ToString()},
                {"ItemUpgrade", item.Upgrade.ToString()},
                {"ItemFRes", item.FireResistance.ToString()},
                {"ItemWRes", item.WaterResistance.ToString()},
                {"ItemLRes", item.LightResistance.ToString()},
                {"ItemDRes", item.DarkResistance.ToString()},
                {"Result", result.ToString()}
            };
            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogItemUpgrade(int channelId, string senderName, long senderId,
            ItemInstanceDTO item, UpgradeProtection protection, sbyte result)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "ITEM_UPGRADE"},
                {"ItemId", item.Id.ToString()},
                {"ItemSerialId", item.EquipmentSerialId.ToString()},
                {"ItemVNum", item.ItemVNum.ToString()},
                {"ItemRare", item.Rare.ToString()},
                {"ItemUpgrade", item.Upgrade.ToString()},
                {"Protection", protection.ToString()},
                {"Result", result.ToString()}
            };
            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogCharacterLogout(int channelId, string senderName, long senderId)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "CHARACTER_LOGOUT"}
            };
            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogCharacterLogin(int channelId, string senderName, long senderId)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "CHARACTER_LOGIN"}
            };
            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogCharacterCreation(int channelId, string senderName, long senderId, string name, byte slot,
            GenderType genderType, HairStyleType hairStyleType, HairColorType hairColorType)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "CHARACTER_CREATE"},
                {"Name", name},
                {"Slot", slot.ToString()},
                {"Gender", genderType.ToString()},
                {"HairStyle", hairStyleType.ToString()},
                {"HairColor", hairColorType.ToString()}
            };
            GameLogServiceClient.Instance.LogEntry(new GameLogEntry
            {
                ChannelId = channelId,
                CharacterId = senderId,
                CharacterName = senderName,
                Content = content,
                GameLogType = GameLogType.Account,
                Timestamp = DateTime.UtcNow
            });
        }

        public void LogCharacterDeletion(int channelId, string senderName, long senderId, CharacterDTO dto)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "CHARACTER_DELETE"},
                {"Id", dto.CharacterId.ToString()},
                {"Name", dto.Name},
                {"Slot", dto.Slot.ToString()},
                {"Level", dto.Level.ToString()}
            };
            GameLogServiceClient.Instance.LogEntry(new GameLogEntry
            {
                ChannelId = channelId,
                CharacterId = senderId,
                CharacterName = senderName,
                Content = content,
                GameLogType = GameLogType.Account,
                Timestamp = DateTime.UtcNow
            });
        }

        public void LogCharacterSave(int channelId, string senderName, long senderId, bool finished)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "CHARACTER_SAVE"},
                {"Finished", finished.ToString()}
            };
            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogCharacterLevelup(int channelId, string senderName, long senderId, byte level,
            byte jobLevel, byte spLevel, byte heroLevel, short mapId, short mapX, short mapY)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "LEVELUP"},
                {"Level", level.ToString()},
                {"JobLevel", jobLevel.ToString()},
                {"SPLevel", spLevel.ToString()},
                {"HeroLevel", heroLevel.ToString()},
                {"MapId", mapId.ToString()},
                {"MapX", mapX.ToString()},
                {"MapY", mapY.ToString()}
            };
            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogReferrerReward(int channelId, string senderName, long senderId, long accId,
            long refId)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "REFREWARD"},
                {"AccountId", accId.ToString()},
                {"ReferralId", refId.ToString()}
            };
            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogPickupGold(int channelId, string senderName, long senderId, int amount, bool isMax)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "REFREWARD"},
                {"Amount", amount.ToString()},
                {"IsMax", isMax.ToString()}
            };
            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogItemCreate(int channelId, string senderName, long senderId,
            ItemInstanceDTO item, short mapId, short mapX, short mapY)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "ITEM_CREATE"},
                {"ItemId", item.Id.ToString()},
                {"ItemSerialId", item.EquipmentSerialId.ToString()},
                {"ItemVNum", item.ItemVNum.ToString()},
                {"ItemAmount", item.Amount.ToString()},
                {"MapId", mapId.ToString()},
                {"MapX", mapX.ToString()},
                {"MapY", mapY.ToString()}
            };
            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogItemBuyPlayerShop(int channelId, string senderName, long senderId, string ownerName,
            long ownerId, ItemInstanceDTO item, short amount, long pricePer, short mapId, short mapX, short mapY)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "ITEM_BUY_PLAYERSHOP"},
                {"OwnerCharacterName", ownerName},
                {"OwnerCharacterId", ownerId.ToString()},
                {"ItemId", item.Id.ToString()},
                {"ItemSerialId", item.EquipmentSerialId.ToString()},
                {"ItemVNum", item.ItemVNum.ToString()},
                {"Amount", amount.ToString()},
                {"PricePer", pricePer.ToString()},
                {"MapId", mapId.ToString()},
                {"MapX", mapX.ToString()},
                {"MapY", mapY.ToString()}
            };
            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogItemBuyNpcShop(int channelId, string senderName, long senderId, long mapNpcId, short vnum,
            short amount, long price, short mapId, short mapX, short mapY)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "ITEM_BUY_NPCSHOP"},
                {"MapNpcId", mapNpcId.ToString()},
                {"ItemVNum", vnum.ToString()},
                {"Amount", amount.ToString()},
                {"Price", price.ToString()},
                {"MapId", mapId.ToString()},
                {"MapX", mapX.ToString()},
                {"MapY", mapY.ToString()}
            };
            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogSkillBuy(int channelId, string senderName, long senderId, long mapNpcId, short vnum, long price,
            short mapId, short mapX, short mapY)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "SKILL_BUY"},
                {"MapNpcId", mapNpcId.ToString()},
                {"SkillVNum", vnum.ToString()},
                {"Price", price.ToString()},
                {"MapId", mapId.ToString()},
                {"MapX", mapX.ToString()},
                {"MapY", mapY.ToString()}
            };
            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogItemDelete(int channelId, string senderName, long senderId,
            ItemInstanceDTO item, short mapId, short mapX, short mapY)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "ITEM_DELETE"},
                {"ItemId", item.Id.ToString()},
                {"ItemSerialId", item.EquipmentSerialId.ToString()},
                {"ItemVNum", item.ItemVNum.ToString()},
                {"ItemAmount", item.Amount.ToString()},
                {"MapId", mapId.ToString()},
                {"MapX", mapX.ToString()},
                {"MapY", mapY.ToString()}
            };
            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogItemDrop(int channelId, string senderName, long senderId,
            ItemInstanceDTO item, short amount, short mapId, short mapX, short mapY)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "ITEM_DELETE"},
                {"ItemId", item.Id.ToString()},
                {"ItemSerialId", item.EquipmentSerialId.ToString()},
                {"ItemVNum", item.ItemVNum.ToString()},
                {"ItemAmount", amount.ToString()},
                {"MapId", mapId.ToString()},
                {"MapX", mapX.ToString()},
                {"MapY", mapY.ToString()}
            };
            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogItemMove(int channelId, string senderName, long senderId, ItemInstanceDTO item,
            short amount, InventoryType destType, short destSlot)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "ITEM_MOVE"},
                {"ItemId", item.Id.ToString()},
                {"ItemSerialId", item.EquipmentSerialId.ToString()},
                {"ItemVNum", item.ItemVNum.ToString()},
                {"Amount", amount.ToString()},
                {"SourceType", item.Type.ToString()},
                {"SourceSlot", item.Slot.ToString()},
                {"DestType", destType.ToString()},
                {"DestSlot", destSlot.ToString()}
            };
            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogEquipmentWear(int channelId, string senderName, long senderId,
            ItemInstanceDTO item)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "EQUIPMENT_WEAR"},
                {"ItemId", item.Id.ToString()},
                {"ItemSerialId", item.EquipmentSerialId.ToString()},
                {"ItemVNum", item.ItemVNum.ToString()},
                {"ItemRare", item.Rare.ToString()},
                {"ItemUpgrade", item.Upgrade.ToString()}
            };
            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogEquipmentUnwear(int channelId, string senderName, long senderId,
            ItemInstanceDTO item)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "EQUIPMENT_UNWEAR"},
                {"ItemId", item.Id.ToString()},
                {"ItemSerialId", item.EquipmentSerialId.ToString()},
                {"ItemVNum", item.ItemVNum.ToString()},
                {"ItemRare", item.Rare.ToString()},
                {"ItemUpgrade", item.Upgrade.ToString()}
            };
            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogParcelReceive(int channelId, string senderName, long senderId, MailDTO dto)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "PARCEL_RECEIVE"},
                {"MailId", dto.MailId.ToString()},
                {"AttachmentVNum", dto.AttachmentVNum.ToString()},
                {"AttachmentAmount", dto.AttachmentAmount.ToString()},
                {"AttachmentRarity", dto.AttachmentRarity.ToString()},
                {"AttachmentUpgrade", dto.AttachmentUpgrade.ToString()},
                {"SenderId", dto.SenderId.ToString()},
                {"Title", dto.Title}
            };
            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogBazaarBuy(int channelId, string senderName, long senderId, BazaarItemDTO dto, short vnum,
            short amount, long price)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "BAZAAR_BUY"},
                {"BazaarItemId", dto.BazaarItemId.ToString()},
                {"SellerId", dto.SellerId.ToString()},
                {"ItemId", dto.ItemInstanceId.ToString()},
                {"ItemVNum", vnum.ToString()},
                {"Amount", amount.ToString()},
                {"Price", price.ToString()}
            };
            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogBazaarInsert(int channelId, string senderName, long senderId, BazaarItemDTO dto, short vnum)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "BAZAAR_INSERT"},
                {"BazaarItemId", dto.BazaarItemId.ToString()},
                {"SellerId", dto.SellerId.ToString()},
                {"ItemId", dto.ItemInstanceId.ToString()},
                {"ItemVNum", vnum.ToString()},
                {"Amount", dto.Amount.ToString()},
                {"Price", dto.Price.ToString()},
                {"Duration", dto.Duration.ToString()}
            };
            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogBazaarRemove(int channelId, string senderName, long senderId, BazaarItemDTO dto,
            ItemInstanceDTO item,
            long price, long taxes)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "BAZAAR_REMOVE"},
                {"BazaarItemId", dto.BazaarItemId.ToString()},
                {"SellerId", dto.SellerId.ToString()},
                {"ItemId", dto.ItemInstanceId.ToString()},
                {"ItemVNum", item.ItemVNum.ToString()},
                {"Amount", dto.Amount.ToString()},
                {"RemainAmount", item.Amount.ToString()},
                {"Price", price.ToString()},
                {"Taxes", taxes.ToString()}
            };
            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogGuildCreate(int channelId, string senderName, long senderId, string familyName, long familyId,
            List<CharacterDTO> founders)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "GUILD_CREATE"},
                {"FamilyName", familyName},
                {"FamilyId", familyId.ToString()}
            };

            int i = 0;
            foreach (CharacterDTO dto in founders)
            {
                content.Add($"Character{i}Id", dto.CharacterId.ToString());
                content.Add($"Character{i}Name", dto.Name);
                i++;
            }

            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogGuildDismiss(int channelId, string senderName, long senderId, string familyName, long familyId)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "GUILD_DISMISS"},
                {"FamilyName", familyName},
                {"FamilyId", familyId.ToString()}
            };

            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogGuildLeave(int channelId, string senderName, long senderId, string familyName, long familyId)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "GUILD_LEAVE"},
                {"FamilyName", familyName},
                {"FamilyId", familyId.ToString()}
            };

            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogGuildJoin(int channelId, string senderName, long senderId, string familyName, long familyId)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "GUILD_JOIN"},
                {"FamilyName", familyName},
                {"FamilyId", familyId.ToString()}
            };

            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogGuildResetSex(int channelId, string senderName, long senderId, string familyName, long familyId)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "GUILD_RESETSEX"},
                {"FamilyName", familyName},
                {"FamilyId", familyId.ToString()}
            };

            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogGuildManagement(int channelId, string senderName, long senderId, string familyName,
            long familyId, string targetName, long targetId, FamilyAuthority authorityType)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "GUILD_MANAGEMENT"},
                {"FamilyName", familyName},
                {"FamilyId", familyId.ToString()},
                {"TargetCharacterName", targetName},
                {"TargetCharacterId", targetId.ToString()},
                {"FamilyAuthority", authorityType.ToString()}
            };

            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogGuildTitle(int channelId, string senderName, long senderId, string familyName, long familyId,
            string targetName, long targetId, FamilyMemberRank memberRank)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "GUILD_TITLE"},
                {"FamilyName", familyName},
                {"FamilyId", familyId.ToString()},
                {"TargetCharacterName", targetName},
                {"TargetCharacterId", targetId.ToString()},
                {"FamilyMemberRank", memberRank.ToString()}
            };

            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogGuildMessage(int channelId, string senderName, long senderId, string familyName, long familyId,
            string message)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "GUILD_MESSAGE"},
                {"FamilyName", familyName},
                {"FamilyId", familyId.ToString()},
                {"Message", message}
            };

            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogGuildToday(int channelId, string senderName, long senderId, string familyName, long familyId,
            string message)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "GUILD_TODAY"},
                {"FamilyName", familyName},
                {"FamilyId", familyId.ToString()},
                {"Message", message}
            };

            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogGuildShout(int channelId, string senderName, long senderId, string familyName, long familyId,
            string message)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "GUILD_SHOUT"},
                {"FamilyName", familyName},
                {"FamilyId", familyId.ToString()},
                {"Message", message}
            };

            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogGuildInvite(int channelId, string senderName, long senderId, string familyName, long familyId,
            string inviteName, long inviteId)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "GUILD_INVITE"},
                {"FamilyName", familyName},
                {"FamilyId", familyId.ToString()},
                {"InviteCharacterName", inviteName},
                {"InviteCharacterId", inviteId.ToString()}
            };

            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogGuildKick(int channelId, string senderName, long senderId, string familyName, long familyId,
            string kickName, long kickId)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "GUILD_KICK"},
                {"FamilyName", familyName},
                {"FamilyId", familyId.ToString()},
                {"KickCharacterName", kickName},
                {"KickCharacterId", kickId.ToString()}
            };

            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogGuildRaidSuccess(int channelId, string senderName, long senderId, MapInstanceType raidType,
            List<CharacterDTO> characters)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content =
                new Dictionary<string, string>
                {
                    {"Type", "GUILD_RAID_SUCCESS"},
                    {"RaidType", raidType.ToString()}
                };
            int i = 0;
            foreach (CharacterDTO dto in characters)
            {
                content.Add($"Character{i}Id", dto.CharacterId.ToString());
                content.Add($"Character{i}Name", dto.Name);
                i++;
            }

            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogBankDeposit(int channelId, string senderName, long senderId, long amount, long oldBank,
            long oldGold, long newBank, long newGold)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "BANK_DEPOSIT"},
                {"Amount", amount.ToString()},
                {"OldBank", oldBank.ToString()},
                {"OldGold", oldGold.ToString()},
                {"NewBank", newBank.ToString()},
                {"NewGold", newGold.ToString()}
            };
            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogBankWithdraw(int channelId, string senderName, long senderId, long amount, long oldBank,
            long oldGold, long newBank, long newGold)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "BANK_WITHDRAW"},
                {"Amount", amount.ToString()},
                {"OldBank", oldBank.ToString()},
                {"OldGold", oldGold.ToString()},
                {"NewBank", newBank.ToString()},
                {"NewGold", newGold.ToString()}
            };
            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogBankIllegal(int channelId, string senderName, long senderId, string mode, string param1,
            string param2)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "BANK_ILLEGAL"},
                {"Mode", mode},
                {"Param1", param1},
                {"Param2", param2}
            };
            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogBankSend(int channelId, string senderName, long senderId, string receiverName, long receiverId,
            long amount, long oldSenderBank, long oldReceiverBank, long newSenderBank, long newReceiverBank)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "BANK_SEND"},
                {"ReceiverCharacterName", receiverName},
                {"ReceiverCharacterId", receiverId.ToString()},
                {"Amount", amount.ToString()},
                {"OldSenderBank", oldSenderBank.ToString()},
                {"OldReceiverBank", oldReceiverBank.ToString()},
                {"NewSenderBank", newSenderBank.ToString()},
                {"NewReceiverBank", newReceiverBank.ToString()}
            };
            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }

        public void LogTemplate(int channelId, string senderName, long senderId)
        {
            if (_isStandAlone)
            {
                return;
            }

            Dictionary<string, string> content = new Dictionary<string, string>
            {
                {"Type", "TEMPLATE"}
            };
            GameLogEntry entry = new GameLogEntry();
            try
            {
                entry.ChannelId = channelId;
                entry.CharacterId = senderId;
                entry.CharacterName = senderName;
                entry.Content = content;
                entry.GameLogType = GameLogType.Client;
                entry.Timestamp = DateTime.UtcNow;

                GameLogServiceClient.Instance.LogEntry(entry);
            }
            catch (Exception ex)
            {
                Logger.LogEventError("GAMELOG_LOG_SEND_FAIL",
                    "Failed to send LogEntry to GameLog Server. Will log locally.", ex);
                Logger.LogUserEvent("GAMELOG_LOCALLOG", senderName, entry.ToString());
            }
        }
    }
}