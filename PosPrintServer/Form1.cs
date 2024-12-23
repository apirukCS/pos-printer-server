﻿using System;
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
        public Form1()
        {
            InitializeComponent();
            GetPrinters();
            ConnectSocket();
            //new PrintQRCode(null);/
            //new PrintKitchen(null);/vvhh,dveeด

            /////
            new PrintKitchen(null);
            //new PrintBill(null);
        }

        private async void GetPrinters()
        {
            var res = await PrinterAPI.GetPrinters("https://demo7riseplus.resrun-pos.com/rails-api//printers", WSToken());
            DisplayPrinters(res);
        }

        static async void ConnectSocket()
        {
            //MessageBox.Show("call connect socket");
            var serverUrl = "wss://demo7riseplus.resrun-pos.com";
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
            return "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJFZERTQSIsImtpZCI6ImFQRG1SOHZDUGgzb3lXVHl3R1dYV3NteV9BanBDcE1teFo4cFRNMlQ2X1UifQ.eyJjb250ZW50Ijp7ImlzUHJpbnRlciI6dHJ1ZSwicHJpbnRlck5hbWUiOiJkZWZhdWx0In0sImRvbWFpbiI6ImRlbW83cmlzZXBsdXMiLCJpYXQiOjE3MzQzMTgwMjd9.iw3y_q-Z3QCMUklpkQ_Xg5FbTw0wu1S1YPS0I8L80Sgn-qHACm-IvSdyXI3OjE7pJss9FX2V0rMTekzRJrewAg";
        }

        static string WSToken()
        {
            return "eyJ0eXAiOiJKV1QiLCJhbGciOiJFZERTQSIsImtpZCI6ImFQRG1SOHZDUGgzb3lXVHl3R1dYV3NteV9BanBDcE1teFo4cFRNMlQ2X1UifQ.eyJjb250ZW50Ijp7ImlzUHJpbnRlciI6dHJ1ZSwicHJpbnRlck5hbWUiOiJkZWZhdWx0In0sImRvbWFpbiI6ImRlbW83cmlzZXBsdXMiLCJpYXQiOjE3MzQzMTgwMjd9.iw3y_q-Z3QCMUklpkQ_Xg5FbTw0wu1S1YPS0I8L80Sgn-qHACm-IvSdyXI3OjE7pJss9FX2V0rMTekzRJrewAg";
        }

        static void DataPrintingQueue(SocketIOResponse data)
        {
            try
            {
                string jsonData = data.ToString();
                var parsedData = JsonSerializer.Deserialize<PrintingQueue[]>(jsonData);
                foreach (var item in parsedData)
                {
                    //MessageBox.Show($"item.printingType {item}");
                    switch (item.printingType)
                    {
                        case "kitchen":
                            KitchenPrint(item); ////
                            break;
                        case "qr-code":
                            QrCodePrint(item); //
                            break;
                        case "pre-bill":
                            PrebillPrint(item);
                            break;
                        case "queues":
                            QueuePrint(item); //////
                            break;
                        case "receipt":
                            ReceiptPrint(item);
                            break;
                        case "sales_reports-daily-summary":
                            SalesReportsDailySummaryPrint(item);
                            break;
                        default:
                            MessageBox.Show("PrintingType invalid");
                            break;
                    }
                    //MessageBox.Show(item.PrintingType);
                    //kitchen //cacel bill item //change bill type(take home)
                    //qr-code
                    //pre-bill (check bill)
                    //queues (บัตรคิว)
                    //receipt //reprint //payment
                    //sales_reports-daily-summary //รายงานลิ้นชักเงินสด //รายงานสรุปยอดขายรายวัน //รายงานสรุปยอดขายรายเดือน
                    //
                }
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"Failed to parse JSON: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void KitchenPrint(PrintingQueue item)
        {
            //MessageBox.Show("test");
            var printer = new PrintKitchen(item);
            GC.KeepAlive(printer);
        }

        static void QrCodePrint(PrintingQueue item)
        {
            //MessageBox.Show("qr code print");
            //Thread.Sleep(100);
            var qrCode = new PrintQRCode(item);
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
            IntPtr ptr = PM.GetPrinterConnection(ip);
            var res=  PM.GetPrinterStatus(ptr, 2);
            //MessageBox.Show($"res {res}");
            return res;
        }
    }
}