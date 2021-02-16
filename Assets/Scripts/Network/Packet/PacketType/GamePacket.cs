using System;
using System.Runtime.Serialization;
using UnityEngine;

namespace Network.Packet.PacketType {
    public sealed class GamePacket : APacket {
        public GamePacket() { }

        public GamePacket(Client.ClientPacketEnum type, object content) =>
            Write((byte) type, ConvertObjectInPacket(content));

        public GamePacket(Server.ServerPacketEnum type, object content) =>
            Write((byte) type, ConvertObjectInPacket(content));

        public static APacket ConvertObjectInPacket(object content) {
            return content switch {
                byte value => new PacketByte(value),
                double value => new PacketDouble(value),
                float value => new PacketFloat(value),
                int value => new PacketInt32(value),
                long value => new PacketInt64(value),
                Vector3 position => new PacketPosition(position),
                Quaternion rotation => new PacketRotation(rotation),
                string value => new PacketString(value),
                _ => throw new SerializationException(
                    $"public APacket Network.Packet.PacketType.GamePacket.ConvertObjectInPacket(object): Can't serialize a {content.GetType().Name} into a Packet")
            };
        }

        public void Write(byte type, APacket content) {
            Write(new PacketByte(type).Read());
            Write(content.Read());
        }

        public void ReadType(out Client.ClientPacketEnum type) {
            new PacketByte(Read(1)[0]).Read(out byte typeByte);
            type = (Client.ClientPacketEnum) typeByte;
        }

        public void ReadType(out Server.ServerPacketEnum type) {
            new PacketByte(Read(1)[0]).Read(out byte typeByte);
            type = (Server.ServerPacketEnum) typeByte;
        }
    }
}