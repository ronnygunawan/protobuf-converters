using Google.Protobuf.Collections;
using MessagePack;
using MessagePack.Formatters;

namespace RG.ProtobufConverters.MessagePack.Internal {
	internal class RepeatedFieldFormatter<T> : CollectionFormatterBase<T, RepeatedField<T>> {
		protected override void Add(RepeatedField<T> collection, int index, T value, MessagePackSerializerOptions options) {
			collection.Add(value);
		}

		protected override RepeatedField<T> Create(int count, MessagePackSerializerOptions options) {
			return new RepeatedField<T>();
		}
	}
}
