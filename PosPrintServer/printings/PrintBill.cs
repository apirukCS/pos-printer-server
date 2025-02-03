using System;
using System.Text;
using PrintingModel;
using System.Net;
using System.Runtime.InteropServices;
using PM = PrinterManager;
using System.Text.Json;
using System.Globalization;
using System.Text.RegularExpressions;

//using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Intrinsics.Arm;

public class PrintBill
{
    public static async Task<PrintBill> Create(IntPtr ptr, BillModel data, string type, bool cashdraw = false)
    {
        var instance = new PrintBill();
        await instance.InitializePrinting(ptr, data, type, cashdraw);
        return instance;
    }

    private async Task InitializePrinting(IntPtr ptr, BillModel bill, string type, bool cashdraw)
    {
        List<BillModel> bills = ParseData(bill);
        for (int i = 0; i< bills.Count; i++) {
            if (type == "bill")
            {
                await PrintingBill(ptr, bills[i], type);
            }
            else 
            {
                await PrintingReceipt(ptr, bills[i], type, cashdraw);
            }
        }
    }

    public async Task PrintingBill(IntPtr printer, BillModel bill, string type)
    {
        try {
            bool[] s = new bool[7];
            //await Task.Run(async () =>
            //{
                PM.AlignCenter(printer);
                s[0] = await AddLogo(printer, bill);
                s[1] = AddBillNo(printer, bill);
                PM.TextAlignLeft(printer);
                s[2] = AddHeader(printer, bill);
                s[3] = AddBillItems(printer, bill);
                s[4] = AddPricing(printer, bill, type);
                s[5] = AddMembership(printer, bill);
                s[6] = await AddFooterBill(printer, bill);
                PM.CutPaper(printer);
                PM.ClosePort(printer);
                PM.ReleasePort(printer);
                if (s.Any(x => x == false)) {
                    WriteLog.WriteFailedPrintLog(bill, type);
                }
                await Task.Delay(100);
            //});
        }
        catch (Exception e) {
            //keep log
            //PM.ClosePort(printer);
            //MessageBox.Show($"{e}");
            WriteLog.WriteFailedPrintLog(bill, type);
        }
    }

    public async Task PrintingReceipt(IntPtr printer, BillModel bill, string type, bool cashdraw)
    {
        try {
            bool[] s = new bool[9];
            //await Task.Run(async () =>
            //{
                if (cashdraw)
                {
                    PM.OpenCashDrawer(printer);
                }
                PM.AlignCenter(printer);
                s[0] = await AddLogo(printer, bill);
                s[1] = AddShopAddress(printer, bill);
                s[2] = AddTitle(printer, bill, type);
                s[3] = AddHeader(printer, bill);
                s[4] = AddBillItems(printer, bill);
                s[5] = AddPricing(printer, bill, type);
                s[6] = AddPayments(printer, bill);
                s[7] = AddMembership(printer, bill);
                s[8] = await AddFooterBill(printer, bill);
                PM.CutPaper(printer);
                PM.ClosePort(printer);
                PM.ReleasePort(printer);
                if (s.Any(x => x == false))
                {
                    WriteLog.WriteFailedPrintLog(bill, type);
                //WriteLog.Write(type, type);

            }
                await Task.Delay(100);
            //}); 
        }
        catch (Exception e) {
            PM.ClosePort(printer);
            PM.ReleasePort(printer);
            //keep log
            //MessageBox.Show($"rer {e}");

            WriteLog.WriteFailedPrintLog(bill, type);
            //WriteLog.Write(type, type);
        }
    }

    static async Task<bool> AddLogo(IntPtr printer, BillModel bill)
    {
        if (!string.IsNullOrEmpty(bill.shop.image_url)) {
            int s = await PM.PrintImageUrl(printer, bill.shop.image_url, "image.jpg");
            PM.NewLine(printer, 70);
            return s == 0;
        }
        return true;
    }

    static bool AddBillNo(IntPtr printer, BillModel bill)
    {
        string language = bill.language ?? "th";
        string topic = language == "th" ? "ใบเรียกเก็บเงิน" : "Bill";
        int s = PM.PrintText(printer, topic);
        string title = $"{(language == "th" ? "เลขที่บิล" : "Bill No.")} {bill.doc_no}";
        int s2 = PM.PrintText(printer, title, true, 120);
        return s == 0 && s2 == 0;
    }

