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
using OpenNos.DAL.EF.Helpers;
using OpenNos.Master.Library.Data;
using OpenNos.Master.Library.Interface;
using OpenNos.SCS.Communication.Scs.Communication.EndPoints.Tcp;
using OpenNos.SCS.Communication.ScsServices.Service;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
// ReSharper disable FormatStringProblem

namespace OpenNos.Master.Server
{
    internal static class Program
    {
        #region Members

        /*
                private static readonly ManualResetEvent Run = new ManualResetEvent(true);
        */

#pragma warning disable 649, IDE0044, RCS1169
        private static bool _isDebug;
#pragma warning restore RCS1169, IDE0044, 649

        #endregion

        #region Methods

        public static void Main(string[] args)
        {
            try
            {
#if DEBUG
                _isDebug = true;
#endif
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");
                Console.Title = $"OpenNos Master Server{(_isDebug ? " Development Environment" : string.Empty)}";

                bool ignoreStartupMessages = false;
                bool ignoreTelemetry = false;
                foreach (string arg in args)
                {
                    switch (arg)
                    {
                        case "--nomsg":
                            ignoreStartupMessages = true;
                            break;

                        case "--notelemetry":
                            ignoreTelemetry = true;
                            break;
                    }
                }

                // initialize Logger
                Logger.InitializeLogger(LogManager.GetLogger(typeof(Program)));

                int port = Convert.ToInt32(ConfigurationManager.AppSettings["MasterPort"]);
                if (!ignoreStartupMessages)
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                    string text = $"MASTER SERVER v{fileVersionInfo.ProductVersion}dev - PORT : {port} by OpenNos Team";
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

                // initialize DB
                if (!DataAccessHelper.Initialize())
                {
                    Console.ReadLine();
                    return;
                }

                Logger.Info(Language.Instance.GetMessageFromKey("CONFIG_LOADED"));

                try
                {
                    // configure Services and Service Host
                    string ipAddress = ConfigurationManager.AppSettings["MasterIP"];
                    IScsServiceApplication server = ScsServiceBuilder.CreateService(new ScsTcpEndPoint(ipAddress, port));

                    server.AddService<ICommunicationService, CommunicationService>(new CommunicationService());
                    server.AddService<IConfigurationService, ConfigurationService>(new ConfigurationService());
                    server.AddService<IMailService, MailService>(new MailService());
                    server.AddService<IMallService, MallService>(new MallService());
                    server.AddService<IAuthentificationService, AuthentificationService>(new AuthentificationService());
                    server.ClientConnected += OnClientConnected;
                    server.ClientDisconnected += OnClientDisconnected;

                    server.Start();
                    Logger.Info(Language.Instance.GetMessageFromKey("STARTED"));
                    if (!ignoreTelemetry)
                    {
                        string guid = ((GuidAttribute)Assembly.GetAssembly(typeof(ScsServiceBuilder)).GetCustomAttributes(typeof(GuidAttribute), true)[0]).Value;
                        Observable.Interval(TimeSpan.FromMinutes(5)).Subscribe(observer =>
                        {
                            try
                            {
                                WebClient wc = new WebClient();
                                foreach (WorldServer world in MsManager.Instance.WorldServers)
                                {
                                    System.Collections.Specialized.NameValueCollection reqparm = new System.Collections.Specialized.NameValueCollection
                                    {
                                        { "key", guid },
                                        { "ip", world.Endpoint.IpAddress },
                                        { nameof(port), world.Endpoint.TcpPort.ToString() },
                                        { "server", world.WorldGroup },
                                        { "channel", world.ChannelId.ToString() },
                                        { "userCount", MsManager.Instance.ConnectedAccounts.CountLinq(c => c.ConnectedWorld?.Id == world.Id).ToString() }
                                    };
                                    byte[] responsebytes = wc.UploadValues("https://mgmt.opennos.io/Statistics/SendStat", "POST", reqparm);
                                    string[] resp = Encoding.UTF8.GetString(responsebytes).Split(':');
                                    if (resp[0] != "saved")
                                    {
                                        Logger.Error(new Exception($"Unable to send statistics to management Server. Please report this issue to the Developer: {resp[0]}"));
                                    }
                                }
                                wc.Dispose();
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(new Exception($"Unable to send statistics to management Server. Please report this issue to the Developer: {ex.Message}"));
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("General Error Server", ex);
                }
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[{DateTime.UtcNow.ToLongTimeString()}][MSG][Main]: Server running!");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Logger.Error("General Error", ex);
                Console.ReadKey();
            }
        }

        private static void OnClientConnected(object sender, ServiceClientEventArgs e) => Logger.Info(Language.Instance.GetMessageFromKey("NEW_CONNECT") + e.Client.ClientId);

        private static void OnClientDisconnected(object sender, ServiceClientEventArgs e) => Logger.Info(Language.Instance.GetMessageFromKey("DISCONNECT") + e.Client.ClientId);

        #endregion
    }
}