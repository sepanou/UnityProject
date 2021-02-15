using System;
using System.Text;
using UnityEngine;

namespace Network.Packet.PacketType {
	public sealed class PacketPosition: PacketType<Vector3> {
		public PacketPosition() { }
		public PacketPosition(Vector3 data) => Write(data);

		public override void Read(out Vector3 position) {
			byte[] buff = Read();
			position = new Vector3 {
				x = BitConverter.ToSingle(buff, 0),
				y = BitConverter.ToSingle(buff, 4),
				z = BitConverter.ToSingle(buff, 8)
			};
		}

		public override void Write(Vector3 position) {
			byte[] bytes = new byte[12]; // 4 bytes per float
 
			Buffer.BlockCopy( BitConverter.GetBytes( position.x ), 0, bytes, 0, 4 );
			Buffer.BlockCopy( BitConverter.GetBytes( position.y ), 0, bytes, 4, 4 );
			Buffer.BlockCopy( BitConverter.GetBytes( position.z ), 0, bytes, 8, 4 );
			
			Write(bytes);
		}
	}
}