    static bool AddHeader(IntPtr printer, BillModel bill)
    {
        string loc = string.Empty;
        string language = bill.language ?? "th";
        if (bill.is_take_home == true && !string.IsNullOrEmpty(bill.delivery_name))
        {
            loc = $"{bill.delivery_name} {(bill.remark != null ? bill.remark : "")}";
        }
        else if (bill.is_take_home == true && bill.is_delivery != true)
        {
            loc = language == "th" ? "กลับบ้าน" : "Take home";
        }
        else if (bill.is_take_home != true && bill.table_id.HasValue)
        {
            loc = $"{(language == "th" ? "โซน" : "Zone")} {bill.table_zone_name} {(language == "th" ? "โต๊ะ" : "Table")} {bill.table_name}";
        }
        else
        {
            loc = language == "th" ? "ทานร้าน" : "Eat in";
        }

        string staffName = $"{(language == "th" ? "พนักงาน" : "staff")} {bill.staff_name ?? bill.cashier_staff_name}";
        string textLeftFiltered = Regex.Replace(loc, "[\u0E31\u0E34-\u0E3A\u0E47-\u0E4D]", "");
        int effectiveLeftLength = textLeftFiltered.Length;

        int spaceBetween = 42 - (effectiveLeftLength + staffName.Length);
        string line = loc + new string(' ', spaceBetween) + staffName;
        int s = PM.PrintTextBold(printer, loc + new string(' ', spaceBetween), false);
        int s2 = PM.PrintTextOnly(printer, staffName);
        PM.NewLine(printer);

        string date = language == "th" ? "วันที่" : "Order time";
        DateTimeHelper dateTimeHelper = new DateTimeHelper();
        string currentDate = dateTimeHelper.GetCurrentDateTime(language);
        int s3 = PM.PrintText(printer, $"{date} {currentDate}");

        string generalCustomer = language == "th" ? "ลูกค้าทั่วไป" : "General Customer";
        string customer = $"{(language == "th" ? "ลูกค้า" : "Customer name")} {(bill.customer_name == "ลูกค้าทั่วไป" ? generalCustomer : bill.customer_name)}";
        int s4 = PM.PrintText(printer, customer, true, 120);

        string headerA = language == "th" ? "รายการที่สั่ง" : "Item";
        int s5 = PM.PrintText(printer, headerA);

        PM.DrawLine(printer);
        string headerB1 = (language == "th") ? "จำนวน" : "QTY";
        string headerB2 = (language == "th") ? "รายการ" : "ITEM";
        string headerB3 = (language == "th") ? "รวม" : "PRICE";
        int s6 = PM.PrintTextThreeColumn(printer, headerB1, headerB2, headerB3);
        PM.DrawLine(printer);
        return s == 0 && s2 == 0 && s3 == 0 && s4 == 0 && s5 == 0 && s6 == 0;
    }

    static bool AddBillItems(IntPtr printer, BillModel bill)
    {
        int s = 0;
        int s2 = 0;
        int s3 = 0;
        foreach (var billItem in bill.bill_items)
        {
            string amount = AmountFormatter(billItem.amount ?? 1);
            string itemName = billItem.product_name;
            if (billItem.product_has_option == true)
            {
                itemName += $" {billItem.product_item_code}";
            }

            string itemPrice = bill.shop.bill_is_show_topping_by_item == true ?
                (billItem.unit_price_item * billItem.amount).ToString() :
                billItem.price.ToString();
            itemPrice = CurrencyFormat(itemPrice);

            s = PM.PrintTextThreeColumn(printer, amount, itemName, itemPrice);


            if (billItem.bill_item_notes != null && billItem.bill_item_notes.Length > 0)
            {
                string wholeNote = $"({string.Join(", ", billItem.bill_item_notes.Select(n => n.note_note))})";
                s2 =  PM.PrintTextThreeColumn(printer, "       ", wholeNote, "     ");
            }


            if (billItem.bill_item_product_toppings != null && billItem.bill_item_product_toppings.Length > 0)
            {
                foreach (var topping in billItem.bill_item_product_toppings)
                {
                    string toppingName = $"({topping.amount}) {topping.product_name}";
                    string toppingPrice = bill.shop.bill_is_show_topping_by_item == true ? topping.total_price.ToString() : string.Empty;
                    toppingPrice = CurrencyFormat(toppingPrice);
                    s3 = PM.PrintTextThreeColumn(printer, "", toppingName, toppingPrice);
                }
            }
        }
        PM.NewLine(printer);
        return s == 0 && s2 == 0 && 23 == 0;
    }

