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

public class PrintReport
{
    public static async Task<PrintReport> Create(IntPtr ptr, Report data)
    {
        var instance = new PrintReport();
        await instance.InitializePrinting(ptr, data);
        return instance;
    }

    private async Task InitializePrinting(IntPtr ptr, Report data)
    {
        await ExecutePrintReport(ptr, data);
        PM.NewLine(ptr);
        PM.CutPaper(ptr);
        PM.ClosePort(ptr);
    }

    public async Task ExecutePrintReport(IntPtr printer, Report reportBill)
    {
        try {
            bool[] s = new bool[8];
            //await Task.Run(async () =>
            //{
                PM.AlignCenter(printer);
                s[0] = await AddLogo(printer, reportBill);
                s[1] = AddShop(printer, reportBill);
                PM.TextAlignLeft(printer);
                s[2] = AddHeader(printer, reportBill);
                PM.DrawLine(printer);
                s[3] = AddProducts(printer, reportBill);
                PM.DrawLine(printer);
                s[4] = AddPromotions(printer, reportBill);
                PM.DrawLine(printer);
                s[5] = AddSales(printer, reportBill);
                PM.DrawLine(printer);
                s[6] = AddPayments(printer, reportBill);
                PM.DrawLine(printer);
                s[7] = AddBillings(printer, reportBill);
                PM.CutPaper(printer);
                PM.ClosePort(printer);
                PM.ReleasePort(printer);
                if (s.Any(x => x == false))
                {
                Console.WriteLine($"some value report is false: {string.Join(", ", s)}");
                    WriteLog.WriteFailedPrintLog(reportBill, "report");
                }
                await Task.Delay(100);
            //});
        }
        catch (Exception e) {
            PM.ClosePort(printer);
            PM.ReleasePort(printer);
            Console.WriteLine($"report error: {e}");
            WriteLog.WriteFailedPrintLog(reportBill, "report");
            //MessageBox.Show($"{e}");
        }
    }

    static async Task<bool> AddLogo(IntPtr printer, Report reportBill)
    {
        if (!string.IsNullOrEmpty(reportBill.shop.image_url)) {
            int s = await PM.PrintImageUrl(printer, reportBill.shop.image_url, "image.jpg");
            PM.NewLine(printer, 70);
            return s == 0;
        }
        return true;
    }

    static bool AddShop(IntPtr printer, Report reportBill)
    {
        //string branchName = reportBill.shop.branch;
        string language = reportBill.language ?? "th";
        PM.TextAlignLeft(printer);
        if (reportBill.shop != null)
        {
            string shopName = $"{(reportBill.language == "th" ? "ชื่อร้านอาหาร" : "Restaurant name")}: {reportBill.shop.name ?? ""}";
            int s = PM.PrintText(printer, shopName);

            string branch = $"{(reportBill.language == "th" ? "สาขา" : "Branch")}: {reportBill.shop.branch_name ?? ""}";
            int s2 = PM.PrintText(printer, branch);

            string addr = $"{(reportBill.language == "th" ? "ที่อยู่" : "Address")}: {reportBill.shop.address ?? ""}";
            int s3 = PM.PrintText(printer, addr);
            return s == 0 && s2 == 0 && s3 == 0;
        }
        PM.NewLine(printer, 50);
        return true;
    }

