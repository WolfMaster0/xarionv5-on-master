using System;
using System.Xml.Serialization;

namespace OpenNos.XMLModel.Event.Events
{
    [Serializable]
    public class SpawnMonster
    {
        #region Properties

        [XmlAttribute]
        public bool IsHostile { get; set; }

        [XmlAttribute]
        public bool Move { get; set; }

        [XmlAttribute]
        public byte Direction { get; set; }

        [XmlAttribute]
        public short MapId { get; set; }

        [XmlAttribute]
        public short PositionX { get; set; }

        [XmlAttribute]
        public short PositionY { get; set; }

        [XmlAttribute]
        public short VNum { get; set; }

        [XmlAttribute]
        public bool ShouldRespawn { get; set; }

        #endregion
    }
}