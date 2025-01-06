using PM = PrinterManager;
using PrintingModel;
using System.Text.Json;

public class PrintQRCode
{
    //public PrintQRCode(IntPtr ptr, PrintingQueue data)
    //{
    //    //foreach (Printer printer in data.printers)
    //    //{
    //    //    WriteFile($"data {data.jsonData}");
    //    //    if (string.IsNullOrEmpty(printer.ip_address)) continue;
    //    //    //IntPtr ptr = PM.GetPrinterConnection(printer.ip_address);
    //    //    IntPtr ptr = ESCPOS.InitPrinter("");
    //    //    Print(ptr, data);
    //    //}
    //    Print(ptr, data);
    //}

    public static async Task<PrintQRCode> Create(IntPtr ptr, QrCodeModel data)
    {
        var instance = new PrintQRCode();
        await instance.InitializePrinting(ptr, data);
        return instance;
    }

    private async Task InitializePrinting(IntPtr ptr, QrCodeModel data)
    {
        //var options = new JsonSerializerOptions
        //{
        //    PropertyNameCaseInsensitive = true
        //};
        ////string jsonString = JsonSerializer.Serialize(data);
        //QrCodeModel model = JsonSerializer.Deserialize<QrCodeModel>(data);
        await Print(ptr, data);
    }

    public async Task Print(IntPtr printer, QrCodeModel q)
    {
        await Task.Run(async () =>
        {
                DateTimeHelper dateTimeHelper = new DateTimeHelper();

                string imageUrl = q.Shop.ImageUrl;
                string qrcode = q.QrCode;
                string table = $"{q.Bill.TableZoneName} {q.Bill.TableName}";
                string qrScan = q.Language == "th" ? "QR code เพื่อสแกนสั่งอาหาร" : "QR code for scan to order";
                string currentDate = dateTimeHelper.GetCurrentDate(q.Language ?? "th");
                var times = q.Bill.OpenTime != null ? q.Bill.OpenTime.Split(':') : [];
                string time = times.Length > 0 ? q.Language == "th"
                    ? $"เวลาเริ่ม: {times[0]}:{times[1]}น."
                    : $"Start time: {times[0]}:{times[1]}" : "";


                PM.AlignCenter(printer);
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    await PM.PrintImageUrl(printer, imageUrl, "logo.jpg");
                    PM.NewLine(printer);
                }

                PM.PrintTextMediumBold(printer, table);
                PM.PrintTextBold(printer, qrScan);
                PM.LineSpace(printer, 40);
                PM.NewLine(printer);
                PM.PrintTextBold(printer, currentDate);
                PM.LineSpaceDefault(printer);
                PM.NewLine(printer);
                if (!string.IsNullOrEmpty(time))
                {
                    PM.PrintTextMediumBold(printer, time);
                }
                PM.NewLine(printer);
                BuffetEndTime(printer, q);
                BuffetName(printer, q);
                PM.PrintSymbol(printer, qrcode);
                PM.NewLine(printer);
                PM.CutPaper(printer);
                PM.ClosePort(printer);
                await Task.Delay(300);
        });
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
        PM.NewLine(printer);
    }

    static void BuffetName(IntPtr printer, QrCodeModel data) {
        if (string.IsNullOrEmpty(data.Bill.BuffetNames)) return;
        string name = data.Bill.BuffetNames;
        PM.PrintTextBold(printer, name);
        PM.NewLine(printer);
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