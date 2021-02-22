using System;

namespace Network.Packet.PacketType {
	public sealed class PacketUInt: PacketType<uint> {
		public override ushort? GetFixedSize() => sizeof(uint);
		
		public PacketUInt() {}
		public PacketUInt(byte[] bytes): base(bytes) { }
		public PacketUInt(uint data) { WriteType(data); }
		
		public override uint ReadType()
			=> BitConverter.ToUInt32(Read(), 0);

		public override void WriteType(uint data)
			=> Write(BitConverter.GetBytes(data));
	}
}