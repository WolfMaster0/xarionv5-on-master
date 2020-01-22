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
using System.Linq;
using System.Threading;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.BazaarPackets
{
    [PacketHeader("c_slist")]
    public class BazaarPersonalRefreshPacket
    {
        #region Properties

        public byte Filter { get; set; }

        public byte Index { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 4)
            {
                return;
            }
            BazaarPersonalRefreshPacket packetDefinition = new BazaarPersonalRefreshPacket();
            if (byte.TryParse(packetSplit[2], out byte index) && byte.TryParse(packetSplit[3], out byte filter))
            {
                packetDefinition.Index = index;
                packetDefinition.Filter = filter;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(BazaarPersonalRefreshPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.InBazaarRefreshMode);
            string list = string.Empty;
            BazaarItemLink[] billist = new BazaarItemLink[ServerManager.Instance.BazaarList.Count + 20];
            ServerManager.Instance.BazaarList.CopyTo(billist);
            foreach (BazaarItemLink bz in billist.Where(s => s != null && s.BazaarItem.SellerId == session.Character.CharacterId).Skip(Index * 50).Take(50))
            {
                if (bz.Item != null)
                {
                    int soldedAmount = bz.BazaarItem.Amount - bz.Item.Amount;
                    int amount = bz.BazaarItem.Amount;
                    bool package = bz.BazaarItem.IsPackage;
                    bool isNosbazar = bz.BazaarItem.MedalUsed;
                    long price = bz.BazaarItem.Price;
                    long minutesLeft = (long)(bz.BazaarItem.DateStart.AddHours(bz.BazaarItem.Duration) - DateTime.UtcNow).TotalMinutes;
                    byte status = minutesLeft >= 0 ? (soldedAmount < amount ? (byte)BazaarType.OnSale : (byte)BazaarType.Solded) : (byte)BazaarType.DelayExpired;
                    if (status == (byte)BazaarType.DelayExpired)
                    {
                        minutesLeft = (long)(bz.BazaarItem.DateStart.AddHours(bz.BazaarItem.Duration).AddDays(isNosbazar ? 30 : 7) - DateTime.UtcNow).TotalMinutes;
                    }
                    string info = string.Empty;
                    if (bz.Item.Item.Type == InventoryType.Equipment)
                    {
                        info = bz.Item?.GenerateEInfo().Replace(' ', '^').Replace("e_info^", string.Empty);
                    }
                    if (Filter == 0 || Filter == status)
                    {
                        list += $"{bz.BazaarItem.BazaarItemId}|{bz.BazaarItem.SellerId}|{bz.Item.ItemVNum}|{soldedAmount}|{amount}|{(package ? 1 : 0)}|{price}|{status}|{minutesLeft}|{(isNosbazar ? 1 : 0)}|0|{bz.Item.Rare}|{bz.Item.Upgrade}|{info} ";
                    }
                }
            }

            session.SendPacket($"rc_slist {Index} {list}");
        }

        #endregion
    }
}