using System;
using System.Text;
using System.Text.Json;
using PrintingModel;
using System.Net;
using System.Runtime.InteropServices;
using PM = PrinterManager;
using System.Globalization;

//test
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

public class PrintKitchen
{
    public PrintKitchen()
    {
        //IntPtr ptr = PM.GetPrinterConnection(ipAddress);
        //KitchenModel model = JsonSerializer.Deserialize<KitchenModel>(data.jsonData);
        //string jsonString = JsonSerializer.Serialize(data.jsonData);

        //// Deserialize JSON string เป็น KitchenModel
        //KitchenModel model = JsonSerializer.Deserialize<KitchenModel>(jsonString);
        //Print(ptr, model);
        //InitializePrinting(ptr, data.jsonData).Wait();
    }

    public static async Task<PrintKitchen> Create(IntPtr ptr, PrintingQueue data)
    {
        var instance = new PrintKitchen();
        await instance.InitializePrinting(ptr, data.jsonData);
        return instance;
    }

    private async Task InitializePrinting(IntPtr ptr, dynamic data)
    {
        //IntPtr ptr = PM.GetPrinterConnection(ipAddress);
        string jsonString = JsonSerializer.Serialize(data);
        //var q = JsonSerializer.Deserialize<QrCodeModel>(jsonString, options);

        KitchenModel model = JsonSerializer.Deserialize<KitchenModel>(jsonString);
        await Print(ptr, model);
    }

    public async Task Print(IntPtr printer, KitchenModel data)
    {
        await Task.Run(async () =>
        {
            //MessageBox.Show("call start print");
            PM.AlignCenter(printer);
            AddKitchenTitle(printer, data);
            PM.TextAlignLeft(printer);
            AddOrderTime(printer, data);
            AddTable(printer, data);
            AddBuffet(printer, data);
            AddBillNo(printer, data);
            AddStaff(printer, data);
            PM.DrawLine(printer);
            AddBillItems(printer, data);
            PM.CutPaper(printer);
            PM.ClosePort(printer);
            await Task.Delay(300);
        });
    }

    //public void Print(IntPtr printer, KitchenModel data)
    //{
    //    //MessageBox.Show($"call printing kitchen {data.orderer_name}");
    //    PM.AlignCenter(printer);
    //    AddKitchenTitle(printer, data);
    //    PM.TextAlignLeft(printer);
    //    AddOrderTime(printer, data);
    //    AddTable(printer, data);
    //    AddBuffet(printer, data);
    //    AddBillNo(printer, data);
    //    AddStaff(printer, data);
    //    PM.DrawLine(printer);
    //    AddBillItems(printer, data);
    //    PM.CutPaper(printer);
    //    PM.ClosePort(printer);
    //}

    static void AddKitchenTitle(IntPtr printer, KitchenModel data)
    {
        string title = data.kitchen_name ?? "-";
        PM.PrintTextBold(printer, title);
        PM.NewLine(printer);
    }

    static void AddBillNo(IntPtr printer, KitchenModel data) 
    {
        string billNo = string.IsNullOrEmpty(data.bill_no) ? "" : $"{data.bill_no}";
        string topic = data.language == "th" ? "เลขที่บิล " : "Bill no. ";
        PM.PrintTextTitleAndSubTitle(printer, topic, billNo);
    }

    static void AddStaff(IntPtr printer, KitchenModel data)
    {
        string orderer_name_en = data.orderer_name == "ลูกค้า" ? "Customer" : data.orderer_name ?? "-";
        string orderer_name = data.language == "en" ? orderer_name_en : data.orderer_name ?? "-";
        string title = string.IsNullOrEmpty(data.orderer_name) ? "" : $"{orderer_name}";
        string topic = data.language == "th" ? "พนักงานที่สั่ง " : "Order staff ";
        PM.PrintTextTitleAndSubTitle(printer, topic, title);
    }

    static void AddOrderTime(IntPtr printer, KitchenModel data) 
    {
        string lang = data.language ?? "th";
        string date = data.language == "th" ? "วันที่" : "Order time";
        DateTimeHelper dateTimeHelper = new DateTimeHelper();
        string currentDate = dateTimeHelper.GetCurrentDateTime(lang);
        PM.PrintText(printer, $"{date} {currentDate}");
    }

