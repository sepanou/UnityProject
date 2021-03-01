using System.Text;

namespace Network.Packet.PacketType {
	/// <summary>
	/// PacketType for <see cref="string"/>
	/// </summary>
	public sealed class PacketString: PacketType<string> {
		/// <summary> creates empty packet </summary>
		public PacketString() {}
		/// <summary> creates packet from raw data </summary>
		public PacketString(byte[] bytes): base(bytes) { }
		/// <summary> creates packet from a string </summary>
		public PacketString(string data) { WriteType(data); }

		/// <summary> reads a string from the packet </summary>
		/// <returns> read data </returns>
		public override string ReadType()
			=> Encoding.UTF8.GetString(Read());

		/// <summary> writes a string to the packet </summary>
		/// <param name="data"> data to write </param>
		public override void WriteType(string data)
			=> Write(Encoding.UTF8.GetBytes(data));
	}
}