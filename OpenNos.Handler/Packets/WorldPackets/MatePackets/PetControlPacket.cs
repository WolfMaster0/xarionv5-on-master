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
using System.Collections.Generic;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace OpenNos.Handler.Packets.WorldPackets.MatePackets
{
    [PacketHeader("ptctl")]
    public class PetControlPacket
    {
        #region Properties

        public byte Amount { get; set; }

        public short MapType { get; set; }

        public List<Tuple<int, short, short>> Pets { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(new[] { ' ' }, 5);
            if (packetSplit.Length < 5)
            {
                return;
            }
            PetControlPacket packetDefinition = new PetControlPacket();
            if (short.TryParse(packetSplit[2], out short mapType)
                && byte.TryParse(packetSplit[3], out byte amount))
            {
                packetDefinition.Pets = new List<Tuple<int, short, short>>();
                packetDefinition.MapType = mapType;
                packetDefinition.Amount = amount;
                string[] petSplit = packetSplit[4].Split(' ');
                for (int i = 0; i + 3 < petSplit.Length && i < amount * 3; i += 3)
                {
                    if (int.TryParse(petSplit[i], out int petId)
                        && short.TryParse(petSplit[i + 1], out short positionX)
                        && short.TryParse(petSplit[i + 2], out short positionY))
                    {
                        packetDefinition.Pets.Add(new Tuple<int, short, short>(petId, positionX, positionY));
                    }
                }

                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(PetControlPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            foreach (Tuple<int, short, short> pet in Pets)
            {
                Mate mate = session.Character.Mates.Find(s => s.MateTransportId == pet.Item1);
                if (mate != null && session.CurrentMapInstance?.Map?.IsBlockedZone(pet.Item2, pet.Item3) == false)
                {
                    mate.PositionX = pet.Item2;
                    mate.PositionY = pet.Item3;
                    session.CurrentMapInstance?.Broadcast(StaticPacketHelper.Move(UserType.Npc, pet.Item1, pet.Item2,
                        pet.Item3, mate.Monster.Speed));
                    if (mate.LastMonsterAggro.AddSeconds(5) > DateTime.UtcNow)
                    {
                        mate.UpdateBushFire();
                    }
                }
            }
        }

        #endregion
    }
}