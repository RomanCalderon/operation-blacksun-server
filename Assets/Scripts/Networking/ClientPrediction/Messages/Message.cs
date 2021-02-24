using System;

namespace NetworkMessages
{
    [Serializable]
    public abstract class Message
    {
        public int SimulationFrame;
        public float DeltaTime;
    }
}