    static void AddTable(IntPtr printer, KitchenModel data) {
        string loc = string.Empty;

        if (data.is_take_home == true && data.delivery_name != null)
        {
            loc = $"{data.delivery_name} {(data.remark != null ? data.remark : string.Empty)}";
        }
        else if (data.is_take_home == true && data.is_delivery != true)
        {
            loc = data.language == "th"
                ? data.bill_type_name
                : data.bill_type_name == "ทานร้าน"
                    ? "Eat In"
                    : "Take Home";
        }
        else if (data.is_take_home != true && data.table_id != null)
        {
            loc = $"{(data.language == "th" ? "โต๊ะ" : "Table")} {data.table_zone_name} {data.table_name}";
        }

        if (!string.IsNullOrEmpty(loc))
        {
            PM.PrintTextBold(printer, loc);
        }
    }

    static void AddBuffet(IntPtr printer, KitchenModel data) {
        if (String.IsNullOrEmpty(data.buffet_text)) return;
        PM.PrintText(printer, data.buffet_text);
    }

    static void AddNotice(IntPtr printer, BillItem billItem)
    {
        foreach (var notice in billItem.notices)
        {
            PM.PrintTextBold(printer, notice.title ?? "");
        }
    }

    static void AddBillItems(IntPtr printer, KitchenModel data)
    {
        if (data.bill_items == null || data.bill_items?.Length == 0) return;
        double totalPrice = 0;

        foreach (var billItem in data.bill_items)
        {
            if (billItem.notices != null && billItem.notices?.Length != 0)
            {
                AddNotice(printer, billItem);
            }

            if (data.is_print_price_kitchen_bill_item == true)
            {
                double unitPrice = billItem.unit_price ?? 0;
                double amount = billItem.amount ?? 1;
                string product = $"{AmountFormatter(amount)}X {billItem.product_name}";
                string price = $"({(unitPrice * amount):F2}.-)";
                totalPrice += unitPrice * amount;
                PM.PrintTextTwoColumn(printer, product, price, 2);
            }
            else
            {
                double amount = billItem.amount ?? 1;
                string text = $"{AmountFormatter(amount)}X   {billItem.product_name}";
                PM.PrintTextBold(printer, text);
            }

            // OPTIONS
            if (billItem.product_is_has_option == true)
            {
                string text = $"- {billItem.product_item_code}";
                PM.PrintTextBold(printer, text);

            }

            // TOPPING
            if (billItem.bill_item_product_toppings?.Length > 0)
            {
                PM.PrintTextBold(printer, "Topping: ");
                foreach (var topping in billItem.bill_item_product_toppings)
                {
                    string text = $"{AmountFormatter(topping.amount ?? 0)}X {topping.product_name}";
                    PM.PrintTextBold(printer, text);
                }
            }

            // NOTE
            if (billItem.bill_item_notes?.Length > 0 || billItem.note != null)
            {
                string text = string.Join(", ", billItem.bill_item_notes.Select(item => item.note_note));
                text = $"- {text}";
                PM.PrintText(printer, text);
            }

            // DESCRIPTION : SUB BILL ITEM
            if (!String.IsNullOrEmpty(billItem.description))
            {
                string text = billItem.description;
                PM.PrintText(printer, $"{(data.language == "th" ? "รายละเอียด" : "Option")} : {text}");
            }

            // IS TAKE HOME
            if (billItem.is_take_home == true)
            {
                string text = (data.language == "th" ? "กลับบ้าน" : "Take Home");
                PM.PrintTextBold(printer, text);
            }

            if (data.is_print_barcode == true)
            {
                AddBarcode(printer, billItem);
            }

            PM.DrawLine(printer);
        }
        
        AddTotalPrice(printer, data, totalPrice);
    }

    static void AddBarcode(IntPtr printer, BillItem billItem)
    {
        // Barcode logic here
        PM.PrintBarcode(printer, billItem.barcode);
    }

    static void AddTotalPrice(IntPtr printer, KitchenModel data, double totalPrice)
    {
        if (data.is_print_price_kitchen_bill_item == true)
        {
            string title = data.language == "th" ? "ราคารวม" : "Total price";
            string total = $"({totalPrice:F2}.-)";
            //MessageBox.Show($"totalPrice {totalPrice} :: total {total}");
            PM.PrintTextTwoColumn(printer, title, total, 2);
            PM.DrawLine(printer);
        }
    }

