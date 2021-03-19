using System;

namespace NetworkMessages
{
    [Serializable]
    public abstract class Message
    {
        public uint SimulationFrame;
        public float DeltaTime;
    }
}
