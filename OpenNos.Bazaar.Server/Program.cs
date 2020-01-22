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
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.ScsServices.Service;
using log4net;
using OpenNos.Bazaar.Server.Networking;
using OpenNos.Core;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
// ReSharper disable FormatStringProblem

namespace OpenNos.Bazaar.Server
{
    internal static class Program
    {
        #region Members

        /*
                private static readonly ManualResetEvent Run = new ManualResetEvent(true);
        */

#pragma warning disable 649, RCS1169, IDE0044
        private static bool _isDebug;
#pragma warning restore IDE0044, RCS1169, 649

        #endregion

        #region Methods

        internal static void Main(string[] args)
        {
            try
            {
#if DEBUG
                _isDebug = true;
#endif
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");
                Console.Title = $"OpenNos Bazaar Server{(_isDebug ? " Development Environment" : string.Empty)}";

                bool ignoreStartupMessages = false;
                foreach (string arg in args)
                {
                    switch (arg)
                    {
                        case "--nomsg":
                            ignoreStartupMessages = true;
                            break;
                    }
                }

                //initialize Logger
                Logger.InitializeLogger(LogManager.GetLogger(typeof(Program)));

                int port = Convert.ToInt32(ConfigurationManager.AppSettings["BazaarServerPort"]);

                if (!ignoreStartupMessages)
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                    string text =
                        $"BAZAAR SERVER v{fileVersionInfo.ProductVersion}dev - PORT : {port} by OpenNos Team";
                    int offset = (Console.WindowWidth / 2) + (text.Length / 2);
                    string separator = new string('=', Console.WindowWidth);
                    Console.WriteLine(separator + string.Format("{0," + offset + "}\n", text) + separator);
                }

                Logger.Info(Language.Instance.GetMessageFromKey("CONFIG_LOADED"));

                try
                {
                    //configure Services and Service Host
                    string ipAddress = ConfigurationManager.AppSettings["BazaarServerIP"];
                    IScsServiceApplication server =
                        ScsServiceBuilder.CreateService(new ScsTcpEndPoint(ipAddress, port));

                    server.AddService<IBazaarService, BazaarService>(new BazaarService());
                    if (BazaarManager.Instance.AuthentificatedClients.Count != 0)
                    {
                        // Do nothing, just verify that BazaarManager is initialized before anyone
                        // can connect
                    }

                    server.ClientConnected += OnClientConnected;
                    server.ClientDisconnected += OnClientDisconnected;

                    server.Start();
                    Logger.Info(Language.Instance.GetMessageFromKey("STARTED"));
                }
                catch (Exception ex)
                {
                    Logger.Error("General Error Server", ex);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("General Error", ex);
            }
            Console.ReadKey();
        }

        private static void OnClientConnected(object sender, ServiceClientEventArgs e) =>
            Logger.Info(Language.Instance.GetMessageFromKey("NEW_CONNECT") + e.Client.ClientId);

        private static void OnClientDisconnected(object sender, ServiceClientEventArgs e) =>
            Logger.Info(Language.Instance.GetMessageFromKey("DISCONNECT") + e.Client.ClientId);

        #endregion
    }
}