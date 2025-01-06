using System.IO;
using System;

using ImageMagick;

using System.Drawing;
using System.Drawing.Text;
using System.IO;

public static class GenerateNumber
{
    public static void create()
    {
        string folderPath = @"C:/pos-printer-server/numbers";
        string fontName = "Kanit";
        int fontSize = 90; // ปรับขนาดฟอนต์ให้เหมาะสม
        int imageWidth = 90;
        int imageHeight = 120;

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Console.WriteLine($"สร้างโฟลเดอร์ {folderPath} เรียบร้อยแล้ว");
        }

        // สร้างไฟล์ภาพสำหรับตัวเลข 1 ถึง 10
        for (int i = 0; i < 10; i++)
        {
            using (Bitmap bitmap = new Bitmap(imageWidth, imageHeight))
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;


                using (Font font = new Font(fontName, fontSize, FontStyle.Bold))
                {
                    StringFormat format = new StringFormat();
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Near;

                    Rectangle rect = new Rectangle(0, -20, imageWidth, imageHeight + 20); // ลดขอบบนด้วยการเพิ่มระยะ Y

                    g.DrawString(i.ToString(), font, Brushes.Black, rect, format);
                }

                string fileName = Path.Combine(folderPath, $"{i}.jpeg");

                // บันทึกเป็นไฟล์ JPEG
                bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
        }
    }
}