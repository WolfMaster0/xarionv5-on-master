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
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$BanAccount", Authority = AuthorityType.GameMaster)]
    public class BanAccountPacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public long AccountId { get; set; }

        public int Duration { get; set; }

        public string Reason { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(new[] {' '}, 5);
            if (!(session is ClientSession sess))
            {
                return;
            }

            if (packetSplit.Length < 4)
            {
                sess.SendPacket(sess.Character.GenerateSay(ReturnHelp(), 10));
                return;
            }

            BanAccountPacket packetDefinition = new BanAccountPacket();
            if (long.TryParse(packetSplit[2], out long accountId) && int.TryParse(packetSplit[3], out int duration))
            {
                packetDefinition._isParsed = true;
                packetDefinition.AccountId = accountId;
                packetDefinition.Duration = duration;
                packetDefinition.Reason = string.IsNullOrEmpty(packetSplit[4]) ? string.Empty : packetSplit[4].Trim();
            }

            packetDefinition.ExecuteHandler(sess);
        }

        public static void Register() => PacketFacility.AddHandler(typeof(BanAccountPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$BanAccount ACCOUNTID DURATION(DAYS) REASON";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(),
                    $"[BanAccount]AccountId: {AccountId} Reason: {Reason} Until: {(Duration == 0 ? DateTime.UtcNow.AddYears(15) : DateTime.UtcNow.AddDays(Duration))}");
                AccountDTO acc = DAOFactory.AccountDAO.LoadById(AccountId);
                if (acc != null)
                {
                    foreach (CharacterDTO dto in DAOFactory.CharacterDAO.LoadByAccount(AccountId))
                    {
                        try
                        {
                            ServerManager.Instance.Kick(dto.Name);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogEventError("BanAccount", "Kicking account failed.", ex);
                        }
                    }
                    PenaltyLogDTO log = new PenaltyLogDTO
                    {
                        AccountId = AccountId,
                        Reason = Reason,
                        Penalty = PenaltyType.Banned,
                        DateStart = DateTime.UtcNow,
                        DateEnd = Duration == 0 ? DateTime.UtcNow.AddYears(15) : DateTime.UtcNow.AddDays(Duration),
                        AdminName = session.Character.Name
                    };
                    Character.InsertOrUpdatePenalty(log);
                    session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
                }
                else
                {
                    session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"),
                        10));
                }
            }
            else
            {
                session.SendPacket(session.Character.GenerateSay(ReturnHelp(), 10));
            }
        }

        #endregion
    }
}