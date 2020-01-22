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
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$InstanceMusic", Authority = AuthorityType.GameMaster)]
    public class InstanceMusicPacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public string Maps { get; set; }

        public string Music { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            if (session is ClientSession sess)
            {
                string[] packetSplit = packet.Split(' ');
                if (packetSplit.Length < 3)
                {
                    sess.SendPacket(sess.Character.GenerateSay(ReturnHelp(), 10));
                    return;
                }
                InstanceMusicPacket packetDefinition = new InstanceMusicPacket();
                if (!string.IsNullOrWhiteSpace(packetSplit[2]))
                {
                    packetDefinition._isParsed = true;
                    packetDefinition.Music = packetSplit[2];
                    if (packetSplit.Length > 3 && !string.IsNullOrWhiteSpace(packetSplit[3]))
                    {
                        packetDefinition.Maps = packetSplit[3];
                    }
                }

                packetDefinition.ExecuteHandler(sess);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(InstanceMusicPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$InstanceMusic MUSIC/? (*)";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(),
                    $"[InstanceMusic]SongId: {Music} Mode: {Maps}");

                void ChangeMusic(bool isRevert)
                {
                    try
                    {
                        foreach (MapInstance instance in ServerManager.GetAllMapInstances())
                        {
                            if (!isRevert && int.TryParse(Music, out int mapMusic))
                            {
                                instance.InstanceMusic = mapMusic;
                            }
                            else
                            {
                                instance.InstanceMusic = instance.Map.Music;
                            }

                            instance.Broadcast($"bgm {instance.InstanceMusic}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }

                if (Maps == "*")
                {
                    if (Music == "?")
                    {
                        ChangeMusic(true);
                    }
                    else
                    {
                        ChangeMusic(false);
                    }
                }
                else if (session.CurrentMapInstance != null)
                {
                    if (Music == "?")
                    {
                        session.CurrentMapInstance.InstanceMusic = session.CurrentMapInstance.Map.Music;
                        session.CurrentMapInstance.Broadcast($"bgm {session.CurrentMapInstance.Map.Music}");
                        return;
                    }

                    if (int.TryParse(Music, out int mapMusic))
                    {
                        session.CurrentMapInstance.InstanceMusic = mapMusic;
                        session.CurrentMapInstance.Broadcast($"bgm {Music}");
                    }
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