    static bool AddHeader(IntPtr printer, Report reportBill)
    {
        int lineIndex = 0;
        string language = reportBill.language == "th" ? "th-TH" : "en-EN";
        DateTimeHelper dt = new DateTimeHelper();

        int[] s = new int[12];

        if (reportBill.pos_round != null)
        {
            // POS and Work Shift Information
            string posInfo = $"{(reportBill.language == "th" ? "ชื่อเครื่อง" : "POS Code")} : {reportBill.pos_round.pos_code} " +
                             $"{(reportBill.language == "th" ? "รอบที่" : "Work shift")} : {reportBill.pos_round.round}";
            s[0] = PM.PrintText(printer, posInfo);

            // Title
            string title = reportBill.language == "th" ? "ใบสรุปยอดรายลิ้นชักเงินสด" : "Cash register report";
            s[1] = PM.PrintText(printer, title);

            // Starting Date and Time
            string startingDate = $"{(reportBill.language == "th" ? "วันที่เปิดลิ้นชักเงินสด" : "Starting date")} " +
                                  $"{DateTime.Parse(reportBill.pos_round?.open_date).ToString("dd MMMM yyyy", new System.Globalization.CultureInfo(language))} " +
                                  $"{(reportBill.language == "th" ? "เวลา" : "time")} {reportBill.pos_round?.open_time}";
            s[2] = PM.PrintText(printer, startingDate);

            // Ending Date and Time
            string endingDate = $"{(reportBill.language == "th" ? "วันที่ปิดลิ้นชักเงินสด" : "Ending date")} " +
                                $"{DateTime.Parse(reportBill.pos_round?.close_date).ToString("dd MMMM yyyy", new System.Globalization.CultureInfo(language))} " +
                                $"{(reportBill.language == "th" ? "เวลา" : "time")} {reportBill.pos_round?.close_time}";
            s[3] = PM.PrintText(printer, endingDate);
        }
        else
        {
            if (reportBill.date != null && !IsValidDate(reportBill.date))
            {
                int date = int.Parse(reportBill.date ?? "1");
                string docDate = $"{(reportBill.language == "th" ? "ใบสรุปยอดรายเดือน" : "Summary report of monthly sales")} : {dt.GetMonthName(date, reportBill.language)}";
                s[4] = PM.PrintText(printer, docDate);
            }
            else
            {
                string docDate = $"{(reportBill.language == "th" ? "ใบสรุปยอดรายวันที่" : "Summary report of daily sales")} : " +
                                 $"{DateTime.Parse(reportBill.date).ToString("dd MMMM yyyy", new System.Globalization.CultureInfo(language))}";
                s[5] = PM.PrintText(printer, docDate);
            }
        }

        // Helper Methods
        bool IsValidDate(string date)
        {
            return DateTime.TryParse(date, out _);
        }

        lineIndex += 2;
        string printDate = $"{(reportBill.language == "th" ? "พิมพ์เมื่อวันที่" : "Print date")} : " +
            $"{DateTime.Now.ToString("dd MMMM yyyy HH:mm", new System.Globalization.CultureInfo(language))}" +
            $"{(reportBill.language == "th" ? " น." : "")}";
        s[6] = PM.PrintText(printer, printDate);

        PM.NewLine(printer);

        // Number of Bills
        lineIndex += 4;
        string title_bill_count_total = reportBill.language == "th" ? "จำนวนบิลทั้งหมด" : "Number of bills";
        string totalBills = reportBill.bill_count_total.ToString();
        s[7] = PM.PrintTextTwoColumn(printer, title_bill_count_total, totalBills);

        // Number of Cancelled Receipts
        lineIndex += 2;
        string title_cancelledBills = reportBill.language == "th" ? "จำนวนบิลยกเลิก" : "Number of cancelled receipt";
        string cancelledBills = reportBill.bill_count_cancel.ToString();
        s[8] = PM.PrintTextTwoColumn(printer, title_cancelledBills, cancelledBills);

        // Number of Cancelled Bills (Table closed before payment)
        lineIndex += 2;
        string title_cancelledNotBills = reportBill.language == "th" ? "จำนวนบิลที่ปิดโต๊ะก่อนชำระ" : "Number of cancelled bill";
        string cancelledNotBills = reportBill.bill_count_cancel_not_bill.ToString();
        s[9] = PM.PrintTextTwoColumn(printer, title_cancelledNotBills, cancelledNotBills);

        // Number of Cancelled Items
        lineIndex += 2;
        string title_bill_item_count_cancel = reportBill.language == "th" ? "จำนวนรายการที่ยกเลิก" : "Number cancelled item";
        string cancelledItems = reportBill.bill_item_count_cancel.ToString();
        s[10] = PM.PrintTextTwoColumn(printer, title_bill_item_count_cancel, cancelledItems);

        // Number of Customers
        lineIndex += 2;
        string title_bill_count_customer = reportBill.language == "th" ? "จำนวนลูกค้า" : "Number of customers";
        string customerCount = reportBill.bill_count_customer.ToString();
        //PM.PrintTextTwoColumn(printer, title_bill_count_customer, customerCount);

        string title_normalBills = reportBill.language == "th" ? "จากจำนวนบิล" : "from Bills";
        string normalBills = reportBill.bill_count_normal.ToString();
        string combindBill = $"{customerCount} {title_normalBills}      {normalBills}";
        s[11] = PM.PrintTextTwoColumn(printer, title_bill_count_customer, combindBill);
        return s.All(x => x == 0);
    }

