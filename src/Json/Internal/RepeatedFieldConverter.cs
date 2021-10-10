using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf.Collections;

namespace RG.ProtobufConverters.Json.Internal {
	internal class RepeatedFieldConverter : JsonConverterFactory {
		private static readonly Dictionary<Type, JsonConverter> ConverterByElementType = new();
		private static readonly object Gate = new();
		public static readonly RepeatedFieldConverter Instance = new();

		private RepeatedFieldConverter() { }

		public override bool CanConvert(Type typeToConvert) => typeToConvert.IsGenericType
			&& typeToConvert.GetGenericTypeDefinition() == typeof(RepeatedField<>);

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) {
			Type elementType = typeToConvert.GetGenericArguments()[0];
			lock (Gate) {
				if (!ConverterByElementType.TryGetValue(elementType, out JsonConverter? converter)) {
					converter = (JsonConverter)Activator.CreateInstance(typeof(RepeatedFieldConverter<>).MakeGenericType(elementType), options)!;
					ConverterByElementType.Add(elementType, converter);
				}
				return converter;
			}
		}
	}

	internal class RepeatedFieldConverter<T> : JsonConverter<RepeatedField<T>> {
		private readonly JsonConverter<T> _valueConverter;

		public RepeatedFieldConverter(JsonSerializerOptions options) {
			_valueConverter = (JsonConverter<T>)options.GetConverter(typeof(T));
		}

		public override RepeatedField<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
			if (reader.TokenType != JsonTokenType.StartArray) {
				throw new JsonException();
			}

			RepeatedField<T> repeatedField = new();

			while (reader.Read()) {
				if (reader.TokenType == JsonTokenType.EndArray) return repeatedField;

				// Get the value
				T value = _valueConverter.Read(ref reader, typeof(T), options)!;

				// Add to repeatedField
				repeatedField.Add(value);
			}

			throw new JsonException();
		}

		public override void Write(Utf8JsonWriter writer, RepeatedField<T> value, JsonSerializerOptions options) {
			writer.WriteStartArray();

			foreach (T item in value) {
				_valueConverter.Write(writer, item, options);
			}

			writer.WriteEndArray();
		}
	}
}
