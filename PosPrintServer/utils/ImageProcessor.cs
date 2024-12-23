using System;
using System.Net.Http;
using System.Threading.Tasks;
using ImageMagick;

using System.Drawing;
using System.Drawing.Text;
using System.IO;

public static class ImageProcessor
{
    public static async Task ProcessImageFromUrlAsync(string url, string outputPath,uint width = 200)
    {
        byte[] imageBytes = await DownloadImageFromUrlAsync(url);
        //string p = "C:\\dotnet\\PosPrintServer\\PosPrintServer\\images\\numbers\\5.jpg";
        //byte[] imageBytes = File.ReadAllBytes(p);
        using (MagickImage image = new MagickImage(imageBytes))
        {
            //image.Threshold(new Percentage(70));
            image.Resize(new MagickGeometry(width, 0));
            image.Quality = 60;
            //image.Density = new Density(72, 72);
            image.Write(outputPath, MagickFormat.Jpg);
        }
    }

    private static async Task<byte[]> DownloadImageFromUrlAsync(string url)
    {
        using (HttpClient client = new HttpClient())
        {
            return await client.GetByteArrayAsync(url);
        }
    }

    public static void MergeTwoImages(string imagePath1, string imagePath2, string outputPath)
    {
        using (Bitmap image1 = new Bitmap(imagePath1))
        using (Bitmap image2 = new Bitmap(imagePath2))
        {
            int newWidth = image1.Width + image2.Width;
            int newHeight = Math.Max(image1.Height, image2.Height);
            using (Bitmap combinedImage = new Bitmap(newWidth, newHeight))
            {
                using (Graphics g = Graphics.FromImage(combinedImage))
                {
                    combinedImage.SetResolution(72, 72);

                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                    g.DrawImage(image1, new Rectangle(0, 0, image1.Width, image1.Height));
                    g.DrawImage(image2, new Rectangle(image1.Width, 0, image2.Width, image2.Height));
                }
                combinedImage.Save(outputPath);
                combinedImage.Dispose();
            }
        }
    }

    public static void MergeThreeImages(string imagePath1, string imagePath2, string imagePath3, string outputPath)
    {
        using (Bitmap image1 = new Bitmap(imagePath1))
        using (Bitmap image2 = new Bitmap(imagePath2))
        using (Bitmap image3 = new Bitmap(imagePath3))
        {
            int newWidth = image1.Width + image2.Width + image3.Width;
            int newHeight = Math.Max(image1.Height, Math.Max(image2.Height, image3.Height));
            using (Bitmap combinedImage = new Bitmap(newWidth, newHeight))
            {
                using (Graphics g = Graphics.FromImage(combinedImage))
                {
                    combinedImage.SetResolution(72, 72);
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                    g.DrawImage(image1, new Rectangle(0, 0, image1.Width, image1.Height));
                    g.DrawImage(image2, new Rectangle(image1.Width, 0, image2.Width, image2.Height));
                    g.DrawImage(image3, new Rectangle(image1.Width + image2.Width, 0, image3.Width, image3.Height));
                }
                combinedImage.Save(outputPath);
                combinedImage.Dispose();
            }
        }
    }

    public static void GenerateNumberImages()
    {
        // ฟอนต์และขนาดที่ต้องการ
        string fontName = "Kanit";
        int fontSize = 70; // ปรับขนาดฟอนต์ให้เหมาะสม
        int imageWidth = 90;
        int imageHeight = 120;

        // สร้างไฟล์ภาพสำหรับตัวเลข 1 ถึง 10
        for (int i = 0; i < 10; i++)
        {
            using (Bitmap bitmap = new Bitmap(imageWidth, imageHeight))
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                // ตั้งค่า background และคุณภาพของการวาด
                g.Clear(Color.White);
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                // ตั้งค่า font style
                using (Font font = new Font(fontName, fontSize, FontStyle.Bold))
                {
                    // จัดข้อความให้อยู่กึ่งกลางแนวนอนและเลื่อนลงด้านล่าง
                    StringFormat format = new StringFormat();
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Near;

                    // คำนวณตำแหน่งสำหรับวาดตัวเลขที่ลดขอบบน
                    Rectangle rect = new Rectangle(0, -20, imageWidth, imageHeight + 20); // ลดขอบบนด้วยการเพิ่มระยะ Y

                    // วาดตัวเลขลงในรูปภาพ
                    g.DrawString(i.ToString(), font, Brushes.Black, rect, format);
                }

                // สร้างชื่อไฟล์ (เช่น 1.jpeg, 2.jpeg, ...)
                string fileName = $@"C:\dotnet\PosPrintServer\PosPrintServer\images\numbers\{i}.jpeg"; // แก้ไข path ตามที่ต้องการ

                // บันทึกเป็นไฟล์ JPEG
                bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
        }

        Console.WriteLine("Number images generated successfully!");
    }

}
