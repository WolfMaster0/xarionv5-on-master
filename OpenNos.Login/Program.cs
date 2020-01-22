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
using log4net;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.DAL.EF.Helpers;
using OpenNos.Handler.Packets.LoginPackets;
using OpenNos.Master.Library.Client;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Reactive.Linq;
using System.Reflection;
using OpenNos.Core.Cryptography;
using OpenNos.GameObject.Networking;

// ReSharper disable FormatStringProblem

namespace OpenNos.Login
{
    public static class Program
    {
        #region Members

#pragma warning disable 649, RCS1169, IDE0044
        private static bool _isDebug;
#pragma warning restore IDE0044, RCS1169, 649

        #endregion

        #region Methods

        public static void Main(string[] args)
        {
            checked
            {
                try
                {
#if DEBUG
                    _isDebug = true;
#endif
                    CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");
                    Console.Title = $"OpenNos Login Server{(_isDebug ? " Development Environment" : string.Empty)}";

                    bool ignoreStartupMessages = false;
                    foreach (string arg in args)
                    {
                        ignoreStartupMessages |= arg == "--nomsg";
                    }

                    // initialize Logger
                    Logger.InitializeLogger(LogManager.GetLogger(typeof(Program)));

                    int port = Convert.ToInt32(ConfigurationManager.AppSettings["LoginPort"]);
                    if (!ignoreStartupMessages)
                    {
                        Assembly assembly = Assembly.GetExecutingAssembly();
                        FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                        string text = $"LOGIN SERVER v{fileVersionInfo.ProductVersion}dev - PORT : {port} by OpenNos Team";
                        string text2 =
                            $"Built on: {new DateTime(((fileVersionInfo.ProductBuildPart - 1) * TimeSpan.TicksPerDay) + (fileVersionInfo.ProductPrivatePart * TimeSpan.TicksPerSecond * 2)).AddYears(1999)}";
                        string text3 = $"Built by: {BuildInfo.BuildUser}@{BuildInfo.BuildHost} ({BuildInfo.BuildOS})";
                        int offset = (Console.WindowWidth / 2) + (text.Length / 2);
                        int offset2 = (Console.WindowWidth / 2) + (text2.Length / 2);
                        int offset3 = (Console.WindowWidth / 2) + (text3.Length / 2);
                        string separator = new string('=', Console.WindowWidth);
                        Console.WriteLine(separator + string.Format("{0," + offset + "}\n", text) +
                                          string.Format("{0," + offset2 + "}\n", text2) +
                                          string.Format("{0," + offset3 + "}\n", text3) + separator);
                    }

                    // initialize api
                    if (CommunicationServiceClient.Instance.Authenticate(ConfigurationManager.AppSettings["MasterAuthKey"]))
                    {
                        Logger.Info(Language.Instance.GetMessageFromKey("API_INITIALIZED"));
                    }

                    // initialize DB
                    if (!DataAccessHelper.Initialize())
                    {
                        Console.ReadKey();
                        return;
                    }
                    Logger.Info(Language.Instance.GetMessageFromKey("CONFIG_LOADED"));

                    Observable.Interval(TimeSpan.FromHours(24)).Subscribe(x => RestartProcess());

                    try
                    {
                        // initialize PacketSerialization
                        PacketFacility.Initialize(typeof(LoginPacket));

                        // ReSharper disable once ObjectCreationAsStatement
                        new NetworkManager<LoginCryptography>(ConfigurationManager.AppSettings["IPAddress"], port, typeof(LoginCryptography), false);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogEventError("INITIALIZATION_EXCEPTION", "General Error Server", ex);
                    }
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[{DateTime.UtcNow.ToLongTimeString()}][MSG][Main]: Server running!");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Logger.LogEventError("INITIALIZATION_EXCEPTION", "General Error", ex);
                    Console.ReadKey();
                }
            }
        }

        private static void RestartProcess()
        {
            Process.Start("OpenNos.Login.exe", "--nomsg");
            Environment.Exit(1);
        }

        #endregion
    }
}