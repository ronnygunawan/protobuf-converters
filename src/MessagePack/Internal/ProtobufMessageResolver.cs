using System.Reflection;
using Google.Protobuf;
using MessagePack;
using MessagePack.Formatters;

namespace RG.ProtobufConverters.MessagePack.Internal {
	internal class ProtobufMessageResolver : IFormatterResolver {
		public static readonly ProtobufMessageResolver Instance;

		static ProtobufMessageResolver() {
			Instance = new();
		}

		private ProtobufMessageResolver() { }

		public IMessagePackFormatter<T>? GetFormatter<T>() {
			return FormatterCache<T>.Formatter;
		}

		private static class FormatterCache<T> {
			public static readonly IMessagePackFormatter<T>? Formatter;

			static FormatterCache() {
				TypeInfo ti = typeof(T).GetTypeInfo();

				if (ti.IsInterface || ti.IsAbstract) {
					return;
				}

				if (!typeof(IMessage).IsAssignableFrom(typeof(T))) {
					return;
				}

				Formatter = ProtobufMessageFormatter<T>.Instance;
			}
		}
	}
}
