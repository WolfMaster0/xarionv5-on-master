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
using System.Diagnostics;
using System.Linq;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Handler.Packets.SharedPacketMethods;

namespace OpenNos.Handler.Packets.WorldPackets.InventoryPackets
{
    [PacketHeader("sl")]
    public class SpecialistTransformPacket
    {
        #region Properties

        public short SpecialistDamage { get; set; }

        public short SpecialistDefense { get; set; }

        public short SpecialistElement { get; set; }

        public short SpecialistHP { get; set; }

        public int TransportId { get; set; }

        public byte Type { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 3)
            {
                return;
            }
            SpecialistTransformPacket packetDefinition = new SpecialistTransformPacket();
            if (byte.TryParse(packetSplit[2], out byte type))
            {
                packetDefinition.Type = type;

                if (packetSplit.Length > 5 && int.TryParse(packetSplit[5], out int transportId))
                {
                    packetDefinition.TransportId = transportId;
                }

                if (packetSplit.Length > 6 && short.TryParse(packetSplit[6], out short specialistDamage))
                {
                    packetDefinition.SpecialistDamage = specialistDamage;
                }

                if (packetSplit.Length > 7 && short.TryParse(packetSplit[7], out short specialistDefense))
                {
                    packetDefinition.SpecialistDefense = specialistDefense;
                }

                if (packetSplit.Length > 8 && short.TryParse(packetSplit[8], out short specialistElement))
                {
                    packetDefinition.SpecialistElement = specialistElement;
                }

                if (packetSplit.Length > 9 && short.TryParse(packetSplit[9], out short specialistHP))
                {
                    packetDefinition.SpecialistHP = specialistHP;
                }

                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(SpecialistTransformPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            ItemInstance specialistInstance =
                session.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.Sp, InventoryType.Wear);

            if (Type == 10)
            {
                short specialistDamage = SpecialistDamage,
                    specialistDefense = SpecialistDefense,
                    specialistElement = SpecialistElement,
                    specialistHealpoints = SpecialistHP;
                int transportId = TransportId;
                if (!session.Character.UseSp || specialistInstance == null
                    || transportId != specialistInstance.TransportId)
                {
                    session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("SPUSE_NEEDED"), 0));
                    return;
                }

                if (CharacterHelper.SpPoint(specialistInstance.SpLevel, specialistInstance.Upgrade)
                    - specialistInstance.SlDamage - specialistInstance.SlHP - specialistInstance.SlElement
                    - specialistInstance.SlDefence - specialistDamage - specialistDefense - specialistElement
                    - specialistHealpoints < 0)
                {
                    return;
                }

                if (specialistDamage < 0 || specialistDefense < 0 || specialistElement < 0
                    || specialistHealpoints < 0)
                {
                    return;
                }

                specialistInstance.SlDamage += specialistDamage;
                specialistInstance.SlDefence += specialistDefense;
                specialistInstance.SlElement += specialistElement;
                specialistInstance.SlHP += specialistHealpoints;

