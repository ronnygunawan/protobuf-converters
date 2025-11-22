using Shouldly;
using Protos.Foo;
using RG.ProtobufConverters.MessagePack;
using Xunit;

namespace Tests {
	public class MessagePackTests {
		[Fact]
		public void CanSerializeGrpcRequest() {
			LoremRequest loremRequest = new() {
				StringField = "asdfg",
				IntField = 123456,
				BoolField = true,
				EnumField = TeletubbiesName.Lala
			};

			byte[] bytes = loremRequest.SerializeUsingMessagePack();
			LoremRequest deserialized = bytes.DeserializeUsingMessagePack<LoremRequest>()!;

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

			byte[] bytes = loremReply.SerializeUsingMessagePack();
			LoremReply deserialized = bytes.DeserializeUsingMessagePack<LoremReply>()!;

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

			byte[] bytes = ipsumReply.SerializeUsingMessagePack();
			IpsumReply deserialized = bytes.DeserializeUsingMessagePack<IpsumReply>()!;

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

			byte[] bytes = dolorReply.SerializeUsingMessagePack();
			DolorReply deserialized = bytes.DeserializeUsingMessagePack<DolorReply>()!;

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

			byte[] bytes = sitReply.SerializeUsingMessagePack();
			SitReply deserialized = bytes.DeserializeUsingMessagePack<SitReply>()!;

			deserialized.StringField.ShouldBe("asd");
			deserialized.Lorem.ShouldBeNull();
		}

		[Fact]
		public void ResolverCanSerializeGrpcReplyWithNoneField() {
			IpsumReply ipsumReply = new() {
			};

			byte[] bytes = ipsumReply.SerializeUsingMessagePack();
			IpsumReply deserialized = bytes.DeserializeUsingMessagePack<IpsumReply>()!;

			deserialized.StatusCase.ShouldBe(IpsumReply.StatusOneofCase.None);
			deserialized.Sasuke.ShouldBeNull();
			deserialized.Naruto.ShouldBeNull();
		}
	}
}
