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
using System.Linq;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Handler.Packets.SharedPacketMethods;

namespace OpenNos.Handler.Packets.WorldPackets.InventoryPackets
{
    [PacketHeader("req_exc")]
    public class ExchangeRequestPacket
    {
        #region Properties

        public long? CharacterId { get; set; }

        public RequestExchangeType RequestType { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 3)
            {
                return;
            }
            ExchangeRequestPacket packetDefinition = new ExchangeRequestPacket();
            if (Enum.TryParse(packetSplit[2], out RequestExchangeType requestType))
            {
                packetDefinition.RequestType = requestType;
                packetDefinition.CharacterId = packetSplit.Length > 3
                    && long.TryParse(packetSplit[3], out long characterId) ? characterId : (long?)null;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(ExchangeRequestPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (session.CurrentMapInstance?.MapInstanceType == MapInstanceType.BaseMapInstance)
            {
                ClientSession sess = null;
                if (CharacterId.HasValue)
                {
                    sess = ServerManager.Instance.GetSessionByCharacterId(CharacterId.Value);
                }

                if (sess != null && session.Character.MapInstanceId
                    != sess.Character.MapInstanceId)
                {
                    sess.Character.ExchangeInfo = null;
                    session.Character.ExchangeInfo = null;
                }
                else
                {
                    switch (RequestType)
                    {
                        case RequestExchangeType.Requested:
                            if (!session.HasCurrentMapInstance || !CharacterId.HasValue)
                            {
                                return;
                            }

                            ClientSession targetSession =
                                session.CurrentMapInstance.GetSessionByCharacterId(CharacterId.Value);
                            if (targetSession == null)
                            {
                                return;
                            }

                            if (targetSession.Character.IsAfk)
                            {
                                session.SendPacket(UserInterfaceHelper.GenerateInfo(
                                    string.Format(Language.Instance.GetMessageFromKey("PLAYER_IS_AFK"),
                                        targetSession.Character.Name)));
                            }

                            if (targetSession.Character.Group != null
                                && targetSession.Character.Group?.GroupType != GroupType.Group)
                            {
                                session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                    Language.Instance.GetMessageFromKey("EXCHANGE_NOT_ALLOWED_IN_RAID"), 0));
                                return;
                            }

                            if (session.Character.Group != null
                                && session.Character.Group?.GroupType != GroupType.Group)
                            {
                                session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                    Language.Instance.GetMessageFromKey("EXCHANGE_NOT_ALLOWED_WITH_RAID_MEMBER"), 0));
                                return;
                            }

                            if (session.Character.IsBlockedByCharacter(CharacterId.Value))
                            {
                                session.SendPacket(
                                    UserInterfaceHelper.GenerateInfo(
                                        Language.Instance.GetMessageFromKey("BLACKLIST_BLOCKED")));
                                return;
                            }

                            if (session.Character.Speed == 0 || targetSession.Character.Speed == 0)
                            {
                                session.Character.ExchangeBlocked = true;
                            }

                            if (targetSession.Character.LastSkillUse.AddSeconds(20) > DateTime.UtcNow
                                || targetSession.Character.LastDefence.AddSeconds(20) > DateTime.UtcNow)
                            {
                                session.SendPacket(UserInterfaceHelper.GenerateInfo(
                                    string.Format(Language.Instance.GetMessageFromKey("PLAYER_IN_BATTLE"),
                                        targetSession.Character.Name)));
                                return;
                            }

                            if (session.Character.LastSkillUse.AddSeconds(20) > DateTime.UtcNow
                                || session.Character.LastDefence.AddSeconds(20) > DateTime.UtcNow)
                            {
                                session.SendPacket(
                                    UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("IN_BATTLE")));
                                return;
                            }

                            if (session.Character.HasShopOpened || targetSession.Character.HasShopOpened)
                            {
                                session.SendPacket(
                                    UserInterfaceHelper.GenerateMsg(
                                        Language.Instance.GetMessageFromKey("HAS_SHOP_OPENED"), 10));
                                return;
                            }

