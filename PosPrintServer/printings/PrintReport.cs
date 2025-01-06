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
    //public PrintReport()
    //{
    //    IntPtr ptr = ESCPOS.InitPrinter("");
    //    Report reportBill = GenerateMockBillData();
    //    ExecutePrintReport(ptr, reportBill, "xxxx");
    //    PM.NewLine(ptr);
    //    PM.CutPaper(ptr);
        //PM.ClosePort(ptr);
    //}

    public static async Task<PrintReport> Create(IntPtr ptr, Report data)
    {
        var instance = new PrintReport();
        await instance.InitializePrinting(ptr, data);
        return instance;
    }

    private async Task InitializePrinting(IntPtr ptr, Report data)
    {
        //string jsonString = JsonSerializer.Serialize(data);
        //Report model = JsonSerializer.Deserialize<Report>(jsonString);
        //await Print(ptr, model);
        ExecutePrintReport(ptr, data);
        PM.NewLine(ptr);
        PM.CutPaper(ptr);
        PM.ClosePort(ptr);
    }

    public async void ExecutePrintReport(IntPtr printer, Report reportBill)
    {
        await Task.Run(async () =>
        {
            PM.AlignCenter(printer);
            await AddLogo(printer, reportBill);
            AddShop(printer, reportBill);
            PM.TextAlignLeft(printer);
            AddHeader(printer, reportBill);
            PM.DrawLine(printer);
            AddProducts(printer, reportBill);
            PM.DrawLine(printer);
            AddPromotions(printer, reportBill);
            PM.DrawLine(printer);
            AddSales(printer, reportBill);
            PM.DrawLine(printer);
            AddPayments(printer, reportBill);
            PM.DrawLine(printer);
            AddBillings(printer, reportBill);
            PM.CutPaper(printer);
            PM.ClosePort(printer);
            Task.Delay(300);
        });
    }

    static async Task AddLogo(IntPtr printer, Report reportBill)
    {
        await PM.PrintImageUrl(printer, reportBill.shop.image_url, "image.jpg");
        PM.NewLine(printer, 70);
    }

    static void AddShop(IntPtr printer, Report reportBill)
    {
        string branchName = reportBill.shop.branch;
        string language = reportBill.language ?? "th";
        PM.TextAlignLeft(printer);
        if (reportBill.shop != null)
        {
            //if (reportBill.shop.branch_type_id == 2)
            //{
            //    branchName = $"{(!string.IsNullOrEmpty(reportBill.shop.branch) ? (language == "th" ? "สาขาที่ " : "Branch No. ") + reportBill.shop.branch : "")}" +
            //                 $"{(reportBill.shop.branch != null && !string.IsNullOrEmpty(reportBill.shop.branch) ? " " : "")}" +
            //                 $"{(!string.IsNullOrEmpty(reportBill.shop.branch) ? (language == "th" ? "สาขา " : "Branch ") + reportBill.shop.branch : "")} ";
            //}
            //else if (reportBill.shop.branch_type_id == 3)
            //{
            //    branchName = string.Empty;
            //}

            string shopName = $"{(reportBill.language == "th" ? "ชื่อร้านอาหาร" : "Shop Name")}: {reportBill.shop.name ?? ""}";
            PM.PrintText(printer, shopName);

            string branch = $"{(reportBill.language == "th" ? "สาขา" : "Branch")}: {reportBill.shop.branch ?? ""}";
            PM.PrintText(printer, branch);

            string addr = $"{(reportBill.language == "th" ? "ที่อยู่" : "Address")}: {reportBill.shop.address ?? ""}";
            PM.PrintText(printer, addr);
        }

        //if (bill.receipt != null && bill.receipt.document_vat_type_id.HasValue && bill.receipt.document_vat_type_id != 3)
        //{
        //    string taxNo = $"{(language == "th" ? "เลขประจำตัวผู้เสียภาษี" : "TAX ID")} {bill.shop.tax_no} ";
        //    PM.PrintText(printer, FormatText(taxNo));
        //}
        PM.NewLine(printer, 50);
        //string tel = $"{(language == "th" ? "เบอร์โทร" : "Tel")} {bill.shop.tel ?? ""} ";
        //PM.PrintText(printer, FormatText(tel));
        //PM.NewLine(printer, 70);
    }

    static void AddHeader(IntPtr printer, Report reportBill)
    {
        int lineIndex = 0;
        int padding = 10; // Example padding
        int lineHeight = 20; // Example line height
        int width = 500; // Example canvas width
        string language = reportBill.language == "th" ? "th-TH" : "en-EN";

        if (reportBill.pos_round != null)
        {
            // POS and Work Shift Information
            string posInfo = $"{(reportBill.language == "th" ? "ชื่อเครื่อง" : "POS Code")} : {reportBill.pos_round.pos_code} " +
                             $"{(reportBill.language == "th" ? "รอบที่" : "Work shift")} : {reportBill.pos_round.round}";
            PM.PrintText(printer, posInfo);

            // Title
            string title = reportBill.language == "th" ? "ใบสรุปยอดรายลิ้นชักเงินสด" : "Cash register report";
            PM.PrintText(printer, title);

            // Starting Date and Time
            string startingDate = $"{(reportBill.language == "th" ? "วันที่เปิดลิ้นชักเงินสด" : "Starting date")} : " +
                                  $"{DateTime.Parse(reportBill.pos_round.open_date).ToString("dd MMMM yyyy", new System.Globalization.CultureInfo(language))} " +
                                  $"{(reportBill.language == "th" ? "เวลา" : "time")} {reportBill.pos_round.open_time}";
            PM.PrintText(printer, startingDate);

            // Ending Date and Time
            string endingDate = $"{(reportBill.language == "th" ? "วันที่ปิดลิ้นชักเงินสด" : "Ending date")} : " +
                                $"{DateTime.Parse(reportBill.pos_round.close_date).ToString("dd MMMM yyyy", new System.Globalization.CultureInfo(language))} " +
                                $"{(reportBill.language == "th" ? "เวลา" : "time")} {reportBill.pos_round.close_time}";
            PM.PrintText(printer, endingDate);
        }
        else
        {
            if (reportBill.date != null && !IsValidDate(reportBill.date))
            //if (reportBill.summary_date != null)
            {
                // Monthly Summary Report
                MessageBox.Show("Monthly Summary Report");
                string docDate = $"{(reportBill.language == "th" ? "ใบสรุปยอดรายเดือน" : "Summary report of monthly sales")} : {GetMonthName(reportBill.date, language)}";
                PM.PrintText(printer, docDate);
            }
            else
            {
                // Daily Summary Report
                MessageBox.Show("Daily Summary Report");
                string docDate = $"{(reportBill.language == "th" ? "ใบสรุปยอดรายวันที่" : "Summary report of daily sales")} : " +
                                 $"{DateTime.Parse(reportBill.date).ToString("dd MMMM yyyy", new System.Globalization.CultureInfo(language))}";
                PM.PrintText(printer, docDate);
            }
        }

        // Helper Methods
        bool IsValidDate(string date)
        {
            return DateTime.TryParse(date, out _);
        }

        string GetMonthName(string date, string language)
        {
            DateTime parsedDate;
            if (DateTime.TryParse(date, out parsedDate))
            {
                return parsedDate.ToString("MMMM yyyy", new System.Globalization.CultureInfo(language));
            }
            return "Invalid Month";
        }

        //DateTime summaryDate;
        //if (DateTime.TryParse(reportBill.summary_date, out summaryDate))
        //{
        //    string report_name = $"{(reportBill.language == "th" ? "สรุปยอดรายได้วันที่" : "Report Summary Date")} {summaryDate.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo(reportBill.language == "th" ? "th-TH" : "en-US"))}";
        //    PM.PrintText(printer, report_name);
        //}
        //else
        //{
        //    // กรณีที่ไม่สามารถแปลง string เป็น DateTime ได้
        //    string report_name = "Invalid date format";
        //}
        // Print Date
        lineIndex += 2;
        string printDate = $"{(reportBill.language == "th" ? "พิมพ์เมื่อวันที่" : "Print date")} : " +
            $"{DateTime.Now.ToString("dd MMMM yyyy HH:mm", new System.Globalization.CultureInfo(language))}" +
            $"{(reportBill.language == "th" ? " น." : "")}";
        PM.PrintText(printer, printDate);

        PM.NewLine(printer);

        // Number of Bills
        lineIndex += 4;
        string title_bill_count_total = reportBill.language == "th" ? "จำนวนบิลทั้งหมด" : "Number of bills";
        string totalBills = reportBill.bill_count_total.ToString();
        PM.PrintTextTwoColumn(printer, title_bill_count_total, totalBills);

        // Number of Cancelled Receipts
        lineIndex += 2;
        string title_cancelledBills = reportBill.language == "th" ? "จำนวนบิลยกเลิก" : "Number of cancelled receipt";
        string cancelledBills = reportBill.bill_count_cancel.ToString();
        PM.PrintTextTwoColumn(printer, title_cancelledBills, cancelledBills);

        // Number of Cancelled Bills (Table closed before payment)
        lineIndex += 2;
        string title_cancelledNotBills = reportBill.language == "th" ? "จำนวนบิลที่ปิดโต๊ะก่อนชำระ" : "Number of cancelled bill";
        string cancelledNotBills = reportBill.bill_count_cancel_not_bill.ToString();
        PM.PrintTextTwoColumn(printer, title_cancelledNotBills, cancelledNotBills);

        // Number of Cancelled Items
        lineIndex += 2;
        string title_bill_item_count_cancel = reportBill.language == "th" ? "จำนวนรายการที่ยกเลิก" : "Number cancelled item";
        string cancelledItems = reportBill.bill_item_count_cancel.ToString();
        PM.PrintTextTwoColumn(printer, title_bill_item_count_cancel, cancelledItems);

        // Number of Customers
        lineIndex += 2;
        string title_bill_count_customer = reportBill.language == "th" ? "จำนวนลูกค้า" : "Number of customers";
        string customerCount = reportBill.bill_count_customer.ToString();
        PM.PrintTextTwoColumn(printer, title_bill_count_customer, customerCount);

        string title_normalBills = reportBill.language == "th" ? "จากจำนวนบิล" : "from Bills";
        string normalBills = reportBill.bill_count_normal.ToString();
        PM.PrintTextTwoColumn(printer, title_normalBills, normalBills);

        PM.DrawLine(printer);
    }

    static void AddPromotions(IntPtr printer, Report reportBill)
    {
        string discountTitle = reportBill.language == "th" ? "ส่วนลด" : "Sales discount";
        string specialTitle = reportBill.language == "th" ? "ส่วนลดพิเศษ" : "Additional sales discount";
        string freeTitle = reportBill.language == "th" ? "ฟรี" : "Discount of free items";
        string decimalDiscountTitle = reportBill.language == "th" ? "ส่วนลดปัดเศษสตางค์" : "Discount of Round down to decimal";
        string totalDiscountTitle = reportBill.language == "th" ? "รวมส่วนลด" : "Total sales discount";
        string serviceTitle = reportBill.language == "th" ? "ค่าบริการ" : "Service Charge";
        PM.PrintTextBold(printer, discountTitle);
        double? discountSum = 0;
        foreach (var promotion in reportBill.promotions)
        {
            discountSum += promotion.discount;
            PM.PrintTextTwoColumn(printer, promotion.promotion_name, $"{promotion.cnt}     {CurrencyFormat($"{promotion.discount}")}");
        }
        if (reportBill.point_promotions != null)
        {
            foreach (var point_promotion in reportBill.point_promotions)
            {
                discountSum += point_promotion.discount;
                PM.PrintTextTwoColumn(printer, point_promotion.point_promotion_name, $"{point_promotion.cnt}     {CurrencyFormat($"{point_promotion.discount}")}");
            }
        }
        if (reportBill.discount_special != null)
        {
            discountSum += reportBill.discount_special;
            PM.PrintTextTwoColumn(printer, specialTitle, $"{reportBill.discount_special_amount}     {CurrencyFormat($"{reportBill.discount_special}")}");
        }
        if (reportBill.bill_item_free_discount != null)
        {
            discountSum += reportBill.bill_item_free_discount;
            PM.PrintTextTwoColumn(printer, freeTitle, CurrencyFormat($"{reportBill.bill_item_free_discount}"));
        }
        if (reportBill.decimal_discount != null)
        {
            discountSum += reportBill.decimal_discount;
            PM.PrintTextTwoColumn(printer, decimalDiscountTitle, CurrencyFormat($"{reportBill.decimal_discount}"));
        }
        PM.PrintTextTwoColumn(printer, totalDiscountTitle, CurrencyFormat($"{discountSum}"));
        if (reportBill.receipt_service_charge != null)
        {
            PM.PrintTextTwoColumn(printer, serviceTitle, CurrencyFormat($"{reportBill.receipt_service_charge}"));
        }
    }

    static void AddSales(IntPtr printer, Report reportBill)
    {
        string noVatTitle = reportBill.language == "th" ? "ยอดขายไม่รวมภาษีมูลค่าเพิ่ม" : "Pre-vat Amount";
        string vatTitle = reportBill.language == "th" ? "ภาษีขาย" : "VAT";
        string grandTotalTitle = reportBill.language == "th" ? "ยอดขายสุทธิ" : "Grand Total";
        PM.PrintTextTwoColumn(printer, noVatTitle, CurrencyFormat(reportBill.receipt_total_before_vat?.ToString()));
        PM.PrintTextTwoColumn(printer, vatTitle, CurrencyFormat(reportBill.receipt_vat?.ToString()));
        PM.PrintTextTwoColumn(printer, grandTotalTitle, CurrencyFormat(reportBill.receipt_total_after_vat?.ToString()));
    }

    static void AddPayments(IntPtr printer, Report reportBill)
    {
        string paymentMethodTitle = reportBill.language == "th" ? "ช่องทางการชำระเงิน" : "Payment methods";
        string cashTitle = reportBill.language == "th" ? "เงินสด" : "Cash";
        string depositTitle = reportBill.language == "th" ? "เงินมัดจำ" : "Deposit";

        PM.PrintTextBold(printer, paymentMethodTitle);
        PM.PrintTextTwoColumn(printer, cashTitle, CurrencyFormat($"{reportBill.receipt_payment_cash}"));
        PM.PrintTextTwoColumn(printer, depositTitle, CurrencyFormat($"{reportBill.receipt_payment_deposit}"));

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
            string transferTitle = $"{(reportBill.language == "th" ? "เงินโอน / QR Code" : "Bank transfer")} {qr_payment.bank_short_name} {payCount}";
            PM.PrintTextTwoColumn(printer, transferTitle, CurrencyFormat($"{sumAmount}"));

            if (qr_payment.bank_account_payment_methods.Any(m => m.bank_account_payment_method_name != null))
            {
                foreach (var method in qr_payment.bank_account_payment_methods)
                {
                    string methodTitle = $"{method.bank_account_payment_method_name ?? (reportBill.language == "th" ? "ไม่ระบุ" : "Not specified")} ({method.payment_count} {(reportBill.language == "th" ? "ครั้ง" : "times")})";
                    PM.PrintTextTwoColumn(printer, methodTitle, CurrencyFormat($"{method.amount}"));
                }
            }
        }

        foreach (var credit_card in reportBill.receipt_payment_credit_card)
        {
            string payCount = $"{credit_card.payment_count} {(reportBill.language == "th" ? "ครั้ง" : "times")}";
            string creditTitle = $"{(reportBill.language == "th" ? "บัตรเครดิต" : "Credit/Debit card Bank")} {credit_card.bank_short_name} {payCount}";
            PM.PrintTextTwoColumn(printer, creditTitle, CurrencyFormat($"{credit_card.amount}"));
        }

        string invoiceTitle = reportBill.language == "th" ? "เงินเชื่อ" : "On credit";
        PM.PrintTextTwoColumn(printer, invoiceTitle, CurrencyFormat($"{reportBill.invoice_total_after_vat}"));
        PM.DrawLine(printer);

        string tipsTitle = reportBill.language == "th" ? "ทิป" : "Tips";
        PM.PrintTextTwoColumn(printer, tipsTitle, CurrencyFormat($"{reportBill.pos_round_close_tip_amount}"));
        PM.DrawLine(printer);

        string netTitle = reportBill.language == "th" ? "ยอดขายสุทธิรวมทิป" : "Net Total";
        PM.PrintTextTwoColumn(printer, netTitle, CurrencyFormat($"{reportBill.invoice_total_after_vat + reportBill.pos_round_close_tip_amount}"));
    }

    static void AddBillings(IntPtr printer, Report reportBill)
    {
        string billingTitle = reportBill.language == "th" ? "รายได้ ตามประเภทบิล" : "Sales report by bill type";
        string atShopTitle = reportBill.language == "th" ? "ทานร้าน" : "Eat in";
        string takeHomeTitle = reportBill.language == "th" ? "กลับบ้าน" : "Take home";
        PM.PrintTextBold(printer, billingTitle);
        PM.PrintTextTwoColumn(printer, atShopTitle, CurrencyFormat($"{reportBill.receipt_restuarant}"));
        PM.PrintTextTwoColumn(printer, takeHomeTitle, CurrencyFormat($"{reportBill.receipt_take_home}"));
        PM.DrawLine(printer);
        if (reportBill.deliveries != null && reportBill.deliveries.Count > 0)
        {
            foreach (var delivery in reportBill.deliveries)
            {
                PM.PrintTextTwoColumn(printer, delivery.name, CurrencyFormat($"{delivery.price}"));
            }
            PM.DrawLine(printer);
        }
        if (reportBill.pos_round != null) 
        {
            string initialTitle = reportBill.language == "th" ? "วงเงินสดเริ่มต้น" : "Begining balance of cash";
            PM.PrintTextTwoColumn(printer, initialTitle, CurrencyFormat($"{reportBill.pos_round.initial_amount}"));
            string incomeTitle = reportBill.language == "th" ? "เงินเข้า" : "Cash In";
            string outcomeTitle = reportBill.language == "th" ? "เงินออก" : "Cash Out";
            string registerCashTitle = reportBill.language == "th" ? "จำนวนเงินตามระบบ" : "Amount of cash receipt by register";
            string onHandCashTitle = reportBill.language == "th" ? "จำนวนเงินที่นับได้จากลิ้นชัก" : "Amount of cash on hand";
            string overShortTitle = reportBill.language == "th" ? "เงินเกิน / -เงินขาด" : "Over/Short of cash";
            string depositCashTitle = reportBill.language == "th" ? "จำนวนเงิน นำฝากเงินเข้าธนาคาร" : "Amount of money deposited into the bank";
            PM.PrintText(printer, incomeTitle);
            if (reportBill.pos_round.in_pos_cash_movements != null && reportBill.pos_round.in_pos_cash_movements.Count > 0)
            {
                foreach (var cash_movement in reportBill.pos_round.in_pos_cash_movements)
                {
                    string movementDetail = $"{cash_movement.created_at} {cash_movement.description}";
                    PM.PrintTextTwoColumn(printer, movementDetail, CurrencyFormat($"{cash_movement.amount}"));
                }
            }
            PM.PrintText(printer, outcomeTitle);
            if (reportBill.pos_round.out_pos_cash_movements != null && reportBill.pos_round.out_pos_cash_movements.Count > 0)
            {
                foreach (var cash_movement in reportBill.pos_round.out_pos_cash_movements)
                {
                    string movementDetail = $"{cash_movement.created_at} {cash_movement.description}";
                    PM.PrintTextTwoColumn(printer, movementDetail, CurrencyFormat($"{cash_movement.amount}"));
                }
            }
            PM.PrintTextTwoColumn(printer, registerCashTitle, CurrencyFormat($"{reportBill.pos_round.total_amount}"));
            PM.PrintTextTwoColumn(printer, onHandCashTitle, CurrencyFormat($"{reportBill.pos_round.close_cash_amount}"));
            PM.PrintTextTwoColumn(printer, overShortTitle, CurrencyFormat($"{reportBill.pos_round.diff_amount}"));
            PM.DrawLine(printer);
            PM.PrintTextTwoColumn(printer, depositCashTitle, CurrencyFormat($"{reportBill.pos_round.bank_deposit_slip}"));
            PM.DrawLine(printer);
        }
        else
        {
            string estimatedTitle = reportBill.language == "th" ? "รายได้ประมาณการจากบิลที่ยังไม่ชำระเงิน" : "Estimated sales of unpaid bills";
            string unpaidTitle = reportBill.language == "th" ? "บิลที่ยังไม่ชำระเงิน" : "Number of unpaid bills";
            PM.PrintTextTwoColumn(printer, estimatedTitle, CurrencyFormat($"{reportBill.est_total_after_vat}"));
            PM.PrintTextTwoColumn(printer, unpaidTitle, CurrencyFormat($"{reportBill.count_bill_status89}"));
            PM.DrawLine(printer);
        }
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

    static void AddProducts(IntPtr printer, Report reportBill)
    {
        double totalPrice = 0.0;

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
                bool isShowReportByMenu = reportBill.shop.is_show_report_by_menu ?? false;

                string key = isShowReportByMenu
                    ? type.product_name
                    : type.product_type_name;

                double amount = Convert.ToDouble(type.amount);
                double price = Convert.ToDouble(type.price);

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

            bool isShowReportByMenuFinal = reportBill.shop.is_show_report_by_menu ?? false;

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
                    PM.PrintTextBold(printer, itemValue.TypeName);
                }

                string priceText = CurrencyFormat(itemValue.Price.ToString());
                string amountText = itemValue.Amount.ToString();

                string name = (reportBill.shop.is_show_report_by_menu ?? false) && itemValue.NO != 0
                    ? $"{itemValue.NO}. {item.Key}"
                    : item.Key;

                PM.PrintTextTwoColumn(printer, name, $"{amountText}    {priceText}");
            }
        }
        PM.NewLine(printer);
        PM.PrintTextTwoColumn(printer, reportBill.language == "th" ? "ยอดขายรวม" : "Total Amount", CurrencyFormat(totalPrice.ToString()));
        PM.NewLine(printer);
    }

    public static Report GenerateMockBillData()
    {
        //ใบสรุปยอดขายรายเดือน
        string json = @"
        {
          ""shop"": {
            ""name"": ""สตรีทคอฟฟี่"",
            ""branch_name"": ""STREET COFFEE 3"",
            ""address"": ""บริชัต แบงคอก เมโทร เน็ทเวิร์คส์ จำกัด สำนักงานใหญ่ เลขที่ 832 ถนนสุทธิสาร รัชดาภิเษก เขต ดินแดง กรุงเทพมหานคร 10400"",
            ""service_charge"": 10,
            ""is_show_report_by_menu"": false,
            ""default_report_printer_id"": 5,
            ""image_url"": """"
          },
          ""date"": ""1"",
          ""bill_count_cancel_not_bill"": 2,
          ""bill_count_total"": 4267,
          ""bill_count_customer"": 0,
          ""bill_count_normal"": 4265,
          ""bill_count_cancel"": 0,
          ""bill_item_count_cancel"": 2,
          ""product_types"": [
            {
              ""product_type_name"": ""STREET COFFEE"",
              ""product_name"": ""Americano (ST)"",
              ""amount"": 1656,
              ""price"": 57960.00
            },
            {
              ""product_type_name"": ""STREET COFFEE"",
              ""product_name"": ""Blue Hawaii coffee (ST)"",
              ""amount"": 1,
              ""price"": 49.00
            },
            {
              ""product_type_name"": ""STREET COFFEE"",
              ""product_name"": ""Blue Hawaii Soda (ST)"",
              ""amount"": 4,
              ""price"": 156.00
            }
          ],
          ""promotions"": [],
          ""point_promotions"": [],
          ""bill_item_free_amount"": 0,
          ""bill_item_free_discount"": 0,
          ""receipt_service_charge"": 0,
          ""receipt_total_before_vat"": 182937.73,
          ""receipt_vat"": 12803.27,
          ""receipt_total_after_vat"": 195741,
          ""discount_special"": 0,
          ""decimal_discount"": 0,
          ""discount_special_amount"": 0,
          ""receipt_payment_cash"": 82498,
          ""receipt_payment_deposit"": 0,
          ""receipt_payment_qrcode"": 113243,
          ""receipt_payment_card"": 0,
          ""receipt_payment_qr_code"": [
            {
              ""bank_id"": 2,
              ""bank_name"": ""ธนาคารกสิกรไทย จำกัด (มหาชน)"",
              ""bank_short_name"": ""KBANK"",
              ""bank_account_payment_methods"": [
                {
                  ""bank_account_payment_method_id"": null,
                  ""bank_account_payment_method_name"": null,
                  ""amount"": 113243,
                  ""payment_count"": 2358
                }
              ]
            }
          ],
          ""receipt_payment_credit_card"": [],
          ""invoice_total_after_vat"": 0,
          ""pos_round_close_tip_amount"": 0,
          ""receipt_restuarant"": 195741,
          ""receipt_take_home"": 0,
          ""deliveries"": [],
          ""est_total_after_vat"": 0,
          ""count_bill_status89"": 0,
          ""language"": ""th""
        }";

        //ใบสรุปยอดขายรายวัน 
        //string json = @"
        //{
        //  ""shop"": {
        //    ""name"": ""สตรีทคอฟฟี่"",
        //    ""branch_name"": ""STREET COFFEE 3"",
        //    ""address"": ""บริเวณ แบงคอก เมโทร เน็ทเวิร์คส์ จำกัด สำนักงานใหญ่ เลขที่ 832 ถนนสุทธิสาร รัชดาภิเษก เขต ดินแดง กรุงเทพมหานคร 10400"",
        //    ""service_charge"": 10,
        //    ""is_show_report_by_menu"": false,
        //    ""default_report_printer_id"": 5,
        //    ""image_url"": """"
        //  },
        //  ""date"": ""2024-02-02"",
        //  ""bill_count_cancel_not_bill"": 0,
        //  ""bill_count_total"": 4,
        //  ""bill_count_customer"": 0,
        //  ""bill_count_normal"": 4,
        //  ""bill_count_cancel"": 0,
        //  ""bill_item_count_cancel"": 0,
        //  ""product_types"": [
        //    {
        //      ""product_type_name"": ""STREET COFFEE"",
        //      ""product_name"": ""Blueberry Coffee (ST)"",
        //      ""amount"": 1,
        //      ""price"": 49.00
        //    },
        //    {
        //      ""product_type_name"": ""STREET COFFEE"",
        //      ""product_name"": ""Blue Hawaii Soda (ST)"",
        //      ""amount"": 10,
        //      ""price"": 390.00
        //    },
        //    {
        //      ""product_type_name"": ""STREET COFFEE"",
        //      ""product_name"": ""Coffee Blend (ST)"",
        //      ""amount"": 11,
        //      ""price"": 110.00
        //    },
        //    {
        //      ""product_type_name"": ""STREET COFFEE"",
        //      ""product_name"": ""Es-Yen (ST)"",
        //      ""amount"": 1,
        //      ""price"": 39.00
        //    },
        //    {
        //      ""product_type_name"": ""STREET COFFEE"",
        //      ""product_name"": ""Honey Lemon Coffee (ST)"",
        //      ""amount"": 10,
        //      ""price"": 490.00
        //    },
        //    {
        //      ""product_type_name"": ""STREET COFFEE"",
        //      ""product_name"": ""Honey (ST)"",
        //      ""amount"": 10,
        //      ""price"": 100.00
        //    },
        //    {
        //      ""product_type_name"": ""STREET COFFEE"",
        //      ""product_name"": ""Punch Coffee (ST)"",
        //      ""amount"": 1,
        //      ""price"": 49.00
        //    },
        //    {
        //      ""product_type_name"": ""STREET COFFEE"",
        //      ""product_name"": ""Punch Soda (ST)"",
        //      ""amount"": 20,
        //      ""price"": 780.00
        //    },
        //    {
        //      ""product_type_name"": ""STREET COFFEE"",
        //      ""product_name"": ""Strawberry Soda (ST)"",
        //      ""amount"": 10,
        //      ""price"": 390.00
        //    }
        //  ],
        //  ""promotions"": [],
        //  ""point_promotions"": [],
        //  ""bill_item_free_amount"": 0,
        //  ""bill_item_free_discount"": 0,
        //  ""receipt_service_charge"": 0,
        //  ""receipt_total_before_vat"": 2240.19,
        //  ""receipt_vat"": 156.81,
        //  ""receipt_total_after_vat"": 2397,
        //  ""discount_special"": 0,
        //  ""decimal_discount"": 0,
        //  ""discount_special_amount"": 0,
        //  ""receipt_payment_cash"": 2397,
        //  ""receipt_payment_deposit"": 0,
        //  ""receipt_payment_qrcode"": 0,
        //  ""receipt_payment_card"": 0,
        //  ""receipt_payment_qr_code"": [],
        //  ""receipt_payment_credit_card"": [],
        //  ""invoice_total_after_vat"": 0,
        //  ""pos_round_close_tip_amount"": 0,
        //  ""receipt_restuarant"": 2397,
        //  ""receipt_take_home"": 0,
        //  ""deliveries"": [],
        //  ""est_total_after_vat"": 0,
        //  ""count_bill_status89"": 0,
        //  ""language"": ""th""
        //}";

        //ใบสรุปลิ้นชัก
        //string json = @"
        //{
        //  ""shop"": {
        //    ""name"": ""สตรีทคอฟฟี่"",
        //    ""branch_name"": ""STREET COFFEE 3"",
        //    ""address"": ""บริสัตย์ แบงคอก เมโทร เน็ตเวิร์คส์ จำกัด สำนักงานใหญ่ เลขที่ 832 ถนนสุทธิสาร รัชดาภิเษก เขต ดินแดง กรุงเทพมหานคร 10400"",
        //    ""service_charge"": 10,
        //    ""is_show_report_by_menu"": false,
        //    ""default_report_printer_id"": 5,
        //    ""image_url"": """"
        //  },
        //  ""date"": ""2024-05-09"",
        //  ""bill_count_cancel_not_bill"": 0,
        //  ""bill_count_total"": 0,
        //  ""bill_count_customer"": 0,
        //  ""bill_count_normal"": 0,
        //  ""bill_count_cancel"": 0,
        //  ""bill_item_count_cancel"": 0,
        //  ""product_types"": [],
        //  ""promotions"": [],
        //  ""point_promotions"": [],
        //  ""bill_item_free_amount"": 0,
        //  ""bill_item_free_discount"": 0,
        //  ""receipt_service_charge"": 0,
        //  ""receipt_total_before_vat"": 0,
        //  ""receipt_vat"": 0,
        //  ""receipt_total_after_vat"": 0,
        //  ""discount_special"": 0,
        //  ""decimal_discount"": 0,
        //  ""discount_special_amount"": 0,
        //  ""receipt_payment_cash"": 0,
        //  ""receipt_payment_deposit"": 0,
        //  ""receipt_payment_qrcode"": 0,
        //  ""receipt_payment_card"": 0,
        //  ""receipt_payment_qr_code"": [],
        //  ""receipt_payment_credit_card"": [],
        //  ""invoice_total_after_vat"": 0,
        //  ""pos_round_close_tip_amount"": 0,
        //  ""receipt_restuarant"": 0,
        //  ""receipt_take_home"": 0,
        //  ""deliveries"": [],
        //  ""est_total_after_vat"": 0,
        //  ""count_bill_status89"": 0,
        //  ""pos_round"": {
        //    ""id"": 71,
        //    ""pos_id"": 7,
        //    ""open_staff_id"": 5,
        //    ""close_staff_id"": 5,
        //    ""initial_amount"": 200,
        //    ""close_cash_amount"": 3900,
        //    ""close_tip_amount"": 0,
        //    ""close_total_amount"": 3900,
        //    ""is_closed"": true,
        //    ""is_current"": false,
        //    ""round"": 2,
        //    ""created_at"": ""2024-05-09T00:57:42.559 07:00"",
        //    ""updated_at"": ""2024-05-09T00:58:45.229 07:00"",
        //    ""bank_deposit_slip"": 0,
        //    ""open_date"": ""2024-05-09"",
        //    ""open_time"": ""00:57"",
        //    ""close_date"": ""2024-05-09"",
        //    ""close_time"": ""00:58"",
        //    ""pos_code"": ""Xprinter"",
        //    ""pos_name"": ""Tesr Xprinter"",
        //    ""in_pos_cash_movements"": [],
        //    ""out_pos_cash_movements"": [
        //      {
        //        ""created_at"": ""00:58"",
        //        ""description"": ""ah"",
        //        ""amount"": 12000
        //      }
        //    ],
        //    ""receipt_payment_amount"": 0,
        //    ""total_after_vat"": 0,
        //    ""in_amount"": 0,
        //    ""out_amount"": 12000,
        //    ""total_amount"": -11800,
        //    ""diff_amount"": 15700,
        //    ""same_date"": true,
        //    ""destination"": {
        //      ""type"": ""usb"",
        //      ""usbTarget"": null
        //    },
        //    ""paperSizeId"": 1
        //  },
        //  ""language"": ""th""
        //}";

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
        return System.Text.Json.JsonSerializer.Deserialize<Report>(json, options);
    }

}

