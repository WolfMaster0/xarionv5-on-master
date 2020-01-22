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
namespace OpenNos.Domain
{
    public enum FamilyLogType : byte
    {
        DailyMessage = 1,
        RaidWon = 2,
        RainbowBattle = 3,
        FamilyXP = 4,
        FamilyLevelUp = 5,
        LevelUp = 6,
        ItemUpgraded = 7,
        RightChanged = 8,
        AuthorityChanged = 9,
        FamilyManaged = 10,
        UserManaged = 11,
        WareHouseAdded = 12,
        WareHouseRemoved = 13
    }
}