using Array = System.Array;

namespace Network.Packet {
	/// <summary>
	/// abstract class to manage packets
	/// interface between C# objects and raw data
	/// </summary>
	public abstract class APacket {
		/// <summary> raw data </summary>
		private byte[] _bytes = new byte[0];
		/// <summary> length of the packet in bytes </summary>
		public ushort Length => (ushort)_bytes.Length;
		
		/// <summary> creates empty packet </summary>
		protected APacket() {}
		/// <summary> creates packet from raw data </summary>
		protected APacket(byte[] bytes) { Write(bytes); }
		
		/// <summary>
		/// is the size of each packet of this type fixed ?
		/// and if yes what is this precise size
		/// </summary>
		public virtual ushort? GetFixedSize() => null;

		/// <summary> writes raw data to the packet </summary>
		/// <param name="bytes"> raw data </param>
		public void Write(byte[] bytes) {
			int oldLen = _bytes.Length;
			Array.Resize(ref _bytes, oldLen + bytes.Length);
			bytes.CopyTo(_bytes, oldLen);
		}


		/// <summary> writes raw data to the packet </summary>
		/// <param name="bytes"> raw data </param>
		/// <param name="offset"> raw data's start index </param>
		/// <param name="count"> raw data's length </param>
		public void Write(byte[] bytes, int offset, int count) {
			int oldLen = _bytes.Length;
			Array.Resize(ref _bytes, oldLen + count);
			Array.Copy(bytes, offset, _bytes, oldLen, count);
		}

		/// <summary> reads packet as raw data </summary>
		/// <returns> raw data </returns>
		public byte[] Read() {
			byte[] bytes = _bytes;
			_bytes = new byte[0];
			return bytes;
		}

		/// <summary> reads packet's part as raw data </summary>
		/// <param name="length"> packet's part's length </param>
		/// <returns> raw data </returns>
		protected byte[] Read(int length) {
			if (length >= _bytes.Length) return Read();
			byte[] bytes = new byte[_bytes.Length - length];
			Array.Copy(_bytes, length, bytes, 0, _bytes.Length - length);
			Array.Resize(ref _bytes, length);
			(_bytes, bytes) = (bytes, _bytes);
			return bytes;
		}
	}
}