using FluentAssertions;
using Protos.Foo;
using RG.System.Text.Json.ProtobufSerializer;
using System.Text.Json;
using Xunit;

namespace Tests {
	public class ResolverTests {
		[Fact]
		public void CanSerializeGrpcRequest() {
			LoremRequest loremRequest = new() {
				StringField = "asdfg",
				IntField = 123456,
				BoolField = true,
				EnumField = TeletubbiesName.Lala
			};

			string json = JsonSerializer.Serialize(loremRequest, ProtobufConverter.Options);

			LoremRequest deserialized = JsonSerializer.Deserialize<LoremRequest>(json, ProtobufConverter.Options)!;

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

			string json = JsonSerializer.Serialize(loremReply, ProtobufConverter.Options);

			LoremReply deserialized = JsonSerializer.Deserialize<LoremReply>(json, ProtobufConverter.Options)!;

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

			string json = JsonSerializer.Serialize(ipsumReply, ProtobufConverter.Options);

			IpsumReply deserialized = JsonSerializer.Deserialize<IpsumReply>(json, ProtobufConverter.Options)!;

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

			string json = JsonSerializer.Serialize(dolorReply, ProtobufConverter.Options);

			DolorReply deserialized = JsonSerializer.Deserialize<DolorReply>(json, ProtobufConverter.Options)!;

			deserialized.StringArray.Should().ContainInOrder("quick", "brown", "fox");
			deserialized.LoremArray.Should().HaveCount(3);
			deserialized.LoremArray[0].StringField.Should().Be("jumps");
			deserialized.LoremArray[1].StringField.Should().Be("over");
			deserialized.LoremArray[2].StringField.Should().Be("the lazy dog");
		}
	}
}
