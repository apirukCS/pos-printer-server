namespace PrintingModel
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class QrCodeModel
    {
        [JsonProperty("shop", NullValueHandling = NullValueHandling.Ignore)]
        public Shop Shop { get; set; }

        [JsonProperty("bill", NullValueHandling = NullValueHandling.Ignore)]
        public Bill Bill { get; set; }

        [JsonProperty("crrent_date", NullValueHandling = NullValueHandling.Ignore)]
        public string CrrentDate { get; set; }

        [JsonProperty("qr_code", NullValueHandling = NullValueHandling.Ignore)]
        public string QrCode { get; set; }

        [JsonProperty("language", NullValueHandling = NullValueHandling.Ignore)]
        public string Language { get; set; }
    }

    public partial class Bill
    {
        [JsonProperty("is_buffet", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsBuffet { get; set; }

        [JsonProperty("table_zone_name", NullValueHandling = NullValueHandling.Ignore)]
        public string TableZoneName { get; set; }

        [JsonProperty("table_name", NullValueHandling = NullValueHandling.Ignore)]
        public string TableName { get; set; }

        [JsonProperty("open_date", NullValueHandling = NullValueHandling.Ignore)]
        public string OpenDate { get; set; }

        [JsonProperty("open_time", NullValueHandling = NullValueHandling.Ignore)]
        public string OpenTime { get; set; }

        [JsonProperty("buffet_category_has_time_limit", NullValueHandling = NullValueHandling.Ignore)]
        public bool? BuffetCategoryHasTimeLimit { get; set; }

        [JsonProperty("buffet_end_time", NullValueHandling = NullValueHandling.Ignore)]
        public string BuffetEndTime { get; set; }

        [JsonProperty("bill_items", NullValueHandling = NullValueHandling.Ignore)]
        public BillItem[] BillItems { get; set; }

        [JsonProperty("buffet_name", NullValueHandling = NullValueHandling.Ignore)]
        public string BuffetName { get; set; }
    }

    public partial class BillItem
    {
        [JsonProperty("product_name", NullValueHandling = NullValueHandling.Ignore)]
        public string ProductName { get; set; }

        [JsonProperty("amount", NullValueHandling = NullValueHandling.Ignore)]
        public long? Amount { get; set; }

        [JsonProperty("product_is_buffet", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ProductIsBuffet { get; set; }
    }

    public partial class Shop
    {
        [JsonProperty("image_url", NullValueHandling = NullValueHandling.Ignore)]
        public string ImageUrl { get; set; }
    }

    public partial class QrCodeModel
    {
        public static QrCodeModel FromJson(string json) => JsonConvert.DeserializeObject<QrCodeModel>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this QrCodeModel self) => JsonConvert.SerializeObject(self, Converter.Settings);
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
}
