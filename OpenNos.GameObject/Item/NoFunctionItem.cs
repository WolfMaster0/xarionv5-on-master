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
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject
{
    public class NoFunctionItem : Item
    {
        #region Instantiation

        public NoFunctionItem(ItemDTO item) : base(item)
        {
        }

        #endregion

        #region Methods

        public override void Use(ClientSession session, ref ItemInstance inv, byte option = 0, string[] packetsplit = null)
        {
            switch (Effect)
            {
                case 10:
                    {
                        switch (EffectValue)
                        {
                            case 1:
                                if (session.Character.Inventory.CountItem(1036) < 1 || session.Character.Inventory.CountItem(1013) < 1)
                                {
                                    return;
                                }
                                session.Character.Inventory.RemoveItemAmount(1036);
                                session.Character.Inventory.RemoveItemAmount(1013);
                                if (ServerManager.RandomNumber() < 25)
                                {
                                    switch (ServerManager.RandomNumber(0, 2))
                                    {
                                        case 0:
                                            session.Character.GiftAdd(1015, 1);
                                            break;
                                        case 1:
                                            session.Character.GiftAdd(1016, 1);
                                            break;
                                    }
                                }
                                break;
                            case 2:
                                if (session.Character.Inventory.CountItem(1038) < 1 || session.Character.Inventory.CountItem(1013) < 1)
                                {
                                    return;
                                }
                                session.Character.Inventory.RemoveItemAmount(1038);
                                session.Character.Inventory.RemoveItemAmount(1013);
                                if (ServerManager.RandomNumber() < 25)
                                {
                                    switch (ServerManager.RandomNumber(0, 4))
                                    {
                                        case 0:
                                            session.Character.GiftAdd(1031, 1);
                                            break;
                                        case 1:
                                            session.Character.GiftAdd(1032, 1);
                                            break;
                                        case 2:
                                            session.Character.GiftAdd(1033, 1);
                                            break;
                                        case 3:
                                            session.Character.GiftAdd(1034, 1);
                                            break;
                                    }
                                }
                                break;

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

                            // Shell Armor
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

                            case 3:
                                if (session.Character.Inventory.CountItem(1037) < 1 || session.Character.Inventory.CountItem(1013) < 1)
                                {
                                    return;
                                }
                                session.Character.Inventory.RemoveItemAmount(1037);
                                session.Character.Inventory.RemoveItemAmount(1013);
                                if (ServerManager.RandomNumber() < 25)
                                {
                                    switch (ServerManager.RandomNumber(0, 17))
                                    {
                                        case 0:
                                        case 1:
                                        case 2:
                                        case 3:
                                        case 4:
                                            session.Character.GiftAdd(1017, 1);
                                            break;
                                        case 5:
                                        case 6:
                                        case 7:
                                        case 8:
                                            session.Character.GiftAdd(1018, 1);
                                            break;
                                        case 9:
                                        case 10:
                                        case 11:
                                            session.Character.GiftAdd(1019, 1);
                                            break;
                                        case 12:
                                        case 13:
                                            session.Character.GiftAdd(1020, 1);
                                            break;
                                        case 14:
                                            session.Character.GiftAdd(1021, 1);
                                            break;
                                        case 15:
                                            session.Character.GiftAdd(1022, 1);
                                            break;
                                        case 16:
                                            session.Character.GiftAdd(1023, 1);
                                            break;
                                    }
                                }
                                break;
                        }

                        session.Character.GiftAdd(1014, (byte)ServerManager.RandomNumber(5, 11));
                    }
                    break;
                default:
                    if (!OpenBoxItem(session, inv))
                    {
                        Logger.Warn(string.Format(Language.Instance.GetMessageFromKey("NO_HANDLER_ITEM"), GetType(),
                            VNum, Effect, EffectValue));
                    }
                    break;
            }
        }

        #endregion
    }
}