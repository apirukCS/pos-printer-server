using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class WriteLog
{
    public static void Write(string msg, string type = "")
    {
        //try
        //{
        //    string logFilePath = @"C:\CodeErrorLogs.txt"; // กำหนดไฟล์ในโฟลเดอร์ Logs
        //    string logDirectory = Path.GetDirectoryName(logFilePath);

        //    // ตรวจสอบและสร้างโฟลเดอร์หากยังไม่มี
        //    if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
        //    {
        //        Directory.CreateDirectory(logDirectory);
        //    }

        //    // สร้างข้อความบันทึก
        //    string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: {msg}";

        //    // เขียนข้อมูลลงไฟล์
        //    using (StreamWriter writer = new StreamWriter(logFilePath, append: true))
        //    {
        //        writer.WriteLine(logMessage);
        //    }
        //}
        //catch (Exception ex)
        //{
        //    // จัดการข้อผิดพลาดในระหว่างการเขียน Log
        //    //Console.WriteLine($"Error writing log: {ex.Message}");
        //    MessageBox.Show($"Error: {ex.Message}");
        //}
    }


    private static readonly object fileLock = new object();

    public static void WriteFailedPrintLog(object jsonData, string printType)
    {
        try
        {
            string logFilePath = "ReceiptPrintFailed.json";
            List<object> logs = new List<object>();

            // Ensure exclusive access using a lock
            lock (fileLock)
            {
                if (!File.Exists(logFilePath))
                {
                    File.WriteAllText(logFilePath, "[]"); // สร้างไฟล์เปล่าพร้อมข้อมูล JSON array
                }
                // Read the existing data safely
                if (File.Exists(logFilePath))
                {
                    string existingData = File.ReadAllText(logFilePath);
                    if (!string.IsNullOrWhiteSpace(existingData))
                    {
                        logs = JsonSerializer.Deserialize<List<object>>(existingData);
                    }
                }

                // Add new log entry
                var logEntry = new
                {
                    Data = jsonData,
                    Timestamp = DateTime.Now,
                    PrintingType = printType,
                };

                logs.Add(logEntry);

                // Write the updated data back to the file
                string updatedData = JsonSerializer.Serialize(logs, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(logFilePath, updatedData);
            }
        }
        catch (Exception e)
        {
            // Log or display the error
            MessageBox.Show($"Error: {e.Message}");
        }
    }
}