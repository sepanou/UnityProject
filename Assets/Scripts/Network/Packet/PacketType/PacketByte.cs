namespace Network.Packet.PacketType {
	public sealed class PacketByte: PacketType<byte> {
		public override int? GetFixedSize() => sizeof(byte);
		
		public static PacketByte Make(byte data)
			=> PacketType<byte>.Make<PacketByte>(data);
		
		public override byte ReadType()
			=> Read()[0];

		public override void WriteType(byte data)
			=> Write(new [] { data });
	}
}