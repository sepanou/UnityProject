using System;

namespace Network.Packet.PacketType {
	public sealed class PacketInt32: PacketType<int> {
		public PacketInt32() { }

		public PacketInt32(int data) => Write(data);

		public override void Read(out int data) {
			data = BitConverter.ToInt32(Read(), 0);
		}

		public override void Write(int data) {
			Write(BitConverter.GetBytes(data));
		}
	}
}