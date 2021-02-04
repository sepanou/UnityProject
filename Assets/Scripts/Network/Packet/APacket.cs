using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Network.Packet {
	public abstract class APacket {
		private byte[] _bytes = new byte[0];
		public int Length => _bytes.Length;

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
			byte[] ret = _bytes;
			_bytes = new byte[0];
			return ret;
		}
		
		protected byte[] Read(int length) {
			if (length > _bytes.Length) throw new ArgumentOutOfRangeException($"{nameof(length)}", $"{length}", $"public byte[] testNet.Packet.Read(int): length cannot be superior to packet size {_bytes.Length}");
			byte[] ret = new byte[length];
			Array.Copy(_bytes, 0, ret, 0, length);
			Array.Copy(_bytes, length, _bytes, 0, _bytes.Length - length);
			Array.Resize(ref _bytes, length);
			return ret;
		}
		
		public void Send<T>(T stream) where T: Stream {
			stream.Write(_bytes, 0, _bytes.Length);
		}
		
		public async Task SendAsync<T>(T stream) where T: Stream {
			await stream.WriteAsync(_bytes, 0, _bytes.Length);
		}

		public void Receive(NetworkStream stream) {
			byte[] bytes = Array.Empty<byte>();
			Array.Resize(ref bytes, 1024);
			while (stream.DataAvailable) {
				int bytesRead = stream.Read(bytes, 0, bytes.Length);
				Write(bytes, 0, bytesRead);
			}
		}

		public async Task ReceiveAsync(NetworkStream stream) {
			byte[] bytes = new byte[1024];
			while (stream.DataAvailable) {
				int bytesRead = await stream.ReadAsync(bytes, 0, bytes.Length);
				Write(bytes, 0, bytesRead);
			}
		}
	}
}