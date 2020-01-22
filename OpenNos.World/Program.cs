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
using OpenNos.ChatLog.Networking;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.DAL.EF.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using OpenNos.Handler.Packets.WorldPackets.BasicPackets;
using OpenNos.Handler.Packets.WorldPackets.BattlePackets;
using OpenNos.Handler.Packets.WorldPackets.BazaarPackets;
using OpenNos.Handler.Packets.WorldPackets.CommandPackets;
using OpenNos.Handler.Packets.WorldPackets.FamilyPackets;
using OpenNos.Handler.Packets.WorldPackets.InventoryPackets;
using OpenNos.Handler.Packets.WorldPackets.MatePackets;
using OpenNos.Handler.Packets.WorldPackets.MinilandPackets;
using OpenNos.Handler.Packets.WorldPackets.NpcPackets;
using OpenNos.Handler.Packets.WorldPackets.ScriptedInstancePackets;
using OpenNos.Handler.Packets.WorldPackets.UselessPackets;
using System.IO;
using System.Security.Authentication;
using OpenNos.Core.Cryptography;
using OpenNos.GameLog.LogHelper;
// ReSharper disable LocalizableElement
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
// ReSharper disable FormatStringProblem

namespace OpenNos.World
{
    public static class Program
    {
        #region Members

/*
        private static readonly ManualResetEvent _run = new ManualResetEvent(true);
*/

        private static EventHandler _exitHandler;

        private static bool _ignoreTelemetry;

#pragma warning disable 649, IDE0044, RCS1169
        private static bool _isDebug;
#pragma warning restore RCS1169, IDE0044, 649

        private static int _port;

        #endregion

        #region Delegates

        private delegate bool EventHandler(CtrlType sig);

        #endregion

        #region Enums

        private enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        #endregion

        #region Methods

