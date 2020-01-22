using System;
using OpenNos.Domain;

namespace OpenNos.GameObject.EventArguments
{
    public class HitEventArgs : EventArgs
    {
        public HitEventArgs(UserType type, object senderEntity, Skill skill, int damage)
        {
            UserType = type;
            SenderEntity = senderEntity;
            Damage = damage;
            Skill = skill;
        }

        public UserType UserType { get; }

        public object SenderEntity { get; }

        public Skill Skill { get; }

        public int Damage { get; }
    }
}