using FluentAssertions;
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

			deserialized.StringField.Should().Be("asdfg");
			deserialized.IntField.Should().Be(123456);
			deserialized.BoolField.Should().BeTrue();
			deserialized.EnumField.Should().Be(TeletubbiesName.Lala);
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

			deserialized.StringField.Should().Be("asdfg");
			deserialized.IntField.Should().Be(123456);
			deserialized.BoolField.Should().BeTrue();
			deserialized.EnumField.Should().Be(TeletubbiesName.Lala);
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

			deserialized.StatusCase.Should().Be(IpsumReply.StatusOneofCase.Naruto);
			deserialized.Sasuke.Should().BeNull();
			deserialized.Naruto.Should().NotBeNull();
			deserialized.Naruto.StringField.Should().Be("hello world");
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

			deserialized.StringArray.Should().ContainInOrder("quick", "brown", "fox");
			deserialized.LoremArray.Should().HaveCount(3);
			deserialized.LoremArray[0].StringField.Should().Be("jumps");
			deserialized.LoremArray[1].StringField.Should().Be("over");
			deserialized.LoremArray[2].StringField.Should().Be("the lazy dog");
		}

		[Fact]
		public void ResolverCanSerializeGrpcReplyContainingNullField() {
			SitReply sitReply = new() {
				StringField = "asd",
				Lorem = null
			};

			byte[] bytes = sitReply.SerializeUsingMessagePack();
			SitReply deserialized = bytes.DeserializeUsingMessagePack<SitReply>()!;

			deserialized.StringField.Should().Be("asd");
			deserialized.Lorem.Should().BeNull();
		}

		[Fact]
		public void ResolverCanSerializeGrpcReplyWithNoneField() {
			IpsumReply ipsumReply = new() {
			};

			byte[] bytes = ipsumReply.SerializeUsingMessagePack();
			IpsumReply deserialized = bytes.DeserializeUsingMessagePack<IpsumReply>()!;

			deserialized.StatusCase.Should().Be(IpsumReply.StatusOneofCase.None);
			deserialized.Sasuke.Should().BeNull();
			deserialized.Naruto.Should().BeNull();
		}
	}
}
