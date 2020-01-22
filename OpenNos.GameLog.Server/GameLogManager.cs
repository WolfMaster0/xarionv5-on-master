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
using OpenNos.GameLog.Shared;
using OpenNos.Core;
using OpenNos.Master.Library.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using OpenNos.Core.Threading;

namespace OpenNos.GameLog.Server
{
    internal class GameLogManager
    {
        #region Members

        private static GameLogManager _instance;

        private readonly LogFileReader _reader;

        #endregion

        #region Instantiation

        public GameLogManager()
        {
            _reader = new LogFileReader();
            AuthentificatedClients = new List<long>();
            GameLogs = new ThreadSafeGenericList<GameLogEntry>();
            AllGameLogs = new ThreadSafeGenericList<GameLogEntry>();
            RecursiveFileOpen("gamelogs");
            AuthentificationServiceClient.Instance.Authenticate(
                ConfigurationManager.AppSettings["AuthentificationServiceAuthKey"]);
            Observable.Interval(TimeSpan.FromMinutes(15)).Subscribe(_ => SaveGameLogs());
        }

        #endregion

        #region Properties

        public static GameLogManager Instance => _instance ?? (_instance = new GameLogManager());

        public List<long> AuthentificatedClients { get; set; }

        public ThreadSafeGenericList<GameLogEntry> GameLogs { get; set; }

        public ThreadSafeGenericList<GameLogEntry> AllGameLogs { get; set; }

        #endregion

        private void SaveGameLogs()
        {
            try
            {
                LogFileWriter writer = new LogFileWriter();
                Logger.Info(Language.Instance.GetMessageFromKey("SAVE_GAMELOGS"));
                List<GameLogEntry> tmp = GameLogs.GetAllItems();
                GameLogs.Clear();
                DateTime current = DateTime.UtcNow;

                string path = "gamelogs";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                path = Path.Combine(path, current.Year.ToString());
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                path = Path.Combine(path, current.Month.ToString());
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                path = Path.Combine(path, current.Day.ToString());
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                writer.WriteLogFile(
                    Path.Combine(path,
                        $"{(current.Hour < 10 ? $"0{current.Hour}" : $"{current.Hour}")}.{(current.Minute < 10 ? $"0{current.Minute}" : $"{current.Minute}")}.ong"),
                    tmp);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void RecursiveFileOpen(string dir)
        {
            try
            {
                foreach (string d in Directory.GetDirectories(dir))
                {
                    foreach (string s in Directory.GetFiles(d).Where(s => s.EndsWith(".ong")))
                    {
                        AllGameLogs.AddRange(_reader.ReadLogFile(s));
                    }

                    RecursiveFileOpen(d);
                }
            }
            catch
            {
                Logger.LogEventError("LogFileRead", "Something went wrong while opening Game Log Files. Exiting...");
                Environment.Exit(-1);
            }
        }
    }
}