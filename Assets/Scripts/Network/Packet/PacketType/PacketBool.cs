using System;

namespace Network.Packet.PacketType {
	public sealed class PacketBool: PacketType<bool> {
		public override ushort? GetFixedSize() => sizeof(bool);
		
		public PacketBool() {}
		public PacketBool(byte[] bytes): base(bytes) { }
		public PacketBool(bool data) { WriteType(data); }
		
		public override bool ReadType()
			=> BitConverter.ToBoolean(Read(), 0);

		public override void WriteType(bool data)
			=> Write(BitConverter.GetBytes(data));
	}
}