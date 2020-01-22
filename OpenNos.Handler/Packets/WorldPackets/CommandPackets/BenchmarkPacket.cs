// This file is part of the OpenNos NosTale Emulator Project.
//
// This program is licensed under a deviated version of the Fair Source License, granting you a
// non-exclusive, non-transferable, royalty-free and fully-paid-up license, under all of the
// Licensor's copyright and patent rights, to use, copy, prepare derivative works of, publicly
// perform and display the Software, subject to the conditions found in the LICENSE file.
//
// THIS FILE IS PROVIDED "AS IS", WITHOUT WARRANTY OR CONDITION, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. THE AUTHORS HEREBY DISCLAIM ALL LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT
// OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE.

using System.Diagnostics;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$Benchmark", Authority = AuthorityType.GameMaster)]
    public class BenchmarkPacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public int Iterations { get; set; }

        public byte Test { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (!(session is ClientSession sess))
            {
                return;
            }
            if (packetSplit.Length < 4)
            {
                sess.SendPacket(sess.Character.GenerateSay(ReturnHelp(), 10));
                return;
            }
            BenchmarkPacket packetDefinition = new BenchmarkPacket();
            if (byte.TryParse(packetSplit[2], out byte test)
                && int.TryParse(packetSplit[3], out int iterations))
            {
                packetDefinition._isParsed = true;
                packetDefinition.Test = test;
                packetDefinition.Iterations = iterations;
            }
            packetDefinition.ExecuteHandler(sess);
        }

        public static void Register() => PacketFacility.AddHandler(typeof(BenchmarkPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$Benchmark TEST ITERATIONS";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                double totalMiliseconds;
                switch (Test)
                {
                    case 1:
                        {
                            session.SendPacket(session.Character.GenerateSay("=== TEST: Receive Object from MS ===", 12));
                            Stopwatch sw = Stopwatch.StartNew();
                            for (int i = 0; i < Iterations; i++)
                            {
                                ConfigurationServiceClient.Instance.GetConfigurationObject();
                            }

                            sw.Stop();
                            totalMiliseconds = sw.Elapsed.TotalMilliseconds;
                        }
                        break;

                    case 2:
                        {
                            ConfigurationObject conf = ConfigurationServiceClient.Instance.GetConfigurationObject();
                            session.SendPacket(session.Character.GenerateSay("=== TEST: Send Object to MS ===", 12));
                            Stopwatch sw = Stopwatch.StartNew();
                            for (int i = 0; i < Iterations; i++)
                            {
                                ConfigurationServiceClient.Instance.UpdateConfigurationObject(conf);
                            }

                            sw.Stop();
                            totalMiliseconds = sw.Elapsed.TotalMilliseconds;
                        }
                        break;

                    default:
                        session.SendPacket(session.Character.GenerateSay(ReturnHelp(), 10));
                        return;
                }

                session.SendPacket(session.Character.GenerateSay(
                    $"The test with {Iterations} iterations took {totalMiliseconds} ms", 12));
                session.SendPacket(session.Character.GenerateSay(
                    $"The each iteration took {((totalMiliseconds * 1000000) / Iterations).ToString("0.00 ns")}",
                    12));
            }
            else
            {
                session.SendPacket(session.Character.GenerateSay(ReturnHelp(), 10));
            }
        }

        #endregion
    }
}