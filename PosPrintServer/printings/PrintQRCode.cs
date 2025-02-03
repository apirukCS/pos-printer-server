using PM = PrinterManager;
using PrintingModel;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using PosPrintServer;
using System.Net;
using System.Reflection;

public class PrintQRCode
{ //Form1 form
    public static async Task<PrintQRCode> Create(IntPtr ptr, QrCodeModel data)
    {
        var instance = new PrintQRCode();
        await instance.InitializePrinting(ptr, data);
        return instance;
    }

    private async Task InitializePrinting(IntPtr ptr, QrCodeModel data)
    {
        //var form1 = new Form1();
        //form1.
        //printers.FirstOrDefault(p => p.IpAddress == ipAddress);


        await Print(ptr, data);
    }

    public async Task Print(IntPtr printer, QrCodeModel q)
    {
        try
        {
            //Console.WriteLine("call qr --");
            //bool[] s = new bool[7];
            int[] s = new int[8];
            //await Task.Run(async () =>
            //{
            DateTimeHelper dateTimeHelper = new DateTimeHelper();

            string imageUrl = q.Shop.ImageUrl;
            string qrcode = q.QrCode;
            string table = $"{q.Bill.TableZoneName} {q.Bill.TableName}";
            string qrScan = q.Language == "th" ? "QR code เพื่อสแกนสั่งอาหาร" : "QR code for scan to order";
            string currentDate = dateTimeHelper.GetCurrentDate(q.Language ?? "th", true);
            var times = q.Bill.OpenTime != null ? q.Bill.OpenTime.Split(':') : [];
            string time = times.Length > 0 ? q.Language == "th"
                ? $"เวลาเริ่ม: {times[0]}:{times[1]}น."
                : $"Start time: {times[0]}:{times[1]}" : "";

            PM.AlignCenter(printer);
            await Task.Delay(100);
            PM.AlignCenter(printer);
            if (!string.IsNullOrEmpty(imageUrl))
            {
                s[0] = await PM.PrintImageUrl(printer, imageUrl, "logo.jpg");
                PM.NewLine(printer);
            }

            s[1] = PM.PrintTextMediumBold(printer, table);
            s[2] = PM.PrintTextBold(printer, qrScan);
            PM.LineSpace(printer, 40);
            PM.NewLine(printer);
            s[3] = PM.PrintTextBold(printer, currentDate);
            PM.LineSpaceDefault(printer);
            PM.NewLine(printer);
            if (!string.IsNullOrEmpty(time))
            {
                s[4] = PM.PrintTextMediumBold(printer, time);
            }
            PM.NewLine(printer);
            s[5] = BuffetEndTime(printer, q);
            s[6] = BuffetName(printer, q);
            s[7] = PM.PrintSymbol(printer, qrcode);
            PM.NewLine(printer);
            PM.CutPaper(printer);
            //PM.Reset(printer);
            PM.PrinterInitialize(printer);
            int st = PM.ClosePort(printer);
            PM.ReleasePort(printer);
            if (s.Any(x => x != 0))
            {
                WriteLog.WriteFailedPrintLog(q, "qr-code");
                //MessageBox.Show($" some data id false ---");
            }
            //});
            await Task.Delay(100);
            //Console.WriteLine("qr print complete --");
        }
        catch (Exception e)
        {
            PM.ClosePort(printer);
            PM.ReleasePort(printer);
            WriteLog.WriteFailedPrintLog(q, "qr-code");
            //MessageBox.Show($"{e}");
        }
    }

    //private TaskCompletionSource<bool> _printingComplete;

    //private PrintQRCode()
    //{
    //    _printingComplete = new TaskCompletionSource<bool>();
    //}

    //public static async Task<PrintQRCode> Create(IntPtr ptr, QrCodeModel data)
    //{
    //    var instance = new PrintQRCode();
    //    await instance.InitializePrinting(ptr, data);
    //    return instance;
    //}

