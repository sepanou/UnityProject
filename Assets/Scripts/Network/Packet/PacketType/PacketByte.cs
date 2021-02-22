namespace Network.Packet.PacketType {
	public sealed class PacketByte: PacketType<byte> {
		public override int? GetFixedSize() => sizeof(byte);
		
		public PacketByte() {}
		public PacketByte(byte[] bytes): base(bytes) { }
		public PacketByte(byte data) { WriteType(data); }
		
		public override byte ReadType()
			=> Read()[0];

		public override void WriteType(byte data)
			=> Write(new [] { data });
	}
}