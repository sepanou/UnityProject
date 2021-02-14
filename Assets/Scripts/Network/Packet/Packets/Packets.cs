using System;

namespace Network.Packet.Packets {
	public abstract class APackets: APacket {
		private readonly byte _size;
		protected APackets(byte size) { _size = size; }
		
		public virtual void Add(APacket packet) {
			byte[] packetMem = new byte[packet.Length + _size];
			int length = packet.Length;
			for (int i = 0; i < _size; ++i) {
				packetMem[packet.Length + _size - i - 1] = (byte)(length & 255);
				length >>= 8;
			}
			if (length != 0) throw new Exception($"{nameof(Add)}: packet was too big to add!");
			packet.Read().CopyTo(packetMem, 0);
			Write(packetMem);
		}
		
		public virtual TPacket Get<TPacket>() where TPacket: APacket, new() {
			int length = 0;
			foreach (byte b in Read(_size)) {
				length <<= 8;
				length |= b;
			}
			TPacket packet = new TPacket();
			packet.Write(Read(length));
			return packet;
		}
	}
	
	public class ShortPackets: APackets {
		public ShortPackets(): base(2) {  }
	}
	public class Packets: APackets {
		public Packets(): base(4) {  }
	}
	public class LongPackets: APackets {
		public LongPackets(): base(8) {  }
	}
}