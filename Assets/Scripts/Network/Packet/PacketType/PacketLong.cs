using System;

namespace Network.Packet.PacketType {
	public sealed class PacketLong: PacketType<long> {
		public override ushort? GetFixedSize() => sizeof(long);
		
		public PacketLong() {}
		public PacketLong(byte[] bytes): base(bytes) { }
		public PacketLong(long data) { WriteType(data); }
		
		public override long ReadType()
			=> BitConverter.ToInt32(Read(), 0);

		public override void WriteType(long data)
			=> Write(BitConverter.GetBytes(data));
	}
}