    static bool AddPromotions(IntPtr printer, Report reportBill)
    {
        string discountTitle = reportBill.language == "th" ? "ส่วนลด" : "Sales discount";
        string specialTitle = reportBill.language == "th" ? "ส่วนลดพิเศษ" : "Additional sales discount";
        string freeTitle = reportBill.language == "th" ? "ฟรี" : "Discount of free items";
        string decimalDiscountTitle = reportBill.language == "th" ? "ส่วนลดปัดเศษสตางค์" : "Discount of Round down to decimal";
        string totalDiscountTitle = reportBill.language == "th" ? "รวมส่วนลด" : "Total sales discount";
        string serviceTitle = reportBill.language == "th" ? $"ค่าบริการ {reportBill.shop.service_charge} %" : $"Service Charge {reportBill.shop.service_charge} %";
        int[] s = new int[7];

        s[0] = PM.PrintTextBold(printer, discountTitle);
        double? discountSum = 0;
        foreach (var promotion in reportBill.promotions)
        {
            discountSum += Convert.ToDouble(promotion.discount != null ? promotion.discount : 0);
            s[1] = PM.PrintTextTwoColumn(printer, promotion.promotion_name, PadText(promotion.cnt ?? "",promotion.discount ?? ""));
        }
        if (reportBill.point_promotions != null)
        {
            foreach (var point_promotion in reportBill.point_promotions)
            {
                discountSum += Convert.ToDouble(point_promotion.discount != null ? point_promotion.discount : 0);
                s[2] = PM.PrintTextTwoColumn(printer, point_promotion.point_promotion_name, PadText(point_promotion.cnt ?? "", CurrencyFormat($"{point_promotion.discount}")));
            }
        }
        if (reportBill.discount_special != null && reportBill.discount_special != 0)
        {
            discountSum += reportBill.discount_special;
            s[3] = PM.PrintTextTwoColumn(printer, specialTitle, PadText($"{reportBill.discount_special_amount}", CurrencyFormat($"{reportBill.discount_special}")));
        }
        if (reportBill.bill_item_free_discount != null && reportBill.bill_item_free_discount != 0)
        {
            double? freeAMount = reportBill.bill_item_free_amount;
            discountSum += reportBill.bill_item_free_discount;
            s[4] = PM.PrintTextTwoColumn(printer, freeTitle, PadText(freeAMount != null ? $"{freeAMount}" : "", CurrencyFormat($"{reportBill.bill_item_free_discount}")));
        }
        if (reportBill.decimal_discount != null && reportBill.decimal_discount != 0)
        {
            discountSum += reportBill.decimal_discount;
            s[5] = PM.PrintTextTwoColumn(printer, decimalDiscountTitle, CurrencyFormat($"{reportBill.decimal_discount}"));
        }
        PM.PrintTextTwoColumn(printer, totalDiscountTitle, CurrencyFormat($"{discountSum}"));
        if (reportBill.receipt_service_charge != null && reportBill.receipt_service_charge != 0)
        {
            s[6] = PM.PrintTextTwoColumn(printer, serviceTitle, CurrencyFormat($"{reportBill.receipt_service_charge}"));
        }
        return s.All(x => x == 0);
    }

    static bool AddSales(IntPtr printer, Report reportBill)
    {
        string noVatTitle = reportBill.language == "th" ? "ยอดขายไม่รวมภาษีมูลค่าเพิ่ม" : "Pre-vat Amount";
        string vatTitle = reportBill.language == "th" ? "ภาษีขาย" : "VAT";
        string grandTotalTitle = reportBill.language == "th" ? "ยอดขายสุทธิ" : "Grand Total";
        int s = PM.PrintTextTwoColumn(printer, noVatTitle, CurrencyFormat(reportBill.receipt_total_before_vat?.ToString()));
        int s2 = PM.PrintTextTwoColumn(printer, vatTitle, CurrencyFormat(reportBill.receipt_vat?.ToString()));
        int s3 = PM.PrintTextTwoColumn(printer, grandTotalTitle, CurrencyFormat(reportBill.receipt_total_after_vat?.ToString()));
        return s == 0 && s2 == 0 && s3 == 0;
    }

