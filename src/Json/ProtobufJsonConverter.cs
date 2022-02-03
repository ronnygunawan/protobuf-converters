using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf;
using Google.Protobuf.Collections;
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
				if (!ConverterByMessageType.TryGetValue(typeToConvert, out JsonConverter? converter)) {
					converter = (JsonConverter)Activator.CreateInstance(typeof(ProtobufJsonConverter<>).MakeGenericType(typeToConvert))!;
					ConverterByMessageType.Add(typeToConvert, converter);
				}
				return converter;
			}
		}
	}

	internal class ProtobufJsonConverter<T> : JsonConverter<T> {
		private readonly IReadOnlyDictionary<string, PropertyInfo> _normalPropertyByName;
		private readonly IReadOnlyDictionary<string, PropertyInfo> _repeatedPropertyByName;
		private readonly IReadOnlyDictionary<string, MethodInfo> _repeatedPropertyAdderByName;
		private readonly IReadOnlyDictionary<string, PropertyInfo> _oneofCasePropertyByName;
		private readonly IReadOnlyDictionary<PropertyInfo, IReadOnlyDictionary<string, PropertyInfo>> _oneofPropertiesByOneofCase;

		public ProtobufJsonConverter() {
			PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

			HashSet<PropertyInfo> repeatedProperties = properties
				.Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(RepeatedField<>))
				.ToHashSet();

			HashSet<PropertyInfo> oneofCaseProperties = properties
				.Where(p => p.PropertyType.IsEnum && !p.CanWrite)
				.ToHashSet();

			HashSet<PropertyInfo> oneofProperties = properties
				.Where(p => oneofCaseProperties
					.SelectMany(ocp => Enum.GetNames(ocp.PropertyType))
					.Where(n => n != "None")
					.Contains(p.Name)
				)
				.ToHashSet();

			_normalPropertyByName = properties
				.Except(repeatedProperties)
				.Except(oneofCaseProperties)
				.Except(oneofProperties)
				.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

			_repeatedPropertyByName = repeatedProperties
				.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

			_repeatedPropertyAdderByName = repeatedProperties
				.ToDictionary(
					keySelector: p => p.Name,
					elementSelector: p => p.PropertyType.GetMethod("Add", new[] { p.PropertyType.GenericTypeArguments[0] })!,
					comparer: StringComparer.OrdinalIgnoreCase
				);

			_oneofCasePropertyByName = oneofCaseProperties
				.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

			Dictionary<string, PropertyInfo> oneofPropertiesByName = oneofProperties
				.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

			_oneofPropertiesByOneofCase = oneofCaseProperties
				.ToDictionary(
					keySelector: p => p,
					elementSelector: p => (IReadOnlyDictionary<string, PropertyInfo>)Enum.GetNames(p.PropertyType)
						.Where(n => n != "None")
						.ToDictionary(
							keySelector: n => n,
							elementSelector: n => oneofPropertiesByName[n],
							comparer: StringComparer.OrdinalIgnoreCase
						)
				);
		}

		public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
			if (reader.TokenType != JsonTokenType.StartObject) {
				throw new JsonException();
			}

			T message = Activator.CreateInstance<T>();

			while (reader.Read()) {
				if (reader.TokenType == JsonTokenType.EndObject) {
					return message;
				}

				// Get the key
				if (reader.TokenType != JsonTokenType.PropertyName) {
					throw new JsonException();
				}

				string propertyName = reader.GetString()!;

				// Get the value
				if (_normalPropertyByName.TryGetValue(propertyName, out PropertyInfo? normalProperty)) {
					object? value = JsonSerializer.Deserialize(ref reader, normalProperty.PropertyType, options);
					normalProperty.SetValue(message, value);
				} else if (_repeatedPropertyByName.TryGetValue(propertyName, out PropertyInfo? repeatedProperty)) {
					ICollection values = (ICollection)JsonSerializer.Deserialize(ref reader, repeatedProperty.PropertyType, options)!;

					if (!_repeatedPropertyAdderByName.TryGetValue(propertyName, out MethodInfo? adder)) throw new JsonException();
					foreach (object value in values) {
						 adder.Invoke(repeatedProperty.GetValue(message), new[] { value });
					}
				} else if (_oneofCasePropertyByName.TryGetValue(propertyName, out PropertyInfo? oneofCaseProperty)
					&& _oneofPropertiesByOneofCase.TryGetValue(oneofCaseProperty, out IReadOnlyDictionary<string, PropertyInfo>? oneofPropertyByName)) {
					string caseName = JsonSerializer.Deserialize(ref reader, oneofCaseProperty.PropertyType, options)!.ToString()!;

					// Skip reading oneof property
					if (caseName == "None") {
						continue;
					}

					// Get next property key
					if (!reader.Read()
						|| reader.TokenType != JsonTokenType.PropertyName
						|| reader.GetString() != caseName) {
						throw new JsonException();
					}

					// Get oneof property value
					if (!oneofPropertyByName.TryGetValue(caseName, out PropertyInfo? oneofProperty)) throw new JsonException();
					object? value = JsonSerializer.Deserialize(ref reader, oneofProperty.PropertyType, options);
					oneofProperty.SetValue(message, value);
				} else {
					throw new JsonException();
				}
			}

			throw new JsonException();
		}

		public override void Write(Utf8JsonWriter writer, T message, JsonSerializerOptions options) {
			writer.WriteStartObject();

			foreach (PropertyInfo normalProperty in _normalPropertyByName.Values) {
				writer.WritePropertyName(
					options.PropertyNamingPolicy?.ConvertName(normalProperty.Name) ?? normalProperty.Name
				);
				JsonSerializer.Serialize(writer, normalProperty.GetValue(message), normalProperty.PropertyType, options);
			}

			foreach (PropertyInfo repeatedProperty in _repeatedPropertyByName.Values) {
				writer.WritePropertyName(
					options.PropertyNamingPolicy?.ConvertName(repeatedProperty.Name) ?? repeatedProperty.Name
				);
				JsonSerializer.Serialize(writer, repeatedProperty.GetValue(message), repeatedProperty.PropertyType, options);
			}

			foreach ((PropertyInfo oneofCaseProperty, IReadOnlyDictionary<string, PropertyInfo> oneofPropertyByName) in _oneofPropertiesByOneofCase) {
				writer.WritePropertyName(
					options.PropertyNamingPolicy?.ConvertName(oneofCaseProperty.Name) ?? oneofCaseProperty.Name
				);
				object value = oneofCaseProperty.GetValue(message)!;
				JsonSerializer.Serialize(writer, value, oneofCaseProperty.PropertyType, options);

				string oneofPropertyName = value.ToString()!;

				// No need to serialize oneof property if case was None
				if (oneofPropertyName == "None") continue;

				writer.WritePropertyName(
					options.PropertyNamingPolicy?.ConvertName(oneofPropertyName) ?? oneofPropertyName
				);
				if (!oneofPropertyByName.TryGetValue(oneofPropertyName, out PropertyInfo? oneofProperty)) throw new JsonException();
				JsonSerializer.Serialize(writer, oneofProperty.GetValue(message), oneofProperty.PropertyType, options);
			}

			writer.WriteEndObject();
		}
	}
}
