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
using System;

namespace OpenNos.Data
{
    [Serializable]
    public class CharacterDTO
    {
        #region Properties

        public long AccountId { get; set; }

        public int Act4Dead { get; set; }

        public int Act4Kill { get; set; }

        public int Act4Points { get; set; }

        public int ArenaWinner { get; set; }

        public string Biography { get; set; }

        public bool BuffBlocked { get; set; }

        public long CharacterId { get; set; }

        public ClassType Class { get; set; }

        public short Compliment { get; set; }

        public float Dignity { get; set; }

        public bool EmoticonsBlocked { get; set; }

        public bool ExchangeBlocked { get; set; }

        public FactionType Faction { get; set; }

        public bool FamilyRequestBlocked { get; set; }

        public bool FriendRequestBlocked { get; set; }

        public GenderType Gender { get; set; }

        public long Gold { get; set; }

        public long GoldBank { get; set; }

        public bool GroupRequestBlocked { get; set; }

        public HairColorType HairColor { get; set; }

        public HairStyleType HairStyle { get; set; }

        public bool HeroChatBlocked { get; set; }

        public byte HeroLevel { get; set; }

        public long HeroXp { get; set; }

        public int Hp { get; set; }

        public bool HpBlocked { get; set; }

        public byte JobLevel { get; set; }

        public long JobLevelXp { get; set; }

        public long LastFamilyLeave { get; set; }

        public byte Level { get; set; }

        public long LevelXp { get; set; }

        public short MapId { get; set; }

        public short MapX { get; set; }

        public short MapY { get; set; }

        public int MasterPoints { get; set; }

        public int MasterTicket { get; set; }

        public byte MaxMateCount { get; set; }

        public byte MaxPartnerCount { get; set; }

        public bool MinilandInviteBlocked { get; set; }

        public string MinilandMessage { get; set; }

        public short MinilandPoint { get; set; }

        public MinilandState MinilandState { get; set; }

        public bool MouseAimLock { get; set; }

        public int Mp { get; set; }

        public string Name { get; set; }

        public bool QuickGetUp { get; set; }

        public long RagePoint { get; set; }

        public byte RaidGlacerusRuns { get; set; }

        public byte RaidDracoRuns { get; set; }

        public long Reputation { get; set; }

        public byte Slot { get; set; }

        public int SpAdditionPoint { get; set; }

        public int SpPoint { get; set; }

        public CharacterState State { get; set; }

        public int TalentLose { get; set; }

        public int TalentSurrender { get; set; }

        public int TalentWin { get; set; }

        public bool WhisperBlocked { get; set; }

        public bool IsPetAutoRelive { get; set; }

        public bool IsPartnerAutoRelive { get; set; }

        #endregion
    }
}