    static bool AddPayments(IntPtr printer, Report reportBill)
    {
        string paymentMethodTitle = reportBill.language == "th" ? "ช่องทางการชำระเงิน" : "Payment methods";
        string cashTitle = reportBill.language == "th" ? "เงินสด" : "Cash";
        string depositTitle = reportBill.language == "th" ? "เงินมัดจำ" : "Deposit";
        int[] s = new int[9];

        s[0] = PM.PrintTextBold(printer, paymentMethodTitle);
        s[1] = PM.PrintTextTwoColumn(printer, cashTitle, CurrencyFormat($"{reportBill.receipt_payment_cash}"));
        s[2] = PM.PrintTextTwoColumn(printer, depositTitle, CurrencyFormat($"{reportBill.receipt_payment_deposit}"));

        double discountSum = 0;
        foreach (var qr_payment in reportBill.receipt_payment_qr_code)
        {
            int? payCountNum = 0;
            decimal? sumAmount = 0;

            foreach (var method in qr_payment.bank_account_payment_methods)
            {
                payCountNum += method.payment_count;
                sumAmount += method.amount;
            }

            string payCount = $"{payCountNum} {(reportBill.language == "th" ? "ครั้ง" : "times")}";
            string transferTitle = $"{(reportBill.language == "th" ? "เงินโอน / QR Code" : "Bank transfer")} ({qr_payment.bank_short_name} {payCount})";
            s[3] = PM.PrintTextTwoColumn(printer, transferTitle, CurrencyFormat($"{sumAmount}"));

            if (qr_payment.bank_account_payment_methods.Any(m => m.bank_account_payment_method_name != null))
            {
                foreach (var method in qr_payment.bank_account_payment_methods)
                {
                    string methodTitle = $"{method.bank_account_payment_method_name ?? (reportBill.language == "th" ? "ไม่ระบุ" : "Not specified")} ({method.payment_count} {(reportBill.language == "th" ? "ครั้ง" : "times")})";
                    s[4] = PM.PrintTextTwoColumn(printer, methodTitle, CurrencyFormat($"{method.amount}"));
                }
            }
        }

        foreach (var credit_card in reportBill.receipt_payment_credit_card)
        {
            string payCount = $"{credit_card.payment_count} {(reportBill.language == "th" ? "ครั้ง" : "times")}";
            string creditTitle = $"{(reportBill.language == "th" ? "บัตรเครดิต" : "Credit/Debit card Bank")} ({credit_card.bank_short_name} {payCount})";
            s[5] = PM.PrintTextTwoColumn(printer, creditTitle, CurrencyFormat($"{credit_card.amount}"));
        }

        string invoiceTitle = reportBill.language == "th" ? "เงินเชื่อ" : "On credit";
        s[6] = PM.PrintTextTwoColumn(printer, invoiceTitle, CurrencyFormat($"{reportBill.invoice_total_after_vat}"));
        PM.DrawLine(printer);

        string tipsTitle = reportBill.language == "th" ? "ทิป" : "Tips";
        s[7] = PM.PrintTextTwoColumn(printer, tipsTitle, CurrencyFormat($"{reportBill.pos_round_close_tip_amount}"));
        PM.DrawLine(printer);

        string netTitle = reportBill.language == "th" ? "ยอดขายสุทธิรวมทิป" : "Net Total";
        s[8] = PM.PrintTextTwoColumn(printer, netTitle, CurrencyFormat($"{reportBill.invoice_total_after_vat + reportBill.pos_round_close_tip_amount}"));
        return s.All(x => x == 0);
    }

