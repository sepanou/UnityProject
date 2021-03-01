using BitConverter = System.BitConverter;

namespace Network.Packet.PacketType {
	/// <summary>
	/// PacketType for <see cref="double"/>
	/// </summary>
	public sealed class PacketDouble: PacketType<double> {
		/// <summary> a double has a fixed size of 8 byte </summary>
		/// <returns> fixed size </returns>
		public override ushort? GetFixedSize() => sizeof(double);
		
		/// <summary> creates empty packet </summary>
		public PacketDouble() {}
		/// <summary> creates packet from raw data </summary>
		public PacketDouble(byte[] bytes): base(bytes) { }
		/// <summary> creates packet from a double </summary>
		public PacketDouble(double data) { WriteType(data); }

		/// <summary> reads a double from the packet </summary>
		/// <returns> read data </returns>
		public override double ReadType()
			=> BitConverter.ToDouble(Read(), 0);

		/// <summary> writes a double to the packet </summary>
		/// <param name="data"> data to write </param>
		public override void WriteType(double data)
			=> Write(BitConverter.GetBytes(data));
	}
}