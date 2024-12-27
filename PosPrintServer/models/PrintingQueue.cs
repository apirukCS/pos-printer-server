using System;
using Newtonsoft.Json;

namespace PrintingModel
{
    public class PrintingQueue
    {
        public string? eventName { get; set; }
        public string? printingType { get; set; }
        public Printer[] printers { get; set; } = Array.Empty<Printer>();
        public string? imgData { get; set; }
        public string? imgKey { get; set; }
        public dynamic jsonData { get; set;  }

        public override string ToString()
        {
            return $"ImgKey: {imgKey}, PrintingType: {printingType}, PrintersCount: {printers.Length}, jsonData: {jsonData}";
        }
    }

    public class Printer
    {
        public int id { get; set; }
        public string? name { get; set; }
        public bool? is_ip_connection { get; set; }
        public string? ip_address { get; set; }

        public override string ToString()
        {
            return $"Printer ID: {id}, Name: {name}, IsIPConnection: {is_ip_connection}, IPAddress: {ip_address}";
        }
    }
}