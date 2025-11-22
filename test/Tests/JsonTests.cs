using System.Text.Json;
using Shouldly;
using Protos.Foo;
using RG.ProtobufConverters.Json;
using Xunit;

namespace Tests {
	public class JsonTests {
		[Fact]
		public void CanSerializeGrpcRequest() {
			LoremRequest loremRequest = new() {
				StringField = "asdfg",
				IntField = 123456,
				BoolField = true,
				EnumField = TeletubbiesName.Lala
			};

			string json = loremRequest.SerializeToJson();
			LoremRequest deserialized = json.DeserializeToProtobufMessage<LoremRequest>()!;

			deserialized.StringField.ShouldBe("asdfg");
			deserialized.IntField.ShouldBe(123456);
			deserialized.BoolField.ShouldBeTrue();
			deserialized.EnumField.ShouldBe(TeletubbiesName.Lala);
		}

		[Fact]
		public void CanSerializeGrpcRequestContainingCamelCaseProperties() {
			var loremRequest = new {
				stringField = "asdfg",
				intField = 123456,
				boolField = true,
				enumField = TeletubbiesName.Lala
			};

			string json = JsonSerializer.Serialize(loremRequest);
			LoremRequest deserialized = json.DeserializeToProtobufMessage<LoremRequest>()!;

			deserialized.StringField.ShouldBe("asdfg");
			deserialized.IntField.ShouldBe(123456);
			deserialized.BoolField.ShouldBeTrue();
			deserialized.EnumField.ShouldBe(TeletubbiesName.Lala);
		}

		[Fact]
		public void ResolverCanSerializeGrpcReply() {
			LoremReply loremReply = new() {
				StringField = "asdfg",
				IntField = 123456,
				BoolField = true,
				EnumField = TeletubbiesName.Lala
			};

			string json = loremReply.SerializeToJson();
			LoremReply deserialized = json.DeserializeToProtobufMessage<LoremReply>()!;

			deserialized.StringField.ShouldBe("asdfg");
			deserialized.IntField.ShouldBe(123456);
			deserialized.BoolField.ShouldBeTrue();
			deserialized.EnumField.ShouldBe(TeletubbiesName.Lala);
		}

		[Fact]
		public void ResolverCanSerializeGrpcReplyContainingOneofCase() {
			IpsumReply ipsumReply = new() {
				Naruto = new IpsumReply.Types.NarutoReply {
					StringField = "hello world"
				}
			};

			string json = ipsumReply.SerializeToJson();
			IpsumReply deserialized = json.DeserializeToProtobufMessage<IpsumReply>()!;

			deserialized.StatusCase.ShouldBe(IpsumReply.StatusOneofCase.Naruto);
			deserialized.Sasuke.ShouldBeNull();
			deserialized.Naruto.ShouldNotBeNull();
			deserialized.Naruto.StringField.ShouldBe("hello world");
		}

		[Fact]
		public void ResolverCanSerializeGrpcReplyContainingRepeatedField() {
			DolorReply dolorReply = new() {
				StringArray = {
					"quick", "brown", "fox"
				},
				LoremArray = {
					new LoremReply { StringField = "jumps" },
					new LoremReply { StringField = "over" },
					new LoremReply { StringField = "the lazy dog" }
				}
			};

			string json = dolorReply.SerializeToJson();
			DolorReply deserialized = json.DeserializeToProtobufMessage<DolorReply>()!;

			deserialized.StringArray.ShouldBe(new[] { "quick", "brown", "fox" });
			deserialized.LoremArray.Count.ShouldBe(3);
			deserialized.LoremArray[0].StringField.ShouldBe("jumps");
			deserialized.LoremArray[1].StringField.ShouldBe("over");
			deserialized.LoremArray[2].StringField.ShouldBe("the lazy dog");
		}

		[Fact]
		public void ResolverCanSerializeGrpcReplyContainingNullField() {
			SitReply sitReply = new() {
				StringField = "asd",
				Lorem = null
			};

			string json = sitReply.SerializeToJson();
			SitReply deserialized = json.DeserializeToProtobufMessage<SitReply>()!;

			deserialized.StringField.ShouldBe("asd");
			deserialized.Lorem.ShouldBeNull();
		}

		[Fact]
		public void ResolverCanSerializeGrpcReplyWithNoneField() {
			IpsumReply ipsumReply = new() {
			};

			string json = ipsumReply.SerializeToJson();
			IpsumReply deserialized = json.DeserializeToProtobufMessage<IpsumReply>()!;

			deserialized.StatusCase.ShouldBe(IpsumReply.StatusOneofCase.None);
			deserialized.Sasuke.ShouldBeNull();
			deserialized.Naruto.ShouldBeNull();
		}
	}
}