        public static void Main(string[] args)
        {
#if DEBUG
            _isDebug = true;
            Thread.Sleep(1000);
#endif
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            Console.Title = $"OpenNos World Server{(_isDebug ? " Development Environment" : string.Empty)}";

            bool ignoreStartupMessages = false;
            _port = Convert.ToInt32(ConfigurationManager.AppSettings["WorldPort"]);
            int portArgIndex = Array.FindIndex(args, s => s == "--port");
            if (portArgIndex != -1
                && args.Length >= portArgIndex + 1
                && int.TryParse(args[portArgIndex + 1], out _port))
            {
                Console.WriteLine("Port override: " + _port);
            }
            foreach (string arg in args)
            {
                switch (arg)
                {
                    case "--nomsg":
                        ignoreStartupMessages = true;
                        break;

                    case "--notelemetry":
                        _ignoreTelemetry = true;
                        break;

                    case "--h":
                    case "--help":
                        Console.WriteLine("OpenNos Help v0.1");
                        Console.WriteLine("--port PORT_NUMBER - overrides the port of the server");
                        Console.WriteLine("--nomsg - disables the startup messages");
                        Console.WriteLine("--notelemetry - disables gathering of the crash data and sending to external server");
                        return;
                }
            }

            // initialize Logger
            Logger.InitializeLogger(LogManager.GetLogger(typeof(Program)));

            if (!ignoreStartupMessages)
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                string text = $"WORLD SERVER v{fileVersionInfo.ProductVersion}dev - PORT : {_port} by OpenNos Team";
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
            string authKey = ConfigurationManager.AppSettings["MasterAuthKey"];
            if (CommunicationServiceClient.Instance.Authenticate(authKey))
            {
                Logger.Info(Language.Instance.GetMessageFromKey("API_INITIALIZED"));
            }

            // initialize DB
            if (DataAccessHelper.Initialize())
            {
                // initialilize maps
                ServerManager.Instance.Initialize();
            }
            else
            {
                Console.ReadKey();
                return;
            }

            PacketFacility.Initialize(typeof(EntryPointPacket));
            PacketFacility.Initialize(typeof(UseSkillPacket));
            PacketFacility.Initialize(typeof(BazaarBuyPacket));
            PacketFacility.Initialize(typeof(CreateFamilyPacket));
            PacketFacility.Initialize(typeof(DeleteItemPacket));
            PacketFacility.Initialize(typeof(MateControlPacket));
            PacketFacility.Initialize(typeof(MinilandAddObjectPacket));
            PacketFacility.Initialize(typeof(BuyPacket));
            PacketFacility.Initialize(typeof(EscapePacket));
            PacketFacility.Initialize(typeof(CClosePacket));
            PacketFacility.Initialize(typeof(HelpPacket));

            try
            {
                _exitHandler += ExitHandler;
                AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
                if (CurrentOS.IsWindows)
                {
                    NativeMethods.SetConsoleCtrlHandler(_exitHandler, true);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("General Error", ex);
            }

            string ipAddress = ConfigurationManager.AppSettings["IPAddress"];
            portloop:
            try
            {
                // ReSharper disable once ObjectCreationAsStatement
                new NetworkManager<WorldCryptography>(ipAddress, _port, typeof(LoginCryptography), true);
            }
            catch (SocketException ex)
            {
                if (ex.ErrorCode == 10048)
                {
                    _port++;
                    Logger.Info("Port already in use! Incrementing...");
                    goto portloop;
                }
                Logger.Error("General Error", ex);
                Environment.Exit(ex.ErrorCode);
            }

            ServerManager.Instance.ServerGroup = ConfigurationManager.AppSettings["ServerGroup"];
            int sessionLimit = ConfigurationServiceClient.Instance.GetSlotCount();
            int? newChannelId = CommunicationServiceClient.Instance.RegisterWorldServer(new SerializableWorldServer(ServerManager.Instance.WorldId, ipAddress, _port, sessionLimit, ServerManager.Instance.ServerGroup));
            if (newChannelId.HasValue)
            {
                ServerManager.Instance.ChannelId = newChannelId.Value;
                MailServiceClient.Instance.Authenticate(authKey, ServerManager.Instance.WorldId);
                ConfigurationServiceClient.Instance.Authenticate(authKey, ServerManager.Instance.WorldId);
                ServerManager.Instance.Configuration = ConfigurationServiceClient.Instance.GetConfigurationObject();
                if (ServerManager.Instance.Configuration.UseChatLogService && !ChatLogServiceClient.Instance.Authenticate(ConfigurationManager.AppSettings["ChatLogKey"]))
                {
                    throw new AuthenticationException();
                }
                if (ServerManager.Instance.Configuration.UseGameLogService)
                {
                    if (!GameLogger.InitializeLogger(ConfigurationManager.AppSettings["GameLogKey"]))
                    {
                        throw new AuthenticationException();
                    }
                }
                else
                {
                    GameLogger.InitializeLogger(null);
                }
                ServerManager.Instance.MallApi = new GameObject.Helpers.MallAPIHelper(ServerManager.Instance.Configuration.MallBaseUrl);
                if (ServerManager.Instance.Configuration.EnableAutoRestart)
                {
                    Observable.Interval(TimeSpan.FromMinutes(1)).Subscribe(x => ServerManager.Instance.AutoRestartProcess());
                }
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[{DateTime.UtcNow.ToLongTimeString()}][MSG][Main]: Server running!");
                Console.ResetColor();
            }
            else
            {
                Logger.Error("Could not retrieve ChannelId from Web API.");
                Console.ReadKey();
            }
        }

        private static bool ExitHandler(CtrlType sig)
        {
            CommunicationServiceClient.Instance.UnregisterWorldServer(ServerManager.Instance.WorldId);
            ServerManager.Shout(string.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_SEC"), 5));
            ServerManager.Instance.SaveAll();
            Thread.Sleep(5000);
            return false;
        }

        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            ServerManager.Instance.InShutdown = true;
            Logger.Error((Exception)e.ExceptionObject);
            try
            {
                if (!_ignoreTelemetry)
                {
                    string guid = ((GuidAttribute)Assembly.GetAssembly(typeof(SCS.Communication.ScsServices.Service.ScsServiceBuilder)).GetCustomAttributes(typeof(GuidAttribute), true)[0]).Value;
                    System.Collections.Specialized.NameValueCollection reqparm = new System.Collections.Specialized.NameValueCollection
                    {
                        { "key", guid },
                        { "error", ((Exception)e.ExceptionObject).ToString() },
                        { "debug", _isDebug.ToString() }
                    };
                    WebClient wc = new WebClient();
                    byte[] responsebytes = wc.UploadValues("https://mgmt.opennos.io/Crash/ReportCrash", "POST", reqparm);
                    string[] response = Encoding.UTF8.GetString(responsebytes).Split(':');
                    if (response[0] != "saved")
                    {
                        Logger.Error(new Exception($"Unable to report crash to management Server. Please report this issue to the Developer: {response[0]}"));
                    }
                    wc.Dispose();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            Logger.Warn("Server crashed! Rebooting gracefully...");
            CommunicationServiceClient.Instance.UnregisterWorldServer(ServerManager.Instance.WorldId);
            ServerManager.Shout(string.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_SEC"), 5));
            ServerManager.Instance.SaveAll();
            if (ServerManager.Instance.ChannelId == 51)
            {
                File.WriteAllText("act4backup.bin", $"{ServerManager.Instance.Act4AngelStat?.Percentage??0}:{ServerManager.Instance.Act4DemonStat?.Percentage??0}");
            }
            Process.Start("OpenNos.Updater.exe");
            Environment.Exit(1);
        }

        #endregion

        #region Classes

        private static class NativeMethods
        {
            #region Methods

            [DllImport("Kernel32")]
            internal static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

            #endregion
        }

        #endregion
    }
}