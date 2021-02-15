using System;
using UnityEngine;

namespace Network.Packet.PacketType {
	public sealed class PacketRotation: PacketType<Quaternion> {
		public PacketRotation() { }
		public PacketRotation(Quaternion data) => Write(data);

		public override void Read(out Quaternion rotation) {
			byte[] buff = Read();
			rotation = new Quaternion {
				x = BitConverter.ToSingle(buff, 0),
				y = BitConverter.ToSingle(buff, 4),
				z = BitConverter.ToSingle(buff, 8),
				w = BitConverter.ToSingle(buff, 12)
			};
		}

		public override void Write(Quaternion position) {
			byte[] bytes = new byte[16]; // 4 bytes per float
 
			Buffer.BlockCopy( BitConverter.GetBytes( position.x ), 0, bytes, 0, 4 );
			Buffer.BlockCopy( BitConverter.GetBytes( position.y ), 0, bytes, 4, 4 );
			Buffer.BlockCopy( BitConverter.GetBytes( position.z ), 0, bytes, 8, 4 );
			Buffer.BlockCopy( BitConverter.GetBytes( position.w ), 0, bytes, 12, 4 );
			
			Write(bytes);
		}
	}
}