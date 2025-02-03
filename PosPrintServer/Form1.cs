using System;
using System.Threading.Tasks;
using SocketIOClient;
using SocketIOClient.Transport;
using System.Text.Json;
using PrintingModel;
using System.Text;
using System.Reflection;
using PM = PrinterManager;

namespace PosPrintServer
{
    public partial class Form1 : Form
    {
        public static List<GroupedData> groupedDataStore = new();
        private static readonly object lockObject = new(); // สำหรับป้องกัน race condition
        private static System.Timers.Timer? printTimer = null;
        private static bool isPrintingInProgress = false;
        private static Dictionary<string, IntPtr> printerConnections = new Dictionary<string, IntPtr>();
        public static List<(string IpAddress, IntPtr Printer)> printers = new List<(string, IntPtr)>();
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public Form1()
        {
            InitializeComponent();
            GenerateNumber.create();

            textBox1.Text = Properties.Settings.Default.Domain;
            textBox2.Text = Properties.Settings.Default.Token;
            if (string.IsNullOrEmpty(Properties.Settings.Default.Domain) || string.IsNullOrEmpty(Properties.Settings.Default.Token))
            {
                MessageBox.Show("กรุณาตั้งค่า Shop Domain และ Access Token!");
            }
            else {
                GetPrinters();
                ConnectSocket();
            }
        }



        private async void GetPrinters()
        {
            var res = await PrinterAPI.GetPrinters($"https://{Properties.Settings.Default.Domain}.resrun-pos.com/rails-api//printers", WSToken());
            DisplayPrinters(res);
        }

        static async void ConnectSocket()
        {
            //MessageBox.Show("call connect socket");
            var serverUrl = $"wss://{Properties.Settings.Default.Domain}.resrun-pos.com";
            var options = new SocketIOOptions
            {
                Reconnection = true,
                Path = "/web-socket",
                Transport = TransportProtocol.WebSocket,
                Auth = new { token = WSTokenWithBearer() },
            };

            var socket = new SocketIOClient.SocketIO(serverUrl, options);
            socket.On("printing-queue", response => DataPrintingQueue(response));
            socket.On("cashdraw", response => CashDraw(response));
            await socket.ConnectAsync();

            socket.OnConnected += (sender, e) =>
            {
                //MessageBox.Show("connected");
            };

            socket.OnError += (sender, e) =>
            {
                //MessageBox.Show($"on error {e.ToString()}");
            };

            socket.OnDisconnected += (sender, e) =>
            {
                //MessageBox.Show("on disconnect");
            };

            socket.On("connect_error", response =>
            {
                //MessageBox.Show("connect_error ", response.ToString());
            });
        }

        static void CashDraw(SocketIOResponse data)
        {
            string jsonData = data.ToString();
            var parsedData = JsonSerializer.Deserialize<List<CashDrawerModel>>(jsonData);

            if (parsedData != null && parsedData.Count > 0)
            {
                string ipTarget = parsedData[0].Destination.IpTarget;
                CashDrawer.Kick(ipTarget);
            }
        }

        static string WSTokenWithBearer()
        {
            return $"Bearer {Properties.Settings.Default.Token}";
        }

        static string WSToken()
        {
            return Properties.Settings.Default.Token;
        }

