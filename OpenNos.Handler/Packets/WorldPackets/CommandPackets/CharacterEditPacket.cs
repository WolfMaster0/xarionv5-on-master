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
using System.Reflection;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$CharEdit", Authority = AuthorityType.GameMaster)]
    public class CharacterEditPacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public string Data { get; set; }

        public string Property { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(new[] { ' ' }, 4);
            if (!(session is ClientSession sess))
            {
                return;
            }
            if (packetSplit.Length < 4)
            {
                sess.SendPacket(sess.Character.GenerateSay(ReturnHelp(), 10));
                return;
            }
            CharacterEditPacket packetDefinition = new CharacterEditPacket();
            if (!string.IsNullOrEmpty(packetSplit[2]) && !string.IsNullOrEmpty(packetSplit[3]))
            {
                packetDefinition._isParsed = true;
                packetDefinition.Property = packetSplit[2];
                packetDefinition.Data = packetSplit[3];
            }
            packetDefinition.ExecuteHandler(sess);
        }

        public static void Register() => PacketFacility.AddHandler(typeof(CharacterEditPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$CharEdit PROPERTYNAME DATA";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(),
                    $"[CharEdit]Property: {Property} Value: {Data}");
                PropertyInfo propertyInfo = session.Character.GetType().GetProperty(Property);
                if (propertyInfo != null)
                {
                    propertyInfo.SetValue(session.Character,
                        Convert.ChangeType(Data, propertyInfo.PropertyType));
                    ServerManager.Instance.ChangeMap(session.Character.CharacterId);
                    session.Character.Save();
                    session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"),
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