using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf.Collections;

namespace RG.System.Text.Json.ProtobufSerializer.Internal {
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

			// Get repeat count
			if (!reader.Read() && reader.TokenType != JsonTokenType.Number) throw new JsonException();
			int repeatCount = reader.GetInt32();

			for (int i = 0; i < repeatCount; i++) {
				// Get the value
				T value;
				if (_valueConverter != null) {
					reader.Read();
					value = _valueConverter.Read(ref reader, typeof(T), options)!;
				} else {
					value = JsonSerializer.Deserialize<T>(ref reader, options)!;
				}

				// Add to repeatedField
				repeatedField.Add(value);
			}

			throw new JsonException();
		}

		public override void Write(Utf8JsonWriter writer, RepeatedField<T> value, JsonSerializerOptions options) {
			writer.WriteStartArray();
			writer.WriteNumberValue(value.Count);

			foreach (T item in value) {
				if (_valueConverter != null) {
					_valueConverter.Write(writer, item, options);
				} else {
					JsonSerializer.Serialize(writer, item, options);
				}
			}

			writer.WriteEndArray();
		}
	}
}
