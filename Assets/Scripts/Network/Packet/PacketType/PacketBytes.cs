namespace Network.Packet.PacketType {
	public sealed class PacketBytes: PacketType<byte[]> {
		public PacketBytes() {}
		public PacketBytes(byte[] bytes): base(bytes) { }
		
		public override byte[] ReadType()
			=> Read();

		public override void WriteType(byte[] data)
			=> Write(data);
	}
}