    static bool AddPricing(IntPtr printer, BillModel bill, string type)
    {
        if (bill.receipt == null) return true;
        int[] s = new int[12];

        string language = bill.language ?? "th";
        string title = language == "th" ? "ราคารวม" : "Total amount";
        string combinePrice = CurrencyFormat(bill.receipt.price.ToString());
        PM.NewLine(printer, 40);
        s[0] = PM.PrintTextTwoColumn(printer, title, combinePrice);
        PM.NewLine(printer, 20);

        // discount
        if ((bill?.receipt?.discount_total != null && bill.receipt?.discount_total != 0) && bill.receipt?.price > 0)
        {
            string discountAmount;
            string discountText = language == "th" ? "ส่วนลด" : "Sale discount";
            s[1] = PM.PrintText(printer, discountText);

            var promotions = type == "invoice" ? bill.receipt?.invoice_promotions ?? [] : bill.receipt?.receipt_promotions ?? [];
            var pointPromotions = type == "invoice" ? bill.receipt?.invoice_point_promotions ?? [] : bill.receipt?.receipt_point_promotions ?? [];

            foreach (var item in promotions)
            {
                discountAmount = CurrencyFormat(item.discount.ToString());
                s[2] = PM.PrintTextThreeColumn(printer, "", item.promotion_name, discountAmount, 42, 2);
            }

            foreach (var item in pointPromotions)
            {
                discountAmount = CurrencyFormat(item.discount.ToString());
                s[3] = PM.PrintTextThreeColumn(printer, "", item.point_promotion_name, discountAmount, 42, 2);
            }

            if (bill.receipt?.discount_special > 0)
            {
                string additionalDiscount = $"{(language == "th" ? "ส่วนลดพิเศษ" : "Additional sales discount")} {bill.receipt?.discount_special_value} {(bill.receipt?.discount_special_type_id == 1 ? "บาท" : "%")}";
                discountAmount = CurrencyFormat(bill.receipt?.discount_special.ToString());
                s[4] = PM.PrintTextThreeColumn(printer, "", additionalDiscount, discountAmount, 42, 2);
            }

            if (bill.receipt?.free_amount > 0)
            {
                string free = language == "th" ? "ฟรี" : "Discount of free items";
                discountAmount = CurrencyFormat(bill.receipt?.free_amount.ToString());
                s[5] = PM.PrintTextThreeColumn(printer, "", free, discountAmount, 42, 2);
            }

            if (bill.receipt?.decimal_discount > 0)
            {
                string discountOfRound = language == "th" ? "ส่วนลดปัดเศษสตางค์" : "Discount of Round down to decimal";
                discountAmount = CurrencyFormat(bill.receipt?.decimal_discount.ToString());
                s[6] = PM.PrintTextThreeColumn(printer, "", discountOfRound, discountAmount, 42, 2);
            }

            string totalSaleDiscount = language == "th" ? "ส่วนลดรวม" : "Total sales discount";
            discountAmount = CurrencyFormat(bill.receipt?.discount_total.ToString());
            s[7] = PM.PrintTextTwoColumn(printer, totalSaleDiscount, discountAmount);
        }

        // service charge
        if (bill.receipt?.service_charge > 0)
        {
            string serviceCharge = $"{(language == "th" ? "ค่าบริการ" : "Service charge")} {bill.receipt?.service_charge_percent} %";
            string amount = CurrencyFormat(bill.receipt?.service_charge.ToString());
            s[8] = PM.PrintTextTwoColumn(printer, serviceCharge, amount);
        }

        // vat
        if (bill.receipt?.document_vat_type_id != null && bill.receipt?.document_vat_type_id != 3)
        {
            string preVatAmount = $"{(language == "th" ? "ราคาไม่รวมภาษีมูลค่าเพิ่ม" : "Pre-vat Amount")} {bill.receipt?.vat_percent} %";
            string amount = CurrencyFormat(bill.receipt?.total_before_vat.ToString());
            s[9] = PM.PrintTextTwoColumn(printer, preVatAmount, amount);

            string vat = $"{(language == "th" ? "ภาษีมูลค่าเพิ่ม" : "VAT")} {bill.receipt?.vat_percent} %";
            amount = CurrencyFormat(bill.receipt?.vat.ToString());
            s[10] = PM.PrintTextTwoColumn(printer, vat, amount);
        }

        // total
        PM.NewLine(printer, 20);
        string grandTotal = language == "th" ? "ราคาสุทธิ" : "Grand total";
        string finalAmount = CurrencyFormat(bill.receipt?.total_after_vat.ToString(), true);
        s[11] = PM.PrintTextTwoColumn(printer, grandTotal, finalAmount, 2);
        PM.NewLine(printer, 70);
        return s.All(x => x == 0);
    }

