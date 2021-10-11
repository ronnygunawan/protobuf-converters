using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using RG.ProtobufConverters.MessagePack.Internal;

namespace RG.ProtobufConverters.MessagePack {
	/// <summary>
	/// Resolver for Protobuf messages
	/// </summary>
	public class ProtobufResolver : IFormatterResolver {
		/// <summary>
		/// The singleton instance that can be used.
		/// </summary>
		public static readonly ProtobufResolver Instance;

		/// <summary>
		/// A <see cref="MessagePackSerializerOptions"/> instance with this formatter pre-configured.
		/// </summary>
		public static readonly MessagePackSerializerOptions Options;

		private static readonly IFormatterResolver[] Resolvers = new IFormatterResolver[] {
			ProtobufMessageResolver.Instance,
			RepeatedFieldResolver.Instance,
			ContractlessStandardResolver.Instance
		};

		static ProtobufResolver() {
			Instance = new();
			Options = MessagePackSerializerOptions.Standard.WithResolver(Instance);
		}

		private ProtobufResolver() { }

		/// <summary>
		/// Gets an <see cref="IMessagePackFormatter{T}"/> instance that can serialize or deserialize some type T.
		/// </summary>
		/// <typeparam name="T">The type of value to be serialized or deserialized.</typeparam>
		/// <returns>A formatter, if this resolver supplies one for type T; otherwise null.</returns>
		public IMessagePackFormatter<T>? GetFormatter<T>() => FormatterCache<T>.Formatter;

		private static class FormatterCache<T> {
			public static readonly IMessagePackFormatter<T>? Formatter;

			static FormatterCache() {
				foreach (IFormatterResolver item in Resolvers) {
					IMessagePackFormatter<T>? f = item.GetFormatter<T>();
					if (f != null) {
						Formatter = f;
						return;
					}
				}
			}
		}
	}
}
