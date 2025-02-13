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
using SocketIOClient;

public class PrintKitchen
{
    public static async Task<PrintKitchen> Create(IntPtr ptr, PrintingQueue data)
    {
        var instance = new PrintKitchen();
        await instance.InitializePrinting(ptr, data.jsonData);
        return instance;
    }

    private async Task InitializePrinting(IntPtr ptr, dynamic data)
    {
        string jsonString = JsonSerializer.Serialize(data);
        KitchenModel? model = JsonSerializer.Deserialize<KitchenModel>(jsonString);
        if (model == null) return;
        await Print(ptr, model);
    }

    public async Task Print(IntPtr printer, KitchenModel data)
    {
        try
        {
            //Console.WriteLine("call kitchen test--");
            bool[] s = new bool[7];
            //await Task.Run(async () =>
            //{
            PM.SetTextFont(printer, 0);
            PM.AlignCenter(printer);
            s[0] = AddKitchenTitle(printer, data);
            PM.TextAlignLeft(printer);
            s[1] = AddOrderTime(printer, data);
            s[2] = AddTable(printer, data);
            s[3] = AddBuffet(printer, data);
            s[4] = AddBillNo(printer, data);
            s[5] = AddStaff(printer, data);
            PM.DrawLine(printer);
            s[6] = AddBillItems(printer, data);
            PM.CutPaper(printer);
            int st = PM.ClosePort(printer);
            PM.ReleasePort(printer);
            if (s.Any(x => x == false))
            {
                //MessageBox.Show($" some data id false ---");
                WriteLog.WriteFailedPrintLog(data, "Kitchen");
            }
            await Task.Delay(100);
            //});
        }
        catch (Exception e)
        {
            //keep log
            PM.ClosePort(printer);
            PM.ReleasePort(printer);
            WriteLog.WriteFailedPrintLog(data, "Kitchen");
            //WriteLog.Write($"{e}", "Kitchen");
            //MessageBox.Show($"{e}");
        }
    }

    //private TaskCompletionSource<bool> _printingComplete;

    //private PrintKitchen()
    //{
    //    _printingComplete = new TaskCompletionSource<bool>();
    //}

    //public static async Task<PrintKitchen> Create(IntPtr ptr, PrintingQueue data)
    //{
    //    var instance = new PrintKitchen();
    //    await instance.InitializePrinting(ptr, data.jsonData);
    //    return instance;
    //}

    //public Task WaitForCompletion()
    //{
    //    return _printingComplete.Task;
    //}

    //private async Task InitializePrinting(IntPtr ptr, dynamic data)
    //{
    //    string jsonString = JsonSerializer.Serialize(data);
    //    KitchenModel model = JsonSerializer.Deserialize<KitchenModel>(jsonString);
    //    await Print(ptr, model);
    //}

    //public async Task Print(IntPtr printer, KitchenModel data)
    //{
    //    try
    //    {
    //        bool[] s = new bool[7];

    //        await Task.Run(() =>
    //        {
    //            PM.SetTextFont(printer, 0);
    //            PM.AlignCenter(printer);
    //            s[0] = AddKitchenTitle(printer, data);
    //            PM.TextAlignLeft(printer);
    //            s[1] = AddOrderTime(printer, data);
    //            s[2] = AddTable(printer, data);
    //            s[3] = AddBuffet(printer, data);
    //            s[4] = AddBillNo(printer, data);
    //            s[5] = AddStaff(printer, data);
    //            PM.DrawLine(printer);
    //            s[6] = AddBillItems(printer, data);
    //            PM.CutPaper(printer);
    //        });

    //        // รอให้การพิมพ์เสร็จจริงๆ
    //        await Task.Delay(1000); // หรือเวลาที่เหมาะสม

    //        int st = PM.ClosePort(printer);
    //        PM.ReleasePort(printer);

    //        if (s.Any(x => x == false))
    //        {
    //            throw new Exception("Some printing operations failed");
    //        }

    //        _printingComplete.SetResult(true);
    //    }
    //    catch (Exception e)
    //    {
    //        WriteLog.WriteFailedPrintLog(data, "Kitchen");
    //        WriteLog.Write($"{e}", "Kitchen");
    //        _printingComplete.SetException(e);
    //        throw;
    //    }
    //    finally
    //    {
    //        try
    //        {
    //            PM.ClosePort(printer);
    //            PM.ReleasePort(printer);
    //        }
    //        catch { }
    //    }
    //}

    static bool AddKitchenTitle(IntPtr printer, KitchenModel data)
    {
        string title = data.kitchen_name ?? "-";
        int s = PM.PrintTextBold(printer, title);
        PM.NewLine(printer);
        return s == 0;
    }

    static bool AddBillNo(IntPtr printer, KitchenModel data) 
    {
        string billNo = string.IsNullOrEmpty(data.bill_no) ? "" : $"{data.bill_no}";
        string topic = data.language == "th" ? "เลขที่บิล " : "Bill no. ";
        int s = PM.PrintTextTitleAndSubTitle(printer, topic, billNo);
        return s == 0;
    }

