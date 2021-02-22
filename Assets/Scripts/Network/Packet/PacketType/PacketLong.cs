using BitConverter = System.BitConverter;

namespace Network.Packet.PacketType {
	/// <summary>
	/// PacketType for <see cref="long"/>
	/// </summary>
	public sealed class PacketLong: PacketType<long> {
		/// <summary> a long has a fixed size of 8 bytes </summary>
		/// <returns> fixed size </returns>
		public override ushort? GetFixedSize() => sizeof(long);
		
		/// <summary> creates empty packet </summary>
		public PacketLong() {}
		/// <summary> creates packet from raw data </summary>
		public PacketLong(byte[] bytes): base(bytes) { }
		/// <summary> creates packet from a long </summary>
		public PacketLong(long data) { WriteType(data); }
		
		/// <summary> reads a long from the packet </summary>
		/// <returns> read data </returns>
		public override long ReadType()
			=> BitConverter.ToInt32(Read(), 0);

		/// <summary> writes a long to the packet </summary>
		/// <param name="data"> data to write </param>
		public override void WriteType(long data)
			=> Write(BitConverter.GetBytes(data));
	}
}