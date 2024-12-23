using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public partial class QueueModel
{
    [JsonProperty("ShopQ", NullValueHandling = NullValueHandling.Ignore)]
    public ShopQ? ShopQ { get; set; }

    [JsonProperty("crrent_date", NullValueHandling = NullValueHandling.Ignore)]
    public string? CrrentDate { get; set; }

    [JsonProperty("queue", NullValueHandling = NullValueHandling.Ignore)]
    public Queue? Queue { get; set; }

    [JsonProperty("language", NullValueHandling = NullValueHandling.Ignore)]
    public string? Language { get; set; }

    public static string CreateMockupData()
    {
        var mockup = new QueueModel
        {
            ShopQ = new ShopQ { ImageUrl = "https://storage.googleapis.com/storage-resrun-pos-com-dev6/423ov82fsqj8z5l400k9cznf231h" },
            CrrentDate = DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
            Queue = new Queue
            {
                QueueNo = 59,
                CustomerAmount = 5,
                WaitQueueCount = 10
            },
            Language = "th"
        };

        return mockup.ToJson();
    }
}

public partial class Queue
{
    [JsonProperty("queue_no", NullValueHandling = NullValueHandling.Ignore)]
    public int? QueueNo { get; set; }

    [JsonProperty("customer_amount", NullValueHandling = NullValueHandling.Ignore)]
    public int? CustomerAmount { get; set; }

    [JsonProperty("wait_queue_count", NullValueHandling = NullValueHandling.Ignore)]
    public int? WaitQueueCount { get; set; }
}

public partial class ShopQ
{
    [JsonProperty("image_url", NullValueHandling = NullValueHandling.Ignore)]
    public string? ImageUrl { get; set; }
}

public partial class QueueModel
{
    public static QueueModel FromJson(string json)
    {
        var result = JsonConvert.DeserializeObject<QueueModel>(json, Converter.Settings);
        if (result == null)
            throw new InvalidOperationException("Invalid JSON format for QueueModel.");

        return result;
    }
}

public static class Serialize
{
    public static string ToJson(this QueueModel self) => JsonConvert.SerializeObject(self, Converter.Settings);
}

internal static class Converter
{
    public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    {
        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
        DateParseHandling = DateParseHandling.None,
        Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
    };
}