    public bool AddMembership(IntPtr printer, BillModel bill)
    {
        string language = bill.language ?? "th";
        int[] s = new int[6];
        if (bill.receipt != null && bill.receipt?.membership != null)
        {
            PM.NewLine(printer);
            string memberName = language == "th"
                ? $"ชื่อสมาชิก {bill.receipt.membership.member_name}"
                : $"Membership {bill.receipt.membership.member_name}";
            s[0] = PM.PrintText(printer, memberName);

            string memberLevel = language == "th"
                ? $"ระดับสมาชิก {bill.receipt.membership.member_level_name}"
                : $"Membership level {bill.receipt.membership.member_level_name}";
            s[1] = PM.PrintText(printer, memberLevel);

            string previousPoint = language == "th"
                ? "คะแนนก่อนหน้า"
                : "Previous Points";
            string beginningBalance = CurrencyFormat(bill.receipt.membership.beginning_balance.ToString(), true, 0);
            s[2] = PM.PrintTextTwoColumn(printer, previousPoint, beginningBalance);

            string pointReward = language == "th"
                ? "คะแนนที่ได้รับ"
                : "Reward Points";
            string pointInc = CurrencyFormat(bill.receipt.membership.point_inc.ToString(), true, 0);
            s[3] = PM.PrintTextTwoColumn(printer, pointReward, pointInc);

            string redeemPoint = language == "th"
                ? "คะแนนที่ใช้"
                : "Redeem Points";
            string pointUsed = CurrencyFormat(bill.receipt.membership.point_used.ToString(), true, 0);
            s[4] = PM.PrintTextTwoColumn(printer, redeemPoint, pointUsed);

            string balancePoint = language == "th"
                ? "คะแนนสะสมคงเหลือ"
                : "Balance Points";
            string balance = CurrencyFormat(bill.receipt.membership.balance.ToString(), true, 0);
            s[5] = PM.PrintTextTwoColumn(printer, balancePoint, balance);
            PM.NewLine(printer, 100);
        }
        return s.All(x => x == 0);
    }

    public async Task<bool> AddFooterBill(IntPtr printer, BillModel bill)
    {
        int[] s = new int[3];
        if (bill.shop != null && bill.shop.receipt_footer_image_url != null)
        {
            string imageUrl = bill.shop.receipt_footer_image_url ?? "";
            if (!string.IsNullOrEmpty(imageUrl)) {
                PM.AlignCenter(printer);
                s[0] = await PM.PrintImageUrl(printer, imageUrl, "logo.jpg");
                PM.TextAlignLeft(printer);
            }
            
        }
        if (bill.shop != null && bill.shop.bill_footer_text != null)
        {
            foreach (var txt in bill.shop.bill_footer_text.Split(new[] { "\n" }, StringSplitOptions.None))
            {
                PM.AlignCenter(printer);
                PM.NewLine(printer);
                s[1] = PM.PrintText(printer, $"{txt} ");
            }
        }

        if (bill.shop != null && bill.shop.is_connect_roommy != true)
        {
            string resrun = "Powered by ResRun Beyond POS";
            PM.AlignCenter(printer);
            PM.NewLine(printer, 70);
            s[2] = PM.PrintText(printer, resrun);
        }
        PM.NewLine(printer);
        return s.All(x => x == 0);
    }

