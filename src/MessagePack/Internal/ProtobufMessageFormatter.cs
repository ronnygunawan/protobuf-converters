using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Google.Protobuf.Collections;
using MessagePack;
using MessagePack.Formatters;

namespace RG.ProtobufConverters.MessagePack.Internal {
	internal class ProtobufMessageFormatter<T> : IMessagePackFormatter<T> {
		public static readonly ProtobufMessageFormatter<T> Instance;

		private readonly IReadOnlyDictionary<string, PropertyInfo> _normalPropertyByName;
		private readonly IReadOnlyDictionary<string, PropertyInfo> _repeatedPropertyByName;
		private readonly IReadOnlyDictionary<string, MethodInfo> _repeatedPropertyAdderByName;
		private readonly IReadOnlyDictionary<string, PropertyInfo> _oneofCasePropertyByName;
		private readonly IReadOnlyDictionary<PropertyInfo, IReadOnlyDictionary<string, PropertyInfo>> _oneofPropertiesByOneofCase;

		static ProtobufMessageFormatter() {
			Instance = new();
		}

		private ProtobufMessageFormatter() {
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
				.ToDictionary(p => p.Name);

			_repeatedPropertyByName = repeatedProperties
				.ToDictionary(p => p.Name);

			_repeatedPropertyAdderByName = repeatedProperties
				.ToDictionary(
					keySelector: p => p.Name,
					elementSelector: p => p.PropertyType.GetMethod("Add", new[] { p.PropertyType.GenericTypeArguments[0] })!
				);

			_oneofCasePropertyByName = oneofCaseProperties
				.ToDictionary(p => p.Name);

			Dictionary<string, PropertyInfo> oneofPropertiesByName = oneofProperties
				.ToDictionary(p => p.Name);

			_oneofPropertiesByOneofCase = oneofCaseProperties
				.ToDictionary(
					keySelector: p => p,
					elementSelector: p => (IReadOnlyDictionary<string, PropertyInfo>)Enum.GetNames(p.PropertyType)
						.Where(n => n != "None")
						.ToDictionary(
							keySelector: n => n,
							elementSelector: n => oneofPropertiesByName[n]
						)
				);
		}

		public T Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
			if (reader.TryReadNil()) {
				return default!;
			}

			T message = Activator.CreateInstance<T>();

			int len = reader.ReadMapHeader();
			for (int i = 0; i < len; i++) {
				// Get the key
				string propertyName = reader.ReadString();

				SkipReadPropertyName:

				// Get the value
				if (_normalPropertyByName.TryGetValue(propertyName, out PropertyInfo? normalProperty)) {
					object value = MessagePackSerializer.Deserialize(normalProperty.PropertyType, ref reader, options);
					normalProperty.SetValue(message, value);
				} else if (_repeatedPropertyByName.TryGetValue(propertyName, out PropertyInfo? repeatedProperty)) {
					ICollection values = (ICollection)MessagePackSerializer.Deserialize(repeatedProperty.PropertyType, ref reader, options);

					if (!_repeatedPropertyAdderByName.TryGetValue(propertyName, out MethodInfo? adder)) throw new MessagePackSerializationException();
					foreach (object value in values) {
						adder.Invoke(repeatedProperty.GetValue(message), new[] { value });
					}
				} else if (_oneofCasePropertyByName.TryGetValue(propertyName, out PropertyInfo? oneofCaseProperty)
					&& _oneofPropertiesByOneofCase.TryGetValue(oneofCaseProperty, out IReadOnlyDictionary<string, PropertyInfo>? oneofPropertyByName)) {
					int caseValue = reader.ReadInt32();
					string caseName = Enum.GetName(oneofCaseProperty.PropertyType, caseValue)!;

					// Skip reading oneof property
					if (caseName == "None") {
						continue;
					} else {
						i++;
					}

					// Default value handling: ignore
					if (reader.ReadString() != caseName) {
						goto SkipReadPropertyName;
					}

					// Get oneof property value
					if (!oneofPropertyByName.TryGetValue(caseName, out PropertyInfo? oneofProperty)) throw new MessagePackSerializationException();
					object? value = MessagePackSerializer.Deserialize(oneofProperty.PropertyType, ref reader, options);
					oneofProperty.SetValue(message, value);
				} else {
					throw new MessagePackSerializationException();
				}
			}

			return message;
		}

		public void Serialize(ref MessagePackWriter writer, T message, MessagePackSerializerOptions options) {
			Dictionary<string, object> keyValuePairs = new();

			foreach ((string propertyName, PropertyInfo normalProperty) in _normalPropertyByName) {
				if (normalProperty.GetValue(message) is object value) {
					keyValuePairs.Add(propertyName, value);
				}
			}

			foreach ((string propertyName, PropertyInfo repeatedProperty) in _repeatedPropertyByName) {
				keyValuePairs.Add(propertyName, repeatedProperty.GetValue(message)!);
			}

			foreach ((PropertyInfo oneofCaseProperty, IReadOnlyDictionary<string, PropertyInfo> oneofPropertyByName) in _oneofPropertiesByOneofCase) {
				object oneofProperty = oneofCaseProperty.GetValue(message)!;
				string oneofPropertyName = oneofProperty.ToString()!;

				// No need to serialize oneof case and property if case was None
				if (oneofPropertyName == "None") continue;

				keyValuePairs.Add(oneofCaseProperty.Name, (int)oneofProperty);

				// Default value handling: ignore
				if (oneofPropertyByName[oneofPropertyName].GetValue(message) is object value) {
					keyValuePairs.Add(oneofPropertyName, value);
				}
			}

			writer.WriteMapHeader(keyValuePairs.Count);

			foreach ((string propertyName, object value) in keyValuePairs) {
				writer.WriteString(Encoding.UTF8.GetBytes(propertyName));
				MessagePackSerializer.Serialize(value.GetType(), ref writer, value, options);
			}

			writer.Flush();
		}
	}
}
