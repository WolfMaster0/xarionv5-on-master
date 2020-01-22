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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenNos.DAL.EF
{
    /// <summary>
    /// Do Not forget to change Mapping in Item GO when changing this class
    /// </summary>
    public sealed class Item
    {
        #region Instantiation

        public Item()
        {
            Drop = new HashSet<Drop>();
            Recipe = new HashSet<Recipe>();
            Mail = new HashSet<Mail>();
            RollGeneratedItem = new HashSet<RollGeneratedItem>();
            RollGeneratedItem2 = new HashSet<RollGeneratedItem>();
            RecipeItem = new HashSet<RecipeItem>();
            RecipeList = new HashSet<RecipeList>();
            ShopItem = new HashSet<ShopItem>();
            BCards = new HashSet<BCard>();
        }

        #endregion

        #region Properties

        public byte BasicUpgrade { get; set; }

        public ICollection<BCard> BCards { get; }

        public byte CellonLvl { get; set; }

        public byte Class { get; set; }

        public short CloseDefence { get; set; }

        public byte Color { get; set; }

        public short Concentrate { get; set; }

        public byte CriticalLuckRate { get; set; }

        public short CriticalRate { get; set; }

        public short DamageMaximum { get; set; }

        public short DamageMinimum { get; set; }

        public byte DarkElement { get; set; }

        public short DarkResistance { get; set; }

        public short DefenceDodge { get; set; }

        public short DistanceDefence { get; set; }

        public short DistanceDefenceDodge { get; set; }

        public ICollection<Drop> Drop { get; }

        public short Effect { get; set; }

        public int EffectValue { get; set; }

        public byte Element { get; set; }

        public short ElementRate { get; set; }

        public EquipmentType EquipmentSlot { get; set; }

        public byte FireElement { get; set; }

        public short FireResistance { get; set; }

        public byte Height { get; set; }

        public short HitRate { get; set; }

        public short Hp { get; set; }

        public short HpRegeneration { get; set; }

        public bool IsBlocked { get; set; }

        public bool IsColored { get; set; }

        public bool IsConsumable { get; set; }

        public bool IsDroppable { get; set; }

        public bool IsHeroic { get; set; }

        public bool IsHolder { get; set; }

        public bool IsMinilandObject { get; set; }

        public bool IsSoldable { get; set; }

        public bool IsTradable { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ICollection<ItemInstance> ItemInstances { get; set; }

        public byte ItemSubType { get; set; }

        public byte ItemType { get; set; }

        public long ItemValidTime { get; set; }

        public byte LevelJobMinimum { get; set; }

        public byte LevelMinimum { get; set; }

        public byte LightElement { get; set; }

        public short LightResistance { get; set; }

        public short MagicDefence { get; set; }

        public ICollection<Mail> Mail { get; }

        public byte MaxCellon { get; set; }

        public byte MaxCellonLvl { get; set; }

        public short MaxElementRate { get; set; }

        public byte MaximumAmmo { get; set; }

        public int MinilandObjectPoint { get; set; }

        public short MoreHp { get; set; }

        public short MoreMp { get; set; }

        public short Morph { get; set; }

        public short Mp { get; set; }

        public short MpRegeneration { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        public long Price { get; set; }

        public short PvpDefence { get; set; }

        public byte PvpStrength { get; set; }

        public ICollection<Recipe> Recipe { get; set; }

        public ICollection<RecipeItem> RecipeItem { get; set; }

        public ICollection<RecipeList> RecipeList { get; set; }

        public short ReduceOposantResistance { get; set; }

        public byte ReputationMinimum { get; set; }

        public long ReputPrice { get; set; }

        public ICollection<RollGeneratedItem> RollGeneratedItem { get; set; }

        public ICollection<RollGeneratedItem> RollGeneratedItem2 { get; set; }

        public byte SecondaryElement { get; set; }

        public byte Sex { get; set; }

        public ICollection<ShopItem> ShopItem { get; set; }

        public byte Speed { get; set; }

        public byte SpType { get; set; }

        public byte Type { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short VNum { get; set; }

        public short WaitDelay { get; set; }

        public byte WaterElement { get; set; }

        public short WaterResistance { get; set; }

        public byte Width { get; set; }

        public PartnerSpecialistType SpecialistType { get; set; }

        #endregion
    }
}