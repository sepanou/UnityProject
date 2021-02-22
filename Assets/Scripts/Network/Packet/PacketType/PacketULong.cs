using System;

namespace Network.Packet.PacketType {
	public sealed class PacketULong: PacketType<ulong> {
		public override int? GetFixedSize() => sizeof(ulong);
		
		public static PacketULong Make(ulong data)
			=> PacketType<ulong>.Make<PacketULong>(data);
		
		public override ulong ReadType()
			=> BitConverter.ToUInt32(Read(), 0);

		public override void WriteType(ulong data)
			=> Write(BitConverter.GetBytes(data));
	}
}