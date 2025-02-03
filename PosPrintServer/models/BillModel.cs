using System;
using Newtonsoft.Json;

public class BillWrapper
{
    public BillModel? bill { get; set; }
    public string? language { get; set; }
}

public partial class BillModel
{
    public int? id { get; set; }
    public string? doc_no { get; set; }
    public int? table_id { get; set; }
    public string? remark { get; set; }
    public string? table_name { get; set; }
    public string? table_zone_name { get; set; }
    public string? customer_name { get; set; }
    public string? cancel_staff_name { get; set; }
    public string? cashier_staff_name { get; set; }
    public string? delivery_name { get; set; }
    public bool? is_take_home { get; set; }
    public BillItem[] bill_items { get; set; } = Array.Empty<BillItem>();
    public Receipt[] receipts { get; set; } = Array.Empty<Receipt>();
    public Receipt[] invoices { get; set; } = Array.Empty<Receipt>();
    public Shop? shop { get; set; }
    public string? staff_name { get; set; }
    public string? language { get; set; }
    public bool? is_delivery {  get; set; }
    public Receipt? receipt { get; set; }
}

public partial class InVoice
{
    //public int? id { get; set; }
    //public int? bill_id { get; set; }
    //public string doc_no { get; set; }
    //public dynamic sys_doc_no { get; set; }
    //public DateTimeOffset? doc_date { get; set; }
    //public int? price { get; set; }
    //public double? service_charge { get; set; }
    //public dynamic promotion_name { get; set; }
    //public int? discount_promotion { get; set; }
    //public dynamic point_promotion_name { get; set; }
    //public int? discount_point_promotion { get; set; }
    //public double? total_before_vat { get; set; }
    //public int? document_vat_type_id { get; set; }
    //public int? vat_percent { get; set; }
    //public double? vat { get; set; }
    //public int? deduct_percent { get; set; }
    //public int? deduct { get; set; }
    //public double? total_after_vat { get; set; }
    //public dynamic delivery_id { get; set; }
    //public int? delivery_charge_percent { get; set; }
    //public int? delivery_charge { get; set; }
    //public int? total_after_delivery_charge { get; set; }
    //public int? document_status_id { get; set; }
    //public dynamic receive_deposit_id { get; set; }
    //public int? deposit_pay { get; set; }
    //public double? service_charge_before_vat { get; set; }
    //public double? service_charge_vat { get; set; }
    //public int? free_amount { get; set; }
    //public int? pos_id { get; set; }
    //public dynamic last_installment_no { get; set; }
    //public bool? is_full_pay { get; set; }
    //public double? remain { get; set; }
    //public int? pay { get; set; }
    //public dynamic creator_id { get; set; }
    //public dynamic updater_id { get; set; }
    //public int? customer_id { get; set; }
    //public int? service_charge_percent { get; set; }
    //public int? decimal_discount { get; set; }
    //public int? discount_special { get; set; }
    //public int? discount_special_type_id { get; set; }
    //public int? discount_special_value { get; set; }
    //public double? service_charge_after_vat { get; set; }
    //public bool? is_diff_delivery { get; set; }
    //public bool? is_service_charge { get; set; }
    //public int? discount_total { get; set; }
    //public dynamic member_name { get; set; }
    //public dynamic member_tel { get; set; }
    //public bool? read_only { get; set; }
    //public dynamic[] editable_fields { get; set; }
    //public DateTimeOffset? created_at { get; set; }
    //public DateTimeOffset? updated_at { get; set; }
    //public InvoiceItem[] invoice_items { get; set; }
    //public InvoicePromotion[] invoice_promotions { get; set; }
    //public dynamic[] invoice_point_promotions { get; set; }
    //public string pos_no { get; set; }
    //public dynamic tax_invoice { get; set; }
}

public partial class InvoiceItem
{
    //public int? id { get; set; }
    //public int? invoice_id { get; set; }
    //public int? bill_item_id { get; set; }
    //public int? product_id { get; set; }
    //public int? product_item_id { get; set; }
    //public double? unit_price { get; set; }
    //public double? amount { get; set; }
    //public double? price { get; set; }
    //public int? document_vat_type_id { get; set; }
    //public double? vat_percent { get; set; }
    //public double? vat { get; set; }
    //public double? deduct_percent { get; set; }
    //public double? deduct { get; set; }
    //public double? total_after_vat { get; set; }
    //public string promotion_name { get; set; }
    //public double? discount_promotion { get; set; }
    //public string point_promotion_name { get; set; }
    //public double? discount_point_promotion { get; set; }
    //public double? total_after_discount { get; set; }
    //public bool? is_free { get; set; }
    //public double? total_before_vat { get; set; }
    //public int? product_group_id { get; set; }
    //public int? product_category_id { get; set; }
    //public int? product_sub_category_id { get; set; }
    //public double? free_discount { get; set; }
    //public int? ingredient_id { get; set; }
    //public int? product_type_id { get; set; }
    //public double? total_before_vat_item { get; set; }
    //public double? vat_item { get; set; }
    //public double? total_after_vat_item { get; set; }
    //public bool? is_sub_product_item { get; set; }
    //public double? product_item_unit_price { get; set; }
    //public dynamic invoice_item_set_id { get; set; }
    //public double? unit_price_item { get; set; }
    //public double? price_item { get; set; }
    //public double? decimal_discount { get; set; }
    //public double? discount_special { get; set; }
    //public int? discount_special_type_id { get; set; }
    //public double? discount_special_value { get; set; }
    //public double? discount_total { get; set; }
    //public double? free_amount { get; set; }
    //public bool? read_only { get; set; }
    //public dynamic[] editable_fields { get; set; }
    //public InvoiceItemPromotion[] invoice_item_promotions { get; set; }
    //public dynamic[] invoice_item_point_promotions { get; set; }
    //public string discount_special_type_name { get; set; }
    //public dynamic[] sub_invoice_items { get; set; }
    //public bool? is_set { get; set; }
}

public partial class InvoiceItemPromotion
{
    //public int? id { get; set; }
    //public int? invoice_item_id { get; set; }
    //public int? promotion_id { get; set; }
    //public double? discount { get; set; }
    //public bool? read_only { get; set; }
    //public dynamic[] editable_fields { get; set; }
}

public partial class InvoicePromotion
{
    //public int? id { get; set; }
    //public int? invoice_id { get; set; }
    //public int? promotion_id { get; set; }
    //public double? discount { get; set; }
    //public bool? read_only { get; set; }
    //public dynamic[] editable_fields { get; set; }
    //public string promotion_name { get; set; }
}


