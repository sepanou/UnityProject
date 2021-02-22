using System;

namespace Network.Packet.PacketType {
	public sealed class PacketUInt: PacketType<uint> {
		public override int? GetFixedSize() => sizeof(uint);
		
		public static PacketUInt Make(uint data)
			=> PacketType<uint>.Make<PacketUInt>(data);
		
		public override uint ReadType()
			=> BitConverter.ToUInt32(Read(), 0);

		public override void WriteType(uint data)
			=> Write(BitConverter.GetBytes(data));
	}
}