                ItemInstance mainWeapon =
                    session.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.MainWeapon,
                        InventoryType.Wear);
                ItemInstance secondaryWeapon =
                    session.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.MainWeapon,
                        InventoryType.Wear);
                List<ShellEffectDTO> effects = new List<ShellEffectDTO>();
                if (mainWeapon?.ShellEffects != null)
                {
                    effects.AddRange(mainWeapon.ShellEffects);
                }

                if (secondaryWeapon?.ShellEffects != null)
                {
                    effects.AddRange(secondaryWeapon.ShellEffects);
                }

                int GetShellWeaponEffectValue(ShellWeaponEffectType effectType)
                {
                    return effects.Where(s => s.Effect == (byte)effectType).OrderByDescending(s => s.Value)
                               .FirstOrDefault()?.Value ?? 0;
                }

                int slElement = CharacterHelper.SlPoint(specialistInstance.SlElement, 2)
                                + GetShellWeaponEffectValue(ShellWeaponEffectType.SlElement)
                                + GetShellWeaponEffectValue(ShellWeaponEffectType.SlGlobal);
                int slHp = CharacterHelper.SlPoint(specialistInstance.SlHP, 3)
                           + GetShellWeaponEffectValue(ShellWeaponEffectType.Slhp)
                           + GetShellWeaponEffectValue(ShellWeaponEffectType.SlGlobal);
                int slDefence = CharacterHelper.SlPoint(specialistInstance.SlDefence, 1)
                                + GetShellWeaponEffectValue(ShellWeaponEffectType.SlDefence)
                                + GetShellWeaponEffectValue(ShellWeaponEffectType.SlGlobal);
                int slHit = CharacterHelper.SlPoint(specialistInstance.SlDamage, 0)
                            + GetShellWeaponEffectValue(ShellWeaponEffectType.SlDamage)
                            + GetShellWeaponEffectValue(ShellWeaponEffectType.SlGlobal);

                #region slHit

                specialistInstance.DamageMinimum = 0;
                specialistInstance.DamageMaximum = 0;
                specialistInstance.HitRate = 0;
                specialistInstance.CriticalLuckRate = 0;
                specialistInstance.CriticalRate = 0;
                specialistInstance.DefenceDodge = 0;
                specialistInstance.DistanceDefenceDodge = 0;
                specialistInstance.ElementRate = 0;
                specialistInstance.DarkResistance = 0;
                specialistInstance.LightResistance = 0;
                specialistInstance.FireResistance = 0;
                specialistInstance.WaterResistance = 0;
                specialistInstance.CriticalDodge = 0;
                specialistInstance.CloseDefence = 0;
                specialistInstance.DistanceDefence = 0;
                specialistInstance.MagicDefence = 0;
                specialistInstance.HP = 0;
                specialistInstance.MP = 0;

                if (slHit >= 1)
                {
                    specialistInstance.DamageMinimum += 5;
                    specialistInstance.DamageMaximum += 5;
                }

                if (slHit >= 10)
                {
                    specialistInstance.HitRate += 10;
                }

                if (slHit >= 20)
                {
                    specialistInstance.CriticalLuckRate += 2;
                }

                if (slHit >= 30)
                {
                    specialistInstance.DamageMinimum += 5;
                    specialistInstance.DamageMaximum += 5;
                    specialistInstance.HitRate += 10;
                }

                if (slHit >= 40)
                {
                    specialistInstance.CriticalRate += 10;
                }

                if (slHit >= 50)
                {
                    specialistInstance.HP += 200;
                    specialistInstance.MP += 200;
                }

                if (slHit >= 60)
                {
                    specialistInstance.HitRate += 15;
                }

                if (slHit >= 70)
                {
                    specialistInstance.HitRate += 15;
                    specialistInstance.DamageMinimum += 5;
                    specialistInstance.DamageMaximum += 5;
                }

                if (slHit >= 80)
                {
                    specialistInstance.CriticalLuckRate += 3;
                }

                if (slHit >= 90)
                {
                    specialistInstance.CriticalRate += 20;
                }

                if (slHit >= 100)
                {
                    specialistInstance.CriticalLuckRate += 3;
                    specialistInstance.CriticalRate += 20;
                    specialistInstance.HP += 200;
                    specialistInstance.MP += 200;
                    specialistInstance.DamageMinimum += 5;
                    specialistInstance.DamageMaximum += 5;
                    specialistInstance.HitRate += 20;
                }

                #endregion

                #region slDefence

                if (slDefence >= 10)
                {
                    specialistInstance.DefenceDodge += 5;
                    specialistInstance.DistanceDefenceDodge += 5;
                }

                if (slDefence >= 20)
                {
                    specialistInstance.CriticalDodge += 2;
                }

                if (slDefence >= 30)
                {
                    specialistInstance.HP += 100;
                }

                if (slDefence >= 40)
                {
                    specialistInstance.CriticalDodge += 2;
                }

                if (slDefence >= 50)
                {
                    specialistInstance.DefenceDodge += 5;
                    specialistInstance.DistanceDefenceDodge += 5;
                }

                if (slDefence >= 60)
                {
                    specialistInstance.HP += 200;
                }

                if (slDefence >= 70)
                {
                    specialistInstance.CriticalDodge += 3;
                }

                if (slDefence >= 75)
                {
                    specialistInstance.FireResistance += 2;
                    specialistInstance.WaterResistance += 2;
                    specialistInstance.LightResistance += 2;
                    specialistInstance.DarkResistance += 2;
                }

                if (slDefence >= 80)
                {
                    specialistInstance.DefenceDodge += 10;
                    specialistInstance.DistanceDefenceDodge += 10;
                    specialistInstance.CriticalDodge += 3;
                }

                if (slDefence >= 90)
                {
                    specialistInstance.FireResistance += 3;
                    specialistInstance.WaterResistance += 3;
                    specialistInstance.LightResistance += 3;
                    specialistInstance.DarkResistance += 3;
                }

                if (slDefence >= 95)
                {
                    specialistInstance.HP += 300;
                }

                if (slDefence >= 100)
                {
                    specialistInstance.DefenceDodge += 20;
                    specialistInstance.DistanceDefenceDodge += 20;
                    specialistInstance.FireResistance += 5;
                    specialistInstance.WaterResistance += 5;
                    specialistInstance.LightResistance += 5;
                    specialistInstance.DarkResistance += 5;
                }

                #endregion

                #region slHp

                if (slHp >= 5)
                {
                    specialistInstance.DamageMinimum += 5;
                    specialistInstance.DamageMaximum += 5;
                }

                if (slHp >= 10)
                {
                    specialistInstance.DamageMinimum += 5;
                    specialistInstance.DamageMaximum += 5;
                }

                if (slHp >= 15)
                {
                    specialistInstance.DamageMinimum += 5;
                    specialistInstance.DamageMaximum += 5;
                }

                if (slHp >= 20)
                {
                    specialistInstance.DamageMinimum += 5;
                    specialistInstance.DamageMaximum += 5;
                    specialistInstance.CloseDefence += 10;
                    specialistInstance.DistanceDefence += 10;
                    specialistInstance.MagicDefence += 10;
                }

                if (slHp >= 25)
                {
                    specialistInstance.DamageMinimum += 5;
                    specialistInstance.DamageMaximum += 5;
                }

                if (slHp >= 30)
                {
                    specialistInstance.DamageMinimum += 5;
                    specialistInstance.DamageMaximum += 5;
                }

                if (slHp >= 35)
                {
                    specialistInstance.DamageMinimum += 5;
                    specialistInstance.DamageMaximum += 5;
                }

                if (slHp >= 40)
                {
                    specialistInstance.DamageMinimum += 5;
                    specialistInstance.DamageMaximum += 5;
                    specialistInstance.CloseDefence += 15;
                    specialistInstance.DistanceDefence += 15;
                    specialistInstance.MagicDefence += 15;
                }

                if (slHp >= 45)
                {
                    specialistInstance.DamageMinimum += 10;
                    specialistInstance.DamageMaximum += 10;
                }

                if (slHp >= 50)
                {
                    specialistInstance.DamageMinimum += 10;
                    specialistInstance.DamageMaximum += 10;
                    specialistInstance.FireResistance += 2;
                    specialistInstance.WaterResistance += 2;
                    specialistInstance.LightResistance += 2;
                    specialistInstance.DarkResistance += 2;
                }

                if (slHp >= 55)
                {
                    specialistInstance.DamageMinimum += 10;
                    specialistInstance.DamageMaximum += 10;
                }

                if (slHp >= 60)
                {
                    specialistInstance.DamageMinimum += 10;
                    specialistInstance.DamageMaximum += 10;
                }

                if (slHp >= 65)
                {
                    specialistInstance.DamageMinimum += 10;
                    specialistInstance.DamageMaximum += 10;
                }

                if (slHp >= 70)
                {
                    specialistInstance.DamageMinimum += 10;
                    specialistInstance.DamageMaximum += 10;
                    specialistInstance.CloseDefence += 20;
                    specialistInstance.DistanceDefence += 20;
                    specialistInstance.MagicDefence += 20;
                }

                if (slHp >= 75)
                {
                    specialistInstance.DamageMinimum += 15;
                    specialistInstance.DamageMaximum += 15;
                }

                if (slHp >= 80)
                {
                    specialistInstance.DamageMinimum += 15;
                    specialistInstance.DamageMaximum += 15;
                }

                if (slHp >= 85)
                {
                    specialistInstance.DamageMinimum += 15;
                    specialistInstance.DamageMaximum += 15;
                    specialistInstance.CriticalDodge++;
                }

                if (slHp >= 86)
                {
                    specialistInstance.CriticalDodge++;
                }

                if (slHp >= 87)
                {
                    specialistInstance.CriticalDodge++;
                }

                if (slHp >= 88)
                {
                    specialistInstance.CriticalDodge++;
                }

                if (slHp >= 90)
                {
                    specialistInstance.DamageMinimum += 15;
                    specialistInstance.DamageMaximum += 15;
                    specialistInstance.CloseDefence += 25;
                    specialistInstance.DistanceDefence += 25;
                    specialistInstance.MagicDefence += 25;
                }

                if (slHp >= 91)
                {
                    specialistInstance.DefenceDodge += 2;
                    specialistInstance.DistanceDefenceDodge += 2;
                }

                if (slHp >= 92)
                {
                    specialistInstance.DefenceDodge += 2;
                    specialistInstance.DistanceDefenceDodge += 2;
                }

                if (slHp >= 93)
                {
                    specialistInstance.DefenceDodge += 2;
                    specialistInstance.DistanceDefenceDodge += 2;
                }

                if (slHp >= 94)
                {
                    specialistInstance.DefenceDodge += 2;
                    specialistInstance.DistanceDefenceDodge += 2;
                }

                if (slHp >= 95)
                {
                    specialistInstance.DamageMinimum += 20;
                    specialistInstance.DamageMaximum += 20;
                    specialistInstance.DefenceDodge += 2;
                    specialistInstance.DistanceDefenceDodge += 2;
                }

                if (slHp >= 96)
                {
                    specialistInstance.DefenceDodge += 2;
                    specialistInstance.DistanceDefenceDodge += 2;
                }

                if (slHp >= 97)
                {
                    specialistInstance.DefenceDodge += 2;
                    specialistInstance.DistanceDefenceDodge += 2;
                }

                if (slHp >= 98)
                {
                    specialistInstance.DefenceDodge += 2;
                    specialistInstance.DistanceDefenceDodge += 2;
                }

                if (slHp >= 99)
                {
                    specialistInstance.DefenceDodge += 2;
                    specialistInstance.DistanceDefenceDodge += 2;
                }

                if (slHp >= 100)
                {
                    specialistInstance.FireResistance += 3;
                    specialistInstance.WaterResistance += 3;
                    specialistInstance.LightResistance += 3;
                    specialistInstance.DarkResistance += 3;
                    specialistInstance.CloseDefence += 30;
                    specialistInstance.DistanceDefence += 30;
                    specialistInstance.MagicDefence += 30;
                    specialistInstance.DamageMinimum += 20;
                    specialistInstance.DamageMaximum += 20;
                    specialistInstance.DefenceDodge += 2;
                    specialistInstance.DistanceDefenceDodge += 2;
                    specialistInstance.CriticalDodge++;
                }

                #endregion

                #region slElement

                if (slElement >= 1)
                {
                    specialistInstance.ElementRate += 2;
                }

                if (slElement >= 10)
                {
                    specialistInstance.MP += 100;
                }

                if (slElement >= 20)
                {
                    specialistInstance.MagicDefence += 5;
                }

                if (slElement >= 30)
                {
                    specialistInstance.FireResistance += 2;
                    specialistInstance.WaterResistance += 2;
                    specialistInstance.LightResistance += 2;
                    specialistInstance.DarkResistance += 2;
                    specialistInstance.ElementRate += 2;
                }

                if (slElement >= 40)
                {
                    specialistInstance.MP += 100;
                }

                if (slElement >= 50)
                {
                    specialistInstance.MagicDefence += 5;
                }

                if (slElement >= 60)
                {
                    specialistInstance.FireResistance += 3;
                    specialistInstance.WaterResistance += 3;
                    specialistInstance.LightResistance += 3;
                    specialistInstance.DarkResistance += 3;
                    specialistInstance.ElementRate += 2;
                }

                if (slElement >= 70)
                {
                    specialistInstance.MP += 100;
                }

                if (slElement >= 80)
                {
                    specialistInstance.MagicDefence += 5;
                }

                if (slElement >= 90)
                {
                    specialistInstance.FireResistance += 4;
                    specialistInstance.WaterResistance += 4;
                    specialistInstance.LightResistance += 4;
                    specialistInstance.DarkResistance += 4;
                    specialistInstance.ElementRate += 2;
                }

                if (slElement >= 100)
                {
                    specialistInstance.FireResistance += 6;
                    specialistInstance.WaterResistance += 6;
                    specialistInstance.LightResistance += 6;
                    specialistInstance.DarkResistance += 6;
                    specialistInstance.MagicDefence += 5;
                    specialistInstance.MP += 200;
                    specialistInstance.ElementRate += 2;
                }

                #endregion

                session.SendPacket(session.Character.GenerateStatChar());
                session.SendPacket(session.Character.GenerateStat());
                session.SendPacket(specialistInstance.GenerateSlInfo());
                session.SendPacket(
                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("POINTS_SET"), 0));
            }
            else if (!session.Character.IsSitting)
            {
                if (session.Character.Skills.Any(s => !s.CanBeUsed()))
                {
                    session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("SKILLS_IN_LOADING"),
                            0));
                    return;
                }

                if (specialistInstance == null)
                {
                    session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NO_SP"),
                        0));
                    return;
                }

                if (session.Character.IsVehicled)
                {
                    session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("REMOVE_VEHICLE"), 0));
                    return;
                }

                double currentRunningSeconds =
                    (DateTime.UtcNow - Process.GetCurrentProcess().StartTime.AddHours(-2).AddSeconds(-50)).TotalSeconds;

                if (session.Character.UseSp)
                {
                    session.Character.LastSp = currentRunningSeconds;
                    session.RemoveSp(specialistInstance.ItemVNum);
                }
                else
                {
                    if (session.Character.LastMove.AddSeconds(1) >= DateTime.UtcNow
                        || session.Character.LastSkillUse.AddSeconds(2) >= DateTime.UtcNow)
                    {
                        return;
                    }

                    if (session.Character.SpPoint == 0 && session.Character.SpAdditionPoint == 0)
                    {
                        session.SendPacket(
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("SP_NOPOINTS"), 0));
                    }

                    double timeSpanSinceLastSpUsage = currentRunningSeconds - session.Character.LastSp;
                    if (timeSpanSinceLastSpUsage >= session.Character.SpCooldown)
                    {
                        if (Type == 1)
                        {
                            if (session.Character.Buff.Any(s => s.Card.BuffType == BuffType.Bad))
                            {
                                session.SendPacket(
                                    UserInterfaceHelper.GenerateMsg(
                                        Language.Instance.GetMessageFromKey("UNDER_BAD_BUFF"), 0));
                                return;
                            }
                            DateTime delay = DateTime.UtcNow.AddSeconds(-6);
                            if (session.Character.LastDelay > delay
                                && session.Character.LastDelay < delay.AddSeconds(2))
                            {
                                session.ChangeSp();
                            }
                        }
                        else
                        {
                            session.Character.LastDelay = DateTime.UtcNow;
                            session.SendPacket(UserInterfaceHelper.GenerateDelay(5000, 3, "#sl^1"));
                            session.CurrentMapInstance?.Broadcast(
                                UserInterfaceHelper.GenerateGuri(2, 1, session.Character.CharacterId),
                                session.Character.PositionX, session.Character.PositionY);
                        }
                    }
                    else
                    {
                        session.SendPacket(UserInterfaceHelper.GenerateMsg(
                            string.Format(Language.Instance.GetMessageFromKey("SP_INLOADING"),
                                session.Character.SpCooldown - (int)Math.Round(timeSpanSinceLastSpUsage, 0)), 0));
                    }
                }
            }
        }

        #endregion
    }
}