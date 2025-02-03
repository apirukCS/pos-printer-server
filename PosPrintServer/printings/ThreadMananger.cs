using PrintingModel;
using SocketIOClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

using System.Text.Json;
using static System.Windows.Forms.Design.AxImporter;
using System.Net;

public class ThreadManager
{
    //private readonly string _printerName;
    private readonly ConcurrentQueue<PrintingQueue> _printQueue;
    private readonly AutoResetEvent _printEvent;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private bool _isPrinting;

    public ThreadManager()
    {
        //_printerName = printerName;
        _printQueue = new ConcurrentQueue<PrintingQueue>();
        _printEvent = new AutoResetEvent(false);
        _cancellationTokenSource = new CancellationTokenSource();
        _isPrinting = false;

        Task.Run(() => ProcessPrintQueue(_cancellationTokenSource.Token));
    }

    private async Task ProcessPrintQueue(CancellationToken cancellationToken) {
        while (!cancellationToken.IsCancellationRequested)
        {
            _printEvent.WaitOne();

            if (!_isPrinting && _printQueue.TryDequeue(out var printData))
            {
                _isPrinting = true;

                try
                {
                    await PrintData(printData);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error printing: {ex.Message}");
                }
                finally
                {
                    _isPrinting = false;
                }
            }
        }
    }

    private async Task PrintData(PrintingQueue data)
    {
        try
        {
            string printingType = data.printingType ?? "";

            for (int i = 0; i < data.printers.Length; i++) {
                string ip = data.printers[i].ip_address ?? "";
                IntPtr ptr = ESCPOS.InitPrinter("");
                int s = ESCPOS.OpenPort(ptr, $"NET,{ip}");

                switch (printingType)
                {
                    case "kitchen":

                        var itemDict = (Dictionary<string, object>)data.jsonData;
                        string jsonString = JsonSerializer.Serialize(itemDict);
                        var item = new PrintingQueue
                        {
                            jsonData = itemDict,
                        };

                        var printer = await PrintKitchen.Create(ptr, item);
                        break;
                    case "qr-code":
                        //var qr = JsonSerializer.Deserialize<QrCodeModel>(jsonString, options);
                        //await QrCodePrint(ptr, qr); //
                        break;
                    default:
                        MessageBox.Show("PrintingType invalid");
                        //keep log
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            //Console.WriteLine($"Print error: {ex.Message}");
        }
    }

    public void AddPrintJob(SocketIOResponse data)
    {
        string jsonData = data.ToString();
        var parsedData = JsonSerializer.Deserialize<PrintingQueue[]>(jsonData);

        for (int i = 0; i < parsedData.Length; i++) {
            _printQueue.Enqueue(parsedData[i]);
            _printEvent.Set();
        }
    }
}
