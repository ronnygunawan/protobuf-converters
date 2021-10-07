using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf;
using RG.ProtobufConverters.Json.Internal;

namespace RG.ProtobufConverters.Json {
	public class ProtobufJsonConverter : JsonConverterFactory {
		private static readonly Dictionary<Type, JsonConverter> ConverterByMessageType = new();
		private static readonly object Gate = new();
		public static readonly ProtobufJsonConverter Instance = new();

		public static readonly JsonSerializerOptions Options = new() {
			Converters = {
				Instance,
				RepeatedFieldConverter.Instance
			}
		};

		private ProtobufJsonConverter() { }

		public override bool CanConvert(Type typeToConvert) => !typeToConvert.IsInterface
			&& !typeToConvert.IsAbstract
			&& typeof(IMessage).IsAssignableFrom(typeToConvert);

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) {
			lock (Gate) {
				if (ConverterByMessageType.TryGetValue(typeToConvert, out JsonConverter? converter)) {
					converter = (JsonConverter)Activator.CreateInstance(typeof(ProtobufJsonConverter<>).MakeGenericType(typeToConvert), options);
					ConverterByMessageType.Add(typeToConvert, converter);
				}
				return converter;
			}
		}
	}

	internal class ProtobufJsonConverter<T> : JsonConverter<T> {
		private readonly JsonConverter<T> _valueConverter;

		public ProtobufJsonConverter(JsonSerializerOptions options) {
			_valueConverter = (JsonConverter<T>)options.GetConverter(typeof(T));
		}


	}
}
