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

namespace OpenNos.GameObject.Helpers
{
    public static class StaticPacketHelper
    {
        #region Methods

        public static string Cancel(byte type = 0, long callerId = 0) => $"cancel {type} {callerId}";

        public static string CastOnTarget(UserType type, long callerId, byte secondaryType, long targetId, short castAnimation, short castEffect, short skillVNum) => $"ct {(byte)type} {callerId} {secondaryType} {targetId} {castAnimation} {castEffect} {skillVNum}";

        public static string GenerateEff(UserType effectType, long callerId, int effectId) => $"eff {(byte)effectType} {callerId} {effectId}";

        public static string GenerateEffT(UserType effectType, long callerId, long targetId, int effectId) => $"eff_t {(byte)effectType} {callerId} 1 {targetId} {effectId}";

        public static string In(UserType type, short callerVNum, long callerId, short mapX, short mapY, int direction, int currentHp, int currentMp, short dialog, InRespawnType respawnType, bool isSitting)
        {
            switch (type)
            {
                case UserType.Npc:
                case UserType.Monster:
                    return $"in {(byte)type} {callerVNum} {callerId} {mapX} {mapY} {direction} {currentHp} {currentMp} {dialog} 0 0 -1 {(byte)respawnType} {(isSitting ? 1 : 0)} -1 - 0 -1 0 0 0 0 0 0 0 0";

                case UserType.Object:
                    return $"in 9 {callerVNum} {callerId} {mapX} {mapY} {direction} 0 0 -1";

                default:
                    return string.Empty;
            }
        }

        public static string Move(UserType type, long callerId, short positionX, short positionY, byte speed) => $"mv {(byte)type} {callerId} {positionX} {positionY} {speed}";

        public static string Out(UserType type, long callerId) => $"out {(byte)type} {callerId}";

        public static string Say(UserType type, long callerId, byte secondaryType, string message) => $"say {(byte)type} {callerId} {secondaryType} {message}";

        public static string SkillReset(int castId) => $"sr {castId}";

        public static string SkillResetWithCoolDown(int castId, int coolDown) => $"sr -10 {castId} {coolDown}";

        public static string SkillUsed(UserType type, long callerId, byte secondaryType, long targetId, short skillVNum,
            short cooldown, short attackAnimation, short skillEffect, short x, short y, bool isAlive, int health,
            int damage, int hitmode, byte skillType) =>
            $"su {(byte) type} {callerId} {secondaryType} {targetId} {skillVNum} {cooldown} {attackAnimation} {skillEffect} {x} {y} {(isAlive ? 1 : 0)} {health} {(hitmode == 1 ? 0 : hitmode == 4 ? 0 : damage)} {hitmode} {skillType}";

        #endregion
    }
}