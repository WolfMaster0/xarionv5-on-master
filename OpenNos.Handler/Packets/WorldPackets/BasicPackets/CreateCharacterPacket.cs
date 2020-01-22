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
using System.Text.RegularExpressions;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.GameLog.LogHelper;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.BasicPackets
{
    [PacketHeader("Char_NEW")]
    public class CreateCharacterPacket
    {
        #region Properties

        public GenderType Gender { get; set; }

        public HairColorType HairColor { get; set; }

        public HairStyleType HairStyle { get; set; }

        public string Name { get; set; }

        public byte Slot { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 7)
            {
                return;
            }
            CreateCharacterPacket charNewPacket = new CreateCharacterPacket();
            if (!string.IsNullOrWhiteSpace(packetSplit[2])
                && byte.TryParse(packetSplit[3], out byte slot)
                && Enum.TryParse(packetSplit[4], out GenderType gender)
                && Enum.TryParse(packetSplit[5], out HairStyleType hairStyle)
                && Enum.TryParse(packetSplit[6], out HairColorType hairColor))
            {
                charNewPacket.Name = packetSplit[2];
                charNewPacket.Slot = slot;
                charNewPacket.Gender = gender;
                charNewPacket.HairStyle = hairStyle;
                charNewPacket.HairColor = hairColor;
                charNewPacket.ExecuteHandler(session as ClientSession, packet);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(CreateCharacterPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session, string packet)
        {
            if (session.HasCurrentMapInstance)
            {
                return;
            }

            long accountId = session.Account.AccountId;
            if (Slot <= 2
                && DAOFactory.CharacterDAO.LoadBySlot(accountId, Slot) == null
                && Name.Length > 3 && Name.Length < 15)
            {
                Regex rg = new Regex(@"^[A-Za-z0-9_äÄöÖüÜß~*<>°+-.!_-Ð™¤£±†‡×ßø^\S]+$");
                if (rg.Matches(Name).Count == 1)
                {
                    if (DAOFactory.CharacterDAO.LoadByName(Name) == null)
                    {
                        GameLogger.Instance.LogCharacterCreation(ServerManager.Instance.ChannelId, session.Account.Name,
                            session.Account.AccountId, Name, Slot, Gender, HairStyle, HairColor);
                        CharacterDTO newCharacter = new CharacterDTO
                        {
                            Class = (byte)ClassType.Adventurer,
                            Gender = Gender,
                            HairColor = HairColor,
                            HairStyle = HairStyle,
                            Hp = 221,
                            JobLevel = 1,
                            Level = 1,
                            MapId = 1,
                            MapX = (short)ServerManager.RandomNumber(78, 81),
                            MapY = (short)ServerManager.RandomNumber(114, 118),
                            Mp = 221,
                            MaxMateCount = 10,
                            SpPoint = 10000,
                            SpAdditionPoint = 0,
                            Name = Name,
                            Slot = Slot,
                            AccountId = accountId,
                            MaxPartnerCount = 3,
                            MinilandMessage = "Welcome on Xarion",
                            State = CharacterState.Active,
                            MinilandPoint = 2000
                        };

                        DAOFactory.CharacterDAO.InsertOrUpdate(ref newCharacter);
                        CharacterSkillDTO sk1 =
                            new CharacterSkillDTO { CharacterId = newCharacter.CharacterId, SkillVNum = 200 };
                        CharacterSkillDTO sk2 =
                            new CharacterSkillDTO { CharacterId = newCharacter.CharacterId, SkillVNum = 201 };
                        CharacterSkillDTO sk3 =
                            new CharacterSkillDTO { CharacterId = newCharacter.CharacterId, SkillVNum = 209 };
                        QuicklistEntryDTO qlst1 = new QuicklistEntryDTO
                        {
                            CharacterId = newCharacter.CharacterId,
                            Type = 1,
                            Slot = 1,
                            Pos = 1
                        };
                        QuicklistEntryDTO qlst2 = new QuicklistEntryDTO
                        {
                            CharacterId = newCharacter.CharacterId,
                            Q2 = 1,
                            Slot = 2
                        };
                        QuicklistEntryDTO qlst3 = new QuicklistEntryDTO
                        {
                            CharacterId = newCharacter.CharacterId,
                            Q2 = 8,
                            Type = 1,
                            Slot = 1,
                            Pos = 16
                        };
                        QuicklistEntryDTO qlst4 = new QuicklistEntryDTO
                        {
                            CharacterId = newCharacter.CharacterId,
                            Q2 = 9,
                            Type = 1,
                            Slot = 3,
                            Pos = 1
                        };
                        DAOFactory.QuicklistEntryDAO.InsertOrUpdate(qlst1);
                        DAOFactory.QuicklistEntryDAO.InsertOrUpdate(qlst2);
                        DAOFactory.QuicklistEntryDAO.InsertOrUpdate(qlst3);
                        DAOFactory.QuicklistEntryDAO.InsertOrUpdate(qlst4);
                        DAOFactory.CharacterSkillDAO.InsertOrUpdate(sk1);
                        DAOFactory.CharacterSkillDAO.InsertOrUpdate(sk2);
                        DAOFactory.CharacterSkillDAO.InsertOrUpdate(sk3);

                        using (Inventory startupInventory = new Inventory(new Character(newCharacter)))
                        {
                            startupInventory.AddNewToInventory(901, 1, InventoryType.Equipment);
                            startupInventory.AddNewToInventory(903, 1, InventoryType.Equipment);
                            startupInventory.AddNewToInventory(905, 1, InventoryType.Equipment);
                            startupInventory.AddNewToInventory(884, 1, InventoryType.Equipment);
                            startupInventory.AddNewToInventory(885, 1, InventoryType.Equipment);
                            startupInventory.AddNewToInventory(886, 1, InventoryType.Equipment);
                            startupInventory.AddNewToInventory(887, 1, InventoryType.Equipment);
                            startupInventory.AddNewToInventory(5228, 1, InventoryType.Main);
                            startupInventory.AddNewToInventory(1, 1, InventoryType.Wear, 7, 8);
                            startupInventory.AddNewToInventory(8, 1, InventoryType.Wear, 7, 8);
                            startupInventory.AddNewToInventory(12, 1, InventoryType.Wear, 7, 8);
                            startupInventory.AddNewToInventory(2081, 255, InventoryType.Etc);
                            startupInventory.ForEach(i => DAOFactory.ItemInstanceDAO.InsertOrUpdate(i));
                            EntryPointPacket.HandlePacket(session, packet);
                        }
                    }
                    else
                    {
                        session.SendPacketFormat($"info {Language.Instance.GetMessageFromKey("ALREADY_TAKEN")}");
                    }
                }
                else
                {
                    session.SendPacketFormat($"info {Language.Instance.GetMessageFromKey("INVALID_CHARNAME")}");
                }
            }
        }

        #endregion
    }
}