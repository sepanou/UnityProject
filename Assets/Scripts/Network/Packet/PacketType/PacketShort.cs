using System;

namespace Network.Packet.PacketType {
	public sealed class PacketShort: PacketType<short> {
		public override int? GetFixedSize() => sizeof(short);
		
		public static PacketShort Make(short data)
			=> PacketType<short>.Make<PacketShort>(data);
		
		public override short ReadType()
			=> BitConverter.ToInt16(Read(), 0);

		public override void WriteType(short data)
			=> Write(BitConverter.GetBytes(data));
	}
}