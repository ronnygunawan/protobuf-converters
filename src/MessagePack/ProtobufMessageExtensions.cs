using System;
using Google.Protobuf;
using MessagePack;

namespace RG.ProtobufConverters.MessagePack {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public static class ProtobufMessageExtensions {
		public static byte[] SerializeUsingMessagePack<T>(this T message) where T : IMessage<T> {
			return MessagePackSerializer.Serialize(message, ProtobufResolver.Options);
		}

		public static T? DeserializeUsingMessagePack<T>(this byte[] bytes) where T : IMessage<T> {
			return MessagePackSerializer.Deserialize<T>(bytes, ProtobufResolver.Options);
		}

		public static T? DeserializeUsingMessagePack<T>(this ReadOnlyMemory<byte> buffer) where T : IMessage<T> {
			return MessagePackSerializer.Deserialize<T>(buffer, ProtobufResolver.Options);
		}
	}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