    static bool AddBillings(IntPtr printer, Report reportBill)
    {
        string billingTitle = reportBill.language == "th" ? "รายได้ ตามประเภทบิล" : "Sales report by bill type";
        string atShopTitle = reportBill.language == "th" ? "ทานร้าน" : "Eat in";
        string takeHomeTitle = reportBill.language == "th" ? "กลับบ้าน" : "Take home";
        int[] s = new int[15];

        s[0] = PM.PrintTextBold(printer, billingTitle);
        s[1] = PM.PrintTextTwoColumn(printer, atShopTitle, CurrencyFormat($"{reportBill.receipt_restuarant}"));
        s[2] = PM.PrintTextTwoColumn(printer, takeHomeTitle, CurrencyFormat($"{reportBill.receipt_take_home}"));
        PM.DrawLine(printer);
        if (reportBill.deliveries != null && reportBill.deliveries.Count > 0)
        {
            foreach (var delivery in reportBill.deliveries)
            {
                if (delivery != null && delivery.delivery_name != null) {
                    s[3] = PM.PrintTextTwoColumn(printer, delivery.delivery_name, CurrencyFormat($"{delivery.total}"));
                }
            }
            PM.DrawLine(printer);
        }
        if (reportBill.pos_round != null)
        {
            string initialTitle = reportBill.language == "th" ? "วงเงินสดเริ่มต้น" : "Begining balance of cash";
            s[4] = PM.PrintTextTwoColumn(printer, initialTitle, CurrencyFormat($"{reportBill.pos_round.initial_amount}"));
            string incomeTitle = reportBill.language == "th" ? "เงินเข้า" : "Cash In";
            string outcomeTitle = reportBill.language == "th" ? "เงินออก" : "Cash Out";
            string registerCashTitle = reportBill.language == "th" ? "จำนวนเงินตามระบบ" : "Amount of cash receipt by register";
            string onHandCashTitle = reportBill.language == "th" ? "จำนวนเงินที่นับได้จากลิ้นชัก" : "Amount of cash on hand";
            string overShortTitle = reportBill.language == "th" ? "เงินเกิน / -เงินขาด" : "Over/Short of cash";
            string depositCashTitle = reportBill.language == "th" ? "จำนวนเงิน นำฝากเงินเข้าธนาคาร" : "Amount of money deposited into the bank";
            s[5] = PM.PrintText(printer, incomeTitle);
            if (reportBill.pos_round.in_pos_cash_movements != null && reportBill.pos_round.in_pos_cash_movements.Count > 0)
            {
                foreach (var cash_movement in reportBill.pos_round.in_pos_cash_movements)
                {
                    string movementDetail = $"{cash_movement.created_at} {cash_movement.description}";
                    s[6] = PM.PrintTextTwoColumn(printer, movementDetail, CurrencyFormat($"{cash_movement.amount}"));
                }
            }
            s[7] = PM.PrintText(printer, outcomeTitle);
            if (reportBill.pos_round.out_pos_cash_movements != null && reportBill.pos_round.out_pos_cash_movements.Count > 0)
            {
                foreach (var cash_movement in reportBill.pos_round.out_pos_cash_movements)
                {
                    string movementDetail = $"{cash_movement.created_at} {cash_movement.description}";
                    s[8] = PM.PrintTextTwoColumn(printer, movementDetail, CurrencyFormat($"{cash_movement.amount}"));
                }
            }
            s[9] = PM.PrintTextTwoColumn(printer, registerCashTitle, CurrencyFormat($"{reportBill.pos_round.total_amount}"));
            s[10] = PM.PrintTextTwoColumn(printer, onHandCashTitle, CurrencyFormat($"{reportBill.pos_round.close_cash_amount}"));
            s[11] = PM.PrintTextTwoColumn(printer, overShortTitle, CurrencyFormat($"{reportBill.pos_round.diff_amount}"));
            PM.DrawLine(printer);
            s[12] = PM.PrintTextTwoColumn(printer, depositCashTitle, CurrencyFormat($"{reportBill.pos_round.bank_deposit_slip}"));
            PM.DrawLine(printer);
        }
        else
        {
            string estimatedTitle = reportBill.language == "th" ? "รายได้ประมาณการจากบิลที่ยังไม่ชำระเงิน" : "Estimated sales of unpaid bills";
            string unpaidTitle = reportBill.language == "th" ? "บิลที่ยังไม่ชำระเงิน" : "Number of unpaid bills";
            s[13] = PM.PrintTextTwoColumn(printer, estimatedTitle, CurrencyFormat($"{reportBill.est_total_after_vat}"));
            s[14] = PM.PrintTextTwoColumn(printer, unpaidTitle, CurrencyFormat($"{reportBill.count_bill_status89}"));
            PM.DrawLine(printer);
        }
        return s.All(x => x == 0);
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

    public class ProductItem
    {
        public double Amount { get; set; }
        public double Price { get; set; }
        public string TypeName { get; set; }
        public int NO { get; set; }
    }

    static bool AddProducts(IntPtr printer, Report reportBill)
    {
        double totalPrice = 0.0;
        int[] s = new int[3];

        if (reportBill.product_types != null) // Correctly access product_types from reportBill
        {
            var items = new Dictionary<string, ProductItem>(); // Use ProductItem for mutable values
            var types = new Dictionary<string, int>();
            double sumAmount = 0.0;
            double sumPrice = 0.0;
            string prevType = null;

            foreach (var type in reportBill.product_types)
            {
                // Ensure is_show_report_by_menu is a non-nullable bool
                bool isShowReportByMenu = reportBill.shop?.is_show_report_by_menu ?? false;

                string key = isShowReportByMenu
                    ? type.product_name
                    : type.product_type_name;

                double amount = Convert.ToDouble(type.amount);
                double price = type.price.ValueKind == JsonValueKind.String
                                ? double.Parse(type.price.GetString())
    :                           type.price.GetDouble();

                totalPrice += price;

                if (prevType == null)
                    prevType = type.product_type_name;

                if (!types.ContainsKey(type.product_type_name))
                {
                    types[type.product_type_name] = 0;

                    if (isShowReportByMenu && sumAmount > 0)
                    {
                        string totalKey = (reportBill.language == "th" ? "รวม" : "Total") + prevType;
                        items[totalKey] = new ProductItem
                        {
                            Amount = sumAmount,
                            Price = sumPrice
                        };

                        sumAmount = 0.0;
                        sumPrice = 0.0;
                    }

                    prevType = type.product_type_name;
                }

                types[type.product_type_name]++;
                sumAmount += amount;
                sumPrice += price;

                if (items.ContainsKey(key))
                {
                    items[key].Amount += amount; // Mutable now
                    items[key].Price += price;   // Mutable now
                }
                else
                {
                    items[key] = new ProductItem
                    {
                        Amount = amount,
                        Price = price,
                        TypeName = type.product_type_name.ToString(),
                        NO = types[type.product_type_name]
                    };
                }
            }

            bool isShowReportByMenuFinal = reportBill.shop?.is_show_report_by_menu ?? false;

            if (isShowReportByMenuFinal && sumAmount > 0)
            {
                string totalKey = (reportBill.language == "th" ? "รวม" : "Total") + prevType;
                items[totalKey] = new ProductItem
                {
                    Amount = sumAmount,
                    Price = sumPrice
                };
            }

            foreach (var item in items)
            {
                var itemValue = item.Value;

                if (itemValue.NO == 1 && (reportBill.shop.is_show_report_by_menu ?? false))
                {
                    s[0] = PM.PrintTextBold(printer, itemValue.TypeName);
                }

                string priceText = CurrencyFormat(itemValue.Price.ToString());
                string amountText = itemValue.Amount.ToString();

                string name = (reportBill.shop.is_show_report_by_menu ?? false) && itemValue.NO != 0
                    ? $"{itemValue.NO}. {item.Key}"
                    : item.Key;

                s[1] = PM.PrintTextTwoColumn(printer, name, PadText(amountText, priceText));
            }
            PM.DrawLine(printer);
        }
        
        s[2] = PM.PrintTextTwoColumn(printer, reportBill.language == "th" ? "ยอดขายรวม" : "Total Amount", CurrencyFormat(totalPrice.ToString()));
        return s.All(x => x == 0);
    }

    public static string PadText(string amountText, string priceText)
    {
        int totalLength = 11;
        int priceLength = priceText.Length;

        if (priceLength >= totalLength)
        {
            return $"{amountText}  {priceText}";
        }

        string paddedPrice = priceText.PadLeft(totalLength);
        return $"{amountText}{paddedPrice}";
    }
}

