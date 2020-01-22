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

namespace OpenNos.GameObject
{
    public class FamilyCharacter : FamilyCharacterDTO
    {
        #region Members

        private CharacterDTO _character;

        #endregion

        #region Instantiation

        public FamilyCharacter()
        {

        }

        public FamilyCharacter(FamilyCharacterDTO input)
        {
            Authority = input.Authority;
            CharacterId = input.CharacterId;
            DailyMessage = input.DailyMessage;
            Experience = input.Experience;
            FamilyCharacterId = input.FamilyCharacterId;
            FamilyId = input.FamilyId;
            Rank = input.Rank;
        }

        #endregion

        #region Properties

        public CharacterDTO Character => _character ?? (_character = DAOFactory.CharacterDAO.LoadById(CharacterId));

        #endregion
    }
}