    static string AmountFormatter(double amount) { 
        return amount.ToString("0.###", CultureInfo.InvariantCulture);
    }

    public KitchenModel GetMockupData()
    {
        // JSON data
        string json = @"{
            ""is_print_price_kitchen_bill_item"": true,
            ""language"": ""th"",
            ""id"": 1277,
            ""kitchen_name"": ""รวม"",
            ""doc_no"": ""B660207000002"",
            ""open_date"": ""2023-02-07"",
            ""open_time"": ""10:03:28.928852"",
            ""close_date"": null,
            ""close_time"": null,
            ""bill_type_id"": 3,
            ""bill_type_with_delivery_id"": ""3-7"",
            ""table_id"": 60,
            ""incharge_staff_id"": null,
            ""customer_id"": 1,
            ""total_after_vat"": 0,
            ""delivery_id"": null,
            ""is_diff_delivery"": false,
            ""document_status_id"": 8,
            ""cancel_date"": null,
            ""cancel_time"": null,
            ""customer_amount"": 0,
            ""delivery_diff"": 0,
            ""delivery_diff_unit_name"": null,
            ""remark"": null,
            ""table_name"": ""5"",
            ""table_zone_name"": ""Zone A"",
            ""customer_name"": ""ลูกค้าทั่วไป"",
            ""bill_type_name"": ""ทานร้าน"",
            ""document_status_name"": ""เปิดบิล"",
            ""cancel_staff_name"": null,
            ""cashier_staff_name"": null,
            ""delivery_name"": null,
            ""is_take_home"": false,
            ""bill_customer_groups"": [],
            ""bill_customer_genders"": [],
            ""bill_customer_ages"": [],
            ""bill_no"": ""B660301000002"",
            ""orderer_name"":""Admin"",
            ""tables"": [
                {
                    ""id"": 60,
                    ""name"": ""5"",
                    ""table_zone_id"": 12,
                    ""seat_count"": 1,
                    ""is_soft_deleted"": false,
                    ""table_status_id"": 2,
                    ""order_no"": null,
                    ""read_only"": false,
                    ""editable_fields"": [],
                    ""bill_id"": 1277,
                    ""table_zone_name"": ""Zone A"",
                    ""table_status_name"": ""OPEN"",
                    ""bill_doc_no"": ""B660207000002"",
                    ""bill_doc_date"": ""2023-02-07"",
                    ""main_table_id"": null,
                    ""main_table_name"": null
                }
            ],
            ""is_buffet"": true,
            ""buffet_category_has_time_limit"": true,
            ""buffet_end_time"": ""2000-01-01T11:03:28.928+07:00"",
            ""bill_items"": [
          {
                    ""id"": 15564,
                    ""bill_id"": 1401,
                    ""product_category_id"": null,
                    ""product_sub_category_id"": null,
                    ""bill_item_status_master_id"": 2,
                    ""product_id"": 498,
                    ""product_item_id"": 606,
                    ""amount"": 8,
                    ""unit_price"": 13.08254200146092,
                    ""price"": 100.24762600438276,
                    ""note"": ""trtyrytr"",
                    ""is_take_home"": true,
                    ""is_free"": false,
                    ""free_amount"": 0,
                    ""free_discount"": 0,
                    ""bill_item_set_id"": 15563,
                    ""is_set"": false,
                    ""unit_price_item"": 13.08254200146092,
                    ""price_item"": 100.24762600438276,
                    ""barcode"": ""8858609532471"",
                    ""created_at"": ""2023-03-09T11:54:35.455+07:00"",
                    ""bill_no"": ""B660301000002"",
                    ""table_name"": null,
                    ""product_name"": ""ผัดกระเพราหมูกรอบไข่ดาว แซ้บๆอร่ยมากๆคุยกันทูน"",
                    ""product_is_buffet"": false,
                    ""product_is_sell_by_weight"": false,
                    ""product_item_name"": ""สิงห์"",
                    ""product_item_code"": ""สิงห์"",
                    ""receipt_item_id"": 15641,
                    ""receipt_id"": 1517,
                    ""product_is_has_option"": false,
                    ""product_sub_category_product_id"": null,
                    ""product_is_show_in_receipt"": null,
                    ""product_image_url"": """",
                    ""sub_bill_items"": [],
                    ""bill_item_product_toppings"": [],
                    ""bill_item_notes"": [{ ""id"": 174, ""note_id"": 7, ""note_note"": ""ไม่เผ็ด"" }],
                    ""has_bill_item_notes"": false,
                    ""remark"": null,
                    ""description"": """"
                },
                {
                    ""id"": 15564,
                    ""bill_id"": 1401,
                    ""product_category_id"": null,
                    ""product_sub_category_id"": null,
                    ""bill_item_status_master_id"": 2,
                    ""product_id"": 498,
                    ""product_item_id"": 606,
                    ""amount"": 3,
                    ""unit_price"": 13.08254200146092,
                    ""price"": 39.24762600438276,
                    ""note"": ""trtyrytr"",
                    ""is_take_home"": true,
                    ""is_free"": false,
                    ""free_amount"": 0,
                    ""free_discount"": 0,
                    ""bill_item_set_id"": 15563,
                    ""is_set"": false,
                    ""unit_price_item"": 13.08254200146092,
                    ""price_item"": 39.24762600438276,
                    ""barcode"": ""8858609532471"",
                    ""created_at"": ""2023-03-09T11:54:35.455+07:00"",
                    ""bill_no"": ""B660301000002"",
                    ""table_name"": null,
                    ""product_name"": ""สิงห์ ดีมาก คุยกันสนุกมาก สุดๆไปเลยตั้งใจมากค่ะทดสอบสิงห์ ดีมาก คุยกันสนุกมาก สุดๆไปเลยตั้งใจมากค่ะทดสอบสิงห์ ดีมาก คุยกันสนุกมาก สุดๆไปเลยตั้งใจมากค่ะทดสอบ"",
                    ""product_is_buffet"": false,
                    ""product_is_sell_by_weight"": false,
                    ""product_item_name"": ""สิงห์"",
                    ""product_item_code"": ""สิงห์"",
                    ""receipt_item_id"": 15641,
                    ""receipt_id"": 1517,
                    ""product_is_has_option"": false,
                    ""product_sub_category_product_id"": null,
                    ""product_is_show_in_receipt"": null,
                    ""product_image_url"": """",
                    ""sub_bill_items"": [],
                    ""bill_item_product_toppings"": [],
                    ""bill_item_notes"": [{ ""id"": 174, ""note_id"": 7, ""note_note"": ""Note3"" }],
                    ""has_bill_item_notes"": false,
                    ""remark"": null,
                    ""description"": ""beer singha (3 ea.)""
                }
            ]
        }";

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        KitchenModel model = JsonSerializer.Deserialize<KitchenModel>(json, options);

        //// Deserialize JSON to Dictionary<string, object>
        //dynamic data = JsonSerializer.Deserialize<dynamic>(json);
        //        string json = @" {
        //  ""id"": 1,
        //  ""name"": ""Printer1"",
        //  ""is_ip_connection"": true,
        //  ""ip_address"": ""192.168.1.1""
        //}";
        //        dynamic data = JsonSerializer.Deserialize<dynamic>(json);
        return model;
    }

    public static void DrawComparisonLine(IntPtr printer)
    {
        //// กำหนดความสูงของฟอนต์ที่ต้องการ
        //byte[] setLineSpacing = new byte[] { 0x1B, 0x33, 0x20 }; // กำหนดระยะห่างของบรรทัด
        //ESCPOS.WriteData(printer, setLineSpacing, setLineSpacing.Length);

        //// ตัวหนา
        //byte[] boldOn = new byte[] { 0x1B, 0x45 }; // เปิดตัวหนา
        //ESCPOS.WriteData(printer, boldOn, boldOn.Length);
        PM.SetTextSize(printer,1);

        // พิมพ์เส้นขีดตัวหนา 42 ตัวอักษร
        string boldLine = new string('-', 42);
        byte[] boldLineBytes = Encoding.GetEncoding("TIS-620").GetBytes(boldLine);
        ESCPOS.WriteData(printer, boldLineBytes, boldLineBytes.Length);
        PM.NewLine(printer);

        // ปิดตัวหนา
        //byte[] boldOff = new byte[] { 0x1B, 0x46 };
        //ESCPOS.WriteData(printer, boldOff, boldOff.Length);

        // เพิ่มระยะห่างบรรทัดเล็กน้อย
        PM.NewLine(printer);

        // จุดตรงกลาง
        string dotLine = new string('.', 42);
        byte[] dotLineBytes = Encoding.GetEncoding("TIS-620").GetBytes(dotLine);
        ESCPOS.WriteData(printer, dotLineBytes, dotLineBytes.Length);
        PM.NewLine(printer);

        // Reset line spacing to default
        byte[] resetLineSpacing = new byte[] { 0x1B, 0x32 };
        ESCPOS.WriteData(printer, resetLineSpacing, resetLineSpacing.Length);
    }

    
    static void KTest(IntPtr printer)
    {
        //byte[] reset = new byte[] { 0x1B, 0x40 };
        //ESCPOS.WriteData(printer, reset, reset.Length);

        byte[] selectFontB = new byte[] { 0x1B, 0x21, 0x01 };
        ESCPOS.WriteData(printer, selectFontB, selectFontB.Length);

        //byte[] emphasizedOn = new byte[] { 0x1B, 0x45, 0x01 }; // เปิดโหมดตัวหนา
        //ESCPOS.WriteData(printer, emphasizedOn, emphasizedOn.Length);

        //byte[] selectLargeBold = new byte[] { 0x1D, 0x21, 0x11 }; // 0x11 เลือกตัวอักษรขนาดใหญ่และตัวหนา
        //ESCPOS.WriteData(printer, selectLargeBold, selectLargeBold.Length);

        byte[] t = Encoding.GetEncoding("TIS-620").GetBytes("ครัว Kitchen");
        ESCPOS.WriteData(printer, t, t.Length);
        PM.NewLine(printer);
        PM.CutPaper(printer);


        return;

        //// 1. เปิดโหมดตัวหนา (Bold)
        byte[] boldOn = new byte[] { 0x1B, 0x45, 0x01 };
        ESCPOS.WriteData(printer, boldOn, boldOn.Length);

        //byte[] textBytes = Encoding.GetEncoding("TIS-620").GetBytes("ครัว Kitchen");
        //ESCPOS.WriteData(printer, textBytes, textBytes.Length);

        //// 2. เปิด Double-strike mode เพื่อให้หนาขึ้นอีก
        byte[] doubleStrikeOn = new byte[] { 0x1B, 0x47, 0x01 };
        ESCPOS.WriteData(printer, doubleStrikeOn, doubleStrikeOn.Length);

        //textBytes = Encoding.GetEncoding("TIS-620").GetBytes("ครัว Kitchen");
        //ESCPOS.WriteData(printer, textBytes, textBytes.Length);

        //// 3. ตั้งค่า Print Density ให้สูงสุด
        byte[] density = new byte[] { 0x1D, 0x7C, 0x0F }; // ค่าสูงสุด
        ESCPOS.WriteData(printer, density, density.Length);

        //textBytes = Encoding.GetEncoding("TIS-620").GetBytes("ครัว Kitchen");
        //ESCPOS.WriteData(printer, textBytes, textBytes.Length);

        //// 4. ตั้งค่าโหมดพิมพ์แบบเน้น (Emphasized mode)
        byte[] e = new byte[] { 0x1B, 0x21, 0x08 };
        ESCPOS.WriteData(printer, e, e.Length);
        ////---section 1
        PM.AlignCenter(printer);

        ////PM.PrintText3(printer, "ครัว Kitchen");
        ////PM.PrintText5(printer, "ครัว Kitchen");
        ////PM.PrintText6(printer,"ครัว Kitchen");

        byte[] textBytes = Encoding.GetEncoding("TIS-620").GetBytes("ครัว Kitchen");
        ESCPOS.WriteData(printer, textBytes, textBytes.Length);

        //ESCPOS.WriteData(printer, reset, reset.Length);

        PM.PrintTextBold(printer, "ครัว Kitchen");
        PM.NewLine(printer);
        PM.TextAlignLeft(printer);
        PM.PrintText(printer, "วันที่ 08 พฤศจิกายน 2567 เวลา 14:34 น.");

        //bill no
        PM.SetTextSize(printer, 1);
        textBytes = Encoding.GetEncoding("TIS-620").GetBytes("เลขที่บิล ");
        ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        PM.SetTextSize(printer, 2);
        textBytes = Encoding.GetEncoding("TIS-620").GetBytes("B671110000001");
        ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        PM.NewLine(printer);

        //staff
        PM.SetTextSize(printer, 1);
        textBytes = Encoding.GetEncoding("TIS-620").GetBytes("พนักงานที่สั่ง ");
        ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        PM.SetTextSize(printer, 2);
        textBytes = Encoding.GetEncoding("TIS-620").GetBytes("admin");
        ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        PM.NewLine(printer);

        PM.DrawLine(printer);
        PM.PrintTextBold(printer, "1X Americano อเมริกาโน่");
        PM.DrawLine(printer);
        PM.NewLine(printer);
        PM.CutPaper(printer);
        return;

        //--- section 2
        PM.AlignCenter(printer);
        PM.PrintTextMediumBold(printer, "ครัว Kitchen");
        PM.NewLine(printer);
        PM.TextAlignLeft(printer);
        PM.PrintText(printer, "วันที่ 08 พฤศจิกายน 2567 เวลา 14:34 น.");

        //bill no
        PM.SetTextSize(printer, 1);
        textBytes = Encoding.GetEncoding("TIS-620").GetBytes("เลขที่บิล ");
        ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        PM.SetTextSize(printer, 3);
        textBytes = Encoding.GetEncoding("TIS-620").GetBytes("B671110000001");
        ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        PM.NewLine(printer);

        //staff
        PM.SetTextSize(printer, 1);
        textBytes = Encoding.GetEncoding("TIS-620").GetBytes("พนักงานที่สั่ง ");
        ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        PM.SetTextSize(printer, 3);
        textBytes = Encoding.GetEncoding("TIS-620").GetBytes("admin");
        ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        PM.NewLine(printer);

        PM.DrawLine(printer);
        PM.PrintTextMediumBold(printer, "1X Americano อเมริกาโน่");
        PM.DrawLine(printer);
        PM.NewLine(printer);
        PM.CutPaper(printer);

        //section 3 ----
        PM.AlignCenter(printer);
        PM.PrintText6(printer, "ครัว Kitchen");
        PM.NewLine(printer);
        PM.TextAlignLeft(printer);
        PM.PrintText(printer, "วันที่ 08 พฤศจิกายน 2567 เวลา 14:34 น.");

        //bill no
        PM.SetTextSize(printer, 1);
        textBytes = Encoding.GetEncoding("TIS-620").GetBytes("เลขที่บิล ");
        ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        PM.SetTextSize(printer, 6);
        //PM.LineSpace(printer, 100);

        //byte[] setPositionForLargeText = new byte[] { 0x1B, 0x24, 0x00, 0x20 }; // เลื่อนลง 32 จุด
        //ESCPOS.WriteData(printer, setPositionForLargeText, setPositionForLargeText.Length);

        byte[] setVerticalPosition = new byte[] { 0x1D, 0x56, 0x00, 0x40 }; // เลื่อนลง 64 จุด
        ESCPOS.WriteData(printer, setVerticalPosition, setVerticalPosition.Length);

        textBytes = Encoding.GetEncoding("TIS-620").GetBytes("B671110000001");
        ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        //PM.LineSpaceDefault(printer);
        PM.NewLine(printer);

        //staff
        PM.SetTextSize(printer, 1);
        textBytes = Encoding.GetEncoding("TIS-620").GetBytes("พนักงานที่สั่ง ");
        ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        PM.SetTextSize(printer, 6);

        textBytes = Encoding.GetEncoding("TIS-620").GetBytes("Admin");
        ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        PM.LineSpaceDefault(printer);
        PM.NewLine(printer);

        PM.DrawLine(printer);
        PM.PrintText6(printer, "1X Americano อเมริกาโน่");
        PM.DrawLine(printer);
        PM.CutPaper(printer);

        //section 4
        return;
        PM.AlignCenter(printer);
        PM.PrintText6(printer, "ครัว Kitchen");
        PM.NewLine(printer);
        PM.TextAlignLeft(printer);
        PM.PrintText(printer, "วันที่ 08 พฤศจิกายน 2567 เวลา 14:34 น.");

        //bill no
        PM.SetTextSize(printer, 1);
        textBytes = Encoding.GetEncoding("TIS-620").GetBytes("เลขที่บิล ");
        ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        PM.SetTextSize(printer, 6);
        textBytes = Encoding.GetEncoding("TIS-620").GetBytes("B671110000001");
        ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        PM.NewLine(printer);

        //staff
        PM.SetTextSize(printer, 1);
        textBytes = Encoding.GetEncoding("TIS-620").GetBytes("พนักงานที่สั่ง ");
        ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        PM.SetTextSize(printer, 6);
        textBytes = Encoding.GetEncoding("TIS-620").GetBytes("admin");
        ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        PM.NewLine(printer);

        PM.DrawLine(printer);
        PM.PrintText6(printer, "1X Americano อเมริกาโน่");
        PM.DrawLine(printer);
        PM.CutPaper(printer);
    }

    static void KTest2(IntPtr printer) { }

    static void KTest3(IntPtr printer) { }

    static void Test(IntPtr printer, PrintingQueue d)
    {
        //MessageBox.Show("เวลาเริ่ม");
        string text = "เวลาเริ่ม";
        byte[] sizeCommand24 = new byte[] { 0x1B, 0x58, 0x00, 0x18, 0x00 }; // nL = 24, nH = 0
        ESCPOS.WriteData(printer, sizeCommand24, sizeCommand24.Length);
        //ESCPOS.WriteData(printer, System.Text.Encoding.ASCII.GetBytes("ABC"), 3);
        byte[] textBytes = Encoding.GetEncoding("TIS-620").GetBytes(text);
        ESCPOS.WriteData(printer, textBytes, textBytes.Length);

        // พิมพ์ข้อความที่ขนาด 48 จุด
        byte[] sizeCommand48 = new byte[] { 0x1B, 0x58, 0x00, 0x30, 0x00 }; // nL = 48, nH = 0
        ESCPOS.WriteData(printer, sizeCommand48, sizeCommand48.Length);
        //ESCPOS.WriteData(printer, System.Text.Encoding.ASCII.GetBytes("DEF"), 3);
        textBytes = Encoding.GetEncoding("TIS-620").GetBytes(text);
        ESCPOS.WriteData(printer, textBytes, textBytes.Length);
    }

    public static void GenerateAndSaveReceiptLineBitmap()
    {
        //MessageBox.Show("gen sdhkjsnkedc");
        // ขนาดของภาพ Bitmap (ปรับขนาดตามเครื่องพิมพ์ของคุณ)
        int width = 580; // กว้างของเครื่องพิมพ์
        int height = 50; // ความสูงตามที่ต้องการ

        using (Bitmap bitmap = new Bitmap(width, height))
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            // ตั้งค่าแบคกราวด์เป็นสีขาว
            graphics.Clear(Color.White);

            // ฟอนต์ที่ต้องการ
            Font font = new Font("Arial", 20, FontStyle.Regular);

            // สีข้อความ
            Brush brush = Brushes.Black;

            // วาดข้อความชิดซ้าย
            string leftText = "1X Americano";
            graphics.DrawString(leftText, font, brush, new PointF(0, 0));

            // วาดข้อความชิดขวา
            string rightText = "65.บาท";
            SizeF textSize = graphics.MeasureString(rightText, font);
            float xPosition = width - textSize.Width;
            graphics.DrawString(rightText, font, brush, new PointF(xPosition, 0));

            // บันทึกไฟล์ภาพเป็น .bmp
            string filePath = "receipt_line_bitmap.bmp";
            bitmap.Save(filePath, ImageFormat.Bmp);

            // ตรวจสอบขนาดไฟล์
            FileInfo fileInfo = new FileInfo(filePath);
            long fileSizeInBytes = fileInfo.Length;
            double fileSizeInKB = fileSizeInBytes / 1024.0;

            Console.WriteLine($"ไฟล์ถูกบันทึกที่: {filePath}");
            Console.WriteLine($"ขนาดไฟล์: {fileSizeInKB:F2} KB");
        }
    }

    public static void PrintTextCompressed(IntPtr printer, string text)
    {
        //MessageBox.Show("atvvbvbvbdfgwd");

        PM.SetTextSize(printer,5);


        //byte[] boldOn = new byte[] { 0x1B, 0x45, 0x01 };
        //ESCPOS.WriteData(printer, boldOn, boldOn.Length);

        // เลือก Font B (Elite Mode)
        //byte[] fontB = new byte[] { 0x1B, 0x4D, 0x01 };
        //ESCPOS.WriteData(printer, fontB, fontB.Length);

        // ส่งข้อความที่ต้องการพิมพ์
        //byte[] textBytes = Encoding.GetEncoding("TIS-620").GetBytes("ข้อความแบบตัวหนาและ Font B");
        //ESCPOS.WriteData(printer, textBytes, textBytes.Length);

        //byte[] textBytes = Encoding.GetEncoding("TIS-620").GetBytes(text);
        //ESCPOS.WriteData(printer, textBytes, textBytes.Length);

        //byte[] setSpacing = new byte[] { 0x1B, 0x20, 0x00 };
        //ESCPOS.WriteData(printer, setSpacing, setSpacing.Length);

        PM.PrintText5(printer, text);

        byte[] fontSelect = new byte[] { 0x1B, 0x4D, 0x01 }; // Select Font B
        ESCPOS.WriteData(printer, fontSelect, fontSelect.Length);

        byte[] textBytes = Encoding.GetEncoding("TIS-620").GetBytes(text);
        ESCPOS.WriteData(printer, textBytes, textBytes.Length);

        byte[] setHMU = new byte[] { 0x1D, 0x50, 0x00, 0x00 };
        ESCPOS.WriteData(printer, setHMU, setHMU.Length);

        textBytes = Encoding.GetEncoding("TIS-620").GetBytes(text);
        ESCPOS.WriteData(printer, textBytes, textBytes.Length);

        byte[] reset = new byte[] { 0x1B, 0x40 };
        ESCPOS.WriteData(printer, reset, reset.Length);

        //// ปิดการใช้งานตัวหนาหลังจากพิมพ์เสร็จ
        //byte[] boldOff = new byte[] { 0x1B, 0x45, 0x00 };
        //ESCPOS.WriteData(printer, boldOff, boldOff.Length);
        PM.NewLine(printer);
        PM.CutPaper(printer);

        return;
        // รีเซ็ตการตั้งค่าก่อน
        byte[] resetCommand = new byte[] { 0x1B, 0x40 };
        ESCPOS.WriteData(printer, resetCommand, resetCommand.Length);

        //ลำดับการส่งคำสั่งสำคัญมาก

         //1.ตั้งค่าความกว้างและความสูงด้วย GS!
         //bit 0 - 3: ความกว้าง(0001 = กว้างขึ้นเล็กน้อย)
         //bit 4 - 7: ความสูง(0001 = สูงขึ้นเล็กน้อย)
        byte[] sizeCommand = new byte[] { 0x1D, 0x21, 0x11 };
        ESCPOS.WriteData(printer, sizeCommand, sizeCommand.Length);

        // 2. ใช้คำสั่ง ESC ! เพื่อควบคุมรูปแบบตัวอักษร
        // bit 0: ตัวหนา
        // bit 3: double-height
        // bit 4: double-width
        byte[] fontCommand = new byte[] { 0x1B, 0x21, 0x00 };
        ESCPOS.WriteData(printer, fontCommand, fontCommand.Length);

        // 3. ตั้งค่าระยะห่างระหว่างตัวอักษรให้น้อยที่สุด
        byte[] spacingCommand = new byte[] { 0x1B, 0x20, 0x00 };
        ESCPOS.WriteData(printer, spacingCommand, spacingCommand.Length);

        // 4. ตั้งค่าความหนาแน่นการพิมพ์ (Print Density)
        // คำสั่ง GS L nL nH - ตั้งค่าขอบซ้าย
        byte[] marginCommand = new byte[] { 0x1D, 0x4C, 0x00, 0x00 };
        ESCPOS.WriteData(printer, marginCommand, marginCommand.Length);

        // 5. ปรับความกว้างตัวอักษร (Character Width)
        byte[] widthCommand = new byte[] { 0x1B, 0x4D, 0x00 }; // 0x00 for smallest width
        ESCPOS.WriteData(printer, widthCommand, widthCommand.Length);

        // พิมพ์ข้อความ
        //byte[] textBytes = Encoding.GetEncoding("TIS-620").GetBytes(text);
        //ESCPOS.WriteData(printer, textBytes, textBytes.Length);
        PM.PrintText(printer, text);
        PM.NewLine(printer);

        PM.CutPaper(printer);

        // รีเซ็ตการตั้งค่ากลับเป็นค่าปกติ
        //ESCPOS.WriteData(printer, resetCommand, resetCommand.Length);
    }

    static void TestTwoColumn(IntPtr printer) {
        string textLeft = "123456789-123456789-123456789-123456789-123456789-";
        string textRight = "200.-";
        PM.PrintTextTwoColumn(printer, textLeft, textRight);
        PM.CutPaper(printer);
    }
}
