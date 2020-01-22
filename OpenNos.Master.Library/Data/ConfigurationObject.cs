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
using System;

namespace OpenNos.Master.Library.Data
{
    [Serializable]
    public class ConfigurationObject
    {
        public string Act4IP { get; set; }

        public int Act4Port { get; set; }

        public int SessionLimit { get; set; }

        public bool SceneOnCreate { get; set; }

        public bool WorldInformation { get; set; }

        public int RateXP { get; set; }

        public int RateHeroicXP { get; set; }

        public int RateGold { get; set; }

        public int RateGoldDrop { get; set; }

        public long MaxGold { get; set; }

        public int RateDrop { get; set; }

        public byte MaxLevel { get; set; }

        public byte MaxJobLevel { get; set; }

        public byte MaxHeroLevel { get; set; }

        public byte HeroicStartLevel { get; set; }

        public byte MaxSpLevel { get; set; }

        public int RateFairyXP { get; set; }

        public byte MaxUpgrade { get; set; }

        public string MallBaseUrl { get; set; }

        public string MallApiKey { get; set; }

        public bool UseChatLogService { get; set; }

        public bool UseGameLogService { get; set; }

        public bool EnableAutoRestart { get; set; }

        public int AutoRestartHour { get; set; }
    }
}
