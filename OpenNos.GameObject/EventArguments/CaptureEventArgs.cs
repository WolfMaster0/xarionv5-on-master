using System;

namespace OpenNos.GameObject.EventArguments
{
    public class CaptureEventArgs : EventArgs
    {
        public CaptureEventArgs(Mate mate) => Mate = mate;

        public Mate Mate { get; }
    }
}