        public static void DataPrintingQueue(SocketIOResponse data)
        {
            try
            {
                string jsonData = data.ToString();
                var parsedData = JsonSerializer.Deserialize<PrintingQueue[]>(jsonData);
                
                bool isFirstQueue = false;

                foreach (var queue in parsedData)
                {
                    var jsonDataDict = new Dictionary<string, object>();
                    if (queue.jsonData is JsonElement jsonElement && jsonElement.ValueKind != JsonValueKind.Undefined)
                    {
                        foreach (var property in jsonElement.EnumerateObject())
                        {
                            jsonDataDict[property.Name] = property.Value;
                        }
                    }

                    jsonDataDict["printing_type"] = queue.printingType;
                    jsonDataDict["cashdraw"] = queue.cashdraw;
                    isFirstQueue = queue.printingType == "receipt" || queue.printingType == "qr-code" || queue.printingType == "pre-bill";

                    if (queue.language != null) {
                        jsonDataDict["language"] = queue.language;
                    }
                    queue.jsonData = jsonDataDict;
                }

                var groupedData = parsedData
                    .SelectMany(queue => queue.printers.Select(printer => new
                    {
                        printer.ip_address,
                        queue.jsonData
                    }))
                    .GroupBy(item => item.ip_address)
                    .ToList();

                lock (lockObject)
                {
                    foreach (var group in groupedData)
                    {
                        var existingGroup = groupedDataStore.FirstOrDefault(g => g.IpAddress == group.Key);
                        if (existingGroup != null)
                        {
                            //existingGroup.JsonDataList.AddRange(group.Select(item => item.jsonData));
                            foreach (var item in group)
                            {
                                if (isFirstQueue)
                                {
                                    existingGroup.JsonDataList.Insert(0, item.jsonData);
                                }
                                else
                                {
                                    existingGroup.JsonDataList.Add(item.jsonData);
                                }
                            }
                        }
                        else
                        {
                            groupedDataStore.Add(new GroupedData
                            {
                                IpAddress = group.Key,
                                JsonDataList = group.Select(item => item.jsonData).ToList()
                            });
                        }
                    }
                }
                isFirstQueue = false;
                if (isPrintingInProgress) return;
                StartPrintingProcess();
            }
            catch (JsonException ex)
            {
                WriteLog.Write($"DataPrintingQueue Error: {ex}");
                //keep log
                //MessageBox.Show($"Failed to parse JSON: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static async Task KitchenPrint(IntPtr ptr, PrintingQueue item)
        {
            var printer = await PrintKitchen.Create(ptr, item);
            GC.KeepAlive(printer);
            await Task.CompletedTask;
        }

        static async Task QrCodePrint(IntPtr ptr, QrCodeModel item)
        {
            var qrCode = await PrintQRCode.Create(ptr, item);
            GC.KeepAlive(qrCode);
            //await Task.CompletedTask;

            //await _semaphore.WaitAsync();
            //try
            //{
            //    var printer = await PrintQRCode.Create(ptr, item);
            //    GC.KeepAlive(printer);
            //}
            //finally
            //{
            //    _semaphore.Release();
            //}
        }

        static async Task PrebillPrint(IntPtr ptr, BillModel item)
        {
            var bill = await PrintBill.Create(ptr, item, "bill");
            GC.KeepAlive(bill);
        }

        static async Task QueuePrint(IntPtr ptr, QueueModel item)
        {
            var q = await PrintQueue.Create(ptr, item);
            GC.KeepAlive(q);
        }

        static async Task ReceiptPrint(IntPtr ptr, BillModel item, bool cashdraw)
        {
            var receipt = await PrintBill.Create(ptr, item, "receipt", cashdraw);
            GC.KeepAlive(receipt);
        }

        static async Task InvoicePrint(IntPtr ptr, BillModel item)
        {
            var receipt = await PrintBill.Create(ptr, item, "invoice");
            GC.KeepAlive(receipt);
        }

        static async Task SalesReportsDailySummaryPrint(IntPtr ptr, Report item)
        {
            var report = await PrintReport.Create(ptr, item);
            GC.KeepAlive(report);
        }


        private void button1_Click(object sender, EventArgs e)
        {
            //label2.Text = "Loading...";
            label1.Text = "";
            GetPrinters();
        }

        private async void DisplayPrinters(List<PrinterModel>? data)
        {
            label1.Font = new Font("Arial", 11);
            label1.Text = "";

            if (data == null)
            {
                label1.Text = "ไม่พบรายการเครื่องพิมพ์";
                return;
            }

            foreach (var printer in data)
            {
                if (!string.IsNullOrEmpty(printer.ip_address))
                {
                    label1.Text += $"{printer.ip_address}   สถานะ: กำลังตรวจสอบ...\n";
                    string status = await Task.Run(() => CheckPrinterStatus(printer.ip_address));
                    label1.Text = label1.Text.Replace("กำลังตรวจสอบ...", status);
                }
            }
        }

        private string CheckPrinterStatus(string ip)
        {
            for (int i = 0; i < printers.Count; i++) {
                PM.ClosePort(printers[i].Printer);
                printers.RemoveAt(i);
            }
            IntPtr ptr = PM.GetPrinterConnection(ip);
            var res = PM.GetPrinterStatus(ptr, 2);
            return res;
        }

        public static void StartPrintingProcess()
        {
            try
            {
                isPrintingInProgress = true;

                printTimer = new System.Timers.Timer(20);
                printTimer.Elapsed += async (sender, e) =>
                {
                    // Prevent overlapping executions
                    printTimer.Stop();

                    try
                    {
                        List<(string IpAddress, dynamic JsonData)> dataToPrint = new();

                        lock (lockObject)
                        {
                            foreach (var group in groupedDataStore.ToList())
                            {
                                if (group.JsonDataList.Any())
                                {
                                    var jsonData = group.JsonDataList.First();
                                    dataToPrint.Add((group.IpAddress, jsonData));
                                    group.JsonDataList.RemoveAt(0);
                                }
                            }
                        }

                        if (dataToPrint.Any())
                        {
                            for (int i = 0; i < dataToPrint.Count; i++)
                            {
                                Console.WriteLine($"Printing... {i}");
                                string res = await PrintDataAsync(dataToPrint[i].IpAddress, dataToPrint[i].JsonData); // Wait for the process to complete
                                Console.WriteLine($"Printed: {res}");
                            }
                        }

                        // Stop the timer if there's no more data to process
                        if (!groupedDataStore.Any(group => group.JsonDataList.Any()))
                        {
                            printTimer.Dispose();
                            isPrintingInProgress = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog.Write($"Error during printing: {ex}");
                    }
                    finally
                    {
                        // Restart the timer if there is still data to process
                        if (isPrintingInProgress)
                        {
                            printTimer.Start();
                        }
                    }
                };

                printTimer.Start();
            }
            catch (Exception ex)
            {
                WriteLog.Write($"StartPrintingProcess Error: {ex}");
            }
        }
        public static void AddToGroupedDataStore(string ipAddress, dynamic jsonData)
        {
            try {
                lock (groupedDataStore)
                {
                    var existingGroup = groupedDataStore.FirstOrDefault(group => group.IpAddress == ipAddress);
                    if (existingGroup != null)
                    {
                        //ToDo
                        existingGroup.JsonDataList.Add(jsonData);
                    }
                    else
                    {
                        groupedDataStore.Add(new GroupedData
                        {
                            IpAddress = ipAddress,
                            JsonDataList = new List<dynamic> { jsonData }
                        });
                    }

                    StartPrintingProcess();
                }
            }
            catch (Exception ex) {
                WriteLog.Write($"AddToGroupedDataStore Error: {ex}");
            }
        }


        private static async Task<string> PrintDataAsync(string ipAddress, dynamic jsonData)
        {
            try
            {
                string printingType = jsonData["printing_type"];
                bool cashdraw = jsonData["cashdraw"] ?? false;

                IntPtr ptr = ESCPOS.InitPrinter("");
                Console.WriteLine($"connecting... {ipAddress}");
                int s = ESCPOS.OpenPort(ptr, $"NET,{ipAddress}");

                if (s != 0)
                {
                    AddToGroupedDataStore(ipAddress, jsonData);
                    return "AddToGroupedDataStore";
                }
                else
                {
                    printers.Add((ipAddress, ptr));
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var itemDict = (Dictionary<string, object>)jsonData;
                string jsonString = JsonSerializer.Serialize(itemDict);
                var item = new PrintingQueue
                {
                    jsonData = itemDict,
                };

                if (printingType == "kitchen") {
                    Console.WriteLine("printing== KitchenPrint");
                    await KitchenPrint(ptr, item); //

                    Console.WriteLine("printed-- KitchenPrint");
                    return "kitchen";
                }
                else if (printingType == "qr-code") {
                    Console.WriteLine("printing== QrCodePrint");
                    var qr = JsonSerializer.Deserialize<QrCodeModel>(jsonString, options);
                    await QrCodePrint(ptr, qr); //
                    Console.WriteLine("printed-- QrCodePrint");
                    return "qr-code";
                } else if (printingType == "pre-bill") {
                    BillWrapper? bill = JsonSerializer.Deserialize<BillWrapper>(jsonString);
                    bill.bill.language = bill.language;
                    await PrebillPrint(ptr, bill.bill); //
                    return "prebill";
                    //break;
                }
                else if (printingType == "queues")
                {
                    var queue = JsonSerializer.Deserialize<QueueModel>(jsonString, options);
                    await QueuePrint(ptr, queue); //
                    return "queue";
                }
                else if (printingType == "receipt")
                {
                    BillWrapper bill = JsonSerializer.Deserialize<BillWrapper>(jsonString);
                    bill.bill.language = bill.language;
                    await ReceiptPrint(ptr, bill.bill, cashdraw); //
                    return "receipt";
                }
                else if (printingType == "invoice") {
                    BillWrapper bill = JsonSerializer.Deserialize<BillWrapper>(jsonString);
                    await InvoicePrint(ptr, bill.bill); //
                }
                else if (printingType == "sales_reports-daily_summary") {
                    try
                    {
                        options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };

                        using var jsonDoc = JsonDocument.Parse(jsonString);
                        var root = jsonDoc.RootElement;

                        var jsonObject = new Dictionary<string, object>();

                        foreach (var property in root.EnumerateObject())
                        {
                            if (property.Name == "pos_round" && property.Value.ValueKind == JsonValueKind.String)
                            {
                                var posRoundJson = property.Value.GetString();
                                var posRoundObject = JsonSerializer.Deserialize<PosRound>(posRoundJson, options);
                                jsonObject[property.Name] = posRoundObject;
                            }
                            else
                            {
                                jsonObject[property.Name] = property.Value.Clone();
                            }
                        }

                        var modifiedJson = JsonSerializer.Serialize(jsonObject, options);
                        var report = JsonSerializer.Deserialize<Report>(modifiedJson, options);
                        await SalesReportsDailySummaryPrint(ptr, report);
                        return "sales_reports-daily_summary";
                    }
                    catch (Exception ex)
                    {
                        //kepp log
                        //WriteLog.Write($"err {ex.Message}");
                        //MessageBox.Show($"Error deserializing Report: {ex.Message}");
                    }
                }

                Console.WriteLine("complete in form1--");
                return "--";
            }
            catch (Exception e)
            {
                WriteLog.Write($"PrintDataAsync Error: {e}");
                return "error";
            }
          
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Domain = textBox1.Text;
            Properties.Settings.Default.Token = textBox2.Text;
            Properties.Settings.Default.Save();
            ConnectSocket();

            MessageBox.Show($"Settings saved!");
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            GetPrinters();
            ConnectSocket();
        }
    }
}
