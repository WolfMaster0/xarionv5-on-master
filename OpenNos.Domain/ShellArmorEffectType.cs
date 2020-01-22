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
    public enum ShellArmorEffectType : byte
    {
        CloseDefence = 1,
        DistanceDefence = 2,
        MagicDefence = 3,
        PercentageTotalDefence = 4,
        ReducedMinorBleeding = 5,
        ReducedBleedingAndMinorBleeding = 6,
        ReducedAllBleedingType = 7,
        ReducedStun = 8,
        ReducedAllStun = 9,
        ReducedParalysis = 10,
        ReducedFreeze = 11,
        ReducedBlind = 12,
        ReducedSlow = 13,
        ReducedArmorDeBuff = 14,
        ReducedShock = 15,
        ReducedPoisonParalysis = 16,
        ReducedAllNegativeEffect = 17,
        RecoveryHPOnRest = 18,
        RecoveryHP = 19,
        RecoveryMPOnRest = 20,
        RecoveryMP = 21,
        RecoveryHPInDefence = 22,
        ReducedCritChanceRecive = 23,
        IncreasedFireResistence = 24,
        IncreasedWaterResistence = 25,
        IncreasedLightResistence = 26,
        IncreasedDarkResistence = 27,
        IncreasedAllResistence = 28,
        ReducedPrideLoss = 29,
        ReducedProductionPointConsumed = 30,
        IncreasedProductionPossibility = 31,
        IncreasedRecoveryItemSpeed = 32,
        PercentageAllPvpDefence = 33,
        CloseDefenceDodgeInPvp = 34,
        DistanceDefenceDodgeInPvp = 35,
        IgnoreMagicDamage = 36,
        DodgeAllAttacksInPvp = 37,
        ProtectMpinPvp = 38,
        FireDamageImmuneInPvp = 39,
        WaterDamageImmuneInPvp = 40,
        LightDamageImmuneInPvp = 41,
        DarkDamageImmuneInPvp = 42,
    }
}