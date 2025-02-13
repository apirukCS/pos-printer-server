using System.Text.Json.Serialization;

public class QrCodeModel
{
    [JsonPropertyName("shop")]
    public ShopInFo? Shop { get; set; }

    [JsonPropertyName("bill")]
    public BillInfo? Bill { get; set; }

    [JsonPropertyName("qr_code")]
    public string? QrCode { get; set; }

    [JsonPropertyName("pos_printers")]
    public List<PosPrinter>? PosPrinters { get; set; }

    [JsonPropertyName("generate_only")]
    public bool? GenerateOnly { get; set; }

    [JsonPropertyName("bill_id")]
    public int? BillId { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("printing_type")]
    public string? PrintingType { get; set; }
}

public class ShopInFo
{
    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }
}

public class BillInfo
{
    [JsonPropertyName("table_zone_name")]
    public string? TableZoneName { get; set; }

    [JsonPropertyName("table_name")]
    public string? TableName { get; set; }

    [JsonPropertyName("open_time")]
    public string? OpenTime { get; set; }

    [JsonPropertyName("buffet_category_has_time_limit")]
    public bool? BuffetCategoryHasTimeLimit { get; set; }

    [JsonPropertyName("buffet_end_time")]
    public string? BuffetEndTime { get; set; }

    [JsonPropertyName("buffet_names")]
    public string? BuffetNames { get; set; }

    [JsonPropertyName("is_buffet")]
    public bool? IsBuffet { get; set; }
}

public class PosPrinter
{
    [JsonPropertyName("printer_id")]
    public int? PrinterId { get; set; }

    [JsonPropertyName("print_type")]
    public string? PrintType { get; set; }
}