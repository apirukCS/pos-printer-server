using Newtonsoft.Json;

public partial class Shop
{
    //public int? id { get; set; }
    //public int? client_id { get; set; }
    public string? name_th { get; set; }
    //public string name_en { get; set; }
    public string? tel { get; set; }
    public string? tax_no { get; set; }
    public string? bill_footer_image_url { get; set; }
    //public string address { get; set; }
    //public int? province_id { get; set; }
    //public int? amphur_id { get; set; }
    //public int? tambon_id { get; set; }
    //public string post_code { get; set; }
    //public string cashier_no { get; set; }
    //public string line_id { get; set; }
    //public string facebook_id { get; set; }
    //public int? shop_type_id { get; set; }
    //public string currency { get; set; }
    //public DateTimeOffset? open_at { get; set; }
    //public DateTimeOffset? close_at { get; set; }
    //public double? service_charge { get; set; }
    //public int? vat_type_id { get; set; }
    //public double? vat_percent { get; set; }
    public string? bill_footer_text { get; set; }
    //public bool? is_show_report_by_menu { get; set; }
    //public bool? is_pay_after { get; set; }
    //public bool? is_kitchen_print_by_item { get; set; }
    //public bool? is_print_to_staff { get; set; }
    //public bool? is_buffet_one_set_per_bill { get; set; }
    public string? branch_code { get; set; }
    public string? branch_name { get; set; }
    public int? branch_type_id { get; set; }
    //public string url_printer_server { get; set; }
    //public string public_ip { get; set; }
    //public bool? is_print_barcode { get; set; }
    //public bool? is_update_order_status_step_by_step { get; set; }
    //public int? default_report_printer_id { get; set; }
    //public int? decimal_type_id { get; set; }
    //public bool? is_round { get; set; }
    //public int? print_queue_loop { get; set; }
    //public int? queue_printer_id { get; set; }
    [JsonProperty("bill_is_show_topping_by_item")]
    public bool? bill_is_show_topping_by_item { get; set; }
    //public string receipt_footer_text { get; set; }
    //public bool? is_show_point { get; set; }
    //public bool? is_print_cancel_kitchen_bill_item { get; set; }
    //public bool? is_print_price_kitchen_bill_item { get; set; }
    //public bool? read_only { get; set; }
    //public dynamic[] editable_fields { get; set; }
    //public string province_name { get; set; }
    //public string province_name_th { get; set; }
    //public string amphur_name { get; set; }
    //public string amphur_name_th { get; set; }
    //public string tambon_name { get; set; }
    //public string tambon_name_th { get; set; }
    //public string shop_type_name { get; set; }
    //public string vat_type_name { get; set; }
    //public string decimal_type_name { get; set; }
    public bool? is_connect_roommy { get; set; }
    [JsonProperty("image_url")]
    public string? image_url { get; set; }
    public string? receipt_footer_image_url { get; set; }
    public string? tax_address { get; set; }
}
