using System;

namespace Network.Packet.PacketType {
	public sealed class PacketDouble: PacketType<double> {
		public PacketDouble() { }

		public PacketDouble(double data) => Write(data);

		public override void Read(out double data) {
			data = BitConverter.ToDouble(Read(), 0);
		}

		public override void Write(double data) {
			Write(BitConverter.GetBytes(data));
		}
	}
}