                            if (targetSession.Character.ExchangeBlocked)
                            {
                                session.SendPacket(
                                    session.Character.GenerateSay(Language.Instance.GetMessageFromKey("TRADE_BLOCKED"),
                                        11));
                            }
                            else
                            {
                                if (session.Character.InExchangeOrTrade || targetSession.Character.InExchangeOrTrade)
                                {
                                    session.SendPacket(
                                        UserInterfaceHelper.GenerateModal(
                                            Language.Instance.GetMessageFromKey("ALREADY_EXCHANGE"), 0));
                                }
                                else
                                {
                                    session.SendPacket(UserInterfaceHelper.GenerateModal(
                                        string.Format(Language.Instance.GetMessageFromKey("YOU_ASK_FOR_EXCHANGE"),
                                            targetSession.Character.Name), 0));

                                    session.Character.TradeRequests.Add(targetSession.Character.CharacterId);
                                    targetSession.SendPacket(UserInterfaceHelper.GenerateDialog(
                                        $"#req_exc^2^{session.Character.CharacterId} #req_exc^5^{session.Character.CharacterId} {string.Format(Language.Instance.GetMessageFromKey("INCOMING_EXCHANGE"), session.Character.Name)}"));
                                }
                            }

                            break;

                        case RequestExchangeType.Confirmed: // click Trade button in exchange window
                            if (session.HasCurrentMapInstance && session.HasSelectedCharacter
                                                              && session.Character.ExchangeInfo != null
                                                              && session.Character.ExchangeInfo.TargetCharacterId
                                                              != session.Character.CharacterId)
                            {
                                if (!session.HasCurrentMapInstance)
                                {
                                    return;
                                }

                                targetSession =
                                    session.CurrentMapInstance.GetSessionByCharacterId(session.Character.ExchangeInfo
                                        .TargetCharacterId);

                                if (targetSession == null)
                                {
                                    return;
                                }

                                if (session.Character.Group != null
                                    && session.Character.Group?.GroupType != GroupType.Group)
                                {
                                    session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                        Language.Instance.GetMessageFromKey("EXCHANGE_NOT_ALLOWED_IN_RAID"), 0));
                                    return;
                                }

