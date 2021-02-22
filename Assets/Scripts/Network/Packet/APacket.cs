using System;

namespace Network.Packet {
	public abstract class APacket {
		private byte[] _bytes = new byte[0];
		public int Length => _bytes.Length;

		private static readonly int? FixedSize = null;
		public virtual int? GetFixedSize() => FixedSize;

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

		public byte[] Read()
			=> (byte[])_bytes.Clone();

		protected byte[] Read(int length) {
			if (length > _bytes.Length) throw new ArgumentOutOfRangeException($"{nameof(length)}", $"{length}", $"public byte[] testNet.Packet.Read(int): length cannot be superior to packet size {_bytes.Length}");
			byte[] ret = new byte[length];
			Array.Copy(_bytes, 0, ret, 0, length);
			Array.Copy(_bytes, length, _bytes, 0, _bytes.Length - length);
			Array.Resize(ref _bytes, length);
			return ret;
		}
	}
}