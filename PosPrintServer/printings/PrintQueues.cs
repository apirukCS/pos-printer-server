using System;
using System.Text;
using PrintingModel;
using System.Net;
using System.Runtime.InteropServices;
using PM = PrinterManager;

public class PrintQueue
{
    public PrintQueue(PrintingQueue data) {
        //MessageBox.Show($"data {data}");
        //foreach (Printer printer in data.printers)
        //{
        //    if (string.IsNullOrEmpty(printer.ip_address)) continue;
        //    IntPtr ptr = PM.GetPrinterConnection(printer.ip_address);
        //    Print(ptr, data);
        //}
        IntPtr ptr = PM.GetPrinterConnection("192.168.1.205");
        Print(ptr, data);
    }

    public async void Print(IntPtr printer, PrintingQueue data)
    {
        //MessageBox.Show($"call print qqqqq");
        string mockupJson = QueueModel.CreateMockupData();
        QueueModel q = QueueModel.FromJson(mockupJson);
        //MessageBox.Show($"call print weq {q}");
        DateTimeHelper dateTimeHelper = new DateTimeHelper();
        //MessageBox.Show($"call print dateTimeHelper {dateTimeHelper}");

        string imageUrl = q.ShopQ?.ImageUrl ?? "";
        //MessageBox.Show($"call print imageUrl {imageUrl}");
        string date = q.CrrentDate ?? dateTimeHelper.GetCurrentDate("th");
        //MessageBox.Show($"call print date {date}");
        int qNo = q.Queue?.QueueNo ?? 0;
        //MessageBox.Show($"call print qNo {qNo}");
        int customerAmount = q.Queue?.CustomerAmount ?? 0;
        //MessageBox.Show($"call print customerAmount {customerAmount}");
        int waitQCount = q.Queue?.WaitQueueCount ?? 0;
        //MessageBox.Show($"call print q {waitQCount} {date} {qNo} {customerAmount}");

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
        //MessageBox.Show("end");
    }
}
