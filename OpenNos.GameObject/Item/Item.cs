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
using OpenNos.DAL;
using OpenNos.Data;
using System.Collections.Generic;
using System.Linq;
using OpenNos.Domain;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject
{
    public abstract class Item : ItemDTO
    {
        #region Instantiation

        protected Item()
        {
        }

        protected Item(ItemDTO item) => InitializeItem(item);

        #endregion

        #region Properties

        public List<BCard> BCards { get; set; }

        public List<RollGeneratedItemDTO> RollGeneratedItems { get; set; }

        #endregion

        #region Methods

        public void InitializeItem(ItemDTO input)
        {
            BasicUpgrade = input.BasicUpgrade;
            CellonLvl = input.CellonLvl;
            Class = input.Class;
            CloseDefence = input.CloseDefence;
            Color = input.Color;
            Concentrate = input.Concentrate;
            CriticalLuckRate = input.CriticalLuckRate;
            CriticalRate = input.CriticalRate;
            DamageMaximum = input.DamageMaximum;
            DamageMinimum = input.DamageMinimum;
            DarkElement = input.DarkElement;
            DarkResistance = input.DarkResistance;
            DefenceDodge = input.DefenceDodge;
            DistanceDefence = input.DistanceDefence;
            DistanceDefenceDodge = input.DistanceDefenceDodge;
            Effect = input.Effect;
            EffectValue = input.EffectValue;
            Element = input.Element;
            ElementRate = input.ElementRate;
            EquipmentSlot = input.EquipmentSlot;
            FireElement = input.FireElement;
            FireResistance = input.FireResistance;
            Height = input.Height;
            HitRate = input.HitRate;
            Hp = input.Hp;
            HpRegeneration = input.HpRegeneration;
            IsBlocked = input.IsBlocked;
            IsColored = input.IsColored;
            IsConsumable = input.IsConsumable;
            IsDroppable = input.IsDroppable;
            IsHeroic = input.IsHeroic;
            IsHolder = input.IsHolder;
            IsMinilandObject = input.IsMinilandObject;
            IsSoldable = input.IsSoldable;
            IsTradable = input.IsTradable;
            ItemSubType = input.ItemSubType;
            ItemType = input.ItemType;
            ItemValidTime = input.ItemValidTime;
            LevelJobMinimum = input.LevelJobMinimum;
            LevelMinimum = input.LevelMinimum;
            LightElement = input.LightElement;
            LightResistance = input.LightResistance;
            MagicDefence = input.MagicDefence;
            MaxCellon = input.MaxCellon;
            MaxCellonLvl = input.MaxCellonLvl;
            MaxElementRate = input.MaxElementRate;
            MaximumAmmo = input.MaximumAmmo;
            MinilandObjectPoint = input.MinilandObjectPoint;
            MoreHp = input.MoreHp;
            MoreMp = input.MoreMp;
            Morph = input.Morph;
            Mp = input.Mp;
            MpRegeneration = input.MpRegeneration;
            Name = input.Name;
            Price = input.Price;
            PvpDefence = input.PvpDefence;
            PvpStrength = input.PvpStrength;
            ReduceOposantResistance = input.ReduceOposantResistance;
            ReputationMinimum = input.ReputationMinimum;
            ReputPrice = input.ReputPrice;
            SecondaryElement = input.SecondaryElement;
            Sex = input.Sex;
            Speed = input.Speed;
            SpType = input.SpType;
            Type = input.Type;
            VNum = input.VNum;
            WaitDelay = input.WaitDelay;
            WaterElement = input.WaterElement;
            WaterResistance = input.WaterResistance;
            Width = input.Width;
            SpecialistType = input.SpecialistType;
            BCards = new List<BCard>();
            DAOFactory.BCardDAO.LoadByItemVNum(input.VNum).ToList().ForEach(o => BCards.Add(new BCard(o)));
            RollGeneratedItems = DAOFactory.RollGeneratedItemDAO.LoadByItemVNum(input.VNum).ToList();
        }

        public abstract void Use(ClientSession session, ref ItemInstance inv, byte option = 0, string[] packetsplit = null);

        public bool OpenBoxItem(ClientSession session, ItemInstance box)
        {
            if (box != null)
            {
                List<RollGeneratedItemDTO> roll = box.Item.RollGeneratedItems.Where(s =>
                    s.MinimumOriginalItemRare <= box.Rare
                    && s.MaximumOriginalItemRare >= box.Rare
                    && s.OriginalItemDesign == box.Design).ToList();
                int probabilities = roll.Sum(s => s.Probability);
                int rnd = ServerManager.RandomNumber(0, probabilities);
                int currentrnd = 0;
                foreach (RollGeneratedItemDTO rollitem in roll)
                {
                    currentrnd += rollitem.Probability;
                    if (currentrnd >= rnd)
                    {
                        Item i = ServerManager.GetItem(rollitem.ItemGeneratedVNum);
                        sbyte rare = 0;
                        byte upgrade = 0;
                        if (i.ItemType == ItemType.Armor || i.ItemType == ItemType.Weapon
                            || i.ItemType == ItemType.Shell)
                        {
                            rare = box.Rare;
                        }

                        if (i.ItemType == ItemType.Shell)
                        {
                            upgrade = (byte)ServerManager.RandomNumber(75, 91);
                        }

                        session.Character.GiftAdd(rollitem.ItemGeneratedVNum, rollitem.ItemGeneratedAmount, rare,
                            upgrade);
                        session.SendPacket($"rdi {rollitem.ItemGeneratedVNum} {rollitem.ItemGeneratedAmount}");
                        session.Character.Inventory.RemoveItemFromInventory(box.Id);
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion
    }
}