// This file is part of the OpenNos NosTale Emulator Project.
//
// This program is licensed under a deviated version of the Fair Source License, granting you a
// non-exclusive, non-transferable, royalty-free and fully-paid-up license, under all of the
// Licensor's copyright and patent rights, to use, copy, prepare derivative works of, publicly
// perform and display the Software, subject to the conditions found in the LICENSE file.
//
// THIS FILE IS PROVIDED "AS IS", WITHOUT WARRANTY OR CONDITION, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. THE AUTHORS HEREBY DISCLAIM ALL LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT
// OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE.
using OpenNos.DAL.DAO;
using OpenNos.DAL.Interface;

namespace OpenNos.DAL
{
    public static class DAOFactory
    {
        #region Members

        private static IAccountDAO _accountDAO;
        private static IBazaarItemDAO _bazaarItemDAO;
        private static IBCardDAO _bcardDAO;
        private static ICardDAO _cardDAO;
        private static ICellonOptionDAO _cellonoptionDAO;
        private static ICharacterDAO _characterDAO;
        private static ICharacterRelationDAO _characterRelationDAO;
        private static ICharacterSkillDAO _characterSkillDAO;
        private static IComboDAO _comboDAO;
        private static IDropDAO _dropDAO;
        private static IEventScriptDAO _eventScriptDAO;
        private static IFamilyCharacterDAO _familyCharacterDAO;
        private static IFamilyDAO _familyDAO;
        private static IFamilyLogDAO _familyLogDAO;
        private static IGeneralLogDAO _generalLogDAO;
        private static IItemDAO _itemDAO;
        private static IItemInstanceDAO _itemInstanceDAO;
        private static IMailDAO _mailDAO;
        private static IMaintenanceLogDAO _maintenanceLogDAO;
        private static IMapDAO _mapDAO;
        private static IMapMonsterDAO _mapMonsterDAO;
        private static IMapNpcDAO _mapNpcDAO;
        private static IMapTypeDAO _mapTypeDAO;
        private static IMapTypeMapDAO _mapTypeMapDAO;
        private static IMateDAO _mateDAO;
        private static IMinigameLogDAO _minigameLogDAO;
        private static IMinilandObjectDAO _minilandObjectDAO;
        private static INpcMonsterDAO _npcMonsterDAO;
        private static INpcMonsterSkillDAO _npcMonsterSkillDAO;
        private static IPenaltyLogDAO _penaltyLogDAO;
        private static IPortalDAO _portalDAO;
        private static IQuestDAO _questDAO;
        private static IQuestProgressDAO _questProgressDAO;
        private static IQuicklistEntryDAO _quicklistEntryDAO;
        private static IRecipeDAO _recipeDAO;
        private static IRecipeItemDAO _recipeItemDAO;
        private static IRecipeListDAO _recipeListDAO;
        private static IRespawnDAO _respawnDAO;
        private static IRespawnMapTypeDAO _respawnMapTypeDAO;
        private static IRollGeneratedItemDAO _rollGeneratedItemDAO;
        private static IScriptedInstanceDAO _scriptedInstanceDAO;
        private static IShellEffectDAO _shellEffectDAO;
        private static IShopDAO _shopDAO;
        private static IShopItemDAO _shopItemDAO;
        private static IShopSkillDAO _shopSkillDAO;
        private static ISkillDAO _skillDAO;
        private static IStaticBonusDAO _staticBonusDAO;
        private static IStaticBuffDAO _staticBuffDAO;
        private static ITeleporterDAO _teleporterDAO;
        private static IMultiAccountExceptionDAO _multiAccountExceptionDAO;
        private static IPartnerSkillsDAO _partnerSkillsDAO;

        #endregion

        #region Properties

        public static IPartnerSkillsDAO PartnerSkillsDAO => _partnerSkillsDAO ?? (_partnerSkillsDAO = new PartnerSkillsDAO());

        public static IAccountDAO AccountDAO => _accountDAO ?? (_accountDAO = new AccountDAO());

        public static IBazaarItemDAO BazaarItemDAO => _bazaarItemDAO ?? (_bazaarItemDAO = new BazaarItemDAO());

        public static IBCardDAO BCardDAO => _bcardDAO ?? (_bcardDAO = new BCardDAO());

        public static ICardDAO CardDAO => _cardDAO ?? (_cardDAO = new CardDAO());

        public static ICellonOptionDAO CellonOptionDAO => _cellonoptionDAO ?? (_cellonoptionDAO = new CellonOptionDAO());

        public static ICharacterDAO CharacterDAO => _characterDAO ?? (_characterDAO = new CharacterDAO());

        public static ICharacterRelationDAO CharacterRelationDAO => _characterRelationDAO ?? (_characterRelationDAO = new CharacterRelationDAO());

        public static ICharacterSkillDAO CharacterSkillDAO => _characterSkillDAO ?? (_characterSkillDAO = new CharacterSkillDAO());

        public static IComboDAO ComboDAO => _comboDAO ?? (_comboDAO = new ComboDAO());

        public static IDropDAO DropDAO => _dropDAO ?? (_dropDAO = new DropDAO());

        public static IEventScriptDAO EventScriptDAO => _eventScriptDAO ?? (_eventScriptDAO = new EventScriptDAO());

        public static IFamilyCharacterDAO FamilyCharacterDAO => _familyCharacterDAO ?? (_familyCharacterDAO = new FamilyCharacterDAO());

