using System;
using System.Text;
using System.Text.Json;
using PrintingModel;
using System.Net;
using System.Runtime.InteropServices;
using PM = PrinterManager;

public class PrintQueue
{
    public static async Task<PrintQueue> Create(IntPtr ptr, QueueModel data)
    {
        var instance = new PrintQueue();
        await instance.InitializePrinting(ptr, data);
        return instance;
    }

    private async Task InitializePrinting(IntPtr ptr, QueueModel data)
    {
        await Print(ptr, data);
    }

    public async Task Print(IntPtr printer, QueueModel q)
    {
        await Task.Run(async () =>
        {
            DateTimeHelper dateTimeHelper = new DateTimeHelper();

            string imageUrl = q.shop?.image_url ?? "";
            string date = dateTimeHelper.GetCurrentDate("th");
            int qNo = q.queue.queue_no ?? 0;
            int customerAmount = q.queue.customer_amount ?? 0;
            int waitQCount = q.queue.wait_queue_count ?? 0;

            PM.AlignCenter(printer);
            if (!string.IsNullOrEmpty(imageUrl)) {
                await PM.PrintImageUrl(printer, imageUrl, "logo.jpg");
            }
            PM.PrintTextBold(printer, "ยินดีต้อนรับ");
            PM.NewLine(printer);
            PM.PrintQueueNumber(printer, qNo);
            PM.NewLine(printer);
            PM.PrintTextBold(printer, $"จํานวนลูกค้า {customerAmount} คน");
            PM.PrintTextBold(printer, $"จํานวนคิวที่รอ {waitQCount} คิว");
            PM.NewLine(printer);
            PM.PrintTextBold(printer, date);
            PM.CutPaper(printer);
            PM.ClosePort(printer);
            await Task.Delay(300);
        });
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
