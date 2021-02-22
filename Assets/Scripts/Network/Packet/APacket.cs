using System;

namespace Network.Packet {
	public abstract class APacket {
		private byte[] _bytes = new byte[0];
		public int Length => _bytes.Length;
		
		public APacket() {}
		public APacket(byte[] bytes) { Write(bytes); }
		
		public virtual int? GetFixedSize() => null;

		public void Write(byte[] bytes) {
			int oldLen = _bytes.Length;
			Array.Resize(ref _bytes, oldLen + bytes.Length);
			bytes.CopyTo(_bytes, oldLen);
		}

		public void Write(byte[] bytes, int offset, int count) {
			int oldLen = _bytes.Length;
			Array.Resize(ref _bytes, oldLen + count);
			Array.Copy(bytes, offset, _bytes, oldLen, count);
		}

		public byte[] Read() {
			byte[] bytes = _bytes;
			_bytes = new byte[0];
			return bytes;
		}

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