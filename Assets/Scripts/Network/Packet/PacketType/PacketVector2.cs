using BitConverter = System.BitConverter;
using Array = System.Array;
using Vector2 = UnityEngine.Vector2;

namespace Network.Packet.PacketType {
	/// <summary>
	/// PacketType for <see cref="Vector2"/>
	/// </summary>
	public sealed class PacketVector2: PacketType<Vector2> {
		/// <summary> a vector2 has a fixed size of 8 bytes (2 floats) </summary>
		/// <returns> fixed size </returns>
		public override ushort? GetFixedSize() => 2 * sizeof(float);
		
		/// <summary> creates empty packet </summary>
		public PacketVector2() {}
		/// <summary> creates packet from raw data </summary>
		public PacketVector2(byte[] bytes): base(bytes) { }
		/// <summary> creates packet from a vector2 </summary>
		public PacketVector2(Vector2 data) { WriteType(data); }
		
		/// <summary> reads a vector2 from the packet </summary>
		/// <returns> read data </returns>
		public override Vector2 ReadType() {
			byte[] buff = Read();
			return new Vector2 {
				x = BitConverter.ToSingle(buff, 0),
				y = BitConverter.ToSingle(buff, 4)
			};
		}

		/// <summary> writes a vector2 to the packet </summary>
		/// <param name="data"> data to write </param>
		public override void WriteType(Vector2 data) {
			const int fSize = sizeof(float);
			byte[] bytes = new byte[2 * fSize];
			Array.Copy(BitConverter.GetBytes(data.x), 0, bytes, 0, fSize);
			Array.Copy(BitConverter.GetBytes(data.y), 0, bytes, fSize, fSize);
			Write(bytes);
		}
	}
}