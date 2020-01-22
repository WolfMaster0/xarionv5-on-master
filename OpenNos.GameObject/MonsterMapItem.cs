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
using OpenNos.Domain;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject
{
    public class MonsterMapItem : MapItem
    {
        #region Instantiation

        public MonsterMapItem(short x, short y, short itemVNum, int amount = 1, long ownerId = -1, bool isQuestItem = false) : base(x, y)
        {
            ItemVNum = itemVNum;
            if (amount < 99)
            {
                Amount = (byte)amount;
            }
            GoldAmount = amount;
            OwnerId = ownerId;
            IsQuestItem = isQuestItem;
        }

        #endregion

        #region Properties

        public sealed override byte Amount { get; set; }

        public int GoldAmount { get; }

        public sealed override short ItemVNum { get; set; }

        public long? OwnerId { get; }

        public bool IsQuestItem { get; }

        #endregion

        #region Methods

        public override ItemInstance GetItemInstance()
        {
            if (MapItemInstance == null && OwnerId != null)
            {
                MapItemInstance = Inventory.InstantiateItemInstance(ItemVNum, OwnerId.Value, Amount);
            }
            return MapItemInstance;
        }

        public void Rarify(ClientSession session)
        {
            ItemInstance instance = GetItemInstance();
            if (instance.Item.Type == InventoryType.Equipment && (instance.Item.ItemType == ItemType.Weapon || instance.Item.ItemType == ItemType.Armor))
            {
                instance.RarifyItem(session, RarifyMode.Drop, RarifyProtection.None);
            }
        }

        #endregion
    }
}