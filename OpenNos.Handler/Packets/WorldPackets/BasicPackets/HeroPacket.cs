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

using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.BasicPackets
{
    [PacketHeader("hero")]
    public class HeroPacket
    {
        #region Properties

        public string Message { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(new[] { ' ' }, 3);
            if (packetSplit.Length < 3)
            {
                return;
            }
            HeroPacket packetDefinition = new HeroPacket();
            if (!string.IsNullOrWhiteSpace(packetSplit[2]))
            {
                packetDefinition.Message = packetSplit[2].Trim();
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(HeroPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            session.Character.IsAfk = false;
            if (session.Character.IsReputationHero() >= 3 )
            {
                ServerManager.Instance.Broadcast(session, $"msg 5 [{session.Character.Name}]:{Message}",
                    ReceiverType.AllNoHeroBlocked);
            }
            else
            {
                session.SendPacket(
                    session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_HERO"), 11));
            }
        }

        #endregion
    }
}