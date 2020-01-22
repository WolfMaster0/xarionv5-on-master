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
using System;
using System.Reactive.Linq;
using System.Threading;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject
{
    public class SnackItem : Item
    {
        #region Instantiation

        public SnackItem(ItemDTO item) : base(item)
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
                    int amount = session.Character.SnackAmount;
                    if (amount < 5)
                    {
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
                        Thread workerThread2 = new Thread(() => Sync(session));
                        workerThread2.Start();
                    }
                    break;
            }
        }

        private static void Regenerate(ClientSession session, Item item)
        {
            session.SendPacket(Helpers.StaticPacketHelper.GenerateEff(UserType.Player, session.Character.CharacterId, 6000));
            session.Character.SnackAmount++;
            session.Character.MaxSnack = 0;
            session.Character.SnackHp += item.Hp / 5;
            session.Character.SnackMp += item.Mp / 5;
            Observable.Timer(TimeSpan.FromSeconds(9)).Subscribe(observer =>
            {
                session.Character.SnackHp -= item.Hp / 5;
                session.Character.SnackMp -= item.Mp / 5;
                session.Character.SnackAmount--;
            });
        }

        private static void Sync(ClientSession session)
        {
            for (session.Character.MaxSnack = 0; session.Character.MaxSnack < 5; session.Character.MaxSnack++)
            {
                Observable.Timer(TimeSpan.FromMilliseconds(1800 * session.Character.MaxFood)).Subscribe(observer =>
                {
                    if (session.Character.Hp <= 0)
                    {
                        return;
                    }
                    session.Character.Hp += session.Character.SnackHp;
                    session.Character.Mp += session.Character.SnackMp;
                    if (session.Character.Mp > session.Character.MPLoad())
                    {
                        session.Character.Mp = (int)session.Character.MPLoad();
                    }
                    if (session.Character.Hp > session.Character.HPLoad())
                    {
                        session.Character.Hp = (int)session.Character.HPLoad();
                    }
                    if (session.Character.Hp < session.Character.HPLoad() || session.Character.Mp < session.Character.MPLoad())
                    {
                        session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateRc(session.Character.SnackHp));
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