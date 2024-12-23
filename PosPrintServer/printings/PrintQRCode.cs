using PM = PrinterManager;
using PrintingModel;

public class PrintQRCode
{
    public PrintQRCode(PrintingQueue data)
    {
        foreach (Printer printer in data.printers)
        {
            //MessageBox.Show($"qr code {printer}");
            if (string.IsNullOrEmpty(printer.ip_address)) continue;
            IntPtr ptr = PM.GetPrinterConnection(printer.ip_address);
            Print(ptr, data);
        }
    }

    public async void Print(IntPtr printer, PrintingQueue data)
    {
        //MessageBox.Show($"call qr code");
        QrCodeModel q = QrCodeModel.GenerateMockData();
        DateTimeHelper dateTimeHelper = new DateTimeHelper();

        string imageUrl = q.Shop.ImageUrl;
        string qrcode = q.QrCode;
        string table = $"{q.Bill.TableZoneName}{q.Bill.TableName}";
        string qrScan = q.Language == "th" ? "QR code เพื่อสแกนสั่งอาหาร" : "QR code for scan to order";
        string currentDate = dateTimeHelper.GetCurrentDate("th");
        var times = q.Bill.OpenTime.Split(':');
        string time = q.Language == "th"
            ? $"เวลาเริ่ม: {times[0]}:{times[1]}น."
            : $"Start time: {times[0]}:{times[1]}";

        //MessageBox.Show("sefjwiejdlqw");

        PM.AlignCenter(printer);
        await PM.PrintImageUrl(printer, imageUrl, "logo.jpg");
        PM.NewLine(printer);
        PM.PrintTextMediumBold(printer, "A3");
        PM.PrintTextBold(printer, qrScan);
        PM.LineSpace(printer, 40);
        PM.NewLine(printer);
        PM.PrintTextBold(printer, currentDate);
        PM.LineSpaceDefault(printer);
        PM.NewLine(printer);
        //start time
        PM.PrintTextMediumBold(printer, time);
        PM.NewLine(printer);
        BuffetEndTime(printer, q);
        PM.NewLine(printer);
        BuffetName(printer, q);
        PM.NewLine(printer);
        await PM.PrintImageUrl(printer, qrcode, "logo.jpg", 260);
        PM.NewLine(printer);
        PM.CutPaper(printer);
    }

    static void BuffetEndTime(IntPtr printer,QrCodeModel data) {
        if (data.Bill.IsBuffet != true) return;
        string time;
        if (data.Bill.BuffetCategoryHasTimeLimit == true && !string.IsNullOrEmpty(data.Bill.BuffetEndTime))
        {
            string[] times = data.Bill.BuffetEndTime.Split(':');
            time = data.Language == "th"
                ? $"เวลาสิ้นสุด: {times[0]}:{times[1]}น."
                : $"End time: {times[0]}:{times[1]}";
        }
        else
        {
            time = data.Language == "th"
                ? "เวลาสิ้นสุด: ไม่จำกัดเวลา"
                : "End time: No time limit";
        }
        PM.PrintTextMediumBold(printer, time);
    }

    static void BuffetName(IntPtr printer, QrCodeModel data) {
        if (string.IsNullOrEmpty(data.Bill.BuffetName)) return;
        string name = data.Bill.BuffetName;
        PM.PrintTextBold(printer, name);
    }

}