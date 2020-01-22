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
using OpenNos.Core.Cryptography;
using OpenNos.Core.Serializing;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.Handler.Packets.WorldPackets.BasicPackets
{
    [PacketHeader(3, "OpenNos.EntryPoint", CharacterRequired = false)]
    public class EntryPointPacket
    {
        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            EntryPointPacket entryPointPacket = new EntryPointPacket();
            entryPointPacket.ExecuteHandler(session as ClientSession, packet);
        }

        public static void Register() => PacketFacility.AddHandler(typeof(EntryPointPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session, string packet)
        {
            string[] loginPacketParts = packet.Split(' ');
            bool isCrossServerLogin = false;

            // Load account by given SessionId
            if (session.Account == null)
            {
                bool hasRegisteredAccountLogin = true;
                AccountDTO account = null;
                if (loginPacketParts.Length > 4)
                {
                    if (loginPacketParts.Length > 7 && loginPacketParts[4] == "DAC"
                        && loginPacketParts[8] == "CrossServerAuthenticate")
                    {
                        isCrossServerLogin = true;
                        account = DAOFactory.AccountDAO.LoadByName(loginPacketParts[5]);
                    }
                    else
                    {
                        account = DAOFactory.AccountDAO.LoadByName(loginPacketParts[4]);
                    }
                }

                try
                {
                    if (account != null)
                    {
                        if (isCrossServerLogin)
                        {
                            hasRegisteredAccountLogin =
                                CommunicationServiceClient.Instance.IsCrossServerLoginPermitted(account.AccountId,
                                    session.SessionId);
                        }
                        else
                        {
                            hasRegisteredAccountLogin =
                                CommunicationServiceClient.Instance.IsLoginPermitted(account.AccountId,
                                    session.SessionId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("MS Communication Failed.", ex);
                    session.Disconnect();
                    return;
                }

                if (loginPacketParts.Length > 4 && hasRegisteredAccountLogin)
                {
                    if (account != null)
                    {
                        if (account.Password.ToLower().Equals(CryptographyBase.Sha512(loginPacketParts[6]))
                            || isCrossServerLogin)
                        {
                            session.InitializeAccount(new Account(account), isCrossServerLogin);
                            ServerManager.Instance.CharacterScreenSessions[session.Account.AccountId] = session;
                        }
                        else
                        {
                            Logger.Debug($"Client {session.ClientId} forced Disconnection, invalid Password.");
                            session.Disconnect();
                            return;
                        }
                    }
                    else
                    {
                        Logger.Debug($"Client {session.ClientId} forced Disconnection, invalid AccountName.");
                        session.Disconnect();
                        return;
                    }
                }
                else
                {
                    Logger.Debug($"Client {session.ClientId} forced Disconnection, login has not been registered or Account is already logged in.");
                    session.Disconnect();
                    return;
                }
            }

            if (isCrossServerLogin)
            {
                if (byte.TryParse(loginPacketParts[6], out byte slot))
                {
                    SelectPacket.HandlePacket(session, $"1 select {slot}");
                }
            }
            else
            {
                IEnumerable<CharacterDTO> characters = DAOFactory.CharacterDAO.LoadByAccount(session.Account.AccountId);
                Logger.Info(string.Format(Language.Instance.GetMessageFromKey("ACCOUNT_ARRIVED"), session.SessionId));

                // load characterlist packet for each character in CharacterDTO
                session.SendPacket("clist_start 0");
                foreach (CharacterDTO character in characters)
                {
                    IEnumerable<ItemInstanceDTO> inventory =
                        DAOFactory.ItemInstanceDAO.LoadByType(character.CharacterId, InventoryType.Wear);

                    ItemInstance[] equipment = new ItemInstance[16];
                    foreach (ItemInstanceDTO equipmentEntry in inventory)
                    {
                        // explicit load of iteminstance
                        ItemInstance currentInstance = new ItemInstance(equipmentEntry);
                        if (currentInstance != null)
                        {
                            equipment[(short)currentInstance.Item.EquipmentSlot] = currentInstance;
                        }
                    }

                    string petlist = string.Empty;
                    List<MateDTO> mates = DAOFactory.MateDAO.LoadByCharacterId(character.CharacterId).ToList();
                    for (int i = 0; i < 26; i++)
                    {
                        //0.2105.1102.319.0.632.0.333.0.318.0.317.0.9.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1
                        petlist += (i != 0 ? "." : string.Empty) + (mates.Count > i ? $"{mates[i].Skin}.{mates[i].NpcMonsterVNum}" : "-1");
                    }

                    // 1 1 before long string of -1.-1 = act completion
                    session.SendPacket($"clist {character.Slot} {character.Name} 0 {(byte)character.Gender} {(byte)character.HairStyle} {(byte)character.HairColor} 0 {(byte)character.Class} {character.Level} {character.HeroLevel} {equipment[(byte)EquipmentType.Hat]?.ItemVNum ?? -1}.{equipment[(byte)EquipmentType.Armor]?.ItemVNum ?? -1}.{equipment[(byte)EquipmentType.WeaponSkin]?.ItemVNum ?? (equipment[(byte)EquipmentType.MainWeapon]?.ItemVNum ?? -1)}.{equipment[(byte)EquipmentType.SecondaryWeapon]?.ItemVNum ?? -1}.{equipment[(byte)EquipmentType.Mask]?.ItemVNum ?? -1}.{equipment[(byte)EquipmentType.Fairy]?.ItemVNum ?? -1}.{equipment[(byte)EquipmentType.CostumeSuit]?.ItemVNum ?? -1}.{equipment[(byte)EquipmentType.CostumeHat]?.ItemVNum ?? -1} {character.JobLevel}  1 1 {petlist} {(equipment[(byte)EquipmentType.Hat]?.Item.IsColored == true ? equipment[(byte)EquipmentType.Hat].Design : 0)} 0");
                }

                session.SendPacket("clist_end");
            }
        }

        #endregion
    }
}