using System.Text.Json;
using Google.Protobuf;

namespace RG.ProtobufConverters.Json {
	public static class ProtobufMessageExtensions {
		public static string SerializeToJson<T>(this T message) where T : IMessage<T> {
			return JsonSerializer.Serialize(message, ProtobufJsonConverter.Options);
		}

		public static T? DeserializeToProtobufMessage<T>(this string json) where T : IMessage<T> {
			return JsonSerializer.Deserialize<T>(json, ProtobufJsonConverter.Options);
		}
	}
}
