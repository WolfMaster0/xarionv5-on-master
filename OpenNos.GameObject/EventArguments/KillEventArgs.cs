using System;
using OpenNos.Domain;

namespace OpenNos.GameObject.EventArguments
{
    public class KillEventArgs : EventArgs
    {
        public KillEventArgs(UserType type, object killedEntity)
        {
            UserType = type;
            KilledEntity = killedEntity;
        }

        public UserType UserType { get; }

        public object KilledEntity { get; }
    }
}