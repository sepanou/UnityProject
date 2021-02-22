using System;

namespace Network.Packet.PacketType {
	public sealed class PacketLong: PacketType<long> {
		private static readonly int? FixedSize = sizeof(long);
		public override int? GetFixedSize() => FixedSize;
		
		public static PacketLong Make(long data)
			=> PacketType<long>.Make<PacketLong>(data);
		
		public override long ReadType()
			=> BitConverter.ToInt32(Read(), 0);

		public override void WriteType(long data)
			=> Write(BitConverter.GetBytes(data));
	}
}