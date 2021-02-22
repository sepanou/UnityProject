namespace Network.Packet.PacketType {
	public sealed class PacketByte: PacketType<byte> {
		private static readonly int? FixedSize = sizeof(byte);
		public override int? GetFixedSize() => FixedSize;
		
		public static PacketByte Make(byte data)
			=> PacketType<byte>.Make<PacketByte>(data);
		
		public override byte ReadType()
			=> Read()[0];

		public override void WriteType(byte data)
			=> Write(new [] { data });
	}
}