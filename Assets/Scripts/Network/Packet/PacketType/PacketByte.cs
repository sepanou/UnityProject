namespace Network.Packet.PacketType {
	/// <summary>
	/// PacketType for <see cref="byte"/>
	/// </summary>
	public sealed class PacketByte: PacketType<byte> {
		/// <summary> a byte has a fixed size of 1 byte </summary>
		/// <returns> fixed size </returns>
		public override ushort? GetFixedSize() => sizeof(byte);
		
		/// <summary> creates empty packet </summary>
		public PacketByte() {}
		/// <summary> creates packet from raw data </summary>
		public PacketByte(byte[] bytes): base(bytes) { }
		/// <summary> creates packet from a byte </summary>
		public PacketByte(byte data) { WriteType(data); }

		/// <summary> reads a byte from the packet </summary>
		/// <returns> read data </returns>
		public override byte ReadType()
			=> Read()[0];

		/// <summary> writes a byte to the packet </summary>
		/// <param name="data"> data to write </param>
		public override void WriteType(byte data)
			=> Write(new [] { data });
	}
}