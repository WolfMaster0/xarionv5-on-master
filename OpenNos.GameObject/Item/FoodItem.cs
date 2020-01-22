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
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using System;
using System.Reactive.Linq;
using System.Threading;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject
{
    public class FoodItem : Item
    {
        #region Instantiation

        public FoodItem(ItemDTO item) : base(item)
        {
        }

        #endregion

        #region Methods

        public override void Use(ClientSession session, ref ItemInstance inv, byte option = 0, string[] packetsplit = null)
        {
            if ((DateTime.UtcNow - session.Character.LastPotion).TotalMilliseconds < 750)
            {
                return;
            }
            session.Character.LastPotion = DateTime.UtcNow;
            Item item = inv.Item;
            switch (Effect)
            {
                default:
                    if (session.Character.Hp <= 0)
                    {
                        return;
                    }
                    if (!session.Character.IsSitting)
                    {
                        session.Character.Rest();
                    }
                    int amount = session.Character.FoodAmount;
                    if (amount < 5)
                    {
                        if (!session.Character.IsSitting)
                        {
                            return;
                        }
                        Thread workerThread = new Thread(() => Regenerate(session, item));
                        workerThread.Start();
                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                    }
                    else
                    {
                        session.SendPacket(session.Character.Gender == GenderType.Female
                            ? session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_HUNGRY_FEMALE"), 1)
                            : session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_HUNGRY_MALE"), 1));
                    }
                    if (amount == 0)
                    {
                        if (!session.Character.IsSitting)
                        {
                            return;
                        }
                        Thread workerThread2 = new Thread(() => Sync(session));
                        workerThread2.Start();
                    }
                    break;
            }
        }

        private static void Regenerate(ClientSession session, Item item)
        {
            session.SendPacket(StaticPacketHelper.GenerateEff(UserType.Player, session.Character.CharacterId, 6000));
            session.Character.FoodAmount++;
            session.Character.MaxFood = 0;
            session.Character.FoodHp += item.Hp / 5;
            session.Character.FoodMp += item.Mp / 5;
            Observable.Timer(TimeSpan.FromSeconds(9)).Subscribe(observer =>
            {
                session.Character.FoodHp = item.Hp / 5;
                session.Character.FoodMp = item.Mp / 5;
                session.Character.FoodAmount--;
            });
        }

        private static void Sync(ClientSession session)
        {
            for (session.Character.MaxFood = 0; session.Character.MaxFood < 5; session.Character.MaxFood++)
            {
                Observable.Timer(TimeSpan.FromMilliseconds(1800 * session.Character.MaxFood)).Subscribe(observer =>
                {
                    if (session.Character.Hp <= 0 || !session.Character.IsSitting)
                    {
                        session.Character.FoodAmount = 0;
                        session.Character.FoodHp = 0;
                        session.Character.FoodMp = 0;
                        return;
                    }

                    session.Character.Hp += session.Character.FoodHp;
                    session.Character.Mp += session.Character.FoodMp;
                    if (session.Character.FoodHp > 0 && session.Character.FoodHp > 0
                        && (session.Character.Hp < session.Character.HPLoad()
                         || session.Character.Mp < session.Character.MPLoad()))
                    {
                        session.CurrentMapInstance?.Broadcast(session,
                            session.Character.GenerateRc(session.Character.FoodHp));
                    }

                    if (session.IsConnected)
                    {
                        session.SendPacket(session.Character.GenerateStat());
                    }
                });
            }
        }

        #endregion
    }
}