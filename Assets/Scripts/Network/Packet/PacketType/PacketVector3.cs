using BitConverter = System.BitConverter;
using Array = System.Array;
using Vector3 = UnityEngine.Vector3;

namespace Network.Packet.PacketType {
	/// <summary>
	/// PacketType for <see cref="Vector3"/>
	/// </summary>
	public sealed class PacketVector3: PacketType<Vector3> {
		/// <summary> a vector2 has a fixed size of 12 bytes (3 floats) </summary>
		/// <returns> fixed size </returns>
		public override ushort? GetFixedSize() => 3 * sizeof(float);
		
		/// <summary> creates empty packet </summary>
		public PacketVector3() {}
		/// <summary> creates packet from raw data </summary>
		public PacketVector3(byte[] bytes): base(bytes) { }
		/// <summary> creates packet from a vector3 </summary>
		public PacketVector3(Vector3 data) { WriteType(data); }
		
		/// <summary> reads a vector3 from the packet </summary>
		/// <returns> read data </returns>
		public override Vector3 ReadType() {
			byte[] buff = Read();
			return new Vector3 {
				x = BitConverter.ToSingle(buff, 0),
				y = BitConverter.ToSingle(buff, 4),
				z = BitConverter.ToSingle(buff, 8)
			};
		}

		/// <summary> writes a vector3 to the packet </summary>
		/// <param name="data"> data to write </param>
		public override void WriteType(Vector3 data) {
			const int fSize = sizeof(float);
			byte[] bytes = new byte[3 * fSize];
			Array.Copy(BitConverter.GetBytes(data.x), 0, bytes, 0, fSize);
			Array.Copy(BitConverter.GetBytes(data.y), 0, bytes, fSize, fSize);
			Array.Copy(BitConverter.GetBytes(data.z), 0, bytes, 2 * fSize, fSize);
			Write(bytes);
		}
	}
}