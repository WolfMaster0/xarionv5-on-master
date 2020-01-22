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
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$AddShellEffect", Authority = AuthorityType.GameMaster)]
    public class AddShellEffectPacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public byte Effect { get; set; }

        public ShellEffectLevelType EffectLevel { get; set; }

        public byte Slot { get; set; }

        public short Value { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (!(session is ClientSession sess))
            {
                return;
            }
            if (packetSplit.Length < 6)
            {
                sess.SendPacket(sess.Character.GenerateSay(ReturnHelp(), 10));
                return;
            }
            AddShellEffectPacket packetDefinition = new AddShellEffectPacket();
            if (byte.TryParse(packetSplit[2], out byte slot)
                && Enum.TryParse(packetSplit[3], out ShellEffectLevelType effectLevel)
                && byte.TryParse(packetSplit[4], out byte effect)
                && short.TryParse(packetSplit[5], out short value))
            {
                packetDefinition._isParsed = true;
                packetDefinition.Slot = slot;
                packetDefinition.EffectLevel = effectLevel;
                packetDefinition.Effect = effect;
                packetDefinition.Value = value;
            }
            packetDefinition.ExecuteHandler(sess);
        }

        public static void Register() => PacketFacility.AddHandler(typeof(AddShellEffectPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$AddShellEffect SLOT EFFECTLEVEL EFFECT VALUE";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(),
                    $"[AddShellEffect]Slot: {Slot} EffectLevel: {EffectLevel} Effect: {Effect} Value: {Value}");
                try
                {
                    ItemInstance instance =
                        session.Character.Inventory.LoadBySlotAndType(Slot, InventoryType.Equipment);
                    if (instance != null)
                    {
                        instance.ShellEffects.Add(new ShellEffectDTO
                        {
                            EffectLevel = EffectLevel,
                            Effect = Effect,
                            Value = Value,
                            EquipmentSerialId = instance.EquipmentSerialId
                        });
                    }
                }
                catch (Exception)
                {
                    session.SendPacket(session.Character.GenerateSay(ReturnHelp(), 10));
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