    public bool AddShopAddress(IntPtr printer, BillModel bill)
    {
        string branchName = bill.shop.branch_name;
        string language = bill.language ?? "th";
        PM.AlignCenter(printer);
        int[] s = new int[5];
        if (bill.shop != null)
        {
            if (bill.shop.branch_type_id == 2)
            {
                branchName = $"{(!string.IsNullOrEmpty(bill.shop.branch_code) ? (language == "th" ? "สาขาที่ " : "Branch No. ") + bill.shop.branch_code : "")}" +
                             $"{(bill.shop.branch_code != null && !string.IsNullOrEmpty(bill.shop.branch_code) ? " " : "")}" +
                             $"{(!string.IsNullOrEmpty(bill.shop.branch_name) ? (language == "th" ? "สาขา " : "Branch ") + bill.shop.branch_name : "")} ";
            }
            else if (bill.shop.branch_type_id == 3)
            {
                branchName = string.Empty;
            }

            string shopName = $"{bill.shop.name_th ?? ""} ";
            s[0] = PM.PrintText(printer, FormatText(shopName));

            string formattedBranchName = (!string.IsNullOrEmpty(branchName) ? $"({branchName})" : string.Empty);
            s[1] = PM.PrintText(printer, FormatText(formattedBranchName));

            string addr = $"{(language == "th" ? "ที่อยู่" : "Address")} {bill.shop.tax_address} ";
            s[2] = PM.PrintText(printer, FormatText(addr));
        }

        if (bill.receipt != null && bill.receipt.document_vat_type_id.HasValue && bill.receipt.document_vat_type_id != 3)
        {
            string taxNo = $"{(language == "th" ? "เลขประจำตัวผู้เสียภาษี" : "TAX ID")} {bill.shop.tax_no} ";
            s[3] = PM.PrintText(printer, FormatText(taxNo));
        }
        PM.NewLine(printer, 50);
        string tel = $"{(language == "th" ? "เบอร์โทร" : "Tel")} {bill.shop.tel ?? ""} ";
        s[4] = PM.PrintText(printer, FormatText(tel));
        PM.NewLine(printer, 70);
        return s.All(x => x == 0);
    }

    public bool AddTitle(IntPtr printer, BillModel bill, string? type = null)
    {
        string vatTypeName = string.Empty;
        int[] s = new int[3];
        if (bill.receipt == null) return true;

        if (bill.receipt.document_vat_type_id == 1)
        {
            vatTypeName = "(VAT Included)";
        }
        else if (bill.receipt.document_vat_type_id == 2)
        {
            vatTypeName = "(VAT Excluded)";
        }

        string title;
        string docText;
        string language = bill.language ?? "th";

        switch (type)
        {
            case "invoice":
                title = bill.receipt.document_vat_type_id != 3
                    ? (language == "th" ? "ใบแจ้งหนี้/ใบกำกับภาษีอย่างย่อ" : "Invoice / Abbreviated Tax Invoice")
                    : (language == "th" ? "ใบแจ้งหนี้" : "Invoice");
                docText = $"{(language == "th" ? "เลขที่ใบแจ้งหนี้" : "Invoice no.")} {bill.receipt.doc_no} {vatTypeName}";
                break;

            default:
                title = bill.receipt.document_vat_type_id != 3
                    ? (language == "th" ? "ใบเสร็จรับเงิน/ใบกำกับภาษีอย่างย่อ" : "Receipt / Abbreviated Tax Invoice")
                    : (language == "th" ? "ใบเสร็จรับเงิน" : "Receipt");
                docText = $"{(language == "th" ? "เลขที่ใบเสร็จ" : "Receipt no.")} {bill.receipt.doc_no} {vatTypeName}";
                break;
        }

        string posText = $"POS ID {bill.receipt.pos_no}";
        s[0] = PM.PrintText(printer, title);
        PM.TextAlignLeft(printer);
        s[1] = PM.PrintText(printer, docText);

        if (bill.receipt.document_vat_type_id != 3)
        {
            s[2] = PM.PrintText(printer, posText);
        }
        return s.All(x => x == 0);
    }

