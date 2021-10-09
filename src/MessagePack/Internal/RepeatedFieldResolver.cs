using System;
using System.Reflection;
using Google.Protobuf.Collections;
using MessagePack;
using MessagePack.Formatters;

namespace RG.ProtobufConverters.MessagePack.Internal {
	internal class RepeatedFieldResolver : IFormatterResolver {
		public static readonly RepeatedFieldResolver Instance;

		static RepeatedFieldResolver() {
			Instance = new();
		}

		private RepeatedFieldResolver() { }

		public IMessagePackFormatter<T>? GetFormatter<T>() {
			return FormatterCache<T>.Formatter;
		}

		private static class FormatterCache<T> {
			public static readonly IMessagePackFormatter<T>? Formatter;

			static FormatterCache() {
				Formatter = (IMessagePackFormatter<T>?)FormatterCache.GetFormatter(typeof(T));
			}
		}

		private static class FormatterCache {
			public static object? GetFormatter(Type t) {
				TypeInfo ti = t.GetTypeInfo();

				if (ti is {
					IsGenericType: true,
					GenericTypeArguments: var genericTypeArguments
				} && ti.GetGenericTypeDefinition() == typeof(RepeatedField<>)) {
					return Activator.CreateInstance(typeof(RepeatedFieldFormatter<>).MakeGenericType(genericTypeArguments));
				}

				return null;
			}
		}
	}
}
