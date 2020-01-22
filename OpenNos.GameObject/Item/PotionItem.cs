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
using OpenNos.Data;
using System;
using System.Linq;
using OpenNos.Core;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject
{
    public class PotionItem : Item
    {
        #region Instantiation

        public PotionItem(ItemDTO item) : base(item)
        {
        }

        #endregion

        #region Methods

        public override void Use(ClientSession session, ref ItemInstance inv, byte Option = 0, string[] packetsplit = null)
        {
            if (!session.HasCurrentMapInstance)
            {
                return;
            }
            if ((DateTime.UtcNow - session.Character.LastPotion).TotalMilliseconds < (session.CurrentMapInstance.Map.MapTypes.OrderByDescending(s => s.PotionDelay).FirstOrDefault()?.PotionDelay ?? 750))
            {
                return;
            }
            session.Character.LastPotion = DateTime.UtcNow;
            switch (Effect)
            {
                case 0:
                    var totalHpRegen = 0;
                    var totalMpRegen = 0;
                    int hpLoad = (int)session.Character.HPLoad();
                    int mpLoad = (int)session.Character.MPLoad();
                    if (hpLoad - session.Character.Hp < Hp)
                    {
                        session.CurrentMapInstance?.Broadcast(session.Character.GenerateRc(hpLoad - session.Character.Hp));
                        totalHpRegen++;
                    }
                    else if (hpLoad - session.Character.Hp > Hp)
                    {
                        session.CurrentMapInstance?.Broadcast(session.Character.GenerateRc(Hp));
                        totalHpRegen++;
                    }
                    session.Character.Mp += Mp;
                    session.Character.Hp += Hp;
                    if (session.Character.Mp > mpLoad)
                    {
                        session.Character.Mp = mpLoad;
                        totalMpRegen++;
                    }
                    if (session.Character.Hp > hpLoad)
                    {
                        session.Character.Hp = hpLoad;
                        totalHpRegen++;
                    }
                    if (session.Character.Level > 0)
                    {
                        if (inv.ItemVNum == 1242 || inv.ItemVNum == 5582)
                        {
                            session.CurrentMapInstance?.Broadcast(session.Character.GenerateRc(hpLoad - session.Character.Hp));
                            session.Character.Hp = hpLoad;

                            if (hpLoad - session.Character.Hp > 0)
                            {
                                totalHpRegen++;
                            }
                        }
                        else if (inv.ItemVNum == 1243 || inv.ItemVNum == 5583)
                        {
                            session.Character.Mp = mpLoad;

                            if (mpLoad - session.Character.Mp > 0)
                            {
                                totalMpRegen++;
                            }
                        }
                        else if (inv.ItemVNum == 1244 || inv.ItemVNum == 5584)
                        {
                            session.CurrentMapInstance?.Broadcast(session.Character.GenerateRc(hpLoad - session.Character.Hp));
                            session.Character.Hp = hpLoad;
                            session.Character.Mp = mpLoad;

                            if (mpLoad - session.Character.Mp > 0 || hpLoad - session.Character.Hp > 0)
                            {
                                totalMpRegen++;
                                totalHpRegen++;
                            }
                        }
                    }
                    session.SendPacket(session.Character.GenerateStat());

                    foreach (Mate mate in session.Character.Mates.Where(s => s.IsTeamMember))
                    {
                        hpLoad = mate.MaxHp;
                        mpLoad = mate.MaxMp;
                        if (hpLoad - mate.Hp < Hp)
                        {
                            session.CurrentMapInstance?.Broadcast(mate.GenerateRc(hpLoad - mate.Hp));
                            totalHpRegen++;
                        }
                        else if (hpLoad - mate.Hp > Hp)
                        {
                            session.CurrentMapInstance?.Broadcast(mate.GenerateRc(Hp));
                            totalHpRegen++;
                        }

                        mate.Mp += Mp;
                        mate.Hp += Hp;
                        if (mate.Mp > mpLoad)
                        {
                            mate.Mp = mpLoad;
                        }

                        if (mate.Hp > hpLoad)
                        {
                            mate.Hp = hpLoad;
                        }

                        if (session.Character.Level > 0)
                        {
                            if (inv.ItemVNum == 1242 || inv.ItemVNum == 5582)
                            {
                                session.CurrentMapInstance?.Broadcast(
                                    mate.GenerateRc(hpLoad - mate.Hp));
                                mate.Hp = hpLoad;

                                if (hpLoad - mate.Hp > 0)
                                {
                                    totalHpRegen++;
                                }
                            }
                            else if (inv.ItemVNum == 1243 || inv.ItemVNum == 5583)
                            {
                                mate.Mp = mpLoad;

                                if (mpLoad - mate.Mp > 0)
                                {
                                    totalMpRegen++;
                                }
                            }
                            else if (inv.ItemVNum == 1244 || inv.ItemVNum == 5584)
                            {
                                session.CurrentMapInstance?.Broadcast(
                                    mate.GenerateRc(hpLoad - mate.Hp));
                                mate.Hp = hpLoad;
                                mate.Mp = mpLoad;

                                if (hpLoad - mate.Hp > 0 || mpLoad - mate.Mp > 0)
                                {
                                    totalMpRegen++;
                                    totalHpRegen++;
                                }
                            }
                        }

                        if (totalHpRegen > 0 || totalMpRegen > 0)
                        {
                            session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                        }

                        session.SendPacket(mate.GenerateStatInfo());
                    }

                    break;
                default:
                    Logger.Warn(string.Format(Language.Instance.GetMessageFromKey("NO_HANDLER_ITEM"), GetType(), VNum, Effect, EffectValue));
                    break;
            }
        }

        #endregion
    }
}