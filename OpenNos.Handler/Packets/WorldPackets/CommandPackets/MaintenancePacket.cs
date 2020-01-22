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

using System;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$Maintenance", Authority = AuthorityType.GameMaster)]
    public class MaintenancePacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public int Delay { get; set; }

        public int Duration { get; set; }

        public string Reason { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            if (session is ClientSession sess)
            {
                string[] packetSplit = packet.Split(new[] {' '}, 5);
                if (packetSplit.Length < 5)
                {
                    sess.SendPacket(sess.Character.GenerateSay(ReturnHelp(), 10));
                    return;
                }

                MaintenancePacket packetDefinition = new MaintenancePacket();
                if (int.TryParse(packetSplit[2], out int delay) && int.TryParse(packetSplit[3], out int duration)
                    && !string.IsNullOrWhiteSpace(packetSplit[4]))
                {
                    packetDefinition._isParsed = true;
                    packetDefinition.Delay = delay;
                    packetDefinition.Duration = duration;
                    packetDefinition.Reason = packetSplit[4];
                }

                packetDefinition.ExecuteHandler(sess);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(MaintenancePacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$Maintenance DELAY DURATION REASON";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(),
                    $"[Maintenance]Delay: {Delay} Duration: {Duration} Reason: {Reason}");
                DateTime dateStart = DateTime.UtcNow.AddMinutes(Delay);
                MaintenanceLogDTO maintenance = new MaintenanceLogDTO
                {
                    DateEnd = dateStart.AddMinutes(Duration),
                    DateStart = dateStart,
                    Reason = Reason
                };
                DAOFactory.MaintenanceLogDAO.Insert(maintenance);
                session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
            }
            else
            {
                session.SendPacket(session.Character.GenerateSay(ReturnHelp(), 10));
            }
        }

        #endregion
    }
}