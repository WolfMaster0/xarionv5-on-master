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

using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$HeroLvl", Authority = AuthorityType.GameMaster)]
    public class ChangeHeroLevelPacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public byte HeroLevel { get; set; }

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

            ChangeHeroLevelPacket packetDefinition = new ChangeHeroLevelPacket();
            if (byte.TryParse(packetSplit[2], out byte heroLevel))
            {
                packetDefinition._isParsed = true;
                packetDefinition.HeroLevel = heroLevel;
            }

            packetDefinition.ExecuteHandler(sess);
        }

        public static void Register() =>
            PacketFacility.AddHandler(typeof(ChangeHeroLevelPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$HeroLvl HEROLEVEL";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(),
                    $"[HeroLvl]HeroLevel: {HeroLevel}");

                if (HeroLevel <= 255)
                {
                    session.Character.HeroLevel = HeroLevel;
                    session.Character.HeroXp = 0;
                    session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("HEROLEVEL_CHANGED"), 0));
                    session.SendPacket(session.Character.GenerateLev());
                    session.SendPacket(session.Character.GenerateStatChar());
                    session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateIn(),
                        ReceiverType.AllExceptMe);
                    session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateGidx(),
                        ReceiverType.AllExceptMe);
                    session.CurrentMapInstance?.Broadcast(
                        StaticPacketHelper.GenerateEff(UserType.Player, session.Character.CharacterId, 6),
                        session.Character.PositionX, session.Character.PositionY);
                    session.CurrentMapInstance?.Broadcast(
                        StaticPacketHelper.GenerateEff(UserType.Player, session.Character.CharacterId, 198),
                        session.Character.PositionX, session.Character.PositionY);
                }
                else
                {
                    session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
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