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
    public enum ShellWeaponEffectType : byte
    {
        DamageImproved = 1,
        PercentageTotalDamage = 2,
        MinorBleeding = 3,
        Bleeding = 4,
        HeavyBleeding = 5,
        Blackout = 6,
        Freeze = 7,
        DeadlyBlackout = 8,
        DamageIncreasedtothePlant = 9,
        DamageIncreasedtotheAnimal = 10,
        DamageIncreasedtotheEnemy = 11,
        DamageIncreasedtotheUnDead = 12,
        DamageincreasedtotheSmallMonster = 13,
        DamageincreasedtotheBigMonster = 14,
        CriticalChance = 15, //Except Staff
        CriticalDamage = 16, //Except Staff
        AntiMagicDisorder = 17, //Only Staff
        IncreasedFireProperties = 18,
        IncreasedWaterProperties = 19,
        IncreasedLightProperties = 20,
        IncreasedDarkProperties = 21,
        IncreasedElementalProperties = 22,
        ReducedMPConsume = 23,
        HPRecoveryForKilling = 24,
        MPRecoveryForKilling = 25,
        SlDamage = 26,
        SlDefence = 27,
        SlElement = 28,
        Slhp = 29,
        SlGlobal = 30,
        GainMoreGold = 31,
        GainMoreXP = 32,
        GainMoreCxp = 33,
        PercentageDamageInPvp = 34,
        ReducesPercentageEnemyDefenceInPvp = 35,
        ReducesEnemyFireResistanceInPvp = 36,
        ReducesEnemyWaterResistanceInPvp = 37,
        ReducesEnemyLightResistanceInPvp = 38,
        ReducesEnemyDarkResistanceInPvp = 39,
        ReducesEnemyAllResistancesInPvp = 40,
        NeverMissInPvp = 41,
        PvpDamageAt15Percent = 42,
        ReducesEnemyMpinPvp = 43,
        InspireFireResistanceWithPercentage = 44,
        InspireWaterResistanceWithPercentage = 45,
        InspireLightResistanceWithPercentage = 46,
        InspireDarkResistanceWithPercentage = 47,
        GainSpForKilling = 48,
        IncreasedPrecision = 49,
        IncreasedFocus = 50
    }
}