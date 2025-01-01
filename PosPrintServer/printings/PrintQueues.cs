using System;
using System.Text;
using PrintingModel;
using System.Net;
using System.Runtime.InteropServices;
using PM = PrinterManager;

public class PrintQueue
{
    public PrintQueue(PrintingQueue data) {
        MessageBox.Show($"data {data}");
        foreach (Printer printer in data.printers)
        {
            if (string.IsNullOrEmpty(printer.ip_address)) continue;
            //IntPtr ptr = PM.GetPrinterConnection(printer.ip_address);
            IntPtr ptr = ESCPOS.InitPrinter("");
            MessageBox.Show($"data {data}");
            Print(ptr, data);
        }
        //IntPtr ptr = PM.GetPrinterConnection("192.168.1.205");
        //Print(ptr, data);
    }

    public async void Print(IntPtr printer, PrintingQueue data)
    {
        string mockupJson = QueueModel.CreateMockupData();
        QueueModel q = QueueModel.FromJson(mockupJson);
        DateTimeHelper dateTimeHelper = new DateTimeHelper();

        string imageUrl = q.ShopQ?.ImageUrl ?? "";
        string date = q.CrrentDate ?? dateTimeHelper.GetCurrentDate("th");
        int qNo = q.Queue?.QueueNo ?? 0;
        int customerAmount = q.Queue?.CustomerAmount ?? 0;
        int waitQCount = q.Queue?.WaitQueueCount ?? 0;

        PM.AlignCenter(printer);
        await PM.PrintImageUrl(printer, imageUrl, "logo.jpg");
        PM.PrintTextBold(printer, "ยินดีต้อนรับ");
        PM.NewLine(printer);
        PM.PrintQueueNumber(printer, qNo);
        PM.NewLine(printer);
        PM.PrintTextBold(printer, $"จํานวนลูกค้า {customerAmount} คน");
        PM.PrintTextBold(printer, $"จํานวนคิวที่รอ {waitQCount} คิว");
        PM.NewLine(printer);
        PM.PrintTextBold(printer, date);
        PM.CutPaper(printer);
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
