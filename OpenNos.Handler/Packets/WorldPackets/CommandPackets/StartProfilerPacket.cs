// This file is part of the OpenNos NosTale Emulator Project.
//
// This program is licensed under a deviated version of the Fair Source License, granting you a
// non-exclusive, non-transferable, royalty-free and fully-paid-up license, under all of the
// Licensor's copyright and patent rights, to use, copy, prepare derivative works of, publicly
// perform and display the Software, subject to the contitions found in the LICENSE file.
//
// THIS FILE IS PROVIDED "AS IS", WITHOUT WARRANTY OR CONDITION, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. THE AUTHORS HEREBY DISCLAIM ALL LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT
// OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE.

using System.Threading;
using JetBrains.Profiler.Windows.Api;
using JetBrains.Profiler.Windows.SelfApi;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$StartProfiler", Authority = AuthorityType.GameMaster)]
    public class StartProfilerPacket
    {
        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            if (session is ClientSession sess)
            {
                string[] packetSplit = packet.Split(' ');
                if (packetSplit.Length < 2)
                {
                    sess.SendPacket(sess.Character.GenerateSay(ReturnHelp(), 10));
                    return;
                }
                StartProfilerPacket packetDefinition = new StartProfilerPacket();
                packetDefinition.ExecuteHandler(sess);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(StartProfilerPacket), HandlePacket);

        public static string ReturnHelp() => "$StartProfiler ";

        // ReSharper disable once UnusedParameter.Local
        private void ExecuteHandler(ClientSession session)
        {
            while (SelfAttach.State != SelfApiState.Active)
            {
                Thread.Sleep(250);  // wait until API starts
            }
            if (PerformanceProfiler.IsActive)
            {
                PerformanceProfiler.Begin();
                PerformanceProfiler.Start();
            }
        }

        #endregion
    }
}