    public bool AddPayments(IntPtr printer, BillModel bill)
    {
        if (bill.receipt == null || bill.receipt?.receipt_payments == null) return true;

        double change = 0;
        string language = bill.language ?? "th";
        int[] s = new int[5];
        foreach (var payment in bill.receipt?.receipt_payments)
        {
            string paymentTypeName = string.Empty;
            switch (payment.payment_type_id)
            {
                case 1:
                    paymentTypeName = language == "th" ? "เงินสด" : "Cash";
                    break;
                case 3:
                    paymentTypeName = language == "th" ? "บัตรเครดิต/เดบิต" : "Credit/Debit";
                    break;
                case 4:
                    paymentTypeName = language == "th" ? "เงินโอน / QR Code" : "Bank transfer / QR Code";
                    break;
                case 5:
                    paymentTypeName = language == "th" ? "เงินเชื่อ" : "Invoice";
                    break;
                case 6:
                    paymentTypeName = language == "th" ? "เงินมัดจำ" : "Deposit";
                    break;
                default:
                    break;
            }

            string paymentMethod = $"{paymentTypeName} {payment.bank_short_name} {payment.bank_account_payment_method_name}";
            string amount = CurrencyFormat(payment.amount.ToString());
            s[0] = PM.PrintTextTwoColumn(printer, paymentMethod, amount);

            change += payment.change ?? 0;
        }

        if (change > 0)
        {
            string changeAmount = CurrencyFormat(change.ToString());
            string changeK = language == "th" ? "เงินทอน" : "Change";
            s[1] = PM.PrintTextTwoColumn(printer, changeK, changeAmount, 2);
        }

        return s.All(x => x == 0);
    }


    static string AmountFormatter(double amount)
    {
        return amount.ToString("0.###", CultureInfo.InvariantCulture);
    }

    static string CurrencyFormat(string? price, bool showZero = false, int digit = 2)
    {
        string formattedPrice = string.Empty;
        if (decimal.TryParse(price ?? "", out decimal priceValue))
        {
            formattedPrice = priceValue.ToString($"N{digit}", new CultureInfo("en-US"));
        }
        else if (showZero)
        {
            formattedPrice = "0";
        }

        return formattedPrice;
    }

    static string FormatText(string text)
    {
        return text.Replace("\n", "\r\n");
    }

    public List<BillModel> ParseData(BillModel data)
    {
        var items = new List<BillModel>();
        var baseItem = new BillModel
        {
            doc_no = data.doc_no,
            table_id = data.table_id,
            remark = data.remark,
            table_name = data.table_name,
            table_zone_name = data.table_zone_name,
            customer_name = data.customer_name,
            cancel_staff_name = data.cancel_staff_name,
            cashier_staff_name = data.cashier_staff_name,
            delivery_name = data.delivery_name,
            is_take_home = data.is_take_home,
            staff_name = data.staff_name,
            language = data.language,
            is_delivery = data.is_delivery,
        };


        if (data.receipts != null && data.receipts.Any())
        {
            foreach (var receipt in data.receipts)
            {
                var item = new BillModel();

                CopyProperties(baseItem, item);

                item.bill_items = data.bill_items
                    .Where(b => b.receipt_id == receipt.id)
                    .ToArray();

                item.receipt = receipt;
                item.shop = data.shop;
                items.Add(item);
            }
        }

        if (data.invoices != null && data.invoices.Any())
        {
            foreach (var invoice in data.invoices)
            {
                var item = new BillModel();
                CopyProperties(baseItem, item);

                item.bill_items = data.bill_items
                    .Where(b => b.invoice_id == invoice.id)
                    .ToArray();

                item.receipt = invoice;
                item.shop = data.shop;
                items.Add(item);
            }
        }

        return items;
    }

    private void CopyProperties(BillModel source, BillModel destination)
    {
        var properties = typeof(BillModel).GetProperties()
            .Where(p => p.CanRead && p.CanWrite);

        foreach (var prop in properties)
        {
            var value = prop.GetValue(source);
            if (value != null)
            {
                prop.SetValue(destination, value);
            }
        }
    }

    static void WriteFile(string jsonString)
    {
        string folderPath = @"C:\pos-printer";
        string filePath = Path.Combine(folderPath, "logs.txt");
        try
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            File.WriteAllText(filePath, jsonString);
            //MessageBox.Show($"JSON has been written to: {filePath}");
        }
        catch (Exception ex)
        {
            //MessageBox.Show($"Error writing to file: {ex.Message}");
        }
    }
}


