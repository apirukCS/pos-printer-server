public partial class KitchenModel
{
    public bool? is_print_price_kitchen_bill_item { get; set; }
    public string? language { get; set; }
    //public int? id { get; set; }
    public string? kitchen_name { get; set; }
    //public string doc_no { get; set; }
    //public DateTimeOffset? open_date { get; set; }
    //public string open_time { get; set; }
    //public DateTimeOffset? close_date { get; set; }
    //public string close_time { get; set; }
    //public int? bill_type_id { get; set; }
    //public string bill_type_with_delivery_id { get; set; }
    public int? table_id { get; set; }
    //public int? incharge_staff_id { get; set; }
    //public int? customer_id { get; set; }
    //public double? total_after_vat { get; set; }
    //public int? delivery_id { get; set; }
    //public bool? is_diff_delivery { get; set; }
    //public int? document_status_id { get; set; }
    //public DateTimeOffset? cancel_date { get; set; }
    //public string cancel_time { get; set; }
    //public int? customer_amount { get; set; }
    //public double? delivery_diff { get; set; }
    //public string delivery_diff_unit_name { get; set; }
    public string? remark { get; set; }
    public string? table_name { get; set; }
    public string? table_zone_name { get; set; }
    //public string customer_name { get; set; }
    public string? bill_type_name { get; set; }
    //public string document_status_name { get; set; }
    //public string cancel_staff_name { get; set; }
    //public string cashier_staff_name { get; set; }
    public string? delivery_name { get; set; }
    public bool? is_take_home { get; set; }
    //public dynamic[] bill_customer_groups { get; set; }
    //public dynamic[] bill_customer_genders { get; set; }
    //public dynamic[] bill_customer_ages { get; set; }
    //public Table[] tables { get; set; }
    //public bool? is_buffet { get; set; }
    //public bool? buffet_category_has_time_limit { get; set; }
    //public DateTimeOffset? buffet_end_time { get; set; }
    public BillItem[] bill_items { get; set; } = Array.Empty<BillItem>();
    //public Receipt[] receipts { get; set; }
    public bool? is_delivery { get; set; }
    ////
    public string? buffet_text { get; set; }
    public string? orderer_name { get; set; }
    public string? bill_no { get; set; }
    public bool? is_print_barcode { get; set; }
}

public class Notice
{
    public string? title { get; set; }
}
