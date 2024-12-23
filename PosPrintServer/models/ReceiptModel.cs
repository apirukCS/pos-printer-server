using Printing;

public partial class Receipt
{
    public int? id { get; set; }
    public int? bill_id { get; set; }
    public string? doc_no { get; set; }
    public DateTimeOffset? doc_date { get; set; }
    public double? price { get; set; }
    public double? discount_promotion { get; set; }
    public double? service_charge { get; set; }
    public double? total_before_vat { get; set; }
    public int? document_vat_type_id { get; set; }
    public double? vat_percent { get; set; }
    public double? vat { get; set; }
    public double? total_after_vat { get; set; }
    public double? free_amount { get; set; }
    public int? document_status_id { get; set; }
    public double? service_charge_before_vat { get; set; }
    public double? service_charge_percent { get; set; }
    public double? decimal_discount { get; set; }
    public double? discount_special { get; set; }
    public int? discount_special_type_id { get; set; }
    public double? discount_special_value { get; set; }
    public double? discount_total { get; set; }
    public bool? is_service_charge { get; set; }
    public ReceiptPayment[] receipt_payments { get; set; } = Array.Empty<ReceiptPayment>();
    public dynamic[] receipt_promotions { get; set; } = Array.Empty<dynamic>();
    public dynamic[] receipt_point_promotions { get; set; } = Array.Empty<dynamic>();
    public string? pos_no { get; set; }
    public string? remark { get; set; }
    public Membership? membership { get; set; }
    public InVoicePromotion[] invoice_promotions { get; set; } = Array.Empty<InVoicePromotion>();
    public InVoicePointPromotion[] invoice_point_promotions { get; set; } = Array.Empty<InVoicePointPromotion>();
}

public partial class Membership
{
    public double? beginning_balance { get; set; }
    public double? point_inc { get; set; }
    public double? point_used { get; set; }
    public double? balance { get; set; }
    public string? member_name { get; set; }
    public string? member_level_name { get; set; }
}

public partial class InVoicePromotion
{
    //public int? id { get; set; }
    public int? invoice_id { get; set; }
    public int? promotion_id { get; set; }
    public int? discount { get; set; }
    //public bool? read_only { get; set; }
    //public dynamic[] editable_fields { get; set; }
    public string? promotion_name { get; set; }
}

public class InVoicePointPromotion
{
    public double? discount { get; set; }
    public string? point_promotion_name { get; set; }
}

namespace Printing
{

    public partial class ReceiptPayment
    {
        //public long? id { get; set; }
        //public long? receipt_id { get; set; }
        public int? payment_type_id { get; set; }
        //public dynamic bank_id { get; set; }
        //public dynamic card_holder_name { get; set; }
        public double? amount { get; set; }
        //public dynamic bank_account_id { get; set; }
        //public dynamic customer_id { get; set; }
        //public dynamic note { get; set; }
        public double? change { get; set; }
        //public dynamic bank_account_payment_method_id { get; set; }
        //public bool? read_only { get; set; }
        //public dynamic[] editable_fields { get; set; }
        public string? payment_type_name { get; set; }
        //public string? bank_name { get; set; }
        public string? bank_short_name { get; set; }
        public string? bank_account_payment_method_name { get; set; }
    }
}


