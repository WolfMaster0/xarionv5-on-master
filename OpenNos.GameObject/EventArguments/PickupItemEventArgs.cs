using System;

namespace OpenNos.GameObject.EventArguments
{
    public class PickupItemEventArgs : EventArgs
    {
        public PickupItemEventArgs(Item item) => Item = item;

        public Item Item { get; }
    }
}