        public static IFamilyDAO FamilyDAO => _familyDAO ?? (_familyDAO = new FamilyDAO());

        public static IFamilyLogDAO FamilyLogDAO => _familyLogDAO ?? (_familyLogDAO = new FamilyLogDAO());

        public static IGeneralLogDAO GeneralLogDAO => _generalLogDAO ?? (_generalLogDAO = new GeneralLogDAO());

        public static IItemDAO ItemDAO => _itemDAO ?? (_itemDAO = new ItemDAO());

        public static IItemInstanceDAO ItemInstanceDAO => _itemInstanceDAO ?? (_itemInstanceDAO = new ItemInstanceDAO());

        public static IMailDAO MailDAO => _mailDAO ?? (_mailDAO = new MailDAO());

        public static IMaintenanceLogDAO MaintenanceLogDAO => _maintenanceLogDAO ?? (_maintenanceLogDAO = new MaintenanceLogDAO());

        public static IMapDAO MapDAO => _mapDAO ?? (_mapDAO = new MapDAO());

        public static IMapMonsterDAO MapMonsterDAO => _mapMonsterDAO ?? (_mapMonsterDAO = new MapMonsterDAO());

        public static IMapNpcDAO MapNpcDAO => _mapNpcDAO ?? (_mapNpcDAO = new MapNpcDAO());

        public static IMapTypeDAO MapTypeDAO => _mapTypeDAO ?? (_mapTypeDAO = new MapTypeDAO());

        public static IMapTypeMapDAO MapTypeMapDAO => _mapTypeMapDAO ?? (_mapTypeMapDAO = new MapTypeMapDAO());

        public static IMateDAO MateDAO => _mateDAO ?? (_mateDAO = new MateDAO());

        public static IMinigameLogDAO MinigameLogDAO => _minigameLogDAO ?? (_minigameLogDAO = new MinigameLogDAO());

        public static IMinilandObjectDAO MinilandObjectDAO => _minilandObjectDAO ?? (_minilandObjectDAO = new MinilandObjectDAO());

        public static INpcMonsterDAO NpcMonsterDAO => _npcMonsterDAO ?? (_npcMonsterDAO = new NpcMonsterDAO());

        public static INpcMonsterSkillDAO NpcMonsterSkillDAO => _npcMonsterSkillDAO ?? (_npcMonsterSkillDAO = new NpcMonsterSkillDAO());

        public static IPenaltyLogDAO PenaltyLogDAO => _penaltyLogDAO ?? (_penaltyLogDAO = new PenaltyLogDAO());

        public static IPortalDAO PortalDAO => _portalDAO ?? (_portalDAO = new PortalDAO());

        public static IQuestDAO QuestDAO => _questDAO ?? (_questDAO = new QuestDAO());

        public static IQuestProgressDAO QuestProgressDAO => _questProgressDAO ?? (_questProgressDAO = new QuestProgressDAO());

        public static IQuicklistEntryDAO QuicklistEntryDAO => _quicklistEntryDAO ?? (_quicklistEntryDAO = new QuicklistEntryDAO());

        public static IRecipeDAO RecipeDAO => _recipeDAO ?? (_recipeDAO = new RecipeDAO());

        public static IRecipeItemDAO RecipeItemDAO => _recipeItemDAO ?? (_recipeItemDAO = new RecipeItemDAO());

        public static IRecipeListDAO RecipeListDAO => _recipeListDAO ?? (_recipeListDAO = new RecipeListDAO());

        public static IRespawnDAO RespawnDAO => _respawnDAO ?? (_respawnDAO = new RespawnDAO());

        public static IRespawnMapTypeDAO RespawnMapTypeDAO => _respawnMapTypeDAO ?? (_respawnMapTypeDAO = new RespawnMapTypeDAO());

        public static IRollGeneratedItemDAO RollGeneratedItemDAO => _rollGeneratedItemDAO ?? (_rollGeneratedItemDAO = new RollGeneratedItemDAO());

        public static IScriptedInstanceDAO ScriptedInstanceDAO => _scriptedInstanceDAO ?? (_scriptedInstanceDAO = new ScriptedInstanceDAO());

        public static IShellEffectDAO ShellEffectDAO => _shellEffectDAO ?? (_shellEffectDAO = new ShellEffectDAO());

        public static IShopDAO ShopDAO => _shopDAO ?? (_shopDAO = new ShopDAO());

        public static IShopItemDAO ShopItemDAO => _shopItemDAO ?? (_shopItemDAO = new ShopItemDAO());

        public static IShopSkillDAO ShopSkillDAO => _shopSkillDAO ?? (_shopSkillDAO = new ShopSkillDAO());

        public static ISkillDAO SkillDAO => _skillDAO ?? (_skillDAO = new SkillDAO());

        public static IStaticBonusDAO StaticBonusDAO => _staticBonusDAO ?? (_staticBonusDAO = new StaticBonusDAO());

        public static IStaticBuffDAO StaticBuffDAO => _staticBuffDAO ?? (_staticBuffDAO = new StaticBuffDAO());

        public static ITeleporterDAO TeleporterDAO => _teleporterDAO ?? (_teleporterDAO = new TeleporterDAO());

        public static IMultiAccountExceptionDAO MultiAccountExceptionDAO => _multiAccountExceptionDAO ?? (_multiAccountExceptionDAO = new MultiAccountExceptionDAO());

        #endregion
    }
}