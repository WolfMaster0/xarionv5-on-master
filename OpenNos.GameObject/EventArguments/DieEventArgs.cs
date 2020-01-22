using System;
using OpenNos.Domain;

namespace OpenNos.GameObject.EventArguments
{
    public class DieEventArgs : EventArgs
    {
        public DieEventArgs(UserType type, object killingEntity)
        {
            UserType = type;
            KillingEntity = killingEntity;
        }

        public UserType UserType { get; }

        public object KillingEntity { get; }
    }
}