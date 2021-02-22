using System;

namespace Network.Packet.PacketType {
	public sealed class PacketDouble: PacketType<double> {
		public override int? GetFixedSize() => sizeof(double);
		
		public static PacketDouble Make(double data)
			=> PacketType<double>.Make<PacketDouble>(data);
		
		public override double ReadType()
			=> BitConverter.ToDouble(Read(), 0);

		public override void WriteType(double data)
			=> Write(BitConverter.GetBytes(data));
	}
}