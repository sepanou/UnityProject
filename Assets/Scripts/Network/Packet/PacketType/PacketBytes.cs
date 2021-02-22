namespace Network.Packet.PacketType {
	public sealed class PacketBytes: PacketType<byte[]> {
		public static PacketBytes Make(byte[] data)
			=> PacketType<byte[]>.Make<PacketBytes>(data);
		
		public override byte[] ReadType()
			=> Read();

		public override void WriteType(byte[] data)
			=> Write(data);
	}
}