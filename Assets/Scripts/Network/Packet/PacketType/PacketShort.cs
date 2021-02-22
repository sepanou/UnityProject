using System;

namespace Network.Packet.PacketType {
	public sealed class PacketShort: PacketType<short> {
		public override ushort? GetFixedSize() => sizeof(short);
		
		public PacketShort() {}
		public PacketShort(byte[] bytes): base(bytes) { }
		public PacketShort(short data) { WriteType(data); }
		
		public override short ReadType()
			=> BitConverter.ToInt16(Read(), 0);

		public override void WriteType(short data)
			=> Write(BitConverter.GetBytes(data));
	}
}