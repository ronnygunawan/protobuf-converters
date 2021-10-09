using System;
using MessagePack;
using MessagePack.Formatters;

namespace RG.ProtobufConverters.MessagePack.Internal {
	internal class ProtobufMessageFormatter<T> : IMessagePackFormatter<T> {
		public static readonly ProtobufMessageFormatter<T> Instance;

		static ProtobufMessageFormatter() {
			Instance = new();
		}

		private ProtobufMessageFormatter() { }

		public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options) => throw new NotImplementedException();
		public T Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => throw new NotImplementedException();
	}
}
