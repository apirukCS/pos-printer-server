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
        string folderPath = "/numbers";
        string fontName = "Kanit";
        int fontSize = 70; // ปรับขนาดฟอนต์ให้เหมาะสม
        int imageWidth = 90;
        int imageHeight = 120;

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

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

                    Rectangle rect = new Rectangle(0, -20, imageWidth, imageHeight + 20);

                    g.DrawString(i.ToString(), font, Brushes.Black, rect, format);
                }

                string fileName = Path.Combine(folderPath, $"{i}.jpeg");
                bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
        }
    }
}