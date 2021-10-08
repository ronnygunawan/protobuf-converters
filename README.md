# RG.ProtobufConverters.Json

[![NuGet](https://img.shields.io/nuget/v/RG.ProtobufConverters.Json.svg)](https://www.nuget.org/packages/RG.ProtobufConverters.Json/) [![.NET](https://github.com/ronnygunawan/protobuf-converters/actions/workflows/dotnet.yml/badge.svg)](https://github.com/ronnygunawan/protobuf-converters/actions/workflows/dotnet.yml)

Use Protobuf generated classes in System.Text.Json serialization.

```cs
// Serialize to json
string json = foo.SerializeToJson();

// Deserialize from json
FooMessage? foo = foo.DeserializeToProtobufMessage<FooMessage>();
```
