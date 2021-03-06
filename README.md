# RG.ProtobufConverters.Json

[![NuGet](https://img.shields.io/nuget/v/RG.ProtobufConverters.Json.svg)](https://www.nuget.org/packages/RG.ProtobufConverters.Json/) [![.NET](https://github.com/ronnygunawan/protobuf-converters/actions/workflows/dotnet.yml/badge.svg)](https://github.com/ronnygunawan/protobuf-converters/actions/workflows/dotnet.yml)

Use Protobuf generated classes in System.Text.Json serialization.

```cs
// Serialize to json
string json = foo.SerializeToJson();

// Deserialize from json
FooMessage? foo = json.DeserializeToProtobufMessage<FooMessage>();
```

## Use Protobuf classes in SignalR

Configure hub:
```cs
services.AddSignalR()
    .AddJsonProtocol(options => options.PayloadSerializerOptions = ProtobufJsonConverter.Options);
```

Configure client:
```cs
var connection = new HubConnectionBuilder()
    .WithUrl("https://yoururl/yourhub")
    .AddJsonProtocol(options => options.PayloadSerializerOptions = ProtobufJsonConverter.Options)
    .Build();
```

---

# RG.ProtobufConverters.MessagePack

[![NuGet](https://img.shields.io/nuget/v/RG.ProtobufConverters.MessagePack.svg)](https://www.nuget.org/packages/RG.ProtobufConverters.MessagePack/) [![.NET](https://github.com/ronnygunawan/protobuf-converters/actions/workflows/dotnet.yml/badge.svg)](https://github.com/ronnygunawan/protobuf-converters/actions/workflows/dotnet.yml)

```cs
// Serialize to MessagePack bytes
byte[] bytes = foo.SerializeUsingMessagePack();

// Deserialize from MessagePack bytes
FooMessage? foo = bytes.DeserializeUsingMessagePack();
```

## Use Protobuf classes in SignalR

Configure hub:
```cs
services.AddSignalR()
    .AddMessagePackProtocol(options => options.SerializerOptions = ProtobufResolver.Options);
```

Configure client:
```cs
var connection = new HubConnectionBuilder()
    .WithUrl("https://yoururl/yourhub")
    .AddMessagePackProtocol(options => options.SerializerOptions = ProtobufResolver.Options)
    .Build();
```
