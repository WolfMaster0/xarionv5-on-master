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

using System;
using System.Linq;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.MinilandPackets
{
    [PacketHeader("mledit")]
    public class MinilandEditPacket
    {
        #region Properties

        public string Parameters { get; set; }

        public byte Type { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(new[] { ' ' }, 4);
            if (packetSplit.Length < 4)
            {
                return;
            }
            MinilandEditPacket packetDefinition = new MinilandEditPacket();
            if (byte.TryParse(packetSplit[2], out byte type)
                && !string.IsNullOrEmpty(packetSplit[3]))
            {
                packetDefinition.Type = type;
                packetDefinition.Parameters = packetSplit[3];
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(MinilandEditPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            switch (Type)
            {
                case 1:
                    session.SendPacket($"mlintro {Parameters.Replace(' ', '^')}");
                    session.SendPacket(
                        UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("MINILAND_INFO_CHANGED")));
                    break;

                case 2:
                    if (Enum.TryParse(Parameters, out MinilandState state))
                    {
                        switch (state)
                        {
                            case MinilandState.Private:
                                session.SendPacket(
                                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("MINILAND_PRIVATE"),
                                        0));

                                //Need to be review to permit one friend limit on the miniland
                                session.Character.Miniland.Sessions.Where(s => s.Character != session.Character).ToList()
                                    .ForEach(s => ServerManager.Instance.ChangeMap(s.Character.CharacterId,
                                        s.Character.MapId, s.Character.MapX, s.Character.MapY));
                                break;

                            case MinilandState.Lock:
                                session.SendPacket(
                                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("MINILAND_LOCK"),
                                        0));
                                session.Character.Miniland.Sessions.Where(s => s.Character != session.Character).ToList()
                                    .ForEach(s => ServerManager.Instance.ChangeMap(s.Character.CharacterId,
                                        s.Character.MapId, s.Character.MapX, s.Character.MapY));
                                break;

                            case MinilandState.Open:
                                session.SendPacket(
                                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("MINILAND_PUBLIC"),
                                        0));
                                break;
                        }
                        session.Character.MinilandState = state;
                    }
                    break;
            }
        }

        #endregion
    }
}