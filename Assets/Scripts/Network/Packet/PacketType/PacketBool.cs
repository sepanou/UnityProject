using BitConverter = System.BitConverter;

namespace Network.Packet.PacketType {
	/// <summary>
	/// PacketType for <see cref="bool"/>
	/// </summary>
	public sealed class PacketBool: PacketType<bool> {
		/// <summary> a bool has a fixed size of 1 byte </summary>
		/// <returns> fixed size </returns>
		public override ushort? GetFixedSize() => sizeof(bool);
		
		/// <summary> creates empty packet </summary>
		public PacketBool() {}
		/// <summary> creates packet from raw data </summary>
		public PacketBool(byte[] bytes): base(bytes) { }
		/// <summary> creates packet from a bool </summary>
		public PacketBool(bool data) { WriteType(data); }
		
		/// <summary> reads a bool from the packet </summary>
		/// <returns> read data </returns>
		public override bool ReadType()
			=> BitConverter.ToBoolean(Read(), 0);

		/// <summary> writes a bool to the packet </summary>
		/// <param name="data"> data to write </param>
		public override void WriteType(bool data)
			=> Write(BitConverter.GetBytes(data));
	}
}