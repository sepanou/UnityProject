using System;

namespace Network.Packet.PacketType {
	public sealed class PacketDouble: PacketType<double> {
		public override ushort? GetFixedSize() => sizeof(double);
		
		public PacketDouble() {}
		public PacketDouble(byte[] bytes): base(bytes) { }
		public PacketDouble(double data) { WriteType(data); }
		
		public override double ReadType()
			=> BitConverter.ToDouble(Read(), 0);

		public override void WriteType(double data)
			=> Write(BitConverter.GetBytes(data));
	}
}