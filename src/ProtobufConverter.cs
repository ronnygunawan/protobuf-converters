using System.Text.Json;
using RG.System.Text.Json.ProtobufSerializer.Internal;

namespace RG.System.Text.Json.ProtobufSerializer {
	public class ProtobufConverter {
		public static readonly JsonSerializerOptions Options;

		static ProtobufConverter() {
			Options = new() {
				Converters = {
					new RepeatedFieldConverterFactory()
				}
			};
		}

		private ProtobufConverter() { }
	}
}
