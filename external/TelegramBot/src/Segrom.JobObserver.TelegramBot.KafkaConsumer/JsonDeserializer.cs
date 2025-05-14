using System.Text;
using System.Text.Json;
using Confluent.Kafka;

namespace Segrom.JobObserver.TelegramBot.KafkaConsumer;

internal sealed class JsonDeserializer<TValue>: IDeserializer<TValue> where TValue: new()
{
	private readonly Encoding _encoding = Encoding.Unicode;
	public JsonDeserializer() { }

	public TValue Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
	{
		var json = _encoding.GetString(data);
		return JsonSerializer.Deserialize<TValue>(json)!;
	}

}