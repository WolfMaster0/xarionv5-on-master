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
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.SharedPacketMethods
{
    internal static class SharedScriptedInstanceMethods
    {
        #region Methods

        internal static void EnterInstance(this ClientSession session, ScriptedInstance input)
        {

            if (input != null)
            {
                ScriptedInstance instance = new ScriptedInstance(input);
                instance.LoadGlobals();
                instance.LoadScript(MapInstanceType.TimeSpaceInstance);
                if (instance.FirstMap == null)
                {
                    return;
                }

                if (session.Character.Level < instance.LevelMinimum)
                {
                    session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_REQUIERED_LEVEL"), 0));
                    return;
                }

                foreach (Gift gift in instance.RequiredItems)
                {
                    if (session.Character.Inventory.CountItem(gift.VNum) < gift.Amount)
                    {
                        session.SendPacket(UserInterfaceHelper.GenerateMsg(
                            string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_REQUIERED_ITEM"),
                                ServerManager.GetItem(gift.VNum).Name), 0));
                        return;
                    }

                    session.Character.Inventory.RemoveItemAmount(gift.VNum, gift.Amount);
                }

                session.Character.MapX = instance.PositionX;
                session.Character.MapY = instance.PositionY;
                ServerManager.Instance.TeleportOnRandomPlaceInMap(session, instance.FirstMap.MapInstanceId);
                instance.InstanceBag.CreatorId = session.Character.CharacterId;
                session.SendPackets(instance.GenerateMinimap());
                session.SendPacket(instance.GenerateMainInfo());
                session.SendPacket(instance.FirstMap.InstanceBag.GenerateScore());

                session.Character.Timespace = instance;
            }
        }

        #endregion
    }
}