    //public Task WaitForCompletion()
    //{
    //    return _printingComplete.Task;
    //}

    //private async Task InitializePrinting(IntPtr ptr, QrCodeModel data)
    //{
    //    await Print(ptr, data);
    //}

    //public async Task Print(IntPtr printer, QrCodeModel q)
    //{
    //    try
    //    {
    //        int[] s = new int[8];

    //        await Task.Run(async () =>
    //        {
    //            DateTimeHelper dateTimeHelper = new DateTimeHelper();
    //            string imageUrl = q.Shop.ImageUrl;
    //            string qrcode = q.QrCode;
    //            string table = $"{q.Bill.TableZoneName} {q.Bill.TableName}";
    //            string qrScan = q.Language == "th" ? "QR code เพื่อสแกนสั่งอาหาร" : "QR code for scan to order";
    //            string currentDate = dateTimeHelper.GetCurrentDate(q.Language ?? "th", true);
    //            var times = q.Bill.OpenTime != null ? q.Bill.OpenTime.Split(':') : [];
    //            string time = times.Length > 0 ? q.Language == "th"
    //                ? $"เวลาเริ่ม: {times[0]}:{times[1]}น."
    //                : $"Start time: {times[0]}:{times[1]}" : "";

    //            PM.AlignCenter(printer);
    //            await Task.Delay(100);
    //            PM.AlignCenter(printer);

    //            if (!string.IsNullOrEmpty(imageUrl))
    //            {
    //                s[0] = await PM.PrintImageUrl(printer, imageUrl, "logo.jpg");
    //                PM.NewLine(printer);
    //            }

    //            s[1] = PM.PrintTextMediumBold(printer, table);
    //            s[2] = PM.PrintTextBold(printer, qrScan);
    //            PM.LineSpace(printer, 40);
    //            PM.NewLine(printer);
    //            s[3] = PM.PrintTextBold(printer, currentDate);
    //            PM.LineSpaceDefault(printer);
    //            PM.NewLine(printer);

    //            if (!string.IsNullOrEmpty(time))
    //            {
    //                s[4] = PM.PrintTextMediumBold(printer, time);
    //            }

    //            PM.NewLine(printer);
    //            s[5] = BuffetEndTime(printer, q);
    //            s[6] = BuffetName(printer, q);
    //            s[7] = PM.PrintSymbol(printer, qrcode);
    //            PM.NewLine(printer);
    //            PM.CutPaper(printer);
    //            PM.PrinterInitialize(printer);
    //        });

    //        // รอให้การพิมพ์เสร็จจริงๆ
    //        await Task.Delay(1000); // ปรับเวลาตามความเหมาะสม

    //        if (s.Any(x => x != 0))
    //        {
    //            WriteLog.WriteFailedPrintLog(q, "qr-code");
    //            throw new Exception("Some printing operations failed");
    //        }

    //        _printingComplete.SetResult(true);
    //    }
    //    catch (Exception e)
    //    {
    //        WriteLog.WriteFailedPrintLog(q, "qr-code");
    //        _printingComplete.SetException(e);
    //        throw;
    //    }
    //    finally
    //    {
    //        try
    //        {
    //            int st = PM.ClosePort(printer);
    //            PM.ReleasePort(printer);
    //        }
    //        catch { }
    //    }
    //}

    static int BuffetEndTime(IntPtr printer,QrCodeModel data) {
        if (data.Bill.IsBuffet != true) return 0;
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
        int s = PM.PrintTextMediumBold(printer, time);
        PM.NewLine(printer);
        return s;
    }

    static int BuffetName(IntPtr printer, QrCodeModel data) {
        if (string.IsNullOrEmpty(data.Bill.BuffetNames)) return 0;
        string name = data.Bill.BuffetNames;
        int s= PM.PrintTextBold(printer, name);
        PM.NewLine(printer);
        return s;
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