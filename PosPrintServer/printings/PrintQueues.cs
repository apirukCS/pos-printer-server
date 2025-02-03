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
        try {
            int[] s = new int[7];
            //await Task.Run(async () =>
            //{
                DateTimeHelper dateTimeHelper = new DateTimeHelper();

                string language = q.language ?? "th";
                string imageUrl = q.shop?.image_url ?? "";
                string date = dateTimeHelper.GetCurrentDate(language, true);
                int qNo = q.queue.queue_no ?? 0;
                int customerAmount = q.queue.customer_amount ?? 0;
                int waitQCount = q.queue.wait_queue_count ?? 0;

                PM.AlignCenter(printer);
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    s[0] = await PM.PrintImageUrl(printer, imageUrl, "logo.jpg");
                    PM.NewLine(printer);
                }
                s[1] = PM.PrintTextBold(printer, language == "en" ? "Welcome": "ยินดีต้อนรับ");
                PM.NewLine(printer);
                if (language == "en") {
                    s[2] = PM.PrintTextBold(printer, "Number of");
                    PM.NewLine(printer);
                }
                s[3] = PM.PrintQueueNumber(printer, qNo);
                PM.NewLine(printer);
                s[4] = PM.PrintTextBold(printer, language == "en" ? $"Customers {customerAmount} Prs." : $"จํานวนลูกค้า {customerAmount} คน");
                if (language == "en")
                {
                    PM.NewLine(printer, 20);
                }
                s[5] = PM.PrintTextBold(printer, language == "en" ? $"Waiting {waitQCount} Queue" : $"จํานวนคิวที่รอ {waitQCount} คิว");
                PM.NewLine(printer);
                s[6] = PM.PrintTextBold(printer, date);
                PM.CutPaper(printer);
                PM.ClosePort(printer);
                PM.ReleasePort(printer);
                if (s.Any(x => x != 0))
                {
                    WriteLog.WriteFailedPrintLog(q, "queue");
                }
                await Task.Delay(100);
            //});
        }
        catch (Exception e) {
            PM.ClosePort(printer);
            PM.ReleasePort(printer);
            WriteLog.WriteFailedPrintLog(q, "queue");
            //MessageBox.Show($"{e}");
        }
    }
}
