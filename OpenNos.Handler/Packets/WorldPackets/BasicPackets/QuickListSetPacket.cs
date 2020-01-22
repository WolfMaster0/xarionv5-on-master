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

using System.Linq;
using OpenNos.Core.Serializing;
using OpenNos.Data;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.BasicPackets
{
    [PacketHeader("qset")]
    public class QuickListSetPacket
    {
        #region Properties

        public short? Data1 { get; set; }

        public short? Data2 { get; set; }

        public short Q1 { get; set; }

        public short Q2 { get; set; }

        public short Type { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 5)
            {
                return;
            }
            QuickListSetPacket packetDefinition = new QuickListSetPacket();
            if (short.TryParse(packetSplit[2], out short type)
                && short.TryParse(packetSplit[3], out short q1)
                && short.TryParse(packetSplit[4], out short q2))
            {
                packetDefinition.Type = type;
                packetDefinition.Q1 = q1;
                packetDefinition.Q2 = q2;
                packetDefinition.Data1 = packetSplit.Length >= 6 && short.TryParse(packetSplit[5], out short data1) ? data1 : (short?)null;
                packetDefinition.Data2 = packetSplit.Length >= 7 && short.TryParse(packetSplit[6], out short data2) ? data2 : (short?)null;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(QuickListSetPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            short data1 = 0, data2 = 0, type = Type, q1 = Q1, q2 = Q2;

            if (Data1.HasValue)
            {
                data1 = Data1.Value;
            }

            if (Data2.HasValue)
            {
                data2 = Data2.Value;
            }

            switch (type)
            {
                case 0:
                case 1:

                    // client says qset 0 1 3 2 6 answer -> qset 1 3 0.2.6.0
                    session.Character.QuicklistEntries.RemoveAll(n =>
                        n.Q1 == q1 && n.Q2 == q2
                                   && (session.Character.UseSp ? n.Morph == session.Character.Morph : n.Morph == 0));
                    session.Character.QuicklistEntries.Add(new QuicklistEntryDTO
                    {
                        CharacterId = session.Character.CharacterId,
                        Type = type,
                        Q1 = q1,
                        Q2 = q2,
                        Slot = data1,
                        Pos = data2,
                        Morph = session.Character.UseSp ? (short)session.Character.Morph : (short)0
                    });
                    session.SendPacket($"qset {q1} {q2} {type}.{data1}.{data2}.0");
                    break;

                case 2:

                    // DragDrop / Reorder qset type to1 to2 from1 from2 vars -> q1 q2 data1 data2
                    QuicklistEntryDTO qlFrom = session.Character.QuicklistEntries.SingleOrDefault(n =>
                        n.Q1 == data1 && n.Q2 == data2
                                      && (session.Character.UseSp ? n.Morph == session.Character.Morph : n.Morph == 0));
                    if (qlFrom != null)
                    {
                        QuicklistEntryDTO qlTo = session.Character.QuicklistEntries.SingleOrDefault(n =>
                            n.Q1 == q1 && n.Q2 == q2 && (session.Character.UseSp
                                ? n.Morph == session.Character.Morph
                                : n.Morph == 0));
                        qlFrom.Q1 = q1;
                        qlFrom.Q2 = q2;
                        if (qlTo == null)
                        {
                            // Put 'from' to new position (datax)
                            session.SendPacket(
                                $"qset {qlFrom.Q1} {qlFrom.Q2} {qlFrom.Type}.{qlFrom.Slot}.{qlFrom.Pos}.0");

                            // old 'from' is now empty.
                            session.SendPacket($"qset {data1} {data2} 7.7.-1.0");
                        }
                        else
                        {
                            // Put 'from' to new position (datax)
                            session.SendPacket(
                                $"qset {qlFrom.Q1} {qlFrom.Q2} {qlFrom.Type}.{qlFrom.Slot}.{qlFrom.Pos}.0");

                            // 'from' is now 'to' because they exchanged
                            qlTo.Q1 = data1;
                            qlTo.Q2 = data2;
                            session.SendPacket($"qset {qlTo.Q1} {qlTo.Q2} {qlTo.Type}.{qlTo.Slot}.{qlTo.Pos}.0");
                        }
                    }

                    break;

                case 3:

                    // Remove from Quicklist
                    session.Character.QuicklistEntries.RemoveAll(n =>
                        n.Q1 == q1 && n.Q2 == q2
                                   && (session.Character.UseSp ? n.Morph == session.Character.Morph : n.Morph == 0));
                    session.SendPacket($"qset {q1} {q2} 7.7.-1.0");
                    break;

                default:
                    return;
            }
        }

        #endregion
    }
}