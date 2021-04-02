using System;

namespace NetworkMessages
{
    [Serializable]
    public abstract class Message
    {
        public uint SimulationFrame;
        public uint ServerTick;
        public float DeltaTime;
    }
}
