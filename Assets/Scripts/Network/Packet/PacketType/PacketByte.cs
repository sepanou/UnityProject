namespace Network.Packet.PacketType {
	public sealed class PacketByte: PacketType<byte> {
		public override byte ReadType()
			=> Read()[0];

		public override void WriteType(byte data)
			=> Write(new [] { data });
	}
}