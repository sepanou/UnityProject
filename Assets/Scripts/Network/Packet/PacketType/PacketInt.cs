using System;

namespace Network.Packet.PacketType {
	public sealed class PacketInt: PacketType<int> {
		public override ushort? GetFixedSize() => sizeof(int);
		
		public PacketInt() {}
		public PacketInt(byte[] bytes): base(bytes) { }
		public PacketInt(int data) { WriteType(data); }
		
		public override int ReadType()
			=> BitConverter.ToInt32(Read(), 0);

		public override void WriteType(int data)
			=> Write(BitConverter.GetBytes(data));
	}
}