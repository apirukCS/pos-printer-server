public class WriteLog
{
    public static void Write(string msg)
    {
        string logFilePath = "log.txt";

        // ข้อความที่ต้องการเขียนลงไฟล์
        string logMessage = $"{DateTime.Now}: {msg}.";

        // เขียน log โดยเพิ่มข้อความลงไปในไฟล์เดิม
        using (StreamWriter writer = new StreamWriter(logFilePath, append: true))
        {
            writer.WriteLine(logMessage);
        }
    }
}