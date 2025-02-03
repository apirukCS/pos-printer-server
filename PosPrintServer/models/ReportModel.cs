using System;
using Newtonsoft.Json;

public partial class ReceiptPaymentQrCode
{
    public int? bank_id { get; set; }
    public string? bank_name { get; set; }
    public string? bank_short_name { get; set; }
    public List<BankAccountPaymentMethod>? bank_account_payment_methods { get; set; } = new List<BankAccountPaymentMethod>();
}

public partial class BankAccountPaymentMethod
{
    public int? bank_account_payment_method_id { get; set; }
    public string? bank_account_payment_method_name { get; set; }
    public decimal? amount { get; set; }
    public int? payment_count { get; set; }
}

public partial class ReceiptPaymentCreditCard
{
    public int? bank_id { get; set; }
    public string? bank_name { get; set; }
    public string? bank_short_name { get; set; }
    public string? amount { get; set; }
    public int? payment_count { get; set; }
}

public partial class Delivery
{
	public string? delivery_name { get; set; }
	public string? total { get; set; }
}

public partial class ProductType
{
	public string? product_type_name { get; set; }
	public double? amount { get; set; }
	public dynamic? price { get; set; }
	public string? product_name { get; set; }
}

//public partial class ReportShop
//{
//	//public string? image_url { get; set; }
//	//public string? name { get; set; }
//	//public string? branch_name { get; set; }
//	//public string? address { get; set; }
//	//public bool? is_show_report_by_menu { get; set; }
// //   public double? service_charge { get; set; }
//    public double? default_report_printer_id { get; set; }

//    public string? image_url { get; set; }
//    public string? name { get; set; }
//    public string? branch_name { get; set; }  // It was 'branch' in your JSON, adjust as needed.
//    public string? address { get; set; }
//    public bool? is_show_report_by_menu { get; set; }
//    public double? service_charge { get; set; }  // Handle missing or null value
//}
public partial class ReportShop
{
    public string? image_url { get; set; }
    public string? name { get; set; }
    public string? branch { get; set; } // Matches JSON key
    public string? address { get; set; }
    public bool? is_show_report_by_menu { get; set; }

    public string? branch_name { get; set; }
    public double? service_charge { get; set; }
}



public partial class Promotion
{
    public string? promotion_name { get; set; }
    public string? discount { get; set; }
    public string? cnt { get; set; }
}

public partial class PointPromotion
{
    public string? point_promotion_name { get; set; }
    public string? discount { get; set; }
    public string? cnt { get; set; }
}

public class PosCashMovement
{
    public string? created_at { get; set; } 
    public string? description { get; set; } 
    public double? amount { get; set; }
}

public class Destination
{
    public string? type { get; set; } 
    public object? usbTarget { get; set; } // Can be changed to a specific type if needed
}

public partial class PosRound
{
    public int? id { get; set; }
    public int? pos_id { get; set; }
    public int? open_staff_id { get; set; }
    public int? close_staff_id { get; set; }
    public double? initial_amount { get; set; }
    public double? close_cash_amount { get; set; }
    public double? close_tip_amount { get; set; }
    public double? close_total_amount { get; set; }
    public bool? is_closed { get; set; }
    public bool? is_current { get; set; }
    public double? round { get; set; }

    public string? created_at { get; set; }
    public string? updated_at { get; set; }
    public dynamic? bank_deposit_slip { get; set; }

    public string? open_date { get; set; }
    public string? close_date { get; set; }
    public string? open_time { get; set; }
    public string? close_time { get; set; }
    public string? pos_code { get; set; }
    public string? pos_name { get; set; }

    public double? receipt_payment_amount { get; set; }
    public double? total_after_vat { get; set; }
    public double? in_amount { get; set; }
    public double? out_amount { get; set; }
    public double? total_amount { get; set; }
    public double? diff_amount { get; set; }
    public bool? same_date { get; set; }
    public double? paperSizeId { get; set; }

    public Destination? destination { get; set; } = new Destination();
    public List<PosCashMovement>? in_pos_cash_movements { get; set; } = new List<PosCashMovement>();
    public List<PosCashMovement>? out_pos_cash_movements { get; set; } = new List<PosCashMovement>();
}

public partial class Report
{
    public List<ProductType> product_types { get; set; } = new List<ProductType>(); //
    public List<ReceiptPaymentQrCode> receipt_payment_qr_code { get; set; } = new List<ReceiptPaymentQrCode>();
    public List<ReceiptPaymentCreditCard> receipt_payment_credit_card { get; set; } = new List<ReceiptPaymentCreditCard>();
    public List<Delivery> deliveries { get; set; } = new List<Delivery>();
    public List<Promotion> promotions { get; set; } = new List<Promotion>();
    public List<PointPromotion> point_promotions { get; set; } = new List<PointPromotion>();
    public ReportShop? shop { get; set; }
    public PosRound? pos_round { get; set; }

    public string? date { get; set; }
    public double? bill_count_cancel_not_bill { get; set; }
    public double? bill_count_total { get; set; }
    public double? bill_count_customer { get; set; }
    public double? bill_count_normal { get; set; }
    public double? bill_count_cancel { get; set; }
    public double? bill_item_count_cancel { get; set; }
    public double? bill_item_free_amount { get; set; }
    public double? bill_item_free_discount { get; set; }
    public double? receipt_service_charge { get; set; }
    public double? receipt_total_before_vat { get; set; }
    public double? receipt_vat { get; set; }
    public double? receipt_total_after_vat { get; set; }
    public double? discount_special { get; set; }
    public double? decimal_discount { get; set; }
    public double? discount_special_amount { get; set; }
    public double? receipt_payment_cash { get; set; }
    public double? receipt_payment_deposit { get; set; }
    public double? receipt_payment_qrcode { get; set; }
    public double? receipt_payment_card { get; set; }
    public double? invoice_total_after_vat { get; set; }
    public double? pos_round_close_tip_amount { get; set; }
    public double? receipt_restuarant { get; set; }
    public double? receipt_take_home { get; set; }
    public string? est_total_after_vat { get; set; }
    public double? count_bill_status89 { get; set; }
    public string? language { get; set; }
}
