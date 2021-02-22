using System;

namespace Network.Packet.PacketType {
	public sealed class PacketBool: PacketType<bool> {
		public override int? GetFixedSize() => sizeof(bool);
		
		public static PacketBool Make(bool data)
			=> PacketType<bool>.Make<PacketBool>(data);
		
		public override bool ReadType()
			=> BitConverter.ToBoolean(Read(), 0);

		public override void WriteType(bool data)
			=> Write(BitConverter.GetBytes(data));
	}
}