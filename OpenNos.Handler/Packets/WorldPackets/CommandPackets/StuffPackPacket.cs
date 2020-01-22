using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$StuffPack", PassNonParseablePacket = true, Authority = AuthorityType.GameMaster)]
    public class StuffPackPacket
    {
        #region Properties

        public string Type { get; set; }

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

            StuffPackPacket packetDefinition = new StuffPackPacket { Type = packetSplit[2] };
            packetDefinition.ExecuteHandler(sess);
        }

        public static void Register() => PacketFacility.AddHandler(typeof(StuffPackPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "Use : \"Archer\", \"Sword\", \"Mage\", \"Martial\", \"Fairy\", \"Partenaire\" or \"Mount\"";

        private void ExecuteHandler(ClientSession session)
        {
            switch (Type)
            {
                case "Archer":
                case "archer":
                    session.Character.Inventory.AddNewToInventory(4986, 1, rare: 8, upgrade: 10); // HERO 50 ARMOR
                    session.Character.Inventory.AddNewToInventory(4983, 1, rare: 8, upgrade: 10); // HERO 50 WEAPON
                    session.Character.Inventory.AddNewToInventory(4980, 1, rare: 8, upgrade: 10); // HERO 50 SECOND WEAPON
                    session.Character.Inventory.AddNewToInventory(903, 1, 0, 15, 15);
                    session.Character.Inventory.AddNewToInventory(904, 1, 0, 15, 15);
                    session.Character.Inventory.AddNewToInventory(911, 1, 0, 15, 15);
                    session.Character.Inventory.AddNewToInventory(912, 1, 0, 15, 15);
                    session.Character.Inventory.AddNewToInventory(4501, 1, 0, 15, 15);
                    session.Character.Inventory.AddNewToInventory(4498, 1, 0, 15, 15);
                    session.Character.Inventory.AddNewToInventory(4492, 1, 0, 15, 15);
                    session.Character.Inventory.AddNewToInventory(4488, 1, 0, 15, 15);
                    break;
                case "Mage":
                case "mage":
                    session.Character.Inventory.AddNewToInventory(4985, 1, rare: 8, upgrade: 10); // HERO 50 ARMOR
                    session.Character.Inventory.AddNewToInventory(4982, 1, rare: 8, upgrade: 10); // HERO 50 WEAPON
                    session.Character.Inventory.AddNewToInventory(4979, 1, rare: 8, upgrade: 10); // HERO 50 SECOND WEAPON
                    session.Character.Inventory.AddNewToInventory(905, 1, 0, 15, 15);
                    session.Character.Inventory.AddNewToInventory(906, 1, 0, 15, 15);
                    session.Character.Inventory.AddNewToInventory(913, 1, 0, 15, 15);
                    session.Character.Inventory.AddNewToInventory(914, 1, 0, 15, 15);
                    session.Character.Inventory.AddNewToInventory(4502, 1, 0, 15, 15);
                    session.Character.Inventory.AddNewToInventory(4499, 1, 0, 15, 15);
                    session.Character.Inventory.AddNewToInventory(4491, 1, 0, 15, 15);
                    session.Character.Inventory.AddNewToInventory(4487, 1, 0, 15, 15);
                    break;
                case "Sword":
                case "sword":
                    session.Character.Inventory.AddNewToInventory(4984, 1, rare: 8, upgrade: 10); // HERO 50 ARMOR
                    session.Character.Inventory.AddNewToInventory(4981, 1, rare: 8, upgrade: 10); // HERO 50 WEAPON
                    session.Character.Inventory.AddNewToInventory(4978, 1, rare: 8, upgrade: 10); // HERO 50 SECOND WEAPON
                    session.Character.Inventory.AddNewToInventory(901, 1, 0, 15, 15);
                    session.Character.Inventory.AddNewToInventory(902, 1, 0, 15, 15);
                    session.Character.Inventory.AddNewToInventory(909, 1, 0, 15, 15);
                    session.Character.Inventory.AddNewToInventory(910, 1, 0, 15, 15);
                    session.Character.Inventory.AddNewToInventory(4500, 1, 0, 15, 15);
                    session.Character.Inventory.AddNewToInventory(4497, 1, 0, 15, 15);
                    session.Character.Inventory.AddNewToInventory(4493, 1, 0, 15, 15);
                    session.Character.Inventory.AddNewToInventory(4489, 1, 0, 15, 15);
                    break;
                case "Mount":
                case "mount":
                    session.Character.Inventory.AddNewToInventory(5196); // Nossi
                    session.Character.Inventory.AddNewToInventory(5330); // Soucoupe
                    session.Character.Inventory.AddNewToInventory(5360); // Planche à voile
                    break;
                case "Résistances":
                case "Résistance":
                case "Rez":
                case "rez":
                    session.Character.Inventory.AddNewToInventory(4967, 1, 0, 100, 100);
                    session.Character.Inventory.AddNewToInventory(4970, 1, 0, 100, 100);
                    break;
                case "Martial":
                case "martial":
                    session.Character.Inventory.AddNewToInventory(4736, 1, rare: 8, upgrade: 10); // HERO 45 Weapon 
                    session.Character.Inventory.AddNewToInventory(4754, 1, rare: 8, upgrade: 10); // HERO 48 Armor 
                    session.Character.Inventory.AddNewToInventory(4486, 1, 0, 15, 15);
                    break;
                case "Fairy":
                case "fairy":
                    session.Character.Inventory.AddNewToInventory(4129);
                    session.Character.Inventory.AddNewToInventory(4130);
                    session.Character.Inventory.AddNewToInventory(4131);
                    session.Character.Inventory.AddNewToInventory(4132);
                    break;

                case "partenaire":
                case "Partenaire":
                    session.Character.Inventory.AddNewToInventory(4324);
                    session.Character.Inventory.AddNewToInventory(4325);
                    session.Character.Inventory.AddNewToInventory(4326);
                    session.Character.Inventory.AddNewToInventory(4343);
                    session.Character.Inventory.AddNewToInventory(4349);
                    session.Character.Inventory.AddNewToInventory(4405);
                    session.Character.Inventory.AddNewToInventory(4413);
                    session.Character.Inventory.AddNewToInventory(4800);
                    session.Character.Inventory.AddNewToInventory(4802);
                    session.Character.Inventory.AddNewToInventory(4803);
                    session.Character.Inventory.AddNewToInventory(4804);
                    session.Character.Inventory.AddNewToInventory(4805);
                    session.Character.Inventory.AddNewToInventory(4806);
                    session.Character.Inventory.AddNewToInventory(4807);
                    session.Character.Inventory.AddNewToInventory(4808);
                    session.Character.Inventory.AddNewToInventory(4809);
                    session.Character.Inventory.AddNewToInventory(4811);
                    session.Character.Inventory.AddNewToInventory(4812);
                    session.Character.Inventory.AddNewToInventory(4813);
                    session.Character.Inventory.AddNewToInventory(4814);
                    session.Character.Inventory.AddNewToInventory(4815);
                    session.Character.Inventory.AddNewToInventory(4818);
                    session.Character.Inventory.AddNewToInventory(4817);
                    session.Character.Inventory.AddNewToInventory(4819);
                    session.Character.Inventory.AddNewToInventory(4820);
                    session.Character.Inventory.AddNewToInventory(4821);
                    session.Character.Inventory.AddNewToInventory(4822);
                    session.Character.Inventory.AddNewToInventory(4823);
                    session.Character.Inventory.AddNewToInventory(4824);
                    session.Character.Inventory.AddNewToInventory(4825);
                    break;
                default:
                    session.SendPacket(session.Character.GenerateSay("Use : \"Archer\", \"Sword\", \"Mage\", \"Martial\", \"Fairy\", \"Partenaire\" or \"Mount\"", 10));
                    break;
            }
        }

        #endregion
    }
}
