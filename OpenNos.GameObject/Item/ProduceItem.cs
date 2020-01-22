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
using OpenNos.Data;
using System.Collections.Generic;
using System.Linq;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject
{
    public class ProduceItem : Item
    {
        #region Instantiation

        public ProduceItem(ItemDTO item) : base(item)
        {
        }

        #endregion

        #region Methods

        public override void Use(ClientSession session, ref ItemInstance inv, byte option = 0, string[] packetsplit = null)
        {
            switch (Effect)
            {
                case 100:
                    session.Character.LastNRunId = 0;
                    session.Character.LastItemVNum = inv.ItemVNum;
                    session.SendPacket("wopen 28 0");
                    List<Recipe> recipeList = ServerManager.Instance.GetRecipesByItemVNum(VNum);
                    string list = recipeList.Where(s => s.Amount > 0).Aggregate("m_list 2", (current, s) => current + $" {s.ItemVNum}");
                    session.SendPacket(list + (EffectValue <= 110 && EffectValue >= 108 ? " 999" : string.Empty));
                    break;

                default:
                    if (!OpenBoxItem(session, inv))
                    {
                        Logger.Warn(string.Format(Language.Instance.GetMessageFromKey("NO_HANDLER_ITEM"), GetType(),
                            VNum, Effect, EffectValue));
                    }
                    break;

                //VEHICULE BOX (OR) 
                case 9974:
                    {
                        int rnd = ServerManager.RandomNumber(0, 100);

                        if (rnd < 2)
                        {
                            short[] vnums =
                            {
                           5360
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 1)], 1);
                        }
                        else if (rnd < 5)
                        {
                            short[] vnums =
                            {
                                 9078, 9079, 9080
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 3)], 1);
                        }
                        else
                        {
                            short[] vnums =
                            {
                               5332, 5386, 5008, 9073, 9070, 9090, 9091, 9092, 9093, 9094, 9054, 9055
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 12)], 1);
                        }
                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                    }
                    break;

                //R8 WEAPON SHELL (EVENT)
                case 573:
                    {
                        int rnd = ServerManager.RandomNumber(0, 100);
                        {
                            short[] vnums =
                                                               {
                                573 //15x
                            };

                            byte[] counts =
                                {
                              1
                            };
                            int item = ServerManager.RandomNumber(0, 1);

                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 1)], 1, 8, 90);
                            session.SendPacket($"rdi {vnums[item]} {counts[item]}");
                        }
                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                    }


                    break;

                // R8 ARMOR SHELL (EVENT)
                case 585:
                    {
                        int rnd = ServerManager.RandomNumber(0, 100);
                        {
                            short[] vnums =
                                                               {
                                585 //15x
                            };

                            byte[] counts =
                                {
                              1
                            };
                            int item = ServerManager.RandomNumber(0, 1);

                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 1)], 1, 8, 90);
                            session.SendPacket($"rdi {vnums[item]} {counts[item]}");
                        }
                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                    }

                    break;

                //VEHICULE BOX (EVENT) 
                case 9975:
                    {
                        int rnd = ServerManager.RandomNumber(0, 100);

                        if (rnd < 2)
                        {
                            short[] vnums =
                            {
                                9115, 5173, 5914
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 3)], 1);
                        }
                        else if (rnd < 5)
                        {
                            short[] vnums =
                            {
                                5323, 9084, 5319
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 3)], 1);
                        }
                        else
                        {
                            short[] vnums =
                            {
                               9081, 9082, 1965, 5330
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 4)], 1);
                        }
                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                    }
                    break;

                //SP WINGS BOX (EVENT) 
                case 9976:
                    {
                        int rnd = ServerManager.RandomNumber(0, 100);

                        if (rnd < 3)
                        {
                            short[] vnums =
                            {
                          5560, 5591, 5702, 5837
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 4)], 1);
                        }
                        else
                        {
                            short[] vnums =
                            {
                                1685, 1686, 5087, 5203, 5372, 5431, 5432, 5498, 5499, 5553
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 10)], 1);
                        }
                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                    }
                    break;

                //PET BOX (EVENT) 
                case 9977:
                    {
                        int rnd = ServerManager.RandomNumber(0, 100);

                        if (rnd < 3)
                        {
                            short[] vnums =
                            {
                           4168, 4200, 4199, 4397, 4188, 4269, 4436, 4407, 4406, 4415, 4398, 448, 449
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 11)], 1);
                        }
                        else
                        {
                            short[] vnums =
                            {
                                4126, 4700, 4153, 4101, 4701, 4703, 4103, 4104, 4105, 447, 4066, 979, 4337, 4398, 348, 888, 540, 541, 542, 543, 544, 545, 546, 4702
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 24)], 1);
                        }
                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                    }
                    break;

                //PET BOX (OR)
                case 9978:
                    {
                        int rnd = ServerManager.RandomNumber(0, 100);

                        if (rnd < 5)
                        {
                            short[] vnums =
                            {
                          4062, 4414
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 2)], 1);
                        }
                        else
                        {
                            short[] vnums =
                            {
                                 4154, 4156, 4155, 4157, 4158, 4087, 4159, 4160, 4702, 4091, 444, 4089, 4336, 398, 445, 4167, 4345, 544, 4198, 930, 4348, 4704, 929, 539, 541, 4086, 943, 397, 950, 4061, 4152

                        };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 31)], 1);

                        }
                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                    }
                    break;

                //FAIRY BOX (OR) 
                case 9979:
                    {
                        int rnd = ServerManager.RandomNumber(0, 100);

                        if (rnd < 2)
                        {
                            short[] vnums =
                            {
                          8009, 8010, 8011, 8012
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 4)], 1);
                        }
                        else if (rnd < 5)
                        {
                            short[] vnums =
                            {
                                8005, 8006, 8007, 8008
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 4)], 1);
                        }
                        else
                        {
                            short[] vnums =
                            {
                                800, 801, 802, 803, 254, 255, 256
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 7)], 1);
                        }
                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                    }
                    break;

                //FAIRY BOX (EVENT) 
                case 9980:
                    {
                        int rnd = ServerManager.RandomNumber(0, 100);

                        if (rnd < 2)
                        {
                            short[] vnums =
                            {
                          4713, 4714, 4715, 4716, 920, 425, 273
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 4)], 1);
                        }
                        else
                        {
                            short[] vnums =
                            {
                                4129, 4130, 4131, 4132, 4705, 4706, 4707, 4708, 4709, 4710, 4711, 4712
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 12)], 1);
                        }
                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                    }
                    break;

                //COSTUME BOX (OR) 
                case 9982:
                    {
                        int rnd = ServerManager.RandomNumber(0, 100);

                        if (rnd < 3)
                        {
                            short[] vnums =
                            {
                            984, 4248, 4303, 4321, 4323, 4377, 4380, 4384, 4388, 4392, 4396, 4402, 4404, 4409, 4425, 4433, 4435, 4441, 4829
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 19)], 1);
                        }
                        else if (rnd < 7)
                        {
                            short[] vnums =
                            {
                           189, 195, 624, 625, 626, 627, 628, 629, 630, 631, 632, 633, 634, 635, 636, 637, 638, 639, 640, 641, 642, 643, 644, 645, 646, 647, 667, 670, 673, 676, 679, 682, 685, 688, 691, 694, 697, 726, 729, 732, 735, 738, 741, 744, 747, 750, 753, 784, 787, 790, 793, 796, 799
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 53)], 1);
                        }
                        else
                        {
                            short[] vnums =
                            {
                            806, 809, 812, 815, 818, 821, 824, 827, 830, 833, 891, 894, 927, 932, 954, 958, 970, 4065, 4073, 4075, 4077, 4107, 4108, 4109, 4110, 4111, 4112, 4113, 4146, 4150, 4177, 4185, 4187, 4196, 4208, 4214, 4225, 4226, 4227, 4237, 4238, 4239, 4244, 4258, 4268, 4285, 4289, 4367                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 48)], 1);
                        }
                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                    }
                    break;

                //HAT BOX (OR) 
                case 9983:
                    {
                        int rnd = ServerManager.RandomNumber(0, 100);

                        if (rnd < 5)
                        {
                            short[] vnums =
                            {
                          988, 4172, 4256, 4301, 4317, 4319, 4375, 4382, 4386, 4390, 4394, 4401, 4411, 4421, 4429, 4439, 4827
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 17)], 1);
                        }
                        else if (rnd < 10)
                        {
                            short[] vnums =
                            {
                        191, 198, 457, 478, 479, 480, 481, 482, 483, 549, 552, 555, 558, 561, 564, 652, 653, 654, 655, 836, 839, 842, 845, 848, 851, 854, 857, 860, 863, 866, 869, 872, 875, 878, 881, 897, 915, 935
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 38)], 1);
                        }
                        else
                        {
                            short[] vnums =
                            {
                        938, 962, 966, 972, 4064, 4074, 4076, 4078, 4114, 4115, 4116, 4117, 4118, 4119, 4120, 4124, 4138, 4142, 4164, 4179, 4181, 4183, 4195, 4204, 4211, 4219, 4220, 4221, 4231, 4232, 4233, 4252, 4260, 4266, 4283, 4287, 4365                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 37)], 1);
                        }
                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                    }
                    break;

                //SKIN WEAPON BOX (EVENT) 
                case 9984:
                    {
                        int rnd = ServerManager.RandomNumber(0, 100);

                        if (rnd < 2)
                        {
                            short[] vnums =
                            {
                         4271
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 1)], 1);
                        }
                        else if (rnd < 5)
                        {
                            short[] vnums =
                            {
                        4273, 4275, 4277, 4279
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 4)], 1);
                        }
                        else
                        {
                            short[] vnums =
                            {
                                4281, 4309, 4310, 4311, 4353, 4354, 4355, 4371, 4372, 4373
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 10)], 1);
                        }
                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                    }
                    break;

                //HAT & MASK BOX (EVENT) 
                case 9985:
                    {
                        int rnd = ServerManager.RandomNumber(0, 100);

                        if (rnd < 1)
                        {
                            short[] vnums =
                            {
                                441, 442
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 2)], 1);
                        }
                        else if (rnd < 3)
                        {
                            short[] vnums =
                            {
                               428, 429, 215, 216, 217, 218, 219, 220, 221, 222, 337, 338, 339, 340, 341, 342, 343, 344, 345, 346, 426, 443
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 22)], 1);
                        }

                        else
                        {
                            short[] vnums =
                            {
                                4021, 4022, 4023, 4024, 4025, 4026, 4027, 4028, 4334, 4344, 4490, 4699, 4755, 4847, 4871, 4929, 4930, 4975, 4987, 4988, 361, 362, 363, 366, 367, 368, 371, 372, 373, 427, 432, 433, 434, 435, 436, 437, 438, 439, 440
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 39)], 1);
                        }
                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                    }
                    break;


                //DAILY REWARD BOX (STANDARD)
                case 9986:
                    {
                        int rnd = ServerManager.RandomNumber(0, 100);

                        if (rnd < 2)
                        {
                            short[] vnums =
                            {

                              10048, 10028, 9131, 9130, 9129, 9036, 9035, 9034, 9033, 9032, 9031
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 11)], 99);
                        }
                        else if (rnd < 5)
                        {
                            short[] vnums =
                            {
                               10065, 10064, 10063, 10062, 10061, 10027, 10026, 10025, 10024, 10023
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 10)], 1);

                        }
                        else
                        {
                            short[] vnums =
                            {
                                10059, 9101, 1134
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 3)], 5);
                        }
                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                    }
                    break;


                //RUNE ARME R8 (OR)
                case 576:
                    {
                        int rnd = ServerManager.RandomNumber(0, 100);
                        {
                            short[] vnums =
                                                               {
                                576 //15x
                            };

                            byte[] counts =
                                {
                              1
                            };
                            int item = ServerManager.RandomNumber(0, 1);

                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 1)], 1, 8, 90);
                            session.SendPacket($"rdi {vnums[item]} {counts[item]}");
                        }
                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                    }

                    break;

                //RUNE ARMURE R8 (OR)
                case 588:
                    {
                        int rnd = ServerManager.RandomNumber(0, 100);
                        {
                            short[] vnums =
                                                               {
                                588 //15x
                            };

                            byte[] counts =
                                {
                              1
                            };
                            int item = ServerManager.RandomNumber(0, 1);

                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 1)], 1, 8, 90);
                            session.SendPacket($"rdi {vnums[item]} {counts[item]}");
                        }
                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                    }

                    break;

                //DAILY REWARD BOX (RARE)
                case 9987:
                    {
                        int rnd = ServerManager.RandomNumber(0, 100);

                        if (rnd < 2)
                        {
                            short[] vnums =
                            {
                                8336, 8042, 8041, 8040, 8039
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 5)], 1);
                        }
                        else if (rnd < 5)
                        {
                            short[] vnums =
                            {
                               10031, 8032, 8031, 8030, 8029, 8028, 8027, 8026, 8025, 8024
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 10)], 1);
                        }
                        else
                        {
                            short[] vnums =
                            {
                                1134
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 1)], 25);
                        }
                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                    }
                    break;

                //CALIGOR (RARE)
                case 9988:
                    {
                        int rnd = ServerManager.RandomNumber(0, 100);

                        if (rnd < 50)
                        {
                            short[] vnums =
                            {
                                1320
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 1)], 1);
                        }

                        else
                        {
                            short[] vnums =
                            {
                                1095
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 1)], 1);
                        }
                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                    }
                    break;

                //CALIGOR (NORMAL)
                case 9989:
                    {
                        int rnd = ServerManager.RandomNumber(0, 100);

                        if (rnd < 50)
                        {
                            short[] vnums =
                            {
                                1092
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 1)], 1);
                        }

                        else
                        {
                            short[] vnums =
                            {
                                1093
                            };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 1)], 1);
                        }
                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                    }
                    break;


            }
        }

        #endregion
    }
}