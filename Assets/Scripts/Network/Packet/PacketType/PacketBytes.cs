namespace Network.Packet.PacketType {
	public sealed class PacketBytes: PacketType<byte[]> {
		public override byte[] ReadType()
			=> Read();

		public override void WriteType(byte[] data)
			=> Write(data);
	}
}