                                if (targetSession.Character.Group != null
                                    && targetSession.Character.Group?.GroupType != GroupType.Group)
                                {
                                    session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                        Language.Instance.GetMessageFromKey("EXCHANGE_NOT_ALLOWED_WITH_RAID_MEMBER"),
                                        0));
                                    return;
                                }

                                if (session.IsDisposing || targetSession.IsDisposing)
                                {
                                    session.CloseExchange(targetSession);
                                    return;
                                }

                                lock (targetSession.Character.Inventory)
                                {
                                    lock (session.Character.Inventory)
                                    {
                                        ExchangeInfo targetExchange = targetSession.Character.ExchangeInfo;
                                        Inventory inventory = targetSession.Character.Inventory;

                                        long gold = targetSession.Character.Gold;
                                        long maxGold = ServerManager.Instance.Configuration.MaxGold;

                                        if (targetExchange == null || session.Character.ExchangeInfo == null)
                                        {
                                            return;
                                        }

                                        if (session.Character.ExchangeInfo.Validated && targetExchange.Validated)
                                        {
                                            try
                                            {
                                                session.Character.ExchangeInfo.Confirmed = true;
                                                if (targetExchange.Confirmed
                                                    && session.Character.ExchangeInfo.Confirmed)
                                                {
                                                    session.SendPacket("exc_close 1");
                                                    targetSession.SendPacket("exc_close 1");

                                                    bool continues = true;
                                                    bool goldmax = false;
                                                    if (!session.Character.Inventory.EnoughPlace(targetExchange
                                                        .ExchangeList))
                                                    {
                                                        continues = false;
                                                    }

                                                    continues &=
                                                        inventory.EnoughPlace(session.Character.ExchangeInfo
                                                            .ExchangeList);
                                                    goldmax |= session.Character.ExchangeInfo.Gold + gold > maxGold;
                                                    if (session.Character.ExchangeInfo.Gold > session.Character.Gold)
                                                    {
                                                        return;
                                                    }

                                                    goldmax |= targetExchange.Gold + session.Character.Gold > maxGold;
                                                    if (!continues || goldmax)
                                                    {
                                                        string message = !continues
                                                            ? UserInterfaceHelper.GenerateMsg(
                                                                Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"),
                                                                0)
                                                            : UserInterfaceHelper.GenerateMsg(
                                                                Language.Instance.GetMessageFromKey("MAX_GOLD"), 0);
                                                        session.SendPacket(message);
                                                        targetSession.SendPacket(message);
                                                        session.CloseExchange(targetSession);
                                                    }
                                                    else
                                                    {
                                                        if (session.Character.ExchangeInfo.ExchangeList.Any(ei =>
                                                            !(ei.Item.IsTradable || ei.IsBound)))
                                                        {
                                                            session.SendPacket(
                                                                UserInterfaceHelper.GenerateMsg(
                                                                    Language.Instance.GetMessageFromKey(
                                                                        "ITEM_NOT_TRADABLE"), 0));
                                                            session.CloseExchange(targetSession);
                                                        }
                                                        else // all items can be traded
                                                        {
                                                            session.Character.IsExchanging =
                                                                targetSession.Character.IsExchanging = true;

                                                            // exchange all items from target to source
                                                            targetSession.Exchange(session);

                                                            // exchange all items from source to target
                                                            session.Exchange(targetSession);

                                                            session.Character.IsExchanging =
                                                                targetSession.Character.IsExchanging = false;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    session.SendPacket(UserInterfaceHelper.GenerateInfo(
                                                        string.Format(
                                                            Language.Instance.GetMessageFromKey("IN_WAITING_FOR"),
                                                            targetSession.Character.Name)));
                                                }
                                            }
                                            catch (NullReferenceException nre)
                                            {
                                                Logger.Error(nre);
                                            }
                                        }
                                    }
                                }
                            }

                            break;

                        case RequestExchangeType.Cancelled: // cancel trade thru exchange window
                            if (session.HasCurrentMapInstance && session.Character.ExchangeInfo != null)
                            {
                                targetSession =
                                    session.CurrentMapInstance.GetSessionByCharacterId(session.Character.ExchangeInfo
                                        .TargetCharacterId);
                                session.CloseExchange(targetSession);
                            }

                            break;

                        case RequestExchangeType.List:
                            if (sess != null
                                && (!session.Character.InExchangeOrTrade || !sess.Character.InExchangeOrTrade))
                            {
                                ClientSession otherSession =
                                    ServerManager.Instance.GetSessionByCharacterId(CharacterId.Value);
                                if (CharacterId.Value == session.Character.CharacterId
                                    || session.Character.Speed == 0 || otherSession?.Character.TradeRequests.All(s => s != session.Character.CharacterId) != false
                                )
                                {
                                    return;
                                }

                                if (session.Character.Group != null
                                    && session.Character.Group?.GroupType != GroupType.Group)
                                {
                                    session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                        Language.Instance.GetMessageFromKey("EXCHANGE_NOT_ALLOWED_IN_RAID"), 0));
                                    return;
                                }

                                if (otherSession.Character.Group != null
                                    && otherSession.Character.Group?.GroupType != GroupType.Group)
                                {
                                    session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                        Language.Instance.GetMessageFromKey("EXCHANGE_NOT_ALLOWED_WITH_RAID_MEMBER"),
                                        0));
                                    return;
                                }

                                session.SendPacket($"exc_list 1 {CharacterId} -1");
                                session.Character.ExchangeInfo = new ExchangeInfo
                                {
                                    TargetCharacterId = CharacterId.Value,
                                    Confirmed = false
                                };
                                sess.Character.ExchangeInfo = new ExchangeInfo
                                {
                                    TargetCharacterId = session.Character.CharacterId,
                                    Confirmed = false
                                };
                                session.CurrentMapInstance?.Broadcast(session,
                                    $"exc_list 1 {session.Character.CharacterId} -1", ReceiverType.OnlySomeone,
                                    string.Empty, CharacterId.Value);
                            }
                            else
                            {
                                if (CharacterId != null)
                                {
                                    session.CurrentMapInstance?.Broadcast(session,
                                        UserInterfaceHelper.GenerateModal(
                                            Language.Instance.GetMessageFromKey("ALREADY_EXCHANGE"), 0),
                                        ReceiverType.OnlySomeone, string.Empty, CharacterId.Value);
                                }
                            }

                            break;

                        case RequestExchangeType.Declined:
                            if (sess != null)
                            {
                                sess.Character.ExchangeInfo = null;
                            }

                            session.Character.ExchangeInfo = null;
                            session.SendPacket(
                                session.Character.GenerateSay(Language.Instance.GetMessageFromKey("YOU_REFUSED"), 10));
                            sess?.SendPacket(
                                session.Character.GenerateSay(
                                    string.Format(Language.Instance.GetMessageFromKey("EXCHANGE_REFUSED"),
                                        session.Character.Name), 10));

                            break;

                        default:
                            Logger.Warn(
                                $"Exchange-Request-Type not implemented. RequestType: {RequestType})");
                            break;
                    }
                }
            }
            else
            {
                session.SendPacket(
                    UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("IMPOSSIBLE_TO_USE")));
            }
        }

        #endregion
    }
}