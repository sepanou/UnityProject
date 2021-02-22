using System;

namespace Network.Packet.PacketType {
	public sealed class PacketInt: PacketType<int> {
		public override int? GetFixedSize() => sizeof(int);
		
		public static PacketInt Make(int data)
			=> PacketType<int>.Make<PacketInt>(data);
		
		public override int ReadType()
			=> BitConverter.ToInt32(Read(), 0);

		public override void WriteType(int data)
			=> Write(BitConverter.GetBytes(data));
	}
}