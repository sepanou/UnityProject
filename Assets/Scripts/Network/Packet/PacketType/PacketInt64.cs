using System;

namespace Network.Packet.PacketType {
	public sealed class PacketInt64: PacketType<long> {
		public PacketInt64() { }

		public PacketInt64(long data) => Write(data);

		public override void Read(out long data) {
			data = BitConverter.ToInt32(Read(), 0);
		}

		public override void Write(long data) {
			Write(BitConverter.GetBytes(data));
		}
	}
}