    static bool AddStaff(IntPtr printer, KitchenModel data)
    {
        string orderer_name_en = data.orderer_name == "ลูกค้า" ? "Customer" : data.orderer_name ?? "-";
        string orderer_name = data.language == "en" ? orderer_name_en : data.orderer_name ?? "-";
        string title = string.IsNullOrEmpty(data.orderer_name) ? "" : $"{orderer_name}";
        string topic = data.language == "th" ? "พนักงานที่สั่ง " : "Order staff ";
        int s = PM.PrintTextTitleAndSubTitle(printer, topic, title);
        return s == 0;
    }

    static bool AddOrderTime(IntPtr printer, KitchenModel data) 
    {
        string lang = data.language ?? "th";
        string date = data.language == "th" ? "วันที่" : "Order time";
        DateTimeHelper dateTimeHelper = new DateTimeHelper();
        string currentDate = dateTimeHelper.GetCurrentDateTime(lang);
        int s = PM.PrintText(printer, $"{date} {currentDate}");
        return s == 0;
    }

    static bool AddTable(IntPtr printer, KitchenModel data) {
        string loc = string.Empty;
        int s = 0;

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

        if (!String.IsNullOrEmpty(loc))
        {
            s = PM.PrintTextBold(printer, loc);
        }
        return s == 0;
    }

    static bool AddBuffet(IntPtr printer, KitchenModel data) {
        if (String.IsNullOrEmpty(data.buffet_text)) return true;
        int s = PM.PrintText(printer, data.buffet_text);
        return s == 0;
    }

    static void AddNotice(IntPtr printer, BillItem billItem)
    {
        foreach (var notice in billItem.notices)
        {
            PM.PrintTextBold(printer, notice.title ?? "");
        }
    }

    static bool AddBillItems(IntPtr printer, KitchenModel data)
    {
        if (data.bill_items == null || data.bill_items?.Length == 0) return true;
        double totalPrice = 0;
        int[] s = new int[8];

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
                string price = $"({AmountFormatter(unitPrice * amount)}.-)";

                totalPrice += unitPrice * amount;
                s[0] = PM.PrintTextTwoColumn(printer, product, price, 2);
            }
            else
            {
                double amount = billItem.amount ?? 1;
                string text = $"{AmountFormatter(amount)}X   {billItem.product_name}";
                s[1] = PM.PrintTextBold(printer, text);
            }

            // OPTIONS
            if (billItem.product_is_has_option == true)
            {
                string text = $"- {billItem.product_item_code}";
                s[2] = PM.PrintTextBold(printer, text);

            }

            // TOPPING
            if (billItem.bill_item_product_toppings?.Length > 0)
            {
                PM.PrintTextBold(printer, "Topping: ");
                foreach (var topping in billItem.bill_item_product_toppings)
                {
                    string text = $"{AmountFormatter(topping.amount ?? 0)}X {topping.product_name}";
                    s[3] = PM.PrintTextBold(printer, text);
                }
            }

            // NOTE
            if (billItem.bill_item_notes?.Length > 0 || billItem.note != null)
            {
                var text = string.Join(", ", billItem.bill_item_notes
                    .Select(item => item.note_note)
                    .Concat(new[] { billItem.note }));
                text = $"- {text}";
                s[4] = PM.PrintText(printer, text);
            }

            // DESCRIPTION : SUB BILL ITEM
            if (!String.IsNullOrEmpty(billItem.description))
            {
                string text = billItem.description;
                s[5] = PM.PrintText(printer, $"{(data.language == "th" ? "รายละเอียด" : "Option")} : {text}");
            }

            // IS TAKE HOME
            if (billItem.is_take_home == true)
            {
                string text = (data.language == "th" ? "กลับบ้าน" : "Take Home");
                s[6] = PM.PrintTextBold(printer, text);
            }

            if (data.is_print_barcode == true)
            {
                PM.NewLine(printer);
                AddBarcode(printer, billItem);
            }

            PM.DrawLine(printer);
        }
        
        bool st = AddTotalPrice(printer, data, totalPrice);
        s[7] = st ? 0 : -1 ;
        return s.All(x => x == 0);
    }

    static void AddBarcode(IntPtr printer, BillItem billItem)
    {
        PM.PrintBarcode(printer, billItem.barcode);
    }

    static bool AddTotalPrice(IntPtr printer, KitchenModel data, double totalPrice)
    {
        if (data.is_print_price_kitchen_bill_item == true)
        {
            //AmountFormatter(topping.amount ?? 0)
            string title = data.language == "th" ? "ราคารวม" : "Total price";
            //string total = $"({totalPrice:F2}.-)";
            string total = $"({AmountFormatter(totalPrice)}.-)";
            int s = PM.PrintTextTwoColumn(printer, title, total, 2);
            PM.DrawLine(printer);
            return s == 0;
        }
        return true;
    }

    static string AmountFormatter(double amount) {
        if (amount % 1 == 0)
            return ((int)amount).ToString("N0", CultureInfo.InvariantCulture); // แสดง comma สำหรับจำนวนเต็ม
        else
            return amount.ToString("N2", CultureInfo.InvariantCulture);
    }
}
