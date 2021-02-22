using System;

namespace Network.Packet.PacketType {
	public sealed class PacketULong: PacketType<ulong> {
		public override ushort? GetFixedSize() => sizeof(ulong);
		
		public PacketULong() {}
		public PacketULong(byte[] bytes): base(bytes) { }
		public PacketULong(ulong data) { WriteType(data); }
		
		public override ulong ReadType()
			=> BitConverter.ToUInt32(Read(), 0);

		public override void WriteType(ulong data)
			=> Write(BitConverter.GetBytes(data));
	}
}