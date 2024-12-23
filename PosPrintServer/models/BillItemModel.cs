public partial class BillItem
{
    //public int? id { get; set; }
    //public int? bill_id { get; set; }
    //public int? product_category_id { get; set; }
    //public int? product_sub_category_id { get; set; }
    //public int? bill_item_status_master_id { get; set; }
    //public int? product_id { get; set; }
    //public int? product_item_id { get; set; }
    public double? amount { get; set; }
    public double? unit_price { get; set; }
    public double? price { get; set; }
    public string? note { get; set; }
    public bool? is_take_home { get; set; }
    //public bool? is_free { get; set; }
    //public double? free_amount { get; set; }
    //public double? free_discount { get; set; }
    //public int? bill_item_set_id { get; set; }
    //public bool? is_set { get; set; }
    public double? unit_price_item { get; set; }
    //public double? price_item { get; set; }
    public string? barcode { get; set; }
    //public DateTimeOffset? created_at { get; set; }
    //public string? bill_no { get; set; }
    //public string? table_name { get; set; }
    public string? product_name { get; set; }
    //public bool? product_is_buffet { get; set; }
    //public bool? product_is_sell_by_weight { get; set; }
    //public string? product_item_name { get; set; }
    public string? product_item_code { get; set; }
    //public int? receipt_item_id { get; set; }
    //public int? receipt_id { get; set; }
    public bool? product_is_has_option { get; set; }
    //public int? product_sub_category_product_id { get; set; }
    //public bool? product_is_show_in_receipt { get; set; }
    //public string? product_image_url { get; set; }
    ////public dynamic[] sub_bill_items { get; set; }
    public BillItemProductTopping[] bill_item_product_toppings { get; set; } = Array.Empty<BillItemProductTopping>();
    public BillItemNote[] bill_item_notes { get; set; } = Array.Empty<BillItemNote>();
    //public bool? has_bill_item_notes { get; set; }
    //public string? remark { get; set; }
    public string? description { get; set; }
    //public int? order_by_product_is_buffet { get; set; }
    //public int? order_by_product_category_id { get; set; }
    //public int? order_by_id { get; set; }
    public Notice[] notices { get; set; } = Array.Empty<Notice>();
    public bool? product_has_option { get; set ; }
}

public partial class BillItemNote
{
    public int? id { get; set; }
    public int? note_id { get; set; }
    public string? note_note { get; set; }
}

public partial class BillItemProductTopping
{
    //public int? id { get; set; }
    //public int? product_id { get; set; }
    //public int? product_item_id { get; set; }
    //public int? price { get; set; }
    public double? amount { get; set; }
    //public int? unit_price { get; set; }
    //public double? total_before_vat { get; set; }
    //public double? vat { get; set; }
    //public int? total_after_vat { get; set; }
    //public int? total_amount { get; set; }
    public double? total_price { get; set; }
    public string? product_name { get; set; }
    //public string? product_item_name { get; set; }
}

