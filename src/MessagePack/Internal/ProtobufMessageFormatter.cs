using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

		public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options) => throw new NotImplementedException();
		public T Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => throw new NotImplementedException();
	}
}
