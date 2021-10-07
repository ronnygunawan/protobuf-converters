using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf.Collections;

namespace RG.System.Text.Json.ProtobufSerializer.Internal {
	internal class RepeatedFieldConverterFactory : JsonConverterFactory {
		private static readonly Dictionary<Type, JsonConverter> ConverterByElementType = new();

		public override bool CanConvert(Type typeToConvert) => typeToConvert.IsGenericType
			&& typeToConvert.GetGenericTypeDefinition() == typeof(RepeatedField<>);

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) {
			Type elementType = typeToConvert.GetGenericArguments()[0];
			if (!ConverterByElementType.TryGetValue(elementType, out JsonConverter? converter)) {
				converter = (JsonConverter)Activator.CreateInstance(typeof(RepeatedFieldConverter<>).MakeGenericType(elementType), options);
				ConverterByElementType.Add(elementType, converter);
			}
			return converter;
		}
	}
}
