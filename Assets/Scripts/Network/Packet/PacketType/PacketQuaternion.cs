using BitConverter = System.BitConverter;
using Array = System.Array;
using Quaternion = UnityEngine.Quaternion;

namespace Network.Packet.PacketType {
	/// <summary>
	/// PacketType for Unity's <see cref="Quaternion"/> (4x float)
	/// </summary>
	public sealed class PacketQuaternion: PacketType<Quaternion> {
		/// <summary> a quaternion has a fixed size of 16 bytes (4 floats) </summary>
		/// <returns> fixed size </returns>
		public override ushort? GetFixedSize() => 4 * sizeof(float);
		
		/// <summary> creates empty packet </summary>
		public PacketQuaternion() {}
		/// <summary> creates packet from raw data </summary>
		public PacketQuaternion(byte[] bytes): base(bytes) { }
		/// <summary> creates packet from a quaternion </summary>
		public PacketQuaternion(Quaternion data) { WriteType(data); }

		/// <summary> reads a quaternion from the packet </summary>
		/// <returns> read data </returns>
		public override Quaternion ReadType() {
			byte[] buff = Read();
			return new Quaternion {
				x = BitConverter.ToSingle(buff, 0),
				y = BitConverter.ToSingle(buff, 4),
				z = BitConverter.ToSingle(buff, 8),
				w = BitConverter.ToSingle(buff, 12)
			};
		}

		/// <summary> writes a quaternion to the packet </summary>
		/// <param name="data"> data to write </param>
		public override void WriteType(Quaternion data) {
			const int fSize = sizeof(float);
			byte[] bytes = new byte[4 * fSize];
			Array.Copy(BitConverter.GetBytes(data.x), 0, bytes, 0, fSize);
			Array.Copy(BitConverter.GetBytes(data.y), 0, bytes, fSize, fSize);
			Array.Copy(BitConverter.GetBytes(data.z), 0, bytes, 2 * fSize, fSize);
			Array.Copy(BitConverter.GetBytes(data.w), 0, bytes, 3 * fSize, fSize);
			Write(bytes);
		}
	}
}