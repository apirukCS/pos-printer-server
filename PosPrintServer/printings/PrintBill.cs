using System;
using System.Text;
using PrintingModel;
using System.Net;
using System.Runtime.InteropServices;
using PM = PrinterManager;
using System.Text.Json;
using System.Globalization;
using System.Text.RegularExpressions;

using Newtonsoft.Json;
using System.Collections.Generic;

public class PrintBill
{
    public PrintBill(PrintingQueue data, string type)
    {
        //foreach (Printer printer in data.printers)
        //{
        //    if (string.IsNullOrEmpty(printer.ip_address)) continue;
        //    IntPtr ptr = await PM.GetPrinterConnection(printer.ip_address);
        //    BillModel bill = GenerateMockBillData();
        //    if (type == "bill")
        //    {
        //        PrintingBill(ptr, bill, type);
        //    }
        //    else if (type == "receipt")
        //    {
        //        PrintingReceipt(ptr, bill, type);
        //    }
        //}
    }



    public async void PrintingBill(IntPtr printer, BillModel bill, string type)
    {
        //MessageBox.Show("PrintingBill");
        PM.AlignCenter(printer);
        await AddLogo(printer, bill);
        AddBillNo(printer, bill);
        PM.TextAlignLeft(printer);
        AddHeader(printer, bill);
        AddBillItems(printer, bill);
        AddPricing(printer, bill, type);
        AddMembership(printer, bill);
        AddFooterBill(printer, bill);
        PM.CutPaper(printer);
    }

    public async void PrintingReceipt(IntPtr printer, BillModel bill, string type)
    {
        //MessageBox.Show("PrintingReceipt");
        PM.AlignCenter(printer);
        await AddLogo(printer, bill);
        AddShopAddress(printer, bill);

        AddTitle(printer, bill);
        AddHeader(printer, bill);
        AddBillItems(printer, bill);
        AddPricing(printer, bill, type);
        AddPayments(printer, bill);
        AddMembership(printer, bill);
        AddFooterBill(printer, bill);
        PM.CutPaper(printer);
    }

    static async Task AddLogo(IntPtr printer, BillModel bill)
    {
        await PM.PrintImageUrl(printer, bill.shop.image_url, "image.jpg");
        PM.NewLine(printer, 70);
    }

    static void AddBillNo(IntPtr printer, BillModel bill)
    {
        string topic = bill.language == "th" ? "ใบเรียกเก็บเงิน" : "Bill";
        PM.PrintText(printer, topic);
        string title = $"{(bill.language == "th" ? "เลขที่บิล" : "Bill No.")} {bill.doc_no}";
        PM.PrintText(printer, title, true, 120);
    }

    static void AddHeader(IntPtr printer, BillModel bill)
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
        //PM.PrintTextBold(printer, loc);

        string staffName = $"{(language == "th" ? "พนักงาน" : "staff")} {bill.staff_name ?? bill.cashier_staff_name}";
        //PM.PrintText(printer, staffName);

        string textLeftFiltered = Regex.Replace(loc, "[\u0E31\u0E34-\u0E3A\u0E47-\u0E4D]", "");
        int effectiveLeftLength = textLeftFiltered.Length;

        int spaceBetween = 42 - (effectiveLeftLength + staffName.Length);
        string line = loc + new string(' ', spaceBetween) + staffName;
        //output.Append(line + "\r\n");
        PM.PrintTextBold(printer, loc + new string(' ', spaceBetween), false);
        PM.PrintTextOnly(printer, staffName);
        PM.NewLine(printer);

        //PM.PrintTextTwoColumn(printer, loc, staffName);

        string date = language == "th" ? "วันที่" : "Order time";
        DateTimeHelper dateTimeHelper = new DateTimeHelper();
        string currentDate = dateTimeHelper.GetCurrentDateTime(language);
        PM.PrintText(printer, $"{date} {currentDate}");

        string generalCustomer = language == "th" ? "ลูกค้าทั่วไป" : "General Customer";
        string customer = $"{(language == "th" ? "ลูกค้า" : "Customer name")} {(bill.customer_name == "ลูกค้าทั่วไป" ? generalCustomer : bill.customer_name)}";
        PM.PrintText(printer, customer, true, 120);

        string headerA = language == "th" ? "รายการที่สั่ง" : "Item";
        PM.PrintText(printer, headerA);

