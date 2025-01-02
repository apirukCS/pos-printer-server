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

        public Form1()
        {
            InitializeComponent();
            //GetPrinters();
            ConnectSocket();
            //new PrintQRCode(null);/
            //new PrintKitchen(null);/vvhh,dveeด

            /////
            //new PrintKitchen(null);
            //new PrintBill(null);
        }

        private async void GetPrinters()
        {
            var res = await PrinterAPI.GetPrinters("https://demo2riseplus.resrun-pos.com/rails-api//printers", WSToken());
            DisplayPrinters(res);
        }

        static async void ConnectSocket()
        {
            //MessageBox.Show("call connect socket");
            var serverUrl = "wss://demo2riseplus.resrun-pos.com";
            var options = new SocketIOOptions
            {
                Reconnection = true,
                Path = "/web-socket",
                Transport = TransportProtocol.WebSocket,
                Auth = new { token = WSTokenWithBearer() },
            };

            var socket = new SocketIOClient.SocketIO(serverUrl, options);
            socket.On("printing-queue", response => DataPrintingQueue(response));
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

        static void PrintReceipt()
        {

        }

        static string WSTokenWithBearer()
        {
            return "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJFZERTQSIsImtpZCI6IjBGNXlXY2dBdmd3WDNhcTZGbUJDSlZMNEI1QVdoa245anFYX0NuV3JtaWMifQ.eyJjb250ZW50Ijp7ImlzUHJpbnRlciI6dHJ1ZSwicHJpbnRlck5hbWUiOiJkZWZhdWx0In0sImRvbWFpbiI6ImRlbW8ycmlzZXBsdXMiLCJpYXQiOjE3MzUxMTk0MzJ9.fTVXHQQooPCq0FP_AxZpEfzFfr4KC-3-25f-dSbFddtW2QAmqqSJ6uJNhjelkEwvUlBmo6yDArtxueoszVMUAA";
        }

        static string WSToken()
        {
            return "eyJ0eXAiOiJKV1QiLCJhbGciOiJFZERTQSIsImtpZCI6IjBGNXlXY2dBdmd3WDNhcTZGbUJDSlZMNEI1QVdoa245anFYX0NuV3JtaWMifQ.eyJjb250ZW50Ijp7ImlzUHJpbnRlciI6dHJ1ZSwicHJpbnRlck5hbWUiOiJkZWZhdWx0In0sImRvbWFpbiI6ImRlbW8ycmlzZXBsdXMiLCJpYXQiOjE3MzUxMTk0MzJ9.fTVXHQQooPCq0FP_AxZpEfzFfr4KC-3-25f-dSbFddtW2QAmqqSJ6uJNhjelkEwvUlBmo6yDArtxueoszVMUAA";
        }

        public static void DataPrintingQueue(SocketIOResponse data)
        {
            try
            {
                string jsonData = data.ToString();
                var parsedData = JsonSerializer.Deserialize<PrintingQueue[]>(jsonData);
                //ฉํนต้องการนำ printingType เข้าไปใน jsonData ที่ตรงนี้

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
                            // เพิ่ม JsonDataList เข้าไปในกลุ่มที่มีอยู่แล้ว
                            existingGroup.JsonDataList.AddRange(group.Select(item => item.jsonData));
                        }
                        else
                        {
                            // สร้างกลุ่มใหม่
                            groupedDataStore.Add(new GroupedData
                            {
                                IpAddress = group.Key,
                                JsonDataList = group.Select(item => item.jsonData).ToList()
                            });
                        }
                    }
                }

                if (isPrintingInProgress) return;
                StartPrintingProcess();
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"Failed to parse JSON: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //static void DataPrintingQueue(SocketIOResponse data)
        //{
        //    try
        //    {
        //        string jsonData = data.ToString();
        //        var parsedData = JsonSerializer.Deserialize<PrintingQueue[]>(jsonData);
        //        //***จัดกลุ่มข้อมูล parsedData ข้อมูลที่จะได้คือ กลุ่มเครื่องปริ้น ตัวอย่างข้อมูลที่จะได้คือ 
        //        //*** [{'ip_address':'192.123.12.1',[{parsedData[...].jsonData},{parsedData[...].jsonData}]}]

        //    }
        //    catch (JsonException ex)
        //    {
        //        MessageBox.Show($"Failed to parse JSON: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //}

        public static async void KitchenPrint(IntPtr ptr, PrintingQueue item)
        {
            //MessageBox.Show("calll kitjjj");
            if (ptr != null) {
                //var printer = new PrintKitchen(ptr, item);
                var printer = await PrintKitchen.Create(ptr, item);
                GC.KeepAlive(printer);
            }
        }

        static void QrCodePrint(IntPtr ptr, PrintingQueue item)
        {
            //MessageBox.Show("qr code print");
            //Thread.Sleep(100);
            var qrCode = new PrintQRCode(ptr ,item);
            GC.KeepAlive(qrCode);
        }

        static void PrebillPrint(PrintingQueue item)
        {
            //Thread.Sleep(100);
            var bill = new PrintBill(item, "bill");
            GC.KeepAlive(bill);
        }

        static void QueuePrint(PrintingQueue item)
        {
            //Thread.Sleep(100);
            //MessageBox.Show("q print");
            var q = new PrintQueue(item);
            GC.KeepAlive(q);
        }

        static void ReceiptPrint(PrintingQueue item)
        {
            //Thread.Sleep(100);
            var receipt = new PrintBill(item, "receipt");
            GC.KeepAlive(receipt);
        }

        static void SalesReportsDailySummaryPrint(PrintingQueue item)
        {

        }

        //section test
        static void SaveResponseToFile(string data)
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), "responseData.txt");
            File.WriteAllText(tempFilePath, data);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label2.Text = "Loading...";
            label1.Text = "";
            GetPrinters();
        }

        private void DisplayPrinters(List<PrinterModel>? data)
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
                //if (printer.ip_address == "192.168.1.70")
                //{
                    label1.Text += $"{printer.ip_address}   สถานะ: {CheckPrinterStatus(printer.ip_address)} \n\n";
            //}
        }
            label2.Text = "";
        }

        private string CheckPrinterStatus(string ip) {
            //IntPtr? ptr = PM.GetPrinterConnection(ip);
            //if (ptr) { }
            //var res =  PM.GetPrinterStatus(ptr, 2);
            //MessageBox.Show($"res {res}");

            IntPtr ptr = ESCPOS.InitPrinter("");
            int s = ESCPOS.OpenPort(ptr, $"NET,{ip}");
            int status = 2;
            int ret = ESCPOS.GetPrinterState(ptr, ref status);
            ESCPOS.ClosePort(ptr);
            if (ret == 0)
            {
                if (0x12 == status)
                {
                    return "Ready";
                }
                else if ((status & 0b100) > 0)
                {
                    return "Cover opened";
                }
                else if ((status & 0b1000) > 0)
                {
                    return "Feed button has been pressed";
                }
                else if ((status & 0b100000) > 0)
                {
                    return "Printer is out of paper";
                }
                else if ((status & 0b1000000) > 0)
                {
                    return "Error condition";
                }
                else
                {
                    return "Other Error";
                }
            }
            else if (ret == -2)
            {
                return "Failed with invalid handle";
            }
            else if (ret == -1)
            {
                return "Invalid argument";
            }
            else if (ret == -4)
            {
                return "Failed, out of memory";
            }
            else if (ret == -9)
            {
                return "Failed to send data";
            }
            else if (ret == -10)
            {
                return "Write data timed out";
            }
            else
            {
                return "Failed to connection";
            }
            return "";
        }

        public static void StartPrintingProcess()
        {
            try {
                //if (isPrintingInProgress) return; // ห้ามเริ่มใหม่หากกำลังพิมพ์อยู่
                //isPrintingInProgress = true;

                //if (printTimer != null)
                //{
                //    printTimer.Stop();
                //    printTimer.Dispose();
                //}

                isPrintingInProgress = true;

                printTimer = new System.Timers.Timer(500); // เรียกทุกๆ 1 วินาที
                printTimer.Elapsed += async (sender, e) =>
                {
                    //printTimer.Stop();

                    List<(string IpAddress, dynamic JsonData)> dataToPrint = new();

                    // ดึงข้อมูลออกจากตัวแปรกลาง (thread-safe)
                    lock (lockObject)
                    {
                        foreach (var group in groupedDataStore.ToList()) // ใช้ ToList เพื่อหลีกเลี่ยง collection ถูกเปลี่ยนระหว่าง loop
                        {
                            if (group.JsonDataList.Any())
                            {
                                // ดึง JsonData ตัวแรกของกลุ่ม
                                var jsonData = group.JsonDataList.First();
                                dataToPrint.Add((group.IpAddress, jsonData));

                                // ลบ JsonData ตัวแรกออกจากกลุ่ม
                                group.JsonDataList.RemoveAt(0);
                            }
                        }
                    }

                    // สร้าง Task เพื่อพิมพ์พร้อมกัน
                    if (dataToPrint.Any())
                    {
                        //WriteLog.Write($"call print receipt");
                        var tasks = dataToPrint.Select(data => (Task)PrintDataAsync(data.IpAddress, data.JsonData));
                        await Task.WhenAll(tasks);
                        //for (int i = 0; i < dataToPrint.Count; i++) {
                        //    await PrintDataAsync(dataToPrint[i].IpAddress, dataToPrint[i].JsonData);
                        //}
                    }

                    if (!groupedDataStore.Any(group => group.JsonDataList.Any()))
                    {
                        printTimer.Stop();
                        printTimer.Dispose();
                        isPrintingInProgress = false;
                    }
                };
                printTimer.Start();
            }
            catch (Exception ex) {
                MessageBox.Show("เกิดข้อผิดพลาด StartPrintingProcess");
            }
        }

        public static void AddToGroupedDataStore(string ipAddress, dynamic jsonData)
        {
            lock (groupedDataStore)
            {
                var existingGroup = groupedDataStore.FirstOrDefault(group => group.IpAddress == ipAddress);
                if (existingGroup != null)
                {
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

                //printTimer = new System.Timers.Timer(200);
                //printTimer.Start();
                StartPrintingProcess();
                //if (printTimer == null) {
                //    StartPrintingProcess();
                //}

                //
            }
        }


        private static async Task PrintDataAsync(string ipAddress, dynamic jsonData)
        {
            try {
                await Task.Run(() =>
                {
                    string printingType = jsonData["printing_type"];
                    var itemDict = (Dictionary<string, object>)jsonData;
                    var item = new PrintingQueue
                    {
                        jsonData = itemDict,
                    };

                    //IntPtr? ptr = PM.GetPrinterConnection(ipAddress);
                    IntPtr ptr = ESCPOS.InitPrinter("");
                    int s = ESCPOS.OpenPort(ptr, $"NET,{ipAddress}");
                    if (s != 0)
                    {
                        AddToGroupedDataStore(ipAddress, jsonData);
                        string jsonString = JsonSerializer.Serialize(jsonData);
                        //WriteLog.Write($"resent {jsonString}");
                        return;
                    }
                    else {
                        //WriteLog.Write($"success {jsonData}");
                    }

                    switch (printingType)
                    {
                        case "kitchen":
                            KitchenPrint(ptr, item); //
                            break;
                        case "qr-code":
                            QrCodePrint(ptr ,item); //
                            break;
                        case "pre-bill":
                            PrebillPrint(item);
                            break;
                        case "queues":
                            QueuePrint(item); //
                            break;
                        case "receipt":
                            ReceiptPrint(item);
                            break;
                        case "sales_reports-daily_summary":
                            // WriteFile($"{item.jsonData}");
                            SalesReportsDailySummaryPrint(item);
                            break;
                        default:
                            MessageBox.Show("PrintingType invalid");
                            break;
                    }
                    // await Task.Delay(500);
                });

            }
            catch (Exception e) {
                //WriteLog.Write($"err {e}");
                MessageBox.Show($"เกิดข้อผิดพลาด PrintDataAsync {e}");
            }
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
}
