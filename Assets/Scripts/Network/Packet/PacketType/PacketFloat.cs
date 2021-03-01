using BitConverter = System.BitConverter;

namespace Network.Packet.PacketType {
	/// <summary>
	/// PacketType for <see cref="float"/>
	/// </summary>
	public sealed class PacketFloat: PacketType<float> {
		/// <summary> a float has a fixed size of 4 bytes </summary>
		/// <returns> fixed size </returns>
		public override ushort? GetFixedSize() => sizeof(float);
		
		/// <summary> creates empty packet </summary>
		public PacketFloat() {}
		/// <summary> creates packet from raw data </summary>
		public PacketFloat(byte[] bytes): base(bytes) { }
		/// <summary> creates packet from a float </summary>
		public PacketFloat(float data) { WriteType(data); }

		/// <summary> reads a float from the packet </summary>
		/// <returns> read data </returns>
		public override float ReadType()
			=>  BitConverter.ToSingle(Read(), 0);

		/// <summary> writes a float to the packet </summary>
		/// <param name="data"> data to write </param>
		public override void WriteType(float data)
			=> Write(BitConverter.GetBytes(data));
	}
}