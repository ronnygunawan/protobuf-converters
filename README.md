# RG.ProtobufConverters.Json
Use Protobuf generated classes in System.Text.Json serialization.

```cs
// Serialize to json
string json = foo.SerializeToJson();

// Deserialize from json
FooMessage? foo = foo.DeserializeToProtobufMessage<FooMessage>();
```
