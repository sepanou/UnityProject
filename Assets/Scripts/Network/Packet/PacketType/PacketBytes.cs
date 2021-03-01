namespace Network.Packet.PacketType {
	/// <summary>
	/// PacketType for <see cref="byte"/> arrays
	/// </summary>
	public sealed class PacketBytes: PacketType<byte[]> {
		/// <summary> creates empty packet </summary>
		public PacketBytes() {}
		/// <summary> creates packet from raw data / byte array </summary>
		public PacketBytes(byte[] bytes): base(bytes) { }

		/// <summary> reads a byte array from the packet </summary>
		/// <returns> read data </returns>
		public override byte[] ReadType()
			=> Read();

		/// <summary> writes a byte array to the packet </summary>
		/// <param name="data"> data to write </param>
		public override void WriteType(byte[] data)
			=> Write(data);
	}
}