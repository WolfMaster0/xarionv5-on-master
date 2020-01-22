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

using JetBrains.Profiler.Windows.SelfApi;
using JetBrains.Profiler.Windows.SelfApi.Config;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$AttachProfiler", Authority = AuthorityType.GameMaster)]
    public class AttachProfilerPacket
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
                AttachProfilerPacket packetDefinition = new AttachProfilerPacket();
                packetDefinition.ExecuteHandler(sess);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(AttachProfilerPacket), HandlePacket);

        public static string ReturnHelp() => "$AttachProfiler ";

        // ReSharper disable once UnusedParameter.Local
        private void ExecuteHandler(ClientSession session)
        {
            SelfAttach.Attach(new SaveSnapshotProfilingConfig
            {
                ProfilingControlKind = ProfilingControlKind.Api,
                SaveDir = "C:\\ProfilerLogs",
                RedistDir = "C:\\ProfilerSDK",
                ProfilingType = ProfilingType.Performance,
                ListFile = "C:\\ProfilerLogs\\snapshot_list.xml"  // the file is created automatically during profiling
            });
        }

        #endregion
    }
}