        PM.DrawLine(printer);
        string headerB1 = (language == "th") ? "จำนวน" : "QTY";
        string headerB2 = (language == "th") ? "รายการ" : "ITEM";
        string headerB3 = (language == "th") ? "รวม" : "PRICE";
        PM.PrintTextThreeColumn(printer, headerB1, headerB2, headerB3);
        PM.DrawLine(printer);
    }

    static void AddBillItems(IntPtr printer, BillModel bill)
    {
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

            PM.PrintTextThreeColumn(printer, amount, itemName, itemPrice);

            if (billItem.bill_item_notes != null && billItem.bill_item_notes.Length > 0)
            {
                string wholeNote = $"({string.Join(", ", billItem.bill_item_notes.Select(n => n.note_note))})";
                PM.PrintTextThreeColumn(printer, "       ", wholeNote, "     ");
            }


            if (billItem.bill_item_product_toppings != null && billItem.bill_item_product_toppings.Length > 0)
            {
                foreach (var topping in billItem.bill_item_product_toppings)
                {
                    string toppingName = $"({topping.amount}) {topping.product_name}";
                    string toppingPrice = bill.shop.bill_is_show_topping_by_item == true ? topping.total_price.ToString() : string.Empty;
                    toppingPrice = CurrencyFormat(toppingPrice);
                    PM.PrintTextThreeColumn(printer, "", toppingName, toppingPrice);
                }
            }
        }
        PM.NewLine(printer);
    }

    static void AddPricing(IntPtr printer, BillModel bill, string type)
    {
        if (bill.receipt == null) return;
        //MessageBox.Show($"bill.receipt \n-{bill.receipt}");
        string language = bill.language ?? "th";
        string title = language == "th" ? "ราคารวม" : "Total amount";
        //string combinePrice = CurrencyFormat(bill.receipt.price.ToString());
        string combinePrice = CurrencyFormat(bill.receipt.price.ToString());
        PM.NewLine(printer, 40);
        PM.PrintTextTwoColumn(printer, title, combinePrice);
        PM.NewLine(printer, 20);

        // discount
        if ((bill?.receipt?.discount_total != null && bill.receipt?.discount_total != 0) && bill.receipt?.price > 0)
        {
            string discountAmount;
            string discountText = language == "th" ? "ส่วนลด" : "Sale discount";
            PM.PrintText(printer, discountText);

            var promotions = type == "invoice" ? bill.receipt?.invoice_promotions ?? [] : bill.receipt?.receipt_promotions ?? [];
            var pointPromotions = type == "invoice" ? bill.receipt?.invoice_point_promotions ?? [] : bill.receipt?.receipt_point_promotions ?? [];

            foreach (var item in promotions)
            {
                discountAmount = CurrencyFormat(item.discount.ToString());
                PM.PrintTextThreeColumn(printer, "", item.promotion_name, discountAmount, 42, 2);
            }

            foreach (var item in pointPromotions)
            {
                discountAmount = CurrencyFormat(item.discount.ToString());
                PM.PrintTextThreeColumn(printer, "", item.point_promotion_name, discountAmount, 42, 2);
            }

            if (bill.receipt?.discount_special > 0)
            {
                string additionalDiscount = $"{(language == "th" ? "ส่วนลดพิเศษ" : "Additional sales discount")} {bill.receipt?.discount_special_value} {(bill.receipt?.discount_special_type_id == 1 ? "บาท" : "%")}";
                discountAmount = CurrencyFormat(bill.receipt?.discount_special.ToString());
                PM.PrintTextThreeColumn(printer, "", additionalDiscount, discountAmount, 42, 2);
            }

            if (bill.receipt?.free_amount > 0)
            {
                string free = language == "th" ? "ฟรี" : "Discount of free items";
                discountAmount = CurrencyFormat(bill.receipt?.free_amount.ToString());
                PM.PrintTextThreeColumn(printer, "", free, discountAmount, 42, 2);
            }

            if (bill.receipt?.decimal_discount > 0)
            {
                string discountOfRound = language == "th" ? "ส่วนลดปัดเศษสตางค์" : "Discount of Round down to decimal";
                discountAmount = CurrencyFormat(bill.receipt?.decimal_discount.ToString());
                PM.PrintTextThreeColumn(printer, "", discountOfRound, discountAmount, 42, 2);
            }

            string totalSaleDiscount = language == "th" ? "ส่วนลดรวม" : "Total sales discount";
            discountAmount = CurrencyFormat(bill.receipt?.discount_total.ToString());
            PM.PrintTextTwoColumn(printer, totalSaleDiscount, discountAmount);
        }

        // service charge
        if (bill.receipt?.service_charge > 0)
        {
            string serviceCharge = $"{(language == "th" ? "ค่าบริการ" : "Service charge")} {bill.receipt?.service_charge_percent} %";
            string amount = CurrencyFormat(bill.receipt?.service_charge.ToString());
            PM.PrintTextTwoColumn(printer, serviceCharge, amount);
        }

        // vat
        if (bill.receipt?.document_vat_type_id != null && bill.receipt?.document_vat_type_id != 3)
        {
            string preVatAmount = $"{(language == "th" ? "ราคาไม่รวมภาษีมูลค่าเพิ่ม" : "Pre-vat Amount")} {bill.receipt?.vat_percent} %";
            string amount = CurrencyFormat(bill.receipt?.total_before_vat.ToString());
            PM.PrintTextTwoColumn(printer, preVatAmount, amount);

            string vat = $"{(language == "th" ? "ภาษีมูลค่าเพิ่ม" : "VAT")} {bill.receipt?.vat_percent} %";
            amount = CurrencyFormat(bill.receipt?.vat.ToString());
            PM.PrintTextTwoColumn(printer, vat, amount);
        }

        // total
        PM.NewLine(printer, 20);
        string grandTotal = language == "th" ? "ราคาสุทธิ" : "Grand total";
        string finalAmount = CurrencyFormat(bill.receipt?.total_after_vat.ToString(), true);
        PM.PrintTextTwoColumn(printer, grandTotal, finalAmount, 2);
        PM.NewLine(printer, 70);
    }

    public void AddMembership(IntPtr printer, BillModel bill)
    {
        string language = bill.language ?? "";
        //if (bill.receipt != null && bill.receipt.membership != null)
        if (bill.receipt != null && bill.receipt?.membership != null)
        {
            PM.NewLine(printer);
            string memberName = language == "th"
                ? $"ชื่อสมาชิก {bill.receipt.membership.member_name}"
                : $"Membership {bill.receipt.membership.member_name}";
            PM.PrintText(printer, memberName);

            string memberLevel = language == "th"
                ? $"ระดับสมาชิก {bill.receipt.membership.member_level_name}"
                : $"Membership level {bill.receipt.membership.member_level_name}";
            PM.PrintText(printer, memberLevel);

            string previousPoint = language == "th"
                ? "คะแนนก่อนหน้า"
                : "Previous Points";
            string beginningBalance = CurrencyFormat(bill.receipt.membership.beginning_balance.ToString(), true, 0);
            PM.PrintTextTwoColumn(printer, previousPoint, beginningBalance);

            string pointReward = language == "th"
                ? "คะแนนที่ได้รับ"
                : "Reward Points";
            string pointInc = CurrencyFormat(bill.receipt.membership.point_inc.ToString(), true, 0);
            PM.PrintTextTwoColumn(printer, pointReward, pointInc);

            string redeemPoint = language == "th"
                ? "คะแนนที่ใช้"
                : "Redeem Points";
            string pointUsed = CurrencyFormat(bill.receipt.membership.point_used.ToString(), true, 0);
            PM.PrintTextTwoColumn(printer, redeemPoint, pointUsed);

            string balancePoint = language == "th"
                ? "คะแนนสะสมคงเหลือ"
                : "Balance Points";
            string balance = CurrencyFormat(bill.receipt.membership.balance.ToString(), true, 0);
            PM.PrintTextTwoColumn(printer, balancePoint, balance);
            PM.NewLine(printer, 100);
        }
    }

    public async void AddFooterBill(IntPtr printer, BillModel bill)
    {
        if (bill.shop != null && bill.shop.receipt_footer_image_url != null)
        {
            string imageUrl = bill.shop.receipt_footer_image_url ?? "";
            await PM.PrintImageUrl(printer, imageUrl, "logo.jpg");
        }
        if (bill.shop != null && bill.shop.bill_footer_text != null)
        {
            foreach (var txt in bill.shop.bill_footer_text.Split(new[] { "\n" }, StringSplitOptions.None))
            {
                PM.AlignCenter(printer);
                PM.NewLine(printer);
                PM.PrintText(printer, $"{txt} ");
            }
        }

        if (bill.shop != null && bill.shop.is_connect_roommy != true)
        {
            string resrun = "Powered by ResRun Beyond POS";
            PM.AlignCenter(printer);
            PM.NewLine(printer, 70);
            PM.PrintText(printer, resrun);
        }
        PM.NewLine(printer);
    }

    public void AddShopAddress(IntPtr printer, BillModel bill)
    {
        string branchName = bill.shop.branch_name;
        string language = bill.language ?? "th";
        PM.AlignCenter(printer);
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
            PM.PrintText(printer, FormatText(shopName));

            string formattedBranchName = (!string.IsNullOrEmpty(branchName) ? $"({branchName})" : string.Empty);
            PM.PrintText(printer, FormatText(formattedBranchName));

            string addr = $"{(language == "th" ? "ที่อยู่" : "Address")} {bill.shop.tax_address} ";
            PM.PrintText(printer, FormatText(addr));
        }

        if (bill.receipt != null && bill.receipt.document_vat_type_id.HasValue && bill.receipt.document_vat_type_id != 3)
        {
            string taxNo = $"{(language == "th" ? "เลขประจำตัวผู้เสียภาษี" : "TAX ID")} {bill.shop.tax_no} ";
            PM.PrintText(printer, FormatText(taxNo));
        }
        PM.NewLine(printer, 50);
        string tel = $"{(language == "th" ? "เบอร์โทร" : "Tel")} {bill.shop.tel ?? ""} ";
        PM.PrintText(printer, FormatText(tel));
        PM.NewLine(printer, 70);
    }

    public void AddTitle(IntPtr printer, BillModel bill, string? type = null)
    {
        string vatTypeName = string.Empty;
        if (bill.receipt == null) return;

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
        PM.PrintText(printer, title);
        PM.TextAlignLeft(printer);
        PM.PrintText(printer, docText);

        if (bill.receipt.document_vat_type_id != 3)
        {
            PM.PrintText(printer, posText);
        }
    }

    public void AddPayments(IntPtr printer, BillModel bill)
    {
        if (bill.receipt == null || bill.receipt?.receipt_payments == null) return;

        double change = 0;
        string language = bill.language;
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
            PM.PrintTextTwoColumn(printer, paymentMethod, amount);

            change += payment.change ?? 0;
        }

        if (change > 0)
        {
            string changeAmount = CurrencyFormat(change.ToString());
            string changeK = language == "th" ? "เงินทอน" : "Change";
            PM.PrintTextTwoColumn(printer, changeK, changeAmount, 2);
        }
    }


    static string AmountFormatter(double amount)
    {
        return amount.ToString("0.###", CultureInfo.InvariantCulture);
    }

    static string CurrencyFormat(string? price, bool showZero = false, int digit = 2)
    {
        string formattedPrice = string.Empty;

        // Try to parse the price as a decimal
        if (decimal.TryParse(price ?? "", out decimal priceValue))
        {
            // If price is valid, format it with the specified number of decimal places
            formattedPrice = priceValue.ToString($"N{digit}", new CultureInfo("en-US"));
        }
        else if (showZero)
        {
            // If the price is invalid and showZero is true, return "0"
            formattedPrice = "0";
        }

        return formattedPrice;
    }

    static string FormatText(string text)
    {
        return text.Replace("\n", "\r\n");
    }

    public static List<BillModel> ParseData(Dictionary<string, JsonElement> data)
    {
        // Extract the necessary data from the dictionary using JsonElement
        var billItems = data.ContainsKey("bill_items") && data["bill_items"].ValueKind == JsonValueKind.Array
            ? System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>(data["bill_items"].ToString())
            : new List<Dictionary<string, object>>();
        //MessageBox.Show($"vbvbvbvb {billItems.Count}");

        var receipts = data.ContainsKey("receipts") && data["receipts"].ValueKind == JsonValueKind.Array
            ? System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>(data["receipts"].ToString())
            : null;
        //MessageBox.Show("qwqwqw");
        var invoices = data.ContainsKey("invoices") && data["invoices"].ValueKind == JsonValueKind.Array
            ? System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>(data["invoices"].ToString())
            : null;
        //MessageBox.Show("qwqwqw");
        //Shop shop = null;
        //if (data.ContainsKey("shop") && data["shop"].ValueKind != JsonValueKind.Null)
        //{
        //    shop = System.Text.Json.JsonSerializer.Deserialize<Shop>(data["shop"].ToString());
        //}
        //MessageBox.Show("iopiopiop");
        var baseItem = data.Where(kv => kv.Key != "bill_items" && kv.Key != "receipts" && kv.Key != "invoices" && kv.Key != "shop" && kv.Key != "staff")
                           .ToDictionary(kv => kv.Key, kv => (object)kv.Value.ToString());

        List<Dictionary<string, object>> items = new List<Dictionary<string, object>>();

        // Process receipts if available
        if (receipts != null)
        {
            foreach (var receipt in receipts)
            {
                var item = new Dictionary<string, object>(baseItem);
                //item["bill_items"] = billItems.Where(b => b["receipt_id"] == receipt["id"]).ToList();
                item["bill_items"] = billItems.Where(b =>
                {
                    string r1 = Convert.ToString(b["receipt_id"]);
                    string r2 = Convert.ToString(receipt["id"]);
                    //MessageBox.Show($"เปรียบเทียบ receipt_id: {b["receipt_id"]} กับ id: {receipt["id"]} ::: {r1 == r2}");
                    return r1 == r2;
                })
                .ToList();
                //var bits = item["bill_items"] as List<Dictionary<string, object>>();
                int count = ((List<Dictionary<string, object>>)item["bill_items"]).Count;

                item["receipt"] = receipt;
                //item["shop"] = shop;
                items.Add(item);
            }
        }

        // Process invoices if available
        if (invoices != null)
        {
            foreach (var invoice in invoices)
            {
                var item = new Dictionary<string, object>(baseItem);
                item["bill_items"] = billItems.Where(b => b.ContainsKey("invoice_id") && b["invoice_id"].Equals(invoice["id"])).ToList();
                item["receipt"] = invoice;
                //item["shop"] = shop;
                items.Add(item);
            }
        }
        //MessageBox.Show("erertert");
        //var options = new JsonSerializerOptions
        //{
        //    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        //};
        //var i = items;
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(items); // Serializing data to JSON
        WriteFile(json);
        return Newtonsoft.Json.JsonConvert.DeserializeObject<List<BillModel>>(json);

        //var json = System.Text.Json.JsonSerializer.Serialize(items);
        //var options = new JsonSerializerOptions
        //{
        //    AllowTrailingCommas = true, // Allow trailing commas if necessary
        //    IgnoreNullValues = true     // Ignore null values during deserialization
        //};
        //MessageBox.Show("nmnmnm");
        //return System.Text.Json.JsonSerializer.Deserialize<List<BillModel>>(json, options);
    }
    //v1
    //public static List<BillModel> ParseData(Dictionary<string, object> json)
    //{
    //    //string jsonString = JsonConvert.SerializeObject(json, Formatting.Indented);


    //    //test
    //    //var jsonConvert = Newtonsoft.Json.JsonConvert.SerializeObject(json);
    //    //BillModel data = Newtonsoft.Json.JsonConvert.DeserializeObject<BillModel>(jsonConvert);
    //    //test
    //    var data = new Dictionary<string, object>(json);
    //    //WriteFile(data["receipts"].ToString());
    //    var shop = data.ContainsKey("shop") ? data["shop"] : null;
    //    var baseItem = data;

    //    baseItem.Remove("bill_items");
    //    baseItem.Remove("receipts");
    //    baseItem.Remove("invoices");
    //    baseItem.Remove("shop");
    //    baseItem.Remove("staff");

    //    var items = new List<Dictionary<string, object>>();

    //    if (data.receipts.Length > 0)
    //    {
    //        //MessageBox.Show($"call if receipt {receipts}");
    //        foreach (var receipt in receipts)
    //        {
    //            MessageBox.Show($"call if receipt {receipt}");
    //            var item = new Dictionary<string, object>();
    //            foreach (var entry in baseItem)
    //            {
    //                item[entry.Key] = entry.Value;
    //            }

    //            if (data.bill_items.Length > 0)
    //            {
    //                item.bill_items = billItems.Where(b => b.receipt_id == receipt.id).ToList();
    //            }

    //            item["receipt"] = receipt;
    //            item["shop"] = shop;
    //            items.Add(item);
    //        }
    //    }

    //    if (data.invoices.Length > 0)
    //    {
    //        foreach (var invoice in invoices)
    //        {
    //            var item = new Dictionary<string, object>();
    //            foreach (var entry in baseItem)
    //            {
    //                item[entry.Key] = entry.Value;
    //            }

    //            if (data.bill_items.Length > 0)
    //            {
    //                item.bill_items = billItems.Where(b => b.receipt_id == invoice.id).ToList();
    //            }

    //            item["receipt"] = invoice;
    //            item["shop"] = shop;
    //            items.Add(item);
    //        }
    //    }

    //    var jsonConvert = Newtonsoft.Json.JsonConvert.SerializeObject(items);
    //    List<BillModel> billModels = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BillModel>>(jsonConvert);
    //    return billModels;
    //}

    //static bool IsContainKey(string key) {
    //    foreach (var key in data.Keys)
    //    {
    //        Console.WriteLine($"Key: {key}");
    //    }
    //}

    //test
    public static BillModel GenerateMockBillData()
    {
        string json = @"
{
    ""id"": 143,
    ""doc_no"": ""B671115000001"",
    ""open_date"": ""2024-11-15"",
    ""open_time"": ""13:19:43.919487"",
    ""close_date"": ""2024-11-15"",
    ""close_time"": ""13:44:52.083943"",
    ""bill_type_id"": 1,
    ""table_id"": 11,
    ""incharge_staff_id"": null,
    ""customer_id"": 1,
    ""total_after_vat"": 128,
    ""delivery_id"": null,
    ""document_status_id"": 1,
    ""cancel_date"": null,
    ""cancel_time"": null,
    ""customer_amount"": 0,
    ""is_diff_delivery"": false,
    ""delivery_diff"": 0,
    ""delivery_diff_unit_name"": null,
    ""remark"": null,
    ""tax_customer_id"": null,
    ""table_name"": ""B05"",
    ""table_zone_name"": ""DRC"",
    ""customer_name"": ""ลูกค้าทั่วไป"",
    ""tax_customer_name"": null,
    ""bill_type_name"": ""ทานร้าน"",
    ""document_status_name"": ""เสร็จสิ้น"",
    ""cancel_staff_name"": null,
    ""cashier_staff_name"": ""Admin"",
    ""delivery_name"": null,
    ""bill_type_with_delivery_id"": 1,
    ""is_take_home"": false,
    ""language"": ""th"",
    ""bill_items"": [
        {
            ""id"": 758,
            ""product_category_id"": 1,
            ""product_sub_category_id"": null,
            ""product_id"": 1,
            ""product_item_id"": 1,
            ""amount"": 1,
            ""unit_price"": 59,
            ""price"": 59,
            ""note"": null,
            ""bill_item_status_master_id"": 5,
            ""is_take_home"": false,
            ""is_free"": false,
            ""created_at"": ""2024-11-15T13:43:29.754+07:00"",
            ""free_discount"": 0,
            ""free_amount"": 0,
            ""bill_item_set_id"": null,
            ""unit_price_item"": 59,
            ""price_item"": 59,
            ""barcode"": ""8857316908425"",
            ""bill_no"": ""B671115000001"",
            ""table_name"": ""B05"",
            ""product_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_is_buffet"": false,
            ""product_is_sell_by_weight"": false,
            ""product_item_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_item_code"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""receipt_item_id"": 284,
            ""receipt_id"": 57,
            ""invoice_item_id"": null,
            ""invoice_id"": null,
            ""product_is_has_option"": false,
            ""product_sub_category_product_id"": 1,
            ""product_is_show_in_receipt"": true,
            ""product_image_url"": ""https://storage.googleapis.com/storage-resrun-pos-com-demo7riseplus/ax0th3ttz9nlh4z1qsy4j9je9ow3"",
            ""sub_bill_items"": [],
            ""bill_item_product_toppings"": [],
            ""bill_item_notes"": [],
            ""has_bill_item_notes"": false,
            ""is_set"": false,
            ""remark"": null,
            ""order_by_product_is_buffet"": 2,
            ""order_by_product_category_id"": -1,
            ""order_by_id"": 758
        },
        {
            ""id"": 758,
            ""product_category_id"": 1,
            ""product_sub_category_id"": null,
            ""product_id"": 1,
            ""product_item_id"": 1,
            ""amount"": 1,
            ""unit_price"": 59,
            ""price"": 59,
            ""note"": null,
            ""bill_item_status_master_id"": 5,
            ""is_take_home"": false,
            ""is_free"": false,
            ""created_at"": ""2024-11-15T13:43:29.754+07:00"",
            ""free_discount"": 0,
            ""free_amount"": 0,
            ""bill_item_set_id"": null,
            ""unit_price_item"": 59,
            ""price_item"": 59,
            ""barcode"": ""8857316908425"",
            ""bill_no"": ""B671115000001"",
            ""table_name"": ""B05"",
            ""product_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_is_buffet"": false,
            ""product_is_sell_by_weight"": false,
            ""product_item_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_item_code"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""receipt_item_id"": 284,
            ""receipt_id"": 57,
            ""invoice_item_id"": null,
            ""invoice_id"": null,
            ""product_is_has_option"": false,
            ""product_sub_category_product_id"": 1,
            ""product_is_show_in_receipt"": true,
            ""product_image_url"": ""https://storage.googleapis.com/storage-resrun-pos-com-demo7riseplus/ax0th3ttz9nlh4z1qsy4j9je9ow3"",
            ""sub_bill_items"": [],
            ""bill_item_product_toppings"": [],
            ""bill_item_notes"": [],
            ""has_bill_item_notes"": false,
            ""is_set"": false,
            ""remark"": null,
            ""order_by_product_is_buffet"": 2,
            ""order_by_product_category_id"": -1,
            ""order_by_id"": 758
        },
        {
            ""id"": 758,
            ""product_category_id"": 1,
            ""product_sub_category_id"": null,
            ""product_id"": 1,
            ""product_item_id"": 1,
            ""amount"": 1,
            ""unit_price"": 59,
            ""price"": 59,
            ""note"": null,
            ""bill_item_status_master_id"": 5,
            ""is_take_home"": false,
            ""is_free"": false,
            ""created_at"": ""2024-11-15T13:43:29.754+07:00"",
            ""free_discount"": 0,
            ""free_amount"": 0,
            ""bill_item_set_id"": null,
            ""unit_price_item"": 59,
            ""price_item"": 59,
            ""barcode"": ""8857316908425"",
            ""bill_no"": ""B671115000001"",
            ""table_name"": ""B05"",
            ""product_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_is_buffet"": false,
            ""product_is_sell_by_weight"": false,
            ""product_item_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_item_code"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""receipt_item_id"": 284,
            ""receipt_id"": 57,
            ""invoice_item_id"": null,
            ""invoice_id"": null,
            ""product_is_has_option"": false,
            ""product_sub_category_product_id"": 1,
            ""product_is_show_in_receipt"": true,
            ""product_image_url"": ""https://storage.googleapis.com/storage-resrun-pos-com-demo7riseplus/ax0th3ttz9nlh4z1qsy4j9je9ow3"",
            ""sub_bill_items"": [],
            ""bill_item_product_toppings"": [],
            ""bill_item_notes"": [],
            ""has_bill_item_notes"": false,
            ""is_set"": false,
            ""remark"": null,
            ""order_by_product_is_buffet"": 2,
            ""order_by_product_category_id"": -1,
            ""order_by_id"": 758
        },
        {
            ""id"": 758,
            ""product_category_id"": 1,
            ""product_sub_category_id"": null,
            ""product_id"": 1,
            ""product_item_id"": 1,
            ""amount"": 1,
            ""unit_price"": 59,
            ""price"": 59,
            ""note"": null,
            ""bill_item_status_master_id"": 5,
            ""is_take_home"": false,
            ""is_free"": false,
            ""created_at"": ""2024-11-15T13:43:29.754+07:00"",
            ""free_discount"": 0,
            ""free_amount"": 0,
            ""bill_item_set_id"": null,
            ""unit_price_item"": 59,
            ""price_item"": 59,
            ""barcode"": ""8857316908425"",
            ""bill_no"": ""B671115000001"",
            ""table_name"": ""B05"",
            ""product_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_is_buffet"": false,
            ""product_is_sell_by_weight"": false,
            ""product_item_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_item_code"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""receipt_item_id"": 284,
            ""receipt_id"": 57,
            ""invoice_item_id"": null,
            ""invoice_id"": null,
            ""product_is_has_option"": false,
            ""product_sub_category_product_id"": 1,
            ""product_is_show_in_receipt"": true,
            ""product_image_url"": ""https://storage.googleapis.com/storage-resrun-pos-com-demo7riseplus/ax0th3ttz9nlh4z1qsy4j9je9ow3"",
            ""sub_bill_items"": [],
            ""bill_item_product_toppings"": [],
            ""bill_item_notes"": [],
            ""has_bill_item_notes"": false,
            ""is_set"": false,
            ""remark"": null,
            ""order_by_product_is_buffet"": 2,
            ""order_by_product_category_id"": -1,
            ""order_by_id"": 758
        },
        {
            ""id"": 758,
            ""product_category_id"": 1,
            ""product_sub_category_id"": null,
            ""product_id"": 1,
            ""product_item_id"": 1,
            ""amount"": 1,
            ""unit_price"": 59,
            ""price"": 59,
            ""note"": null,
            ""bill_item_status_master_id"": 5,
            ""is_take_home"": false,
            ""is_free"": false,
            ""created_at"": ""2024-11-15T13:43:29.754+07:00"",
            ""free_discount"": 0,
            ""free_amount"": 0,
            ""bill_item_set_id"": null,
            ""unit_price_item"": 59,
            ""price_item"": 59,
            ""barcode"": ""8857316908425"",
            ""bill_no"": ""B671115000001"",
            ""table_name"": ""B05"",
            ""product_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_is_buffet"": false,
            ""product_is_sell_by_weight"": false,
            ""product_item_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_item_code"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""receipt_item_id"": 284,
            ""receipt_id"": 57,
            ""invoice_item_id"": null,
            ""invoice_id"": null,
            ""product_is_has_option"": false,
            ""product_sub_category_product_id"": 1,
            ""product_is_show_in_receipt"": true,
            ""product_image_url"": ""https://storage.googleapis.com/storage-resrun-pos-com-demo7riseplus/ax0th3ttz9nlh4z1qsy4j9je9ow3"",
            ""sub_bill_items"": [],
            ""bill_item_product_toppings"": [],
            ""bill_item_notes"": [],
            ""has_bill_item_notes"": false,
            ""is_set"": false,
            ""remark"": null,
            ""order_by_product_is_buffet"": 2,
            ""order_by_product_category_id"": -1,
            ""order_by_id"": 758
        },
        {
            ""id"": 758,
            ""product_category_id"": 1,
            ""product_sub_category_id"": null,
            ""product_id"": 1,
            ""product_item_id"": 1,
            ""amount"": 1,
            ""unit_price"": 59,
            ""price"": 59,
            ""note"": null,
            ""bill_item_status_master_id"": 5,
            ""is_take_home"": false,
            ""is_free"": false,
            ""created_at"": ""2024-11-15T13:43:29.754+07:00"",
            ""free_discount"": 0,
            ""free_amount"": 0,
            ""bill_item_set_id"": null,
            ""unit_price_item"": 59,
            ""price_item"": 59,
            ""barcode"": ""8857316908425"",
            ""bill_no"": ""B671115000001"",
            ""table_name"": ""B05"",
            ""product_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_is_buffet"": false,
            ""product_is_sell_by_weight"": false,
            ""product_item_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_item_code"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""receipt_item_id"": 284,
            ""receipt_id"": 57,
            ""invoice_item_id"": null,
            ""invoice_id"": null,
            ""product_is_has_option"": false,
            ""product_sub_category_product_id"": 1,
            ""product_is_show_in_receipt"": true,
            ""product_image_url"": ""https://storage.googleapis.com/storage-resrun-pos-com-demo7riseplus/ax0th3ttz9nlh4z1qsy4j9je9ow3"",
            ""sub_bill_items"": [],
            ""bill_item_product_toppings"": [],
            ""bill_item_notes"": [],
            ""has_bill_item_notes"": false,
            ""is_set"": false,
            ""remark"": null,
            ""order_by_product_is_buffet"": 2,
            ""order_by_product_category_id"": -1,
            ""order_by_id"": 758
        },
        {
            ""id"": 758,
            ""product_category_id"": 1,
            ""product_sub_category_id"": null,
            ""product_id"": 1,
            ""product_item_id"": 1,
            ""amount"": 1,
            ""unit_price"": 59,
            ""price"": 59,
            ""note"": null,
            ""bill_item_status_master_id"": 5,
            ""is_take_home"": false,
            ""is_free"": false,
            ""created_at"": ""2024-11-15T13:43:29.754+07:00"",
            ""free_discount"": 0,
            ""free_amount"": 0,
            ""bill_item_set_id"": null,
            ""unit_price_item"": 59,
            ""price_item"": 59,
            ""barcode"": ""8857316908425"",
            ""bill_no"": ""B671115000001"",
            ""table_name"": ""B05"",
            ""product_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_is_buffet"": false,
            ""product_is_sell_by_weight"": false,
            ""product_item_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_item_code"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""receipt_item_id"": 284,
            ""receipt_id"": 57,
            ""invoice_item_id"": null,
            ""invoice_id"": null,
            ""product_is_has_option"": false,
            ""product_sub_category_product_id"": 1,
            ""product_is_show_in_receipt"": true,
            ""product_image_url"": ""https://storage.googleapis.com/storage-resrun-pos-com-demo7riseplus/ax0th3ttz9nlh4z1qsy4j9je9ow3"",
            ""sub_bill_items"": [],
            ""bill_item_product_toppings"": [],
            ""bill_item_notes"": [],
            ""has_bill_item_notes"": false,
            ""is_set"": false,
            ""remark"": null,
            ""order_by_product_is_buffet"": 2,
            ""order_by_product_category_id"": -1,
            ""order_by_id"": 758
        },
        {
            ""id"": 758,
            ""product_category_id"": 1,
            ""product_sub_category_id"": null,
            ""product_id"": 1,
            ""product_item_id"": 1,
            ""amount"": 1,
            ""unit_price"": 59,
            ""price"": 59,
            ""note"": null,
            ""bill_item_status_master_id"": 5,
            ""is_take_home"": false,
            ""is_free"": false,
            ""created_at"": ""2024-11-15T13:43:29.754+07:00"",
            ""free_discount"": 0,
            ""free_amount"": 0,
            ""bill_item_set_id"": null,
            ""unit_price_item"": 59,
            ""price_item"": 59,
            ""barcode"": ""8857316908425"",
            ""bill_no"": ""B671115000001"",
            ""table_name"": ""B05"",
            ""product_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_is_buffet"": false,
            ""product_is_sell_by_weight"": false,
            ""product_item_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_item_code"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""receipt_item_id"": 284,
            ""receipt_id"": 57,
            ""invoice_item_id"": null,
            ""invoice_id"": null,
            ""product_is_has_option"": false,
            ""product_sub_category_product_id"": 1,
            ""product_is_show_in_receipt"": true,
            ""product_image_url"": ""https://storage.googleapis.com/storage-resrun-pos-com-demo7riseplus/ax0th3ttz9nlh4z1qsy4j9je9ow3"",
            ""sub_bill_items"": [],
            ""bill_item_product_toppings"": [],
            ""bill_item_notes"": [],
            ""has_bill_item_notes"": false,
            ""is_set"": false,
            ""remark"": null,
            ""order_by_product_is_buffet"": 2,
            ""order_by_product_category_id"": -1,
            ""order_by_id"": 758
        },
        {
            ""id"": 758,
            ""product_category_id"": 1,
            ""product_sub_category_id"": null,
            ""product_id"": 1,
            ""product_item_id"": 1,
            ""amount"": 1,
            ""unit_price"": 59,
            ""price"": 59,
            ""note"": null,
            ""bill_item_status_master_id"": 5,
            ""is_take_home"": false,
            ""is_free"": false,
            ""created_at"": ""2024-11-15T13:43:29.754+07:00"",
            ""free_discount"": 0,
            ""free_amount"": 0,
            ""bill_item_set_id"": null,
            ""unit_price_item"": 59,
            ""price_item"": 59,
            ""barcode"": ""8857316908425"",
            ""bill_no"": ""B671115000001"",
            ""table_name"": ""B05"",
            ""product_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_is_buffet"": false,
            ""product_is_sell_by_weight"": false,
            ""product_item_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_item_code"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""receipt_item_id"": 284,
            ""receipt_id"": 57,
            ""invoice_item_id"": null,
            ""invoice_id"": null,
            ""product_is_has_option"": false,
            ""product_sub_category_product_id"": 1,
            ""product_is_show_in_receipt"": true,
            ""product_image_url"": ""https://storage.googleapis.com/storage-resrun-pos-com-demo7riseplus/ax0th3ttz9nlh4z1qsy4j9je9ow3"",
            ""sub_bill_items"": [],
            ""bill_item_product_toppings"": [],
            ""bill_item_notes"": [],
            ""has_bill_item_notes"": false,
            ""is_set"": false,
            ""remark"": null,
            ""order_by_product_is_buffet"": 2,
            ""order_by_product_category_id"": -1,
            ""order_by_id"": 758
        },
        {
            ""id"": 758,
            ""product_category_id"": 1,
            ""product_sub_category_id"": null,
            ""product_id"": 1,
            ""product_item_id"": 1,
            ""amount"": 1,
            ""unit_price"": 59,
            ""price"": 59,
            ""note"": null,
            ""bill_item_status_master_id"": 5,
            ""is_take_home"": false,
            ""is_free"": false,
            ""created_at"": ""2024-11-15T13:43:29.754+07:00"",
            ""free_discount"": 0,
            ""free_amount"": 0,
            ""bill_item_set_id"": null,
            ""unit_price_item"": 59,
            ""price_item"": 59,
            ""barcode"": ""8857316908425"",
            ""bill_no"": ""B671115000001"",
            ""table_name"": ""B05"",
            ""product_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_is_buffet"": false,
            ""product_is_sell_by_weight"": false,
            ""product_item_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_item_code"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""receipt_item_id"": 284,
            ""receipt_id"": 57,
            ""invoice_item_id"": null,
            ""invoice_id"": null,
            ""product_is_has_option"": false,
            ""product_sub_category_product_id"": 1,
            ""product_is_show_in_receipt"": true,
            ""product_image_url"": ""https://storage.googleapis.com/storage-resrun-pos-com-demo7riseplus/ax0th3ttz9nlh4z1qsy4j9je9ow3"",
            ""sub_bill_items"": [],
            ""bill_item_product_toppings"": [],
            ""bill_item_notes"": [],
            ""has_bill_item_notes"": false,
            ""is_set"": false,
            ""remark"": null,
            ""order_by_product_is_buffet"": 2,
            ""order_by_product_category_id"": -1,
            ""order_by_id"": 758
        },
        {
            ""id"": 758,
            ""product_category_id"": 1,
            ""product_sub_category_id"": null,
            ""product_id"": 1,
            ""product_item_id"": 1,
            ""amount"": 1,
            ""unit_price"": 59,
            ""price"": 59,
            ""note"": null,
            ""bill_item_status_master_id"": 5,
            ""is_take_home"": false,
            ""is_free"": false,
            ""created_at"": ""2024-11-15T13:43:29.754+07:00"",
            ""free_discount"": 0,
            ""free_amount"": 0,
            ""bill_item_set_id"": null,
            ""unit_price_item"": 59,
            ""price_item"": 59,
            ""barcode"": ""8857316908425"",
            ""bill_no"": ""B671115000001"",
            ""table_name"": ""B05"",
            ""product_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_is_buffet"": false,
            ""product_is_sell_by_weight"": false,
            ""product_item_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_item_code"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""receipt_item_id"": 284,
            ""receipt_id"": 57,
            ""invoice_item_id"": null,
            ""invoice_id"": null,
            ""product_is_has_option"": false,
            ""product_sub_category_product_id"": 1,
            ""product_is_show_in_receipt"": true,
            ""product_image_url"": ""https://storage.googleapis.com/storage-resrun-pos-com-demo7riseplus/ax0th3ttz9nlh4z1qsy4j9je9ow3"",
            ""sub_bill_items"": [],
            ""bill_item_product_toppings"": [],
            ""bill_item_notes"": [],
            ""has_bill_item_notes"": false,
            ""is_set"": false,
            ""remark"": null,
            ""order_by_product_is_buffet"": 2,
            ""order_by_product_category_id"": -1,
            ""order_by_id"": 758
        },
        {
            ""id"": 758,
            ""product_category_id"": 1,
            ""product_sub_category_id"": null,
            ""product_id"": 1,
            ""product_item_id"": 1,
            ""amount"": 1,
            ""unit_price"": 59,
            ""price"": 59,
            ""note"": null,
            ""bill_item_status_master_id"": 5,
            ""is_take_home"": false,
            ""is_free"": false,
            ""created_at"": ""2024-11-15T13:43:29.754+07:00"",
            ""free_discount"": 0,
            ""free_amount"": 0,
            ""bill_item_set_id"": null,
            ""unit_price_item"": 59,
            ""price_item"": 59,
            ""barcode"": ""8857316908425"",
            ""bill_no"": ""B671115000001"",
            ""table_name"": ""B05"",
            ""product_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_is_buffet"": false,
            ""product_is_sell_by_weight"": false,
            ""product_item_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_item_code"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""receipt_item_id"": 284,
            ""receipt_id"": 57,
            ""invoice_item_id"": null,
            ""invoice_id"": null,
            ""product_is_has_option"": false,
            ""product_sub_category_product_id"": 1,
            ""product_is_show_in_receipt"": true,
            ""product_image_url"": ""https://storage.googleapis.com/storage-resrun-pos-com-demo7riseplus/ax0th3ttz9nlh4z1qsy4j9je9ow3"",
            ""sub_bill_items"": [],
            ""bill_item_product_toppings"": [],
            ""bill_item_notes"": [],
            ""has_bill_item_notes"": false,
            ""is_set"": false,
            ""remark"": null,
            ""order_by_product_is_buffet"": 2,
            ""order_by_product_category_id"": -1,
            ""order_by_id"": 758
        },
        {
            ""id"": 758,
            ""product_category_id"": 1,
            ""product_sub_category_id"": null,
            ""product_id"": 1,
            ""product_item_id"": 1,
            ""amount"": 1,
            ""unit_price"": 59,
            ""price"": 59,
            ""note"": null,
            ""bill_item_status_master_id"": 5,
            ""is_take_home"": false,
            ""is_free"": false,
            ""created_at"": ""2024-11-15T13:43:29.754+07:00"",
            ""free_discount"": 0,
            ""free_amount"": 0,
            ""bill_item_set_id"": null,
            ""unit_price_item"": 59,
            ""price_item"": 59,
            ""barcode"": ""8857316908425"",
            ""bill_no"": ""B671115000001"",
            ""table_name"": ""B05"",
            ""product_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_is_buffet"": false,
            ""product_is_sell_by_weight"": false,
            ""product_item_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_item_code"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""receipt_item_id"": 284,
            ""receipt_id"": 57,
            ""invoice_item_id"": null,
            ""invoice_id"": null,
            ""product_is_has_option"": false,
            ""product_sub_category_product_id"": 1,
            ""product_is_show_in_receipt"": true,
            ""product_image_url"": ""https://storage.googleapis.com/storage-resrun-pos-com-demo7riseplus/ax0th3ttz9nlh4z1qsy4j9je9ow3"",
            ""sub_bill_items"": [],
            ""bill_item_product_toppings"": [],
            ""bill_item_notes"": [],
            ""has_bill_item_notes"": false,
            ""is_set"": false,
            ""remark"": null,
            ""order_by_product_is_buffet"": 2,
            ""order_by_product_category_id"": -1,
            ""order_by_id"": 758
        },
        {
            ""id"": 758,
            ""product_category_id"": 1,
            ""product_sub_category_id"": null,
            ""product_id"": 1,
            ""product_item_id"": 1,
            ""amount"": 1,
            ""unit_price"": 59,
            ""price"": 59,
            ""note"": null,
            ""bill_item_status_master_id"": 5,
            ""is_take_home"": false,
            ""is_free"": false,
            ""created_at"": ""2024-11-15T13:43:29.754+07:00"",
            ""free_discount"": 0,
            ""free_amount"": 0,
            ""bill_item_set_id"": null,
            ""unit_price_item"": 59,
            ""price_item"": 59,
            ""barcode"": ""8857316908425"",
            ""bill_no"": ""B671115000001"",
            ""table_name"": ""B05"",
            ""product_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_is_buffet"": false,
            ""product_is_sell_by_weight"": false,
            ""product_item_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_item_code"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""receipt_item_id"": 284,
            ""receipt_id"": 57,
            ""invoice_item_id"": null,
            ""invoice_id"": null,
            ""product_is_has_option"": false,
            ""product_sub_category_product_id"": 1,
            ""product_is_show_in_receipt"": true,
            ""product_image_url"": ""https://storage.googleapis.com/storage-resrun-pos-com-demo7riseplus/ax0th3ttz9nlh4z1qsy4j9je9ow3"",
            ""sub_bill_items"": [],
            ""bill_item_product_toppings"": [],
            ""bill_item_notes"": [],
            ""has_bill_item_notes"": false,
            ""is_set"": false,
            ""remark"": null,
            ""order_by_product_is_buffet"": 2,
            ""order_by_product_category_id"": -1,
            ""order_by_id"": 758
        },
        {
            ""id"": 758,
            ""product_category_id"": 1,
            ""product_sub_category_id"": null,
            ""product_id"": 1,
            ""product_item_id"": 1,
            ""amount"": 1,
            ""unit_price"": 59,
            ""price"": 59,
            ""note"": null,
            ""bill_item_status_master_id"": 5,
            ""is_take_home"": false,
            ""is_free"": false,
            ""created_at"": ""2024-11-15T13:43:29.754+07:00"",
            ""free_discount"": 0,
            ""free_amount"": 0,
            ""bill_item_set_id"": null,
            ""unit_price_item"": 59,
            ""price_item"": 59,
            ""barcode"": ""8857316908425"",
            ""bill_no"": ""B671115000001"",
            ""table_name"": ""B05"",
            ""product_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_is_buffet"": false,
            ""product_is_sell_by_weight"": false,
            ""product_item_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_item_code"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""receipt_item_id"": 284,
            ""receipt_id"": 57,
            ""invoice_item_id"": null,
            ""invoice_id"": null,
            ""product_is_has_option"": false,
            ""product_sub_category_product_id"": 1,
            ""product_is_show_in_receipt"": true,
            ""product_image_url"": ""https://storage.googleapis.com/storage-resrun-pos-com-demo7riseplus/ax0th3ttz9nlh4z1qsy4j9je9ow3"",
            ""sub_bill_items"": [],
            ""bill_item_product_toppings"": [],
            ""bill_item_notes"": [],
            ""has_bill_item_notes"": false,
            ""is_set"": false,
            ""remark"": null,
            ""order_by_product_is_buffet"": 2,
            ""order_by_product_category_id"": -1,
            ""order_by_id"": 758
        },
        {
            ""id"": 758,
            ""product_category_id"": 1,
            ""product_sub_category_id"": null,
            ""product_id"": 1,
            ""product_item_id"": 1,
            ""amount"": 1,
            ""unit_price"": 59,
            ""price"": 59,
            ""note"": null,
            ""bill_item_status_master_id"": 5,
            ""is_take_home"": false,
            ""is_free"": false,
            ""created_at"": ""2024-11-15T13:43:29.754+07:00"",
            ""free_discount"": 0,
            ""free_amount"": 0,
            ""bill_item_set_id"": null,
            ""unit_price_item"": 59,
            ""price_item"": 59,
            ""barcode"": ""8857316908425"",
            ""bill_no"": ""B671115000001"",
            ""table_name"": ""B05"",
            ""product_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_is_buffet"": false,
            ""product_is_sell_by_weight"": false,
            ""product_item_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_item_code"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""receipt_item_id"": 284,
            ""receipt_id"": 57,
            ""invoice_item_id"": null,
            ""invoice_id"": null,
            ""product_is_has_option"": false,
            ""product_sub_category_product_id"": 1,
            ""product_is_show_in_receipt"": true,
            ""product_image_url"": ""https://storage.googleapis.com/storage-resrun-pos-com-demo7riseplus/ax0th3ttz9nlh4z1qsy4j9je9ow3"",
            ""sub_bill_items"": [],
            ""bill_item_product_toppings"": [],
            ""bill_item_notes"": [],
            ""has_bill_item_notes"": false,
            ""is_set"": false,
            ""remark"": null,
            ""order_by_product_is_buffet"": 2,
            ""order_by_product_category_id"": -1,
            ""order_by_id"": 758
        },
        {
            ""id"": 758,
            ""product_category_id"": 1,
            ""product_sub_category_id"": null,
            ""product_id"": 1,
            ""product_item_id"": 1,
            ""amount"": 1,
            ""unit_price"": 59,
            ""price"": 59,
            ""note"": null,
            ""bill_item_status_master_id"": 5,
            ""is_take_home"": false,
            ""is_free"": false,
            ""created_at"": ""2024-11-15T13:43:29.754+07:00"",
            ""free_discount"": 0,
            ""free_amount"": 0,
            ""bill_item_set_id"": null,
            ""unit_price_item"": 59,
            ""price_item"": 59,
            ""barcode"": ""8857316908425"",
            ""bill_no"": ""B671115000001"",
            ""table_name"": ""B05"",
            ""product_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_is_buffet"": false,
            ""product_is_sell_by_weight"": false,
            ""product_item_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_item_code"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""receipt_item_id"": 284,
            ""receipt_id"": 57,
            ""invoice_item_id"": null,
            ""invoice_id"": null,
            ""product_is_has_option"": false,
            ""product_sub_category_product_id"": 1,
            ""product_is_show_in_receipt"": true,
            ""product_image_url"": ""https://storage.googleapis.com/storage-resrun-pos-com-demo7riseplus/ax0th3ttz9nlh4z1qsy4j9je9ow3"",
            ""sub_bill_items"": [],
            ""bill_item_product_toppings"": [],
            ""bill_item_notes"": [],
            ""has_bill_item_notes"": false,
            ""is_set"": false,
            ""remark"": null,
            ""order_by_product_is_buffet"": 2,
            ""order_by_product_category_id"": -1,
            ""order_by_id"": 758
        },
        {
            ""id"": 758,
            ""product_category_id"": 1,
            ""product_sub_category_id"": null,
            ""product_id"": 1,
            ""product_item_id"": 1,
            ""amount"": 1,
            ""unit_price"": 59,
            ""price"": 59,
            ""note"": null,
            ""bill_item_status_master_id"": 5,
            ""is_take_home"": false,
            ""is_free"": false,
            ""created_at"": ""2024-11-15T13:43:29.754+07:00"",
            ""free_discount"": 0,
            ""free_amount"": 0,
            ""bill_item_set_id"": null,
            ""unit_price_item"": 59,
            ""price_item"": 59,
            ""barcode"": ""8857316908425"",
            ""bill_no"": ""B671115000001"",
            ""table_name"": ""B05"",
            ""product_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_is_buffet"": false,
            ""product_is_sell_by_weight"": false,
            ""product_item_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_item_code"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""receipt_item_id"": 284,
            ""receipt_id"": 57,
            ""invoice_item_id"": null,
            ""invoice_id"": null,
            ""product_is_has_option"": false,
            ""product_sub_category_product_id"": 1,
            ""product_is_show_in_receipt"": true,
            ""product_image_url"": ""https://storage.googleapis.com/storage-resrun-pos-com-demo7riseplus/ax0th3ttz9nlh4z1qsy4j9je9ow3"",
            ""sub_bill_items"": [],
            ""bill_item_product_toppings"": [],
            ""bill_item_notes"": [],
            ""has_bill_item_notes"": false,
            ""is_set"": false,
            ""remark"": null,
            ""order_by_product_is_buffet"": 2,
            ""order_by_product_category_id"": -1,
            ""order_by_id"": 758
        },
        {
            ""id"": 758,
            ""product_category_id"": 1,
            ""product_sub_category_id"": null,
            ""product_id"": 1,
            ""product_item_id"": 1,
            ""amount"": 1,
            ""unit_price"": 59,
            ""price"": 59,
            ""note"": null,
            ""bill_item_status_master_id"": 5,
            ""is_take_home"": false,
            ""is_free"": false,
            ""created_at"": ""2024-11-15T13:43:29.754+07:00"",
            ""free_discount"": 0,
            ""free_amount"": 0,
            ""bill_item_set_id"": null,
            ""unit_price_item"": 59,
            ""price_item"": 59,
            ""barcode"": ""8857316908425"",
            ""bill_no"": ""B671115000001"",
            ""table_name"": ""B05"",
            ""product_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_is_buffet"": false,
            ""product_is_sell_by_weight"": false,
            ""product_item_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_item_code"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""receipt_item_id"": 284,
            ""receipt_id"": 57,
            ""invoice_item_id"": null,
            ""invoice_id"": null,
            ""product_is_has_option"": false,
            ""product_sub_category_product_id"": 1,
            ""product_is_show_in_receipt"": true,
            ""product_image_url"": ""https://storage.googleapis.com/storage-resrun-pos-com-demo7riseplus/ax0th3ttz9nlh4z1qsy4j9je9ow3"",
            ""sub_bill_items"": [],
            ""bill_item_product_toppings"": [],
            ""bill_item_notes"": [],
            ""has_bill_item_notes"": false,
            ""is_set"": false,
            ""remark"": null,
            ""order_by_product_is_buffet"": 2,
            ""order_by_product_category_id"": -1,
            ""order_by_id"": 758
        },
        {
            ""id"": 758,
            ""product_category_id"": 1,
            ""product_sub_category_id"": null,
            ""product_id"": 1,
            ""product_item_id"": 1,
            ""amount"": 1,
            ""unit_price"": 59,
            ""price"": 59,
            ""note"": null,
            ""bill_item_status_master_id"": 5,
            ""is_take_home"": false,
            ""is_free"": false,
            ""created_at"": ""2024-11-15T13:43:29.754+07:00"",
            ""free_discount"": 0,
            ""free_amount"": 0,
            ""bill_item_set_id"": null,
            ""unit_price_item"": 59,
            ""price_item"": 59,
            ""barcode"": ""8857316908425"",
            ""bill_no"": ""B671115000001"",
            ""table_name"": ""B05"",
            ""product_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_is_buffet"": false,
            ""product_is_sell_by_weight"": false,
            ""product_item_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_item_code"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""receipt_item_id"": 284,
            ""receipt_id"": 57,
            ""invoice_item_id"": null,
            ""invoice_id"": null,
            ""product_is_has_option"": false,
            ""product_sub_category_product_id"": 1,
            ""product_is_show_in_receipt"": true,
            ""product_image_url"": ""https://storage.googleapis.com/storage-resrun-pos-com-demo7riseplus/ax0th3ttz9nlh4z1qsy4j9je9ow3"",
            ""sub_bill_items"": [],
            ""bill_item_product_toppings"": [],
            ""bill_item_notes"": [],
            ""has_bill_item_notes"": false,
            ""is_set"": false,
            ""remark"": null,
            ""order_by_product_is_buffet"": 2,
            ""order_by_product_category_id"": -1,
            ""order_by_id"": 758
        },
        {
            ""id"": 758,
            ""product_category_id"": 1,
            ""product_sub_category_id"": null,
            ""product_id"": 1,
            ""product_item_id"": 1,
            ""amount"": 1,
            ""unit_price"": 59,
            ""price"": 59,
            ""note"": null,
            ""bill_item_status_master_id"": 5,
            ""is_take_home"": false,
            ""is_free"": false,
            ""created_at"": ""2024-11-15T13:43:29.754+07:00"",
            ""free_discount"": 0,
            ""free_amount"": 0,
            ""bill_item_set_id"": null,
            ""unit_price_item"": 59,
            ""price_item"": 59,
            ""barcode"": ""8857316908425"",
            ""bill_no"": ""B671115000001"",
            ""table_name"": ""B05"",
            ""product_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_is_buffet"": false,
            ""product_is_sell_by_weight"": false,
            ""product_item_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_item_code"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""receipt_item_id"": 284,
            ""receipt_id"": 57,
            ""invoice_item_id"": null,
            ""invoice_id"": null,
            ""product_is_has_option"": false,
            ""product_sub_category_product_id"": 1,
            ""product_is_show_in_receipt"": true,
            ""product_image_url"": ""https://storage.googleapis.com/storage-resrun-pos-com-demo7riseplus/ax0th3ttz9nlh4z1qsy4j9je9ow3"",
            ""sub_bill_items"": [],
            ""bill_item_product_toppings"": [],
            ""bill_item_notes"": [],
            ""has_bill_item_notes"": false,
            ""is_set"": false,
            ""remark"": null,
            ""order_by_product_is_buffet"": 2,
            ""order_by_product_category_id"": -1,
            ""order_by_id"": 758
        },
        {
            ""id"": 758,
            ""product_category_id"": 1,
            ""product_sub_category_id"": null,
            ""product_id"": 1,
            ""product_item_id"": 1,
            ""amount"": 1,
            ""unit_price"": 59,
            ""price"": 59,
            ""note"": null,
            ""bill_item_status_master_id"": 5,
            ""is_take_home"": false,
            ""is_free"": false,
            ""created_at"": ""2024-11-15T13:43:29.754+07:00"",
            ""free_discount"": 0,
            ""free_amount"": 0,
            ""bill_item_set_id"": null,
            ""unit_price_item"": 59,
            ""price_item"": 59,
            ""barcode"": ""8857316908425"",
            ""bill_no"": ""B671115000001"",
            ""table_name"": ""B05"",
            ""product_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_is_buffet"": false,
            ""product_is_sell_by_weight"": false,
            ""product_item_name"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""product_item_code"": ""A001 เอสเปรสโซร้อน Hot Espresso"",
            ""receipt_item_id"": 284,
            ""receipt_id"": 57,
            ""invoice_item_id"": null,
            ""invoice_id"": null,
            ""product_is_has_option"": false,
            ""product_sub_category_product_id"": 1,
            ""product_is_show_in_receipt"": true,
            ""product_image_url"": ""https://storage.googleapis.com/storage-resrun-pos-com-demo7riseplus/ax0th3ttz9nlh4z1qsy4j9je9ow3"",
            ""sub_bill_items"": [],
            ""bill_item_product_toppings"": [],
            ""bill_item_notes"": [],
            ""has_bill_item_notes"": false,
            ""is_set"": false,
            ""remark"": null,
            ""order_by_product_is_buffet"": 2,
            ""order_by_product_category_id"": -1,
            ""order_by_id"": 758
        },
        {
            ""id"": 759,
            ""product_category_id"": 1,
            ""product_sub_category_id"": null,
            ""product_id"": 2,
            ""product_item_id"": 2,
            ""amount"": 1,
            ""unit_price"": 69,
            ""price"": 69,
            ""note"": null,
            ""bill_item_status_master_id"": 5,
            ""is_take_home"": false,
            ""is_free"": false,
            ""created_at"": ""2024-11-15T13:19:42.872+07:00"",
            ""free_discount"": 0,
            ""free_amount"": 0,
            ""bill_item_set_id"": null,
            ""unit_price_item"": 69,
            ""price_item"": 69,
            ""barcode"": ""8854196375208"",
            ""bill_no"": ""B671115000001"",
            ""table_name"": ""B05"",
            ""product_name"": ""A002 เอสเปรสโซร้อนดับเบิ้ลช็อต Double Hot Espresso"",
            ""product_is_buffet"": false,
            ""product_is_sell_by_weight"": false,
            ""product_item_name"": ""A002 เอสเปรสโซร้อนดับเบิ้ลช็อต Double Hot Espresso"",
            ""product_item_code"": ""A002 เอสเปรสโซร้อนดับเบิ้ลช็อต Double Hot Espresso"",
            ""receipt_item_id"": 285,
            ""receipt_id"": 57,
            ""invoice_item_id"": null,
            ""invoice_id"": null,
            ""product_is_has_option"": false,
            ""product_sub_category_product_id"": 2,
            ""product_is_show_in_receipt"": true,
            ""product_image_url"": ""https://storage.googleapis.com/storage-resrun-pos-com-demo7riseplus/n8l598x0z9cjzkfup8tu7p2td8h3"",
            ""sub_bill_items"": [],
            ""bill_item_product_toppings"": [],
            ""bill_item_notes"": [],
            ""has_bill_item_notes"": false,
            ""is_set"": false,
            ""remark"": null,
            ""order_by_product_is_buffet"": 2,
            ""order_by_product_category_id"": -1,
            ""order_by_id"": 759
        }
    ],
    ""shop"": {
    ""name_th"": ""บริษัท หมอนทอง 3695 จำกัด"",
    ""name_en"": ""DurianismCafeFutureParkRangsit"",
    ""tel"": ""0917946456"",
    ""tax_no"": ""0555567000460"",
    ""address"": ""ฟิวเจอร์พาร์ครังสิต ชั้นB โซนโรบินสัน"",
    ""province_id"": 4,
    ""amphur_id"": 65,
    ""tax_address"": ""ฟิวเจอร์พาร์ครังสิต ชั้นB โซนโรบินสัน ตำบล ประชาธิปัตย์ อำเภอธัญบุรี ปทุมธานี 12130"",
    ""branch_type_id"": 2,
    ""is_registered_pos"": false,
    ""is_mobile_print"": true,
    ""language_for_print"": ""en"",
    ""tambon_id"": 278,
    ""post_code"": ""12130"",
    ""cashier_no"": ""null"",
    ""line_id"": """",
    ""facebook_id"": """",
    ""shop_type_id"": 1,
    ""open_at"": ""2001-01-01T09:21:00.000+07:00"",
    ""close_at"": ""2001-01-01T23:59:00.000+07:00"",
    ""service_charge"": 0,
    ""vat_type_id"": 1,
    ""vat_percent"": 7,
    ""bill_footer_text"": ""ทุเรียนสดพร้อมเสิร์ฟ 365 วัน"",
    ""is_show_report_by_menu"": false,
    ""is_pay_after"": false,
    ""is_kitchen_print_by_item"": false,
    ""is_print_to_staff"": false,
    ""is_buffet_one_set_per_bill"": false,
    ""branch_code"": """",
    ""branch_name"": ""ทุเรียนนิซึ่มคาเฟ่สาขาฟิวเจอร์รังสิต"",
    ""url_printer_server"": ""https://printer.resrun-pos.internal:5463"",
    ""is_print_barcode"": false,
    ""is_update_order_status_step_by_step"": true,
    ""default_report_printer_id"": 1,
    ""queue_printer_id"": null,
    ""print_queue_loop"": 0,
    ""is_default_print_qr_code"": true,
    ""shop_default_print_qr_codes_id"": 1,
    ""decimal_type_id"": null,
    ""is_round"": false,
    ""bill_is_show_topping_by_item"": true,
    ""receipt_footer_text"": ""ทุเรียนสดพร้อมเสิร์ฟ 365 วัน"",
    ""is_show_point"": true,
    ""is_print_cancel_kitchen_bill_item"": true,
    ""is_print_price_kitchen_bill_item"": false,
    ""pos_crm_secret"": ""bd6fca3951574d222f97f4deccb3e37c73969ac6765da45c4f9c6f8620b288fd"",
    ""pos_crm_key"": ""f002e548-e42f-45fa-a0d9-9f2f511539eb"",
    ""shop_code"": ""drcrangsit"",
    ""is_confirm_order_lock"": false,
    ""is_limit_order_amount"": false,
    ""limit_order_amount"": 0,
    ""is_limit_bill_item"": false,
    ""limit_bill_item"": 0,
    ""id"": 1,
    ""client_id"": null,
    ""currency"": ""บาท"",
    ""public_ip"": null,
    ""is_show_item_for_receipt"": false,
    ""pos_crm_token"": ""eyJhbGciOiJIUzI1NiJ9.eyJzaG9wX2NvZGUiOiJkcmNyYW5nc2l0IiwiZXhwaXJlX2RhdGUiOiIyMDI0LTExLTE0IDA5OjUwOjIyICswNzAwIn0.N2226BgrCbzyQPmtVAhyOcoOKebA8f2nMHRG3NJrsBM"",
    ""is_round_product_sell_by_weight"": false,
    ""read_only"": false,
    ""editable_fields"": [],
    ""branch_type_name"": ""สาขา"",
    ""shop_default_print_name"": """",
    ""province_name"": ""Pathum Thani"",
    ""province_name_th"": ""ปทุมธานี"",
    ""amphur_name"": ""Thanyaburi"",
    ""amphur_name_th"": ""ธัญบุรี"",
    ""tambon_name"": ""Pracha Thipat"",
    ""tambon_name_th"": ""ประชาธิปัตย์"",
    ""shop_type_name"": ""ชาบู / ปิ้งย่าง"",
    ""vat_type_name"": ""ราคารวมภาษี"",
    ""decimal_type_name"": """",
    ""image_url"": ""https://storage.googleapis.com/storage-resrun-pos-com-demo7riseplus/5k9o4z0advnhsthf5fscl2veg3tg"",
    ""bill_footer_image_url"": """",
    ""receipt_footer_image_url"": """"
},
    ""receipt"":        {
            ""id"": 57,
            ""doc_no"": ""RVPOS1671115000002"",
            ""doc_date"": ""2024-11-15"",
            ""price"": 128,
            ""service_charge"": 0,
            ""discount_promotion"": 0,
            ""total_before_vat"": 119.62616822429906,
            ""document_vat_type_id"": 1,
            ""vat_percent"": 7,
            ""vat"": 8.373831775700936,
            ""total_after_vat"": 128,
            ""document_status_id"": 1,
            ""service_charge_before_vat"": 0,
            ""free_amount"": 0,
            ""service_charge_percent"": 0,
            ""decimal_discount"": 0,
            ""discount_special"": 0,
            ""discount_special_type_id"": null,
            ""discount_special_value"": 0,
            ""is_service_charge"": false,
            ""discount_total"": 0,
            ""member_name"": null,
            ""member_tel"": null,
            ""receipt_payments"":[
    {
        ""id"": 57,
        ""receipt_id"": 57,
        ""payment_type_id"": 1,
        ""bank_id"": null,
        ""card_holder_name"": null,
        ""amount"": 128,
        ""bank_account_id"": null,
        ""customer_id"": null,
        ""note"": null,
        ""change"": 0,
        ""bank_account_payment_method_id"": null,
        ""read_only"": false,
        ""editable_fields"": [],
        ""payment_type_name"": ""เงินสด"",
        ""bank_name"": """",
        ""bank_short_name"": """",
        ""bank_account_payment_method_name"": """"
    }
],
            ""receipt_items"": [
                {
                    ""id"": 284,
                    ""receipt_id"": 57,
                    ""bill_item_id"": 758,
                    ""product_id"": 1,
                    ""product_item_id"": 1,
                    ""unit_price"": 59,
                    ""amount"": 1,
                    ""price"": 59,
                    ""document_vat_type_id"": 1,
                    ""vat_percent"": 7,
                    ""vat"": 3.8598130841121496,
                    ""deduct_percent"": 0,
                    ""deduct"": 0,
                    ""total_after_vat"": 59,
                    ""promotion_name"": null,
                    ""discount_promotion"": 0,
                    ""point_promotion_name"": null,
                    ""discount_point_promotion"": 0,
                    ""total_after_discount"": 59,
                    ""is_free"": false,
                    ""total_before_vat"": 55.14018691588785,
                    ""product_group_id"": 2,
                    ""product_category_id"": 1,
                    ""product_sub_category_id"": null,
                    ""free_discount"": 0,
                    ""ingredient_id"": 4,
                    ""product_type_id"": 7,
                    ""total_before_vat_item"": 55.14018691588785,
                    ""vat_item"": 3.8598130841121496,
                    ""total_after_vat_item"": 59,
                    ""is_sub_product_item"": false,
                    ""product_item_unit_price"": 59,
                    ""receipt_item_set_id"": null,
                    ""unit_price_item"": 59,
                    ""price_item"": 59,
                    ""decimal_discount"": 0,
                    ""discount_special"": 0,
                    ""discount_special_type_id"": null,
                    ""discount_special_value"": 0,
                    ""discount_total"": 0,
                    ""free_amount"": 0,
                    ""read_only"": false,
                    ""editable_fields"": [],
                    ""receipt_item_promotions"": [],
                    ""receipt_item_point_promotions"": [],
                    ""discount_special_type_name"": """",
                    ""sub_receipt_items"": [],
                    ""is_set"": false
                },
                {
                    ""id"": 285,
                    ""receipt_id"": 57,
                    ""bill_item_id"": 759,
                    ""product_id"": 2,
                    ""product_item_id"": 2,
                    ""unit_price"": 69,
                    ""amount"": 1,
                    ""price"": 69,
                    ""document_vat_type_id"": 1,
                    ""vat_percent"": 7,
                    ""vat"": 4.03030303030303,
                    ""deduct_percent"": 0,
                    ""deduct"": 0,
                    ""total_after_vat"": 69,
                    ""promotion_name"": null,
                    ""discount_promotion"": 0,
                    ""point_promotion_name"": null,
                    ""discount_point_promotion"": 0,
                    ""total_after_discount"": 69,
                    ""is_free"": false,
                    ""total_before_vat"": 64.4079538328505,
                    ""product_group_id"": 2,
                    ""product_category_id"": 1,
                    ""product_sub_category_id"": null,
                    ""free_discount"": 0,
                    ""ingredient_id"": 4,
                    ""product_type_id"": 7,
                    ""total_before_vat_item"": 64.4079538328505,
                    ""vat_item"": 4.03030303030303,
                    ""total_after_vat_item"": 69,
                    ""is_sub_product_item"": false,
                    ""product_item_unit_price"": 69,
                    ""receipt_item_set_id"": null,
                    ""unit_price_item"": 69,
                    ""price_item"": 69,
                    ""decimal_discount"": 0,
                    ""discount_special"": 0,
                    ""discount_special_type_id"": null,
                    ""discount_special_value"": 0,
                    ""discount_total"": 0,
                    ""free_amount"": 0,
                    ""read_only"": false,
                    ""editable_fields"": [],
                    ""receipt_item_promotions"": [],
                    ""receipt_item_point_promotions"": [],
                    ""discount_special_type_name"": """",
                    ""sub_receipt_items"": [],
                    ""is_set"": false
                }
            ]
        },
    ""receipts"": [
        {
            ""id"": 57,
            ""doc_no"": ""RVPOS1671115000002"",
            ""doc_date"": ""2024-11-15"",
            ""price"": 128,
            ""service_charge"": 0,
            ""discount_promotion"": 0,
            ""total_before_vat"": 119.62616822429906,
            ""document_vat_type_id"": 1,
            ""vat_percent"": 7,
            ""vat"": 8.373831775700936,
            ""total_after_vat"": 128,
            ""document_status_id"": 1,
            ""service_charge_before_vat"": 0,
            ""free_amount"": 0,
            ""service_charge_percent"": 0,
            ""decimal_discount"": 0,
            ""discount_special"": 0,
            ""discount_special_type_id"": null,
            ""discount_special_value"": 0,
            ""is_service_charge"": false,
            ""discount_total"": 0,
            ""member_name"": null,
            ""member_tel"": null,
            ""receipt_items"": [
                {
                    ""id"": 284,
                    ""receipt_id"": 57,
                    ""bill_item_id"": 758,
                    ""product_id"": 1,
                    ""product_item_id"": 1,
                    ""unit_price"": 59,
                    ""amount"": 1,
                    ""price"": 59,
                    ""document_vat_type_id"": 1,
                    ""vat_percent"": 7,
                    ""vat"": 3.8598130841121496,
                    ""deduct_percent"": 0,
                    ""deduct"": 0,
                    ""total_after_vat"": 59,
                    ""promotion_name"": null,
                    ""discount_promotion"": 0,
                    ""point_promotion_name"": null,
                    ""discount_point_promotion"": 0,
                    ""total_after_discount"": 59,
                    ""is_free"": false,
                    ""total_before_vat"": 55.14018691588785,
                    ""product_group_id"": 2,
                    ""product_category_id"": 1,
                    ""product_sub_category_id"": null,
                    ""free_discount"": 0,
                    ""ingredient_id"": 4,
                    ""product_type_id"": 7,
                    ""total_before_vat_item"": 55.14018691588785,
                    ""vat_item"": 3.8598130841121496,
                    ""total_after_vat_item"": 59,
                    ""is_sub_product_item"": false,
                    ""product_item_unit_price"": 59,
                    ""receipt_item_set_id"": null,
                    ""unit_price_item"": 59,
                    ""price_item"": 59,
                    ""decimal_discount"": 0,
                    ""discount_special"": 0,
                    ""discount_special_type_id"": null,
                    ""discount_special_value"": 0,
                    ""discount_total"": 0,
                    ""free_amount"": 0,
                    ""read_only"": false,
                    ""editable_fields"": [],
                    ""receipt_item_promotions"": [],
                    ""receipt_item_point_promotions"": [],
                    ""discount_special_type_name"": """",
                    ""sub_receipt_items"": [],
                    ""is_set"": false
                },
                {
                    ""id"": 285,
                    ""receipt_id"": 57,
                    ""bill_item_id"": 759,
                    ""product_id"": 2,
                    ""product_item_id"": 2,
                    ""unit_price"": 69,
                    ""amount"": 1,
                    ""price"": 69,
                    ""document_vat_type_id"": 1,
                    ""vat_percent"": 7,
                    ""vat"": 4.03030303030303,
                    ""deduct_percent"": 0,
                    ""deduct"": 0,
                    ""total_after_vat"": 69,
                    ""promotion_name"": null,
                    ""discount_promotion"": 0,
                    ""point_promotion_name"": null,
                    ""discount_point_promotion"": 0,
                    ""total_after_discount"": 69,
                    ""is_free"": false,
                    ""total_before_vat"": 64.4079538328505,
                    ""product_group_id"": 2,
                    ""product_category_id"": 1,
                    ""product_sub_category_id"": null,
                    ""free_discount"": 0,
                    ""ingredient_id"": 4,
                    ""product_type_id"": 7,
                    ""total_before_vat_item"": 64.4079538328505,
                    ""vat_item"": 4.03030303030303,
                    ""total_after_vat_item"": 69,
                    ""is_sub_product_item"": false,
                    ""product_item_unit_price"": 69,
                    ""receipt_item_set_id"": null,
                    ""unit_price_item"": 69,
                    ""price_item"": 69,
                    ""decimal_discount"": 0,
                    ""discount_special"": 0,
                    ""discount_special_type_id"": null,
                    ""discount_special_value"": 0,
                    ""discount_total"": 0,
                    ""free_amount"": 0,
                    ""read_only"": false,
                    ""editable_fields"": [],
                    ""receipt_item_promotions"": [],
                    ""receipt_item_point_promotions"": [],
                    ""discount_special_type_name"": """",
                    ""sub_receipt_items"": [],
                    ""is_set"": false
                }
            ]
        }
    ]
}
";

        //Deserialize the JSON string into the BillModel object
        //var options = new JsonSerializerOptions
        //{
        //    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        //};

        //Dictionary<string, object> bill = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json, options);
        //return System.Text.Json.JsonSerializer.Deserialize<Dictionary<BillModel>>(json);
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        return System.Text.Json.JsonSerializer.Deserialize<BillModel>(json, options);
    }

    static void WriteFile(string jsonString)
    {
        string folderPath = @"C:\dotnet\PosPrintServer\PosPrintServer\bin\Debug\net8.0-windows";
        string filePath = Path.Combine(folderPath, "json_log.txt");
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


