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
using OpenNos.Master.Library.Data;
using OpenNos.SCS.Communication.ScsServices.Service;
using System.Collections.Generic;
using System.Configuration;
using System.Reactive.Linq;
using OpenNos.Core.Threading;
using OpenNos.Data;
using OpenNos.DAL;

namespace OpenNos.Master.Server
{
    internal class MsManager
    {
        #region Members

        private static MsManager _instance;

        #endregion

        #region Instantiation

        public MsManager()
        {
            WorldServers = new List<WorldServer>();
            LoginServers = new List<IScsServiceClient>();
            ConnectedAccounts = new ThreadSafeGenericList<AccountConnection>();
            AuthentificatedClients = new List<long>();
            ConfigurationObject = new ConfigurationObject
            {
                RateXP = int.Parse(ConfigurationManager.AppSettings["RateXp"]),
                RateHeroicXP = int.Parse(ConfigurationManager.AppSettings["RateHeroicXp"]),
                RateDrop = int.Parse(ConfigurationManager.AppSettings["RateDrop"]),
                MaxGold = long.Parse(ConfigurationManager.AppSettings["MaxGold"]),
                RateGoldDrop = int.Parse(ConfigurationManager.AppSettings["GoldRateDrop"]),
                RateGold = int.Parse(ConfigurationManager.AppSettings["RateGold"]),
                RateFairyXP = int.Parse(ConfigurationManager.AppSettings["RateFairyXp"]),
                MaxLevel = byte.Parse(ConfigurationManager.AppSettings["MaxLevel"]),
                MaxJobLevel = byte.Parse(ConfigurationManager.AppSettings["MaxJobLevel"]),
                MaxSpLevel = byte.Parse(ConfigurationManager.AppSettings["MaxSPLevel"]),
                MaxHeroLevel = byte.Parse(ConfigurationManager.AppSettings["MaxHeroLevel"]),
                HeroicStartLevel = byte.Parse(ConfigurationManager.AppSettings["HeroicStartLevel"]),
                MaxUpgrade = byte.Parse(ConfigurationManager.AppSettings["MaxUpgrade"]),
                SceneOnCreate = bool.Parse(ConfigurationManager.AppSettings["SceneOnCreate"]),
                SessionLimit = int.Parse(ConfigurationManager.AppSettings["SessionLimit"]),
                WorldInformation = bool.Parse(ConfigurationManager.AppSettings["WorldInformation"]),
                Act4IP = ConfigurationManager.AppSettings["Act4IP"],
                Act4Port = int.Parse(ConfigurationManager.AppSettings["Act4Port"]),
                MallBaseUrl = ConfigurationManager.AppSettings["MallBaseURL"],
                MallApiKey = ConfigurationManager.AppSettings["MallAPIKey"],
                UseChatLogService = bool.Parse(ConfigurationManager.AppSettings["UseChatLogService"]),
                UseGameLogService = bool.Parse(ConfigurationManager.AppSettings["UseGameLogService"]),
                EnableAutoRestart = bool.Parse(ConfigurationManager.AppSettings["EnableAutoRestart"]),
                AutoRestartHour = byte.Parse(ConfigurationManager.AppSettings["AutoRestartHour"])
            };

            if (ConfigurationObject.EnableAutoRestart)
            {
                Observable.Interval(TimeSpan.FromMinutes(1)).Subscribe(observer => AutoMaintenanceProcess());
            }
        }

        #endregion

        #region Properties

        public static MsManager Instance => _instance ?? (_instance = new MsManager());

        public List<long> AuthentificatedClients { get; set; }

        public ConfigurationObject ConfigurationObject { get; set; }

        public ThreadSafeGenericList<AccountConnection> ConnectedAccounts { get; set; }

        public List<IScsServiceClient> LoginServers { get; set; }

        public List<WorldServer> WorldServers { get; set; }

        #endregion

        public void AutoMaintenanceProcess()
        {
            if (DateTime.UtcNow.Hour == ConfigurationObject.AutoRestartHour - 1 && DateTime.UtcNow.Minute == 50)
            {
                MaintenanceLogDTO maintenance = new MaintenanceLogDTO
                {
                    DateEnd = DateTime.UtcNow.AddMinutes(25),
                    DateStart = DateTime.UtcNow.AddMinutes(9),
                    Reason = "Daily restart and cleanup routines."
                };
                DAOFactory.MaintenanceLogDAO.Insert(maintenance);
            }
        }
    }
}