﻿// This file is part of the OpenNos NosTale Emulator Project.
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
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameLog.LogHelper;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$Bank", Authority = AuthorityType.User)]
    public class BankPacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public string Mode { get; set; }

        public string Param1 { get; set; }

        public long? Param2 { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (!(session is ClientSession sess))
            {
                return;
            }
            if (packetSplit.Length < 3)
            {
                sess.SendPacket(sess.Character.GenerateSay(ReturnHelp(), 10));
                return;
            }
            BankPacket packetDefinition = new BankPacket();
            if (!string.IsNullOrEmpty(packetSplit[2]))
            {
                packetDefinition._isParsed = true;
                packetDefinition.Mode = packetSplit[2];
                if (packetSplit.Length > 3)
                {
                    packetDefinition.Param1 = packetSplit[3];
                }

                packetDefinition.Param2 = packetSplit.Length >= 5
                    && long.TryParse(packetSplit[4], out long param2) ? param2 : (long?)null;
            }
            packetDefinition.ExecuteHandler(sess);
        }

        public static void Register() => PacketFacility.AddHandler(typeof(BankPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "Display Help: $Bank Help\n" +
                   "Display Balance: $Bank Balance\n" +
                   "Deposit Gold: $Bank Deposit AMOUNT\n" +
                   "Withdraw Gold: $Bank Withdraw AMOUNT\n" +
                   "Send Gold: $Bank Send RECEIVER AMOUNT";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed && !session.Character.IsExchanging)
            {
                switch (Mode?.ToLower())
                {
                    case "balance":
                        session.SendPacket(
                            session.Character.GenerateSay($"Current Balance: {session.Character.GoldBank} Gold.", 10));
                        return;

                    case "deposit":
                        if (Param1 != null
                            && (long.TryParse(Param1, out long amount) || string.Equals(Param1,
                                 "all", StringComparison.OrdinalIgnoreCase)))
                        {
                            if (string.Equals(Param1, "all", StringComparison.OrdinalIgnoreCase)
                                && session.Character.Gold > 0)
                            {
                                GameLogger.Instance.LogBankDeposit(ServerManager.Instance.ChannelId,
                                    session.Character.Name, session.Character.CharacterId, session.Character.Gold,
                                    session.Character.GoldBank, session.Character.Gold,
                                    session.Character.GoldBank + session.Character.Gold, 0);
                                session.SendPacket(
                                    session.Character.GenerateSay($"Deposited ALL({session.Character.Gold}) Gold.",
                                        10));
                                session.Character.GoldBank += session.Character.Gold;
                                session.Character.Gold = 0;
                                session.SendPacket(session.Character.GenerateGold());
                                session.SendPacket(
                                    session.Character.GenerateSay($"New Balance: {session.Character.GoldBank} Gold.",
                                        10));
                            }
                            else if (amount <= session.Character.Gold && session.Character.Gold > 0)
                            {
                                if (amount < 1)
                                {
                                    GameLogger.Instance.LogBankIllegal(ServerManager.Instance.ChannelId,
                                        session.Character.Name, session.Character.CharacterId, Mode, Param1,
                                        Param2?.ToString() ?? string.Empty);

                                    session.SendPacket(session.Character.GenerateSay(
                                        "I'm afraid I can't let you do that. This incident has been logged.", 10));
                                }
                                else
                                {
                                    GameLogger.Instance.LogBankDeposit(ServerManager.Instance.ChannelId,
                                        session.Character.Name, session.Character.CharacterId, amount,
                                        session.Character.GoldBank, session.Character.Gold,
                                        session.Character.GoldBank + amount, session.Character.Gold - amount);
                                    session.SendPacket(session.Character.GenerateSay($"Deposited {amount} Gold.", 10));
                                    session.Character.GoldBank += amount;
                                    session.Character.Gold -= amount;
                                    session.SendPacket(session.Character.GenerateGold());
                                    session.SendPacket(
                                        session.Character.GenerateSay(
                                            $"New Balance: {session.Character.GoldBank} Gold.", 10));
                                }
                            }
                        }
                        return;

                    case "withdraw":
                        if (Param1 != null && long.TryParse(Param1, out amount)
                            && amount <= session.Character.GoldBank && session.Character.GoldBank > 0
                            && (session.Character.Gold + amount) <= ServerManager.Instance.Configuration.MaxGold)
                        {
                            if (amount < 1)
                            {
                                GameLogger.Instance.LogBankIllegal(ServerManager.Instance.ChannelId,
                                    session.Character.Name, session.Character.CharacterId, Mode, Param1,
                                    Param2?.ToString() ?? string.Empty);

                                session.SendPacket(session.Character.GenerateSay(
                                    "I'm afraid I can't let you do that. This incident has been logged.", 10));
                                return;
                            }
                            else
                            {
                                GameLogger.Instance.LogBankWithdraw(ServerManager.Instance.ChannelId,
                                    session.Character.Name, session.Character.CharacterId, amount,
                                    session.Character.GoldBank, session.Character.Gold,
                                    session.Character.GoldBank - amount, session.Character.Gold + amount);

                                session.SendPacket(session.Character.GenerateSay($"Withdrawn {amount} Gold.", 10));
                                session.Character.GoldBank -= amount;
                                session.Character.Gold += amount;
                                session.SendPacket(session.Character.GenerateGold());
                                session.SendPacket(
                                    session.Character.GenerateSay($"New Balance: {session.Character.GoldBank} Gold.",
                                        10));
                            }
                        }
                        return;

                    case "send":
                        if (Param1 != null && Param2.HasValue)
                        {
                            ClientSession receiver =
                                ServerManager.Instance.GetSessionByCharacterName(Param1);
                            if (Param2 <= session.Character.GoldBank && session.Character.GoldBank > 0
                                && receiver != null)
                            {
                                if (Param2 < 1)
                                {
                                    GameLogger.Instance.LogBankIllegal(ServerManager.Instance.ChannelId,
                                        session.Character.Name, session.Character.CharacterId, Mode, Param1,
                                        Param2.ToString());

                                    session.SendPacket(session.Character.GenerateSay(
                                        "I'm afraid I can't let you do that. This incident has been logged.", 10));
                                    return;
                                }
                                else
                                {
                                    GameLogger.Instance.LogBankSend(ServerManager.Instance.ChannelId,
                                        session.Character.Name, session.Character.CharacterId, receiver.Character.Name,
                                        receiver.Character.CharacterId, Param2.Value, session.Character.GoldBank,
                                        receiver.Character.GoldBank, session.Character.GoldBank - Param2.Value,
                                        receiver.Character.GoldBank + Param2.Value);

                                    session.SendPacket(
                                        session.Character.GenerateSay(
                                            $"Sent {Param2.Value} Gold to {receiver.Character.Name}", 10));
                                    receiver.SendPacket(
                                        session.Character.GenerateSay(
                                            $"Received {Param2.Value} Gold from {session.Character.Name}", 10));
                                    session.Character.GoldBank -= Param2.Value;
                                    receiver.Character.GoldBank += Param2.Value;
                                    session.SendPacket(
                                        session.Character.GenerateSay(
                                            $"New Balance: {session.Character.GoldBank} Gold.", 10));
                                    receiver.SendPacket(
                                        session.Character.GenerateSay(
                                            $"New Balance: {receiver.Character.GoldBank} Gold.", 10));
                                }
                            }
                        }
                        return;

                    default:
                        {
                            session.SendPacket(session.Character.GenerateSay(ReturnHelp(), 10));
                            return;
                        }
                }
            }

            session.SendPacket(session.Character.GenerateSay(ReturnHelp(), 10));
        }

        #endregion
    }
}