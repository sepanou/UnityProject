using System;

namespace Network.Packet.PacketType {
	public sealed class PacketUShort: PacketType<ushort> {
		public override int? GetFixedSize() => sizeof(ushort);
		
		public PacketUShort() {}
		public PacketUShort(byte[] bytes): base(bytes) { }
		public PacketUShort(ushort data) { WriteType(data); }
		
		public override ushort ReadType()
			=> BitConverter.ToUInt16(Read(), 0);

		public override void WriteType(ushort data)
			=> Write(BitConverter.GetBytes(data));
	}
}