using BitConverter = System.BitConverter;

namespace Network.Packet.PacketType {
	/// <summary>
	/// PacketType for <see cref="int"/>
	/// </summary>
	public sealed class PacketInt: PacketType<int> {
		/// <summary> an int has a fixed size of 4 bytes </summary>
		/// <returns> fixed size </returns>
		public override ushort? GetFixedSize() => sizeof(int);
		
		/// <summary> creates empty packet </summary>
		public PacketInt() {}
		/// <summary> creates packet from raw data </summary>
		public PacketInt(byte[] bytes): base(bytes) { }
		/// <summary> creates packet from an int </summary>
		public PacketInt(int data) { WriteType(data); }

		/// <summary> reads an int from the packet </summary>
		/// <returns> read data </returns>
		public override int ReadType()
			=> BitConverter.ToInt32(Read(), 0);

		/// <summary> writes an int to the packet </summary>
		/// <param name="data"> data to write </param>
		public override void WriteType(int data)
			=> Write(BitConverter.GetBytes(data));
	}
}