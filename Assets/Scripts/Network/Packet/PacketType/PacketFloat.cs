using System;

namespace Network.Packet.PacketType {
    public sealed class PacketFloat : PacketType<float> {
        public PacketFloat() { }

        public PacketFloat(float data) => Write(data);

        public override void Read(out float data) {
            data = BitConverter.ToSingle(Read(), 0);
        }

        public override void Write(float data) {
            Write(BitConverter.